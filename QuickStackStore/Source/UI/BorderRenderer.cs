using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    [HarmonyPatch(typeof(InventoryGrid))]
    internal static class BorderRenderer
    {
        public static Sprite border;
        private static bool imagesCreated = false;

        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyPatch(nameof(InventoryGrid.UpdateGui))]
        [HarmonyPostfix]
        internal static void PostFix(InventoryGrid __instance)
        {

            Player player = Player.m_localPlayer;
            if (player == null || player.m_inventory != __instance.GetInventory())
                return;

            Inventory inventory = __instance.GetInventory();
            List<InventoryGrid.Element> elements = __instance.m_elements;
            UserConfig playerConfig = UserConfig.GetPlayerConfig(player.GetPlayerID());

            // Process Inventory Slots
            int width = inventory.GetWidth();
            for (int y = 0; y < inventory.GetHeight(); y++)
            {
                for (int x = 0; x < inventory.GetWidth(); x++)
                {
                    int index = y * width + x;

                    Image img = elements[index].m_go.transform.Find("favorite")?.GetComponent<Image>();

                    if (img is null)
                        img = CreateBorderImage(elements[index].m_go.transform, elements[index]);

                    img.color = FavoriteConfig.BorderColorFavoritedSlot.Value;
                    img.enabled = playerConfig.IsSlotFavorited(new Vector2i(x, y));
                }
            }

            // Process Inventory Items
            foreach (ItemDrop.ItemData itemData in inventory.m_inventory)
            {
                int index = itemData.GridVectorToGridIndex(width);

                Image img = elements[index].m_go.transform.Find("favorite").GetComponent<Image>();
                if (img is null)
                    img = CreateBorderImage(elements[index].m_go.transform, elements[index]);

                var isItemFavorited = playerConfig.IsItemNameFavorited(itemData.m_shared);
                if (isItemFavorited)
                {
                    // enabled -> slot is favorited
                    if (img.enabled)
                        img.color = FavoriteConfig.BorderColorFavoritedItemOnFavoritedSlot.Value;
                    else
                        img.color = FavoriteConfig.BorderColorFavoritedItem.Value;

                    // do this at the end of the if statement, so we can use img.enabled to deduce the slot favoriting
                    img.enabled |= isItemFavorited;
                }
                else
                {
                    var isItemTrashFlagged = playerConfig.IsItemNameConsideredTrashFlagged(itemData.m_shared);

                    if (isItemTrashFlagged)
                    {
                        // enabled -> slot is favorited
                        if (img.enabled)
                            img.color = FavoriteConfig.BorderColorTrashFlaggedItemOnFavoritedSlot.Value;
                        else
                            img.color = FavoriteConfig.BorderColorTrashFlaggedItem.Value;

                        // do this at the end of the if statement, so we can use img.enabled to deduce the slot favoriting
                        img.enabled |= isItemTrashFlagged;
                    }
                }
            }
        }

        private static Image CreateBorderImage(Transform parent, InventoryGrid.Element baseElement)
        {
            if (border is null)
            {
                border = baseElement.m_food.sprite;
            }

            Image obj = null;

            if (!CompatibilitySupport.HasAuga())
            {
                // set m_queued parent as parent first, so the position is correct
                obj = Object.Instantiate(baseElement.m_queued, parent);
                // set the new border image
                obj.sprite = border;
            }
            else
            {
                obj = Object.Instantiate(baseElement.m_icon, parent);
                obj.rectTransform.Rotate(Vector3.forward, 90f);
                obj.rectTransform.anchoredPosition = new Vector2(28, -28);
                obj.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 7);
                obj.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 7);
            }

            obj.name = "favorite";
            obj.sprite = border;

            return obj;
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

            if (!Helper.IsInFavoritingMode())
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

    [HarmonyPatch(typeof(Inventory))]
    internal class PatchInventory
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

    public static class ItemDataExtension
    {
        public static int GridVectorToGridIndex(this ItemDrop.ItemData item, int width)
        {
            return item.m_gridPos.y * width + item.m_gridPos.x;
        }
    }
}