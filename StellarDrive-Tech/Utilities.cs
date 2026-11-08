using MelonLoader;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Research.Interface.Values;
using Ships.Interface.Settings;
using UI.BuildMenu;

using BuildMenuRow = UI.BuildMenu.BuildMenuTabConfig.BuildMenuRow;

namespace StellarDrive_Tech;

internal static class Utilities
{
    public static void MigrateMaterials(GameObject gameObject)
    {
        Shader planetObjectShader = Shader.Find("Custom/PlanetObject");

        MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
        foreach (Material material in renderers.SelectMany(renderer => renderer.materials))
        {
            if (material.shader.name == planetObjectShader.name)
            {
                material.shader = planetObjectShader;
            }
        }
    }

    public static void AddPart(PartSettings part, uint tabId, uint? rowId, ushort researchId)
    {
        ResearchSettingsList researchSettings = Resources.FindObjectsOfTypeAll<ResearchSettingsList>().First();
        ResearchSettings research = researchSettings.GetResearchSettingById(researchId);

        BuildMenu buildMenu = Resources.FindObjectsOfTypeAll<BuildMenu>().First();
        FieldInfo tabsConfigField = buildMenu.GetType().GetField("tabsConfig", BindingFlags.NonPublic | BindingFlags.Instance);
        BuildMenuTabConfig[] tabConfigs = tabsConfigField.GetValue(buildMenu) as BuildMenuTabConfig[];
        BuildMenuTabConfig tabConfig = tabConfigs[tabId];

        BuildMenuRow buildMenuRow;
        if (rowId.HasValue)
        {
            buildMenuRow = tabConfig.rows[rowId.Value];
        }
        else
        {
            buildMenuRow = new();
            tabConfig.rows = tabConfig.rows.AddToArray(buildMenuRow);
        }

        research.unlockedParts = research.unlockedParts.AddToArray(part);
        Melon<Core>.Logger.Msg($"added part id {part.id} to {research.techName} research");

        buildMenuRow.parts = buildMenuRow.parts.AddToArray(part);
        Melon<Core>.Logger.Msg($"added part id {part.id} to {tabConfig.name} tab");
    }

    public static void RemovePart(uint partId)
    {
        ResearchSettingsList researchSettings = Resources.FindObjectsOfTypeAll<ResearchSettingsList>().First();

        BuildMenu buildMenu = Resources.FindObjectsOfTypeAll<BuildMenu>().First();
        FieldInfo tabsConfigField = buildMenu.GetType().GetField("tabsConfig", BindingFlags.NonPublic | BindingFlags.Instance);
        BuildMenuTabConfig[] tabConfigs = tabsConfigField.GetValue(buildMenu) as BuildMenuTabConfig[];

        foreach (ResearchSettings research in researchSettings.list)
        {
            List<PartSettings> unlockedParts = new();
            foreach (PartSettings part in research.unlockedParts)
            {
                if (part.id == partId)
                {
                    UnityEngine.Object.Destroy(part);
                    Melon<Core>.Logger.Msg($"removed part id {partId} from {research.techName} research");
                }
                else
                {
                    unlockedParts.Add(part);
                }
            }
            research.unlockedParts = unlockedParts.ToArray();
        }

        foreach (BuildMenuTabConfig tabConfig in tabConfigs)
        {
            List<BuildMenuRow> rows = new();
            foreach (BuildMenuRow row in tabConfig.rows)
            {
                List<PartSettings> parts = new();
                foreach (PartSettings part in row.parts)
                {
                    if (part.id == partId)
                    {
                        UnityEngine.Object.Destroy(part);
                        Melon<Core>.Logger.Msg($"removed part id {partId} from {tabConfig.name} tab");
                    }
                    else
                    {
                        parts.Add(part);
                    }
                }
                if (parts.Count > 0)
                {
                    row.parts = parts.ToArray();
                    rows.Add(row);
                }
            }
            tabConfig.rows = rows.ToArray();
        }
    }
}
