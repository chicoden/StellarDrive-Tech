using Items.Model;
using Items.Services;
using MelonLoader;
using Ships.Interface.Model;
using Ships.Interface.Settings;
using System.Reflection;
using UnityEngine;
using WasmEngine = Wasmtime.Engine;
using WasmEngineConfig = Wasmtime.Config;
using WasmModule = Wasmtime.Module;
using WasmMemory = Wasmtime.Memory;
using WasmLinker = Wasmtime.Linker;
using WasmStore = Wasmtime.Store;
using WasmInstance = Wasmtime.Instance;
using WasmException = Wasmtime.WasmtimeException;

[assembly: MelonInfo(typeof(StellarDrive_Tech.Core), "StellarDrive Tech", "1.0.0", "oneshade", null)]
[assembly: MelonGame("CuriousOwlGames", "StellarDrive")]

namespace StellarDrive_Tech;

public class Core : MelonMod
{
    private enum PartId : ushort
    {
        Microcontroller = 8192
    }

    private enum ItemId : uint
    {
        Copper = 112
    }

    public override void OnInitializeMelon()
    {
        string programsRoot = "UserData/SDTech/Programs";
        string projectName = "hello";
        string projectRoot = Path.Combine(programsRoot, projectName);
        string projectBinaryPath = Path.Combine(projectRoot, $"{projectName}.wasm");

        TinyCC cc = new();
        cc.SetErrorCallback(error => LoggerInstance.Msg(error));
        string sourceFilePath = Path.Combine(projectRoot, "main.c");
        LoggerInstance.Msg($"compiling source file {sourceFilePath}...");
        cc.AddFile(sourceFilePath);
        LoggerInstance.Msg($"writing executable {projectBinaryPath}...");
        cc.OutputFile(projectBinaryPath);
        LoggerInstance.Msg("build complete.");
        cc.Dispose();

        WasmEngine engine = new(
            new WasmEngineConfig()
                .WithEpochInterruption(true)
        );

        WasmModule module = WasmModule.FromFile(engine, projectBinaryPath);

        WasmMemory memory = null;
        WasmLinker linker = new(engine);
        linker.DefineFunction(
            "env",
            "memset",
            int (int addr, int value, int size) =>
            {
                int endAddr = addr + size;
                while (addr < endAddr)
                {
                    memory.WriteByte(addr, (byte)value);
                    addr++;
                }
                return 0;
            }
        );
        linker.DefineFunction(
            "env",
            "print",
            (int str) =>
            {
                LoggerInstance.Msg($"wasm: {memory.ReadNullTerminatedString(str)}");
            }
        );

        WasmStore store = new(engine);
        store.SetEpochDeadline(1);
        WasmInstance instance = linker.Instantiate(store, module);
        memory = instance.GetMemory("memory")!;
        Action setupRoutine = instance.GetAction("setup")!;
        Action loopRoutine = instance.GetAction("loop")!;

        store.SetEpochDeadline(1);
        new Timer(_ => engine.IncrementEpoch(), null, 2, Timeout.Infinite);
        try
        {
            setupRoutine();
        }
        catch (WasmException)
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
            catch (WasmException)
            {
                LoggerInstance.Msg("loop took too long");
            }
        }
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (buildIndex != 1) return;
        CreateMicrocontrollerPart();
    }

    public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
    {
        if (buildIndex != 1) return;
        foreach (PartSettings partSettings in UnityEngine.Object.FindObjectsByType<PartSettings>(FindObjectsSortMode.None))
        {
            if (partSettings.id == (ushort)PartId.Microcontroller)
            {
                UnityEngine.Object.Destroy(partSettings);
            }
        }
    }

    private void CreateMicrocontrollerPart()
    {
        GameObject part = new("Microcontroller Part");
        ///

        PartSettings partSettings = ScriptableObject.CreateInstance<PartSettings>();
        partSettings.id = (ushort)PartId.Microcontroller;
        partSettings.fullLabel = "Microcontroller";
        partSettings.internalStateClass = null;///
        partSettings.customCreationClass = null;
        partSettings.snappingStyle = SnappingStyle.PreciseOnAny;
        partSettings.description = "A programmable device that can be pretty much anything you want, provided you can be specific. Very specific.";
        partSettings.localizedDescription = null;
        partSettings.part = part;
        partSettings.mass = 0.1f;

        ItemSettingsProvider itemSettingsProvider = UnityEngine.Object.FindFirstObjectByType<ItemSettingsProvider>();
        FieldInfo buildingCostField = partSettings.GetType().GetField("buildingCost", BindingFlags.NonPublic | BindingFlags.Instance);
        buildingCostField.SetValue(partSettings, new List<ItemInstance>
        {
            new(itemSettingsProvider.GetItemSettingsById((uint)ItemId.Copper), 4)
        });

        Utilities.AddPart(partSettings, 3, 1);
    }
}
