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
    public static void AddPart(PartSettings partSettings, uint tabId, uint? rowId, ushort researchId)
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

        research.unlockedParts = research.unlockedParts.AddToArray(partSettings);
        buildMenuRow.parts = buildMenuRow.parts.AddToArray(partSettings);
    }
}
