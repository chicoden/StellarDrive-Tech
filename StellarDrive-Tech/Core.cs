using UnityEngine;
using Wasmtime;
using MelonLoader;
using System.Diagnostics;

[assembly: MelonInfo(typeof(StellarDrive_Tech.Core), "StellarDrive Tech", "1.0.0", "oneshade", null)]
[assembly: MelonGame("CuriousOwlGames", "StellarDrive")]

namespace StellarDrive_Tech
{
    public class Core : MelonMod
    {
        private static string _compilerPath;
        private static string _linkerPath;

        public override void OnInitializeMelon()
        {
            string gameRoot = Directory.GetParent(Application.dataPath).FullName;
            _compilerPath = Path.Combine(gameRoot, "UserLibs/clang-23.1.4/x86_64-pc-windows-msvc/clang++.exe");
            _linkerPath = Path.Combine(gameRoot, "UserLibs/clang-23.1.4/x86_64-pc-windows-msvc/wasm-ld.exe");

            string projectRoot = Path.Combine(Application.persistentDataPath, "programs/hello");

            {
                ProcessStartInfo startInfo = new();
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = projectRoot;
                startInfo.FileName = _compilerPath;
                startInfo.Arguments = "--target=wasm32 -fno-exceptions -nostdlibinc -nostdinc -nostdlib -c hello.cpp -o hello.o";
                using (Process proc = Process.Start(startInfo))
                {
                    proc.WaitForExit();
                    LoggerInstance.Msg(proc.StandardError.ReadToEnd());
                    LoggerInstance.Msg("compiler exited with code " + proc.ExitCode.ToString());
                }
            }

            {
                ProcessStartInfo startInfo = new();
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = projectRoot;
                startInfo.FileName = _linkerPath;
                startInfo.Arguments = "hello.o --no-entry --export=start --export=loop --initial-memory=131072 --no-growable-memory -o hello.wasm";
                using (Process proc = Process.Start(startInfo))
                {
                    proc.WaitForExit();
                    LoggerInstance.Msg(proc.StandardError.ReadToEnd());
                    LoggerInstance.Msg("linker exited with code " + proc.ExitCode.ToString());
                }
            }

            Engine engine = new();
            Module module = Module.FromFile(engine, Path.Combine(projectRoot, "hello.wasm"));
            Memory memory = null;
            Store store = new(engine);
            Linker linker = new(engine);
            linker.DefineFunction("env", "print", (Int32 str) => LoggerInstance.Msg(memory.ReadNullTerminatedString(str)));
            Instance instance = linker.Instantiate(store, module);
            memory = instance.GetMemory("memory")!;
            Action startRoutine = instance.GetAction("start")!;
            Action loopRoutine = instance.GetAction("loop")!;
            startRoutine();

            LoggerInstance.Msg("Initialized.");
        }
    }
}
