using HarmonyLib;
using UnityEngine;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore.UI
{
    [HarmonyPatch(typeof(Inventory))]
    internal static class PatchInventory
    {
        [HarmonyPatch(nameof(Inventory.TopFirst))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        public static bool TopFirstPatch(ref bool __result)
        {
            if (GeneralConfig.UseTopDownLogicForEverything.Value)
            {
                __result = true;
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
