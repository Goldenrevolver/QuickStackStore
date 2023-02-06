using HarmonyLib;

namespace QuickStackStore.UI.Buttons
{
    internal class ButtonRenderer
    {
        internal static ButtonRendererInstance instance;

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

                instance.CreateGui(__instance);
            }
        }
    }
}