using System.Runtime.InteropServices;

namespace StellarDrive_Tech
{
    internal class TinyCC
    {
        IntPtr _state;

        public enum OutputType : int
        {
            Object = 3
        };

        public class CompilationFailedException : Exception
        {
            public CompilationFailedException() {}
            public CompilationFailedException(string message) : base(message) {}
            public CompilationFailedException(string message, Exception inner) : base(message, inner) {}
        }

        public class OutputFailedException : Exception
        {
            public OutputFailedException() { }
            public OutputFailedException(string message) : base(message) { }
            public OutputFailedException(string message, Exception inner) : base(message, inner) { }
        }

        public TinyCC()
        {
            _state = tcc_new();
            tcc_set_output_type(_state, (int)OutputType.Object);
        }

        public void SetErrorCallback(Action<string> callback)
        {
            tcc_set_error_func(_state, IntPtr.Zero, (IntPtr _, string error) => callback(error));
        }

        public void AddSysIncludePath(string path)
        {
            tcc_add_sysinclude_path(_state, path);
        }

        public void AddIncludePath(string path)
        {
            tcc_add_include_path(_state, path);
        }

        public void AddFile(string path)
        {
            if (tcc_add_file(_state, path) != 0)
            {
                throw new CompilationFailedException();
            }
        }

        public void OutputFile(string path)
        {
            if (tcc_output_file(_state, path) != 0)
            {
                throw new OutputFailedException();
            }
        }

        public void Dispose()
        {
            tcc_delete(_state);
            _state = IntPtr.Zero;
        }

        [DllImport("libtcc.dll")]
        private static extern IntPtr tcc_new();

        [DllImport("libtcc.dll")]
        private static extern void tcc_delete(IntPtr tcc_state);

        [DllImport("libtcc.dll")]
        private static extern int tcc_set_output_type(IntPtr tcc_state, int output_type);

        [DllImport("libtcc.dll")]
        private static extern void tcc_set_error_func(IntPtr tcc_state, IntPtr error_opaque, Action<IntPtr, string> error_func);

        [DllImport("libtcc.dll")]
        private static extern int tcc_add_sysinclude_path(IntPtr tcc_state, string pathname);

        [DllImport("libtcc.dll")]
        private static extern int tcc_add_include_path(IntPtr tcc_state, string pathname);

        [DllImport("libtcc.dll")]
        private static extern int tcc_add_file(IntPtr tcc_state, string filename);

        [DllImport("libtcc.dll")]
        private static extern int tcc_output_file(IntPtr tcc_state, string filename);
    }
}
