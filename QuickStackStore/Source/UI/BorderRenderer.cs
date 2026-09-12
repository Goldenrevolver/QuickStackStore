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

        public const string borderImageKey = "QuickStackStoreBorderImage";

        [HarmonyPatch(nameof(InventoryGrid.UpdateGui))]
        [HarmonyPostfix]
        internal static void UpdateGui(Player player, Inventory ___m_inventory, List<InventoryElement> ___m_elements)
        {
            if (player == null || player.m_inventory != ___m_inventory)
            {
                return;
            }

            int width = ___m_inventory.GetWidth();
            UserConfig playerConfig = UserConfig.GetPlayerConfig(player.GetPlayerID());

            for (int y = 0; y < ___m_inventory.GetHeight(); y++)
            {
                for (int x = 0; x < ___m_inventory.GetWidth(); x++)
                {
                    int index = y * width + x;

                    Image img = ___m_elements[index].m_queued.transform.Find(borderImageKey)?.GetComponent<Image>();

                    if (img == null)
                    {
                        img = CreateBorderImage(___m_elements[index].m_queued);
                    }

                    if (img != null)
                    {
                        img.color = FavoriteConfig.BorderColorFavoritedSlot.Value;
                        img.enabled = playerConfig.IsSlotFavorited(new Vector2i(x, y));
                    }
                }
            }

            foreach (ItemDrop.ItemData itemData in ___m_inventory.m_inventory)
            {
                int index = itemData.GridVectorToGridIndex(width);

                Image img = ___m_elements[index].m_queued.transform.Find(borderImageKey)?.GetComponent<Image>();

                if (img == null)
                {
                    img = CreateBorderImage(___m_elements[index].m_queued);
                }

                if (img == null)
                {
                    continue;
                }

                bool isItemFavorited = playerConfig.IsItemNameFavorited(itemData.m_shared);
                if (isItemFavorited)
                {
                    // enabled -> slot is favorited
                    if (img.enabled)
                    {
                        img.color = FavoriteConfig.BorderColorFavoritedItemOnFavoritedSlot.Value;
                    }
                    else
                    {
                        img.color = FavoriteConfig.BorderColorFavoritedItem.Value;
                    }

                    // do this at the end of the if statement, so we can use img.enabled to deduce the slot favoriting
                    img.enabled |= isItemFavorited;
                }
                else
                {
                    bool isItemTrashFlagged = playerConfig.IsItemNameConsideredTrashFlagged(itemData.m_shared);

                    if (isItemTrashFlagged)
                    {
                        // enabled -> slot is favorited
                        if (img.enabled)
                        {
                            img.color = FavoriteConfig.BorderColorTrashFlaggedItemOnFavoritedSlot.Value;
                        }
                        else
                        {
                            img.color = FavoriteConfig.BorderColorTrashFlaggedItem.Value;
                        }

                        // do this at the end of the if statement, so we can use img.enabled to deduce the slot favoriting
                        img.enabled |= isItemTrashFlagged;
                    }
                }
            }
        }

        private static Image CreateBorderImage(Image baseImg)
        {
            // set m_queued parent as parent first, so the position is correct
            var obj = Object.Instantiate(baseImg, baseImg.transform.parent);

            // destroy all children of the newly created clone of m_queued image (as other mods add children to it too)
            foreach (Transform child in obj.transform)
            {
                Object.Destroy(child.gameObject);
            }

            // set the name to something unique so we can find it later, and be compatible with other mods
            obj.name = BorderRenderer.borderImageKey;
            // change the parent to the m_queued image so we can access the new image without a loop
            obj.transform.SetParent(baseImg.transform);
            // set the new border image
            obj.sprite = border;

            return obj;
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