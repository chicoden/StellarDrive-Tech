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

            using (
                Process proc = Process.Start(
                    new ProcessStartInfo()
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        WorkingDirectory = projectRoot,
                        FileName = _compilerPath,
                        Arguments = "--target=wasm32 -fno-exceptions -nostdlibinc -nostdinc -nostdlib -c hello.cpp -o hello.o"
                    }
                )
            )
            {
                proc.WaitForExit();
                LoggerInstance.Msg(proc.StandardError.ReadToEnd());
                LoggerInstance.Msg("compiler exited with code " + proc.ExitCode.ToString());
            }

            using (
                Process proc = Process.Start(
                    new ProcessStartInfo()
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        WorkingDirectory = projectRoot,
                        FileName = _linkerPath,
                        Arguments = "hello.o --no-entry --export=setup --export=loop --initial-memory=131072 --no-growable-memory -o hello.wasm"
                    }
                )
            )
            {
                proc.WaitForExit();
                LoggerInstance.Msg(proc.StandardError.ReadToEnd());
                LoggerInstance.Msg("linker exited with code " + proc.ExitCode.ToString());
            }

            Engine engine = new(
                new Config()
                    .WithEpochInterruption(true)
            );

            Module module = Module.FromFile(engine, Path.Combine(projectRoot, "hello.wasm"));

            Memory memory = null;
            Linker linker = new(engine);
            linker.DefineFunction("env", "print", (Int32 str) => LoggerInstance.Msg(memory.ReadNullTerminatedString(str)));

            Store store = new(engine);
            store.SetEpochDeadline(1);
            Instance instance = linker.Instantiate(store, module);
            memory = instance.GetMemory("memory")!;
            Action setupRoutine = instance.GetAction("setup")!;
            Action loopRoutine = instance.GetAction("loop")!;

            store.SetEpochDeadline(1);
            new Timer(_ => engine.IncrementEpoch(), null, 1, Timeout.Infinite);
            try
            {
                setupRoutine();
            }
            catch (WasmtimeException)
            {
                LoggerInstance.Msg("setup took too long");
            }
            for (int i = 0; i < 32; i++)
            {
                store.SetEpochDeadline(1);
                new Timer(_ => engine.IncrementEpoch(), null, 1, Timeout.Infinite);
                try
                {
                    loopRoutine();
                }
                catch (WasmtimeException)
                {
                    LoggerInstance.Msg("loop took too long");
                }
            }
        }
    }
}
