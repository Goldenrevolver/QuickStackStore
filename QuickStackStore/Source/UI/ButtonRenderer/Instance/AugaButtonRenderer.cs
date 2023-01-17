using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace QuickStackStore.UI.ButtonRenderer
{
    internal class AugaButtonRenderer : ButtonRendererInstance
    {
        private static readonly float _ButtonXIncrement = (_ButtonWidth + _ButtonSpacing) * -1;

        private const float _ButtonSpacing = 10;
        private const float _ButtonWidth = 110;
        private const float _StartingButtonX = -52;
        private const float _StartingButtonY = -22;

        private bool foundTakeAll = false;
        private float buttonX = 0;

        public AugaButtonRenderer()
        {
            Auga.API.IsLoaded();
        }

        protected override void CreateGuiInternal(InventoryGui __instance)
        {
            buttonX = _StartingButtonX;

            if (QSSConfig.CanDisplayInventorySortButton)
            {
                CreateOrUpdateButton(
                    sortInventoryButton
                    , nameof(sortInventoryButton)
                    , __instance.m_player
                    , __instance.m_currentContainer
                    , new UnityAction(() => SortModule.Sort(Player.m_localPlayer.m_inventory, UserConfig.GetPlayerConfig(Player.m_localPlayer.GetPlayerID())))
                    );
            }

            if (QSSConfig.CanDisplayInventoryRestockButton)
            {
                CreateOrUpdateButton(
                    restockAreaButton
                    , nameof(restockAreaButton)
                    , __instance.m_player
                    , __instance.m_currentContainer
                    , new UnityAction(() => QuickStackRestockModule.DoRestock(Player.m_localPlayer))
                    );
            }

            if (QSSConfig.CanDisplayInventoryQuickStackButton)
            {
                CreateOrUpdateButton(
                    quickStackAreaButton
                    , nameof(quickStackAreaButton)
                    , __instance.m_player
                    , __instance.m_currentContainer
                    , new UnityAction(() => QuickStackRestockModule.DoQuickStack(Player.m_localPlayer))
                    );
            }

            if (__instance.m_currentContainer != null)
            {
                buttonX = _StartingButtonX;

                if (QSSConfig.CanDisplayContainerSortButton)
                {
                    CreateOrUpdateButton(
                        sortContainerButton
                        , nameof(sortContainerButton)
                        , __instance.m_container
                        , __instance.m_currentContainer
                        , new UnityAction(() => SortModule.Sort(__instance.m_currentContainer.m_inventory))
                        );
                }

                if (QSSConfig.CanDisplayContainerRestockButton)
                {
                    CreateOrUpdateButton(
                        restockFromContainerButton
                        , nameof(restockFromContainerButton)
                        , __instance.m_container
                        , __instance.m_currentContainer
                        , new UnityAction(() => QuickStackRestockModule.DoRestock(Player.m_localPlayer, true))
                        );
                }

                if (QSSConfig.CanDisplayStoreAllButton)
                {
                    CreateOrUpdateButton(
                        storeAllButton
                        , nameof(storeAllButton)
                        , __instance.m_container
                        , __instance.m_currentContainer
                        , new UnityAction(() => StoreTakeAllModule.StoreAllItemsInOrder(Player.m_localPlayer))
                        );
                }

                if (QSSConfig.CanDisplayContainerQuickStackButton)
                {
                    CreateOrUpdateButton(
                        quickStackToContainerButton
                        , nameof(quickStackToContainerButton)
                        , __instance.m_container
                        , __instance.m_currentContainer
                        , new UnityAction(() => QuickStackRestockModule.DoQuickStack(Player.m_localPlayer, true))
                        );
                }

                if (QSSConfig.CanMoveTakeAllButton && !foundTakeAll)
                {
                    Transform takeAll = __instance.m_container.Find("TakeAll");
                    if (takeAll != null)
                    {
                        foundTakeAll = true;

                        // Move the button down with the rest
                        var rt = (RectTransform)takeAll;
                        rt.anchorMin = new Vector2(1, 0);
                        rt.anchorMax = new Vector2(1, 0);
                        rt.anchoredPosition = new Vector2(buttonX, _StartingButtonY);
                        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _ButtonWidth);

                        // Adjust the divider to fill the gap left behind by moving the button.
                        RectTransform rtBottomDivider = (RectTransform)__instance.m_container.Find("Divider Bottom");
                        rtBottomDivider.anchoredPosition = new Vector2(rtBottomDivider.anchoredPosition.y - 35, rtBottomDivider.anchoredPosition.y);
                        rtBottomDivider.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 509);
                    }
                }
            }
        }

        private void CreateOrUpdateButton(Button button, string buttonName, RectTransform buttonParent, Container currentContainer, UnityAction clickAction)
        {
            if (button is null)
            {
                button = Auga.API.SmallButton_Create(buttonParent, buttonName, LocalizationConfig.GetRelevantTranslation(buttonName));
                var rt = (RectTransform)button.transform;
                rt.anchorMin = new Vector2(1, 0);
                rt.anchorMax = new Vector2(1, 0);
                rt.anchoredPosition = new Vector2(buttonX, _StartingButtonY);
                buttonX += _ButtonXIncrement;
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _ButtonWidth);
            }
            else
            {
                button.gameObject.SetActive(currentContainer != null);
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(clickAction);
        }
    }
}
