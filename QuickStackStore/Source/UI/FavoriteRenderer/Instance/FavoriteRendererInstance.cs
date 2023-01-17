using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore.UI.FavoriteRenderer
{
    internal abstract class FavoriteRendererInstance
    {

        public Sprite indicator;

        public abstract Image GetOrApplyIndicatorImage(List<InventoryGrid.Element> elements, int index);

        protected FavoriteRendererInstance(Sprite indicator)
        {
            this.indicator = indicator;
        }

        internal void UpdateGui(Player player, Inventory inventory, List<InventoryGrid.Element> elements)
        {

            UserConfig playerConfig = UserConfig.GetPlayerConfig(player.GetPlayerID());

            UpdateInventorySlots(playerConfig, inventory, elements);
            UpdateItems(playerConfig, inventory, elements);

        }

        private void UpdateInventorySlots(UserConfig playerConfig, Inventory inventory, List<InventoryGrid.Element> elements)
        {
            int width = inventory.GetWidth();
            for (int y = 0; y < inventory.GetHeight(); y++)
            {
                for (int x = 0; x < inventory.GetWidth(); x++)
                {
                    int index = y * width + x;

                    // Ensure the indicator image is applied to the inventory grid slot.
                    Image img = GetOrApplyIndicatorImage(elements, index);

                    // Update color and enabled state based on favoriting.
                    img.color = FavoriteConfig.BorderColorFavoritedSlot.Value;
                    img.enabled = playerConfig.IsSlotFavorited(new Vector2i(x, y));
                }
            }
        }

        private void UpdateItems(UserConfig playerConfig, Inventory inventory, List<InventoryGrid.Element> elements)
        {
            foreach (ItemDrop.ItemData itemData in inventory.m_inventory)
            {
                int index = itemData.GridVectorToGridIndex(inventory.GetWidth());

                // Ensure the indicator image is applied to the slot the item is in.
                Image img = GetOrApplyIndicatorImage(elements, index);

                // Update favoriting icon color/state based on item's favoriting/trash status.
                var isItemFavorited = playerConfig.IsItemNameFavorited(itemData.m_shared);
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
                    var isItemTrashFlagged = playerConfig.IsItemNameConsideredTrashFlagged(itemData.m_shared);

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
    }
}