using HarmonyLib;
using Ships.Interface.Settings;
using System.Reflection;
using UI.BuildMenu;
using UnityEngine;

namespace StellarDrive_Tech;

internal static class Utilities
{
    public static void AddPart(PartSettings partSettings, uint tabId, uint? rowId)
    {
        BuildMenu buildMenu = Resources.FindObjectsOfTypeAll<BuildMenu>().First();
        FieldInfo tabsConfigField = buildMenu.GetType().GetField("tabsConfig", BindingFlags.NonPublic | BindingFlags.Instance);
        BuildMenuTabConfig[] tabsConfig = tabsConfigField.GetValue(buildMenu) as BuildMenuTabConfig[];
        if (rowId.HasValue)
        {
            tabsConfig[tabId].rows[rowId.Value].parts.AddItem(partSettings);
        }
        else
        {
            BuildMenuTabConfig.BuildMenuRow row = new();
            row.parts.AddItem(partSettings);
            tabsConfig[tabId].rows.AddItem(row);
        }
    }
}
