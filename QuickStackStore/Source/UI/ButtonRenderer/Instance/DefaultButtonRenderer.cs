using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static QuickStackStore.CompatibilitySupport;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore.UI.ButtonRenderer
{
    internal class DefaultButtonRenderer : ButtonRendererInstance
    {
        private float origButtonLength = -1;
        private Vector3 origButtonPosition;

        private const float shrinkFactor = 0.9f;
        private const int vPadding = 8;
        private const int hAlign = 1;

        protected override void CreateGuiInternal(InventoryGui __instance)
        {
            RectTransform takeAllButtonRect = null;
            if (__instance.m_takeAllButton != null)
                takeAllButtonRect = __instance.m_takeAllButton.GetComponent<RectTransform>();

            if (takeAllButtonRect is null)
                return;

            if (origButtonLength == -1)
            {
                origButtonLength = takeAllButtonRect.sizeDelta.x;
                origButtonPosition = takeAllButtonRect.localPosition;

                takeAllButtonRect.GetComponent<Button>().onClick.RemoveAllListeners();
                takeAllButtonRect.GetComponent<Button>().onClick.AddListener(new UnityAction(() => StoreTakeAllModule.ContextSensitiveTakeAll(__instance)));
            }

            // intentionally not checking "ShouldBlockChangesToTakeAllButton", because then everything would look stupid
            if (takeAllButtonRect.sizeDelta.x == origButtonLength)
            {
                takeAllButtonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, origButtonLength * shrinkFactor);
            }

            int extraContainerButtons = 0;

            if (GeneralConfig.OverrideButtonDisplay.Value != OverrideButtonDisplay.DisableAllNewButtons)
            {
                if (StoreTakeAllConfig.DisplayStoreAllButton.Value)
                {
                    extraContainerButtons++;
                }

                if (QuickStackConfig.DisplayQuickStackButtons.Value != ShowTwoButtons.OnlyInventoryButton)
                {
                    extraContainerButtons++;
                }

                if (RestockConfig.DisplayRestockButtons.Value != ShowTwoButtons.OnlyInventoryButton)
                {
                    extraContainerButtons++;
                }

                if (SortConfig.DisplaySortButtons.Value != ShowTwoButtons.OnlyInventoryButton)
                {
                    extraContainerButtons++;
                }
            }

            float vOffset = takeAllButtonRect.sizeDelta.y + vPadding;

            Vector3 startOffset = takeAllButtonRect.localPosition;

            if (takeAllButtonRect.localPosition == origButtonPosition)
            {
                if (extraContainerButtons <= 1)
                {
                    // move the button to the left by half of its removed length
                    startOffset -= new Vector3((origButtonLength / 2) * (1 - shrinkFactor), 0);
                }
                else
                {
                    startOffset = OppositePositionOfTakeAllButton();

                    bool goToTop = QuickStackConfig.DisplayQuickStackButtons.Value == ShowTwoButtons.OnlyInventoryButton;
                    startOffset += new Vector3(origButtonLength - hAlign, goToTop ? 0 : -vOffset);
                }

                if (!ShouldBlockChangesToTakeAllButton())
                {
                    takeAllButtonRect.localPosition = startOffset;
                }
            }

            if (GeneralConfig.OverrideButtonDisplay.Value == OverrideButtonDisplay.DisableAllNewButtons)
            {
                return;
            }

            int miniButtons = 0;

            var weight = __instance.m_player.transform.Find("Weight");

            var displaySortButtons = SortConfig.DisplaySortButtons.Value;

            var randyStatus = HasRandyPlugin();

            if (SortConfig.DisplaySortButtons.Value != ShowTwoButtons.OnlyContainerButton)
            {
                // this one is deliberately unaffected by the randy equipment slot compatibility
                bool shouldShow = __instance.m_currentContainer == null || displaySortButtons != ShowTwoButtons.BothButDependingOnContext;

                if (sortInventoryButton == null)
                {
                    sortInventoryButton = CreateMiniButton(__instance, nameof(sortInventoryButton));

                    sortInventoryButton.gameObject.SetActive(shouldShow);

                    if (shouldShow)
                    {
                        __instance.StartCoroutine(WaitAFrameToRepositionMiniButton(__instance, sortInventoryButton.transform, weight, ++miniButtons, randyStatus));
                    }

                    sortInventoryButton.onClick.RemoveAllListeners();
                    sortInventoryButton.onClick.AddListener(new UnityAction(() => SortModule.Sort(Player.m_localPlayer.m_inventory, UserConfig.GetPlayerConfig(Player.m_localPlayer.GetPlayerID()))));

                }
                else
                {
                    RepositionMiniButton(__instance, sortInventoryButton.transform, weight, ++miniButtons, randyStatus);
                }

            }

            var displayRestockButtons = RestockConfig.DisplayRestockButtons.Value;

            if (displayRestockButtons != ShowTwoButtons.OnlyContainerButton)
            {
                bool shouldntShow = __instance.m_currentContainer != null && (displayRestockButtons == ShowTwoButtons.BothButDependingOnContext || randyStatus == RandyStatus.EnabledWithQuickSlots);

                if (restockAreaButton == null)
                {
                    restockAreaButton = CreateMiniButton(__instance, nameof(restockAreaButton));
                    restockAreaButton.gameObject.SetActive(!shouldntShow);

                    if (!shouldntShow)
                    {
                        __instance.StartCoroutine(WaitAFrameToRepositionMiniButton(__instance, restockAreaButton.transform, weight, ++miniButtons, randyStatus));
                    }

                    restockAreaButton.onClick.RemoveAllListeners();
                    restockAreaButton.onClick.AddListener(new UnityAction(() => QuickStackRestockModule.DoRestock(Player.m_localPlayer)));
                }
                else
                {
                    restockAreaButton.gameObject.SetActive(!shouldntShow);

                    if (!shouldntShow)
                    {
                        RepositionMiniButton(__instance, restockAreaButton.transform, weight, ++miniButtons, randyStatus);
                    }
                }
            }

            var displayQuickStackButtons = QuickStackConfig.DisplayQuickStackButtons.Value;

            if (displayQuickStackButtons != ShowTwoButtons.OnlyContainerButton)
            {
                bool shouldntShow = __instance.m_currentContainer != null && (displayQuickStackButtons == ShowTwoButtons.BothButDependingOnContext || randyStatus == RandyStatus.EnabledWithQuickSlots);

                if (quickStackAreaButton == null)
                {
                    quickStackAreaButton = CreateMiniButton(__instance, nameof(quickStackAreaButton));
                    quickStackAreaButton.gameObject.SetActive(!shouldntShow);

                    if (!shouldntShow)
                    {
                        __instance.StartCoroutine(WaitAFrameToRepositionMiniButton(__instance, quickStackAreaButton.transform, weight, ++miniButtons, randyStatus));
                    }

                    quickStackAreaButton.onClick.RemoveAllListeners();
                    quickStackAreaButton.onClick.AddListener(new UnityAction(() => QuickStackRestockModule.DoQuickStack(Player.m_localPlayer)));
                }
                else
                {
                    quickStackAreaButton.gameObject.SetActive(!shouldntShow);

                    if (!shouldntShow)
                    {
                        RepositionMiniButton(__instance, quickStackAreaButton.transform, weight, ++miniButtons, randyStatus);
                    }
                }
            }

            var favConf = FavoriteConfig.DisplayFavoriteToggleButton.Value;

            if (favConf != FavoritingToggling.Disabled)
            {
                int index;
                Transform parent;

                if (favConf == FavoritingToggling.EnabledBottomButton)
                {
                    index = ++miniButtons;
                    parent = weight;
                }
                else
                {
                    index = -1;
                    parent = __instance.m_player.transform.Find("Armor");
                }

                if (favoritingTogglingButton == null)
                {
                    favoritingTogglingButton = CreateMiniButton(__instance, nameof(favoritingTogglingButton));
                    favoritingTogglingButton.gameObject.SetActive(true);

                    favoritingTogglingButtonText = favoritingTogglingButton.transform.Find("Text").GetComponent<Text>();

                    // trigger text reset without changing value
                    Helper.HasCurrentlyToggledFavoriting |= false;

                    __instance.StartCoroutine(WaitAFrameToRepositionMiniButton(__instance, favoritingTogglingButton.transform, parent, index, randyStatus));

                    favoritingTogglingButton.onClick.RemoveAllListeners();
                    favoritingTogglingButton.onClick.AddListener(new UnityAction(() => Helper.HasCurrentlyToggledFavoriting ^= true));
                }
                else
                {
                    RepositionMiniButton(__instance, favoritingTogglingButton.transform, parent, index, randyStatus);
                }
            }

            int buttonsBelowTakeAll = 0;

            if (QuickStackConfig.DisplayQuickStackButtons.Value != ShowTwoButtons.OnlyInventoryButton)
            {
                if (quickStackToContainerButton == null)
                {
                    quickStackToContainerButton = Object.Instantiate(__instance.m_takeAllButton, takeAllButtonRect.parent);

                    if (randyStatus == RandyStatus.EnabledWithQuickSlots)
                    {
                        // jump to the opposite side of the default 'take all' button position, because we are out of space due to randy's quickslots
                        MoveButtonToIndex(ref quickStackToContainerButton, startOffset, -vOffset, 1, 1);
                    }
                    else if (ShouldBlockChangesToTakeAllButton())
                    {
                        MoveButtonToIndex(ref quickStackToContainerButton, startOffset, 0, extraContainerButtons, 1);
                    }
                    else
                    {
                        // revert the vertical movement from the 'take all' button
                        MoveButtonToIndex(ref quickStackToContainerButton, startOffset, -vOffset, extraContainerButtons, 1);
                    }

                    quickStackToContainerButton.onClick.RemoveAllListeners();
                    quickStackToContainerButton.onClick.AddListener(new UnityAction(() => QuickStackRestockModule.DoQuickStack(Player.m_localPlayer, true)));
                }

                quickStackToContainerButton.gameObject.SetActive(__instance.m_currentContainer != null);
            }

            if (StoreTakeAllConfig.DisplayStoreAllButton.Value)
            {
                if (storeAllButton == null)
                {
                    storeAllButton = Object.Instantiate(__instance.m_takeAllButton, takeAllButtonRect.parent);
                    MoveButtonToIndex(ref storeAllButton, startOffset, vOffset, extraContainerButtons, ++buttonsBelowTakeAll);

                    storeAllButton.onClick.RemoveAllListeners();
                    storeAllButton.onClick.AddListener(new UnityAction(() => StoreTakeAllModule.StoreAllItemsInOrder(Player.m_localPlayer)));
                }

                storeAllButton.gameObject.SetActive(__instance.m_currentContainer != null);
            }

            if (RestockConfig.DisplayRestockButtons.Value != ShowTwoButtons.OnlyInventoryButton)
            {
                if (restockFromContainerButton == null)
                {
                    restockFromContainerButton = Object.Instantiate(__instance.m_takeAllButton, takeAllButtonRect.parent);
                    MoveButtonToIndex(ref restockFromContainerButton, startOffset, vOffset, extraContainerButtons, ++buttonsBelowTakeAll);

                    restockFromContainerButton.onClick.RemoveAllListeners();
                    restockFromContainerButton.onClick.AddListener(new UnityAction(() => QuickStackRestockModule.DoRestock(Player.m_localPlayer, true)));
                }

                restockFromContainerButton.gameObject.SetActive(__instance.m_currentContainer != null);
            }

            if (SortConfig.DisplaySortButtons.Value != ShowTwoButtons.OnlyInventoryButton)
            {
                if (sortContainerButton == null)
                {
                    sortContainerButton = Object.Instantiate(__instance.m_takeAllButton, takeAllButtonRect.parent);
                    MoveButtonToIndex(ref sortContainerButton, startOffset, vOffset, extraContainerButtons, ++buttonsBelowTakeAll);

                    sortContainerButton.onClick.RemoveAllListeners();
                    sortContainerButton.onClick.AddListener(new UnityAction(() => SortModule.Sort(__instance.m_currentContainer.m_inventory)));
                }

                sortContainerButton.gameObject.SetActive(__instance.m_currentContainer != null);
            }

            if (!ShouldBlockChangesToTakeAllButton())
            {
                takeAllButtonRect.gameObject.SetActive(__instance.m_currentContainer != null);
            }

            OnButtonTextTranslationSettingChanged(false);
        }

        internal override void OnButtonRelevantSettingChanged(QuickStackStorePlugin plugin, bool includeTrashButton = false)
        {
            if (InventoryGui.instance != null)
            {
                var takeAllButton = InventoryGui.instance.m_takeAllButton;

                if (takeAllButton != null)
                {
                    if (!ShouldBlockChangesToTakeAllButton())
                    {
                        takeAllButton.transform.localPosition = origButtonPosition;
                    }
                }
            }

            base.OnButtonRelevantSettingChanged(plugin, includeTrashButton);
        }

        private void MoveButtonToIndex(ref Button buttonToMove, Vector3 startVector, float vOffset, int visibleExtraButtons, int buttonsBelowTakeAll)
        {
            if (visibleExtraButtons == 1)
            {
                buttonToMove.transform.localPosition = OppositePositionOfTakeAllButton();
            }
            else
            {
                buttonToMove.transform.localPosition = startVector;
                buttonToMove.transform.localPosition -= new Vector3(0, buttonsBelowTakeAll * vOffset);
            }
        }

        private Vector3 OppositePositionOfTakeAllButton()
        {
            // move the button to the right by half of its removed length
            var scaleBased = (origButtonLength / 2) * (1 - shrinkFactor);
            return origButtonPosition + new Vector3(440f + scaleBased, 0f);
        }



        private const int miniButtonSize = 38;
        private const int miniButtonHPadding = 2;
        private const float normalMiniButtonVOffset = -56f;
        private const float lowerMiniButtonVOffset = -75f;

        private Button CreateMiniButton(InventoryGui instance, string name)
        {
            var playerInventory = instance.m_player.transform;

            Transform obj = Object.Instantiate(instance.m_takeAllButton.transform, playerInventory);
            obj.name = name;

            var rect = (RectTransform)obj.transform;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, miniButtonSize);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, miniButtonSize);

            Text text = rect.Find("Text").GetComponent<Text>();
            text.resizeTextForBestFit = true;

            return rect.GetComponent<Button>();
        }

        private void RepositionMiniButton(InventoryGui instance, Transform button, Transform weight, int existingMiniButtons, RandyStatus randyStatus)
        {
            if (existingMiniButtons == -1)
            {
                button.localPosition = weight.localPosition + new Vector3(hAlign, 70f);
                return;
            }

            float distanceToMove = (miniButtonSize + miniButtonHPadding) * (existingMiniButtons - 1);

            if (randyStatus == RandyStatus.EnabledWithQuickSlots)
            {
                button.localPosition = weight.localPosition + new Vector3(hAlign, -distanceToMove + normalMiniButtonVOffset);
            }
            else
            {
                var shouldMoveLower = randyStatus == RandyStatus.EnabledWithoutQuickSlots || (HasPluginThatRequiresMiniButtonVMove() && instance.m_player.Find("EquipmentBkg") != null);
                float vPos = shouldMoveLower ? lowerMiniButtonVOffset : normalMiniButtonVOffset;

                button.localPosition = weight.localPosition + new Vector3(hAlign + distanceToMove, vPos);
            }
        }

        /// <summary>
        /// Wait for one frame, so the two Odin equipment slot mods can finish spawning the 'EquipmentBkg' object
        /// </summary>
        internal IEnumerator WaitAFrameToRepositionMiniButton(InventoryGui instance, Transform button, Transform weight, int existingMiniButtons, RandyStatus randyStatus)
        {
            yield return null;
            RepositionMiniButton(instance, button, weight, existingMiniButtons, randyStatus);
        }
    }
}
