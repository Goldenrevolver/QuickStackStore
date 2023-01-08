using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using QuickStackStore.Source.UI;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static QuickStackStore.CompatibilitySupport;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    internal class ButtonRenderer
    {
        internal static ButtonRendererManagerBase manager;

        [HarmonyPatch(typeof(InventoryGui))]
        internal static class PatchInventoryGui
        {
            // slightly lower priority so we get rendered on top of equipment slot mods
            // (lower priority -> later rendering -> you get rendered on top)
            [HarmonyPriority(Priority.LowerThanNormal)]
            [HarmonyPatch(nameof(InventoryGui.Show))]
            [HarmonyPostfix]
            public static void Show_Postfix(InventoryGui __instance)
            {
                if (__instance != InventoryGui.instance)
                    return;

                manager.CreateGui(__instance);
            }
        }
    }
}