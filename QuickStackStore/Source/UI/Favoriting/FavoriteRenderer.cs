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
        internal static void PostFix(InventoryGrid __instance)
        {

            Player player = Player.m_localPlayer;
            if (player == null || player.m_inventory != __instance.GetInventory() || instance == null)
            {
                return;
            }

            instance.UpdateGui(player, __instance.GetInventory(), __instance.m_elements);

        }

        [HarmonyPatch(nameof(InventoryGrid.OnRightClick))]
        [HarmonyPrefix]
        internal static bool OnRightClick(InventoryGrid __instance, UIInputHandler element)
        {
            return HandleClick(__instance, element, false);
        }

        [HarmonyPatch(nameof(InventoryGrid.OnLeftClick))]
        [HarmonyPrefix]
        internal static bool OnLeftClick(InventoryGrid __instance, UIInputHandler clickHandler)
        {
            return HandleClick(__instance, clickHandler, true);
        }

        internal static bool HandleClick(InventoryGrid __instance, UIInputHandler clickHandler, bool isLeftClick)
        {
            if (InventoryGui.instance.m_playerGrid != __instance)
            {
                return true;
            }

            Player localPlayer = Player.m_localPlayer;

            if (localPlayer.IsTeleporting())
            {
                return true;
            }

            if (InventoryGui.instance.m_dragGo)
            {
                return true;
            }

            if (!FavoritingMode.IsInFavoritingMode())
            {
                return true;
            }

            GameObject gameObject = clickHandler.gameObject;
            Vector2i buttonPos = __instance.GetButtonPos(gameObject);

            if (buttonPos == new Vector2i(-1, -1))
            {
                return true;
            }

            if (!isLeftClick)
            {
                UserConfig.GetPlayerConfig(localPlayer.GetPlayerID()).ToggleSlotFavoriting(buttonPos);
            }
            else
            {
                ItemDrop.ItemData itemAt = __instance.m_inventory.GetItemAt(buttonPos.x, buttonPos.y);

                if (itemAt == null)
                {
                    return true;
                }

                bool wasToggleSuccessful = UserConfig.GetPlayerConfig(localPlayer.GetPlayerID()).ToggleItemNameFavoriting(itemAt.m_shared);

                if (!wasToggleSuccessful)
                {
                    localPlayer.Message(MessageHud.MessageType.Center, LocalizationConfig.GetRelevantTranslation(LocalizationConfig.CantFavoriteTrashFlaggedItemWarning, nameof(LocalizationConfig.CantFavoriteTrashFlaggedItemWarning)), 0, null);
                }
            }

            return false;
        }
    }
}