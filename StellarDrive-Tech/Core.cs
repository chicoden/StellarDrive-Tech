using UnityEngine;
using Wasmtime;
using MelonLoader;

[assembly: MelonInfo(typeof(StellarDrive_Tech.Core), "StellarDrive Tech", "1.0.0", "oneshade", null)]
[assembly: MelonGame("CuriousOwlGames", "StellarDrive")]

namespace StellarDrive_Tech
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            string projectRoot = Path.Combine(Application.persistentDataPath, "programs/hello");
            TinyCC cc = new();
            cc.SetErrorCallback((string error) => LoggerInstance.Msg(error));
            cc.AddFile(Path.Combine(projectRoot, "hello.c"));
            cc.OutputFile(Path.Combine(projectRoot, "hello.wasm"));
            cc.Dispose();

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
            new Timer(_ => engine.IncrementEpoch(), null, 2, Timeout.Infinite);
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
                new Timer(_ => engine.IncrementEpoch(), null, 2, Timeout.Infinite);
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
