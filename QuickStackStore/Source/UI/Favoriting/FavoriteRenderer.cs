using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace QuickStackStore.UI.Favoriting
{
    [HarmonyPatch(typeof(InventoryGrid))]
    internal static class FavoriteRenderer
    {

        public static FavoriteRendererInstance instance;

        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyPatch(nameof(InventoryGrid.UpdateGui))]
        [HarmonyPostfix]
        internal static void UpdateGui(Player player, Inventory ___m_inventory, List<InventoryGrid.Element> ___m_elements)
        { 
            if (player == null || player.m_inventory != ___m_inventory || instance == null)
            {
                return;
            }

            instance.UpdateGui(player, ___m_inventory, ___m_elements);

        }
    }
}