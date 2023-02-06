using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace QuickStackStore.UI.Buttons
{
    internal abstract class ButtonRendererInstance
    {
        internal bool hasOpenedInventoryOnce = false;
        internal float origButtonLength = -1;
        internal Vector3 origButtonPosition;


        internal Text favoritingTogglingButtonText;


        internal Button favoritingTogglingButton;
        internal Button quickStackAreaButton;
        internal Button sortInventoryButton;
        internal Button restockAreaButton;
        

        internal Button quickStackToContainerButton;
        internal Button storeAllButton;
        internal Button sortContainerButton;
        internal Button restockFromContainerButton;


        private const float shrinkFactor = 0.9f;
        private const int vPadding = 8;
        private const int hAlign = 1;

        protected abstract void CreateGuiInternal(InventoryGui __instance);

        public ButtonRendererInstance()
        {

        }

        public void CreateGui(InventoryGui __instance)
        {
            if (Player.m_localPlayer)
            {
                // reset in case player forgot to turn it off
                FavoritingMode.HasCurrentlyToggledFavoriting = false;

                if (QSSConfig.ShouldAutoSortInventory)
                    SortModule.DoSort(Player.m_localPlayer);
                
                if (QSSConfig.ShouldAutoSortContainer)
                    SortModule.DoSort(Player.m_localPlayer);
            }

            CreateGuiInternal(__instance);

        }

        internal virtual void OnButtonRelevantSettingChanged(BaseUnityPlugin plugin, bool includeTrashButton = false)
        {
            // reminder to never use ?. on monobehaviors
            var buttons = new Button[] { storeAllButton, quickStackToContainerButton, sortContainerButton, restockFromContainerButton, sortInventoryButton, quickStackAreaButton, restockAreaButton, favoritingTogglingButton };

            foreach (var button in buttons)
            {
                if (button != null)
                {
                    Object.Destroy(button.gameObject);
                }
            }

            favoritingTogglingButtonText = null;

            if (includeTrashButton)
            {
                if (TrashModule.trashRoot != null)
                {
                    Object.Destroy(TrashModule.trashRoot.gameObject);
                }
            }

            plugin.StartCoroutine(WaitAFrameToUpdateUIElements(InventoryGui.instance, includeTrashButton));
        }

        public void OnButtonTextTranslationSettingChanged(bool includeTrashButton = true)
        {
            // reminder to never use ?. on monobehaviors
            if (InventoryGui.instance != null)
            {
                var takeAllButton = InventoryGui.instance.m_takeAllButton;

                if (takeAllButton != null)
                {
                    var text = takeAllButton.GetComponentInChildren<Text>();

                    if (text != null)
                    {
                        text.text = !LocalizationConfig.TakeAllLabel.Value.IsNullOrWhiteSpace() ? LocalizationConfig.TakeAllLabel.Value : Localization.instance.Translate("inventory_takeall");
                    }
                }
            }

            UpdateButtonTextTranslation(storeAllButton, LocalizationConfig.StoreAllLabel, nameof(LocalizationConfig.StoreAllLabel));
            UpdateButtonTextTranslation(quickStackToContainerButton, LocalizationConfig.QuickStackLabel, nameof(LocalizationConfig.QuickStackLabel));
            UpdateButtonTextTranslation(restockFromContainerButton, LocalizationConfig.RestockLabel, nameof(LocalizationConfig.RestockLabel));
            UpdateButtonTextTranslation(sortInventoryButton, LocalizationConfig.SortLabelCharacter, nameof(LocalizationConfig.SortLabelCharacter));
            UpdateButtonTextTranslation(quickStackAreaButton, LocalizationConfig.QuickStackLabelCharacter, nameof(LocalizationConfig.QuickStackLabelCharacter));
            UpdateButtonTextTranslation(restockAreaButton, LocalizationConfig.RestockLabelCharacter, nameof(LocalizationConfig.RestockLabelCharacter));

            if (sortContainerButton != null)
            {
                var text = sortContainerButton.GetComponentInChildren<Text>();

                if (text != null)
                {
                    var label = LocalizationConfig.GetRelevantTranslation(LocalizationConfig.SortLabel, nameof(LocalizationConfig.SortLabel));

                    if (QSSConfig.CanDisplaySortCriteriaInLabel)
                    {
                        label += $" ({SortCriteriaToShortHumanReadableString(QSSConfig.SortConfig.SortCriteria.Value)})";
                    }

                    text.text = label;
                }
            }

            if (includeTrashButton && TrashModule.trashButton != null)
            {
                var text = TrashModule.trashButton.GetComponentInChildren<Text>();

                if (text != null)
                {
                    text.text = LocalizationConfig.GetRelevantTranslation(LocalizationConfig.TrashLabel, nameof(LocalizationConfig.TrashLabel));
                }
            }
        }

        public string SortCriteriaToShortHumanReadableString(QSSConfig.SortCriteriaEnum sortingCriteria)
        {
            switch (sortingCriteria)
            {
                case QSSConfig.SortCriteriaEnum.InternalName:
                    return LocalizationConfig.GetRelevantTranslation(LocalizationConfig.SortByInternalNameLabel, nameof(LocalizationConfig.SortByInternalNameLabel));

                case QSSConfig.SortCriteriaEnum.TranslatedName:
                    return LocalizationConfig.GetRelevantTranslation(LocalizationConfig.SortByTranslatedNameLabel, nameof(LocalizationConfig.SortByTranslatedNameLabel));

                case QSSConfig.SortCriteriaEnum.Value:
                    return LocalizationConfig.GetRelevantTranslation(LocalizationConfig.SortByValueLabel, nameof(LocalizationConfig.SortByValueLabel));

                case QSSConfig.SortCriteriaEnum.Weight:
                    return LocalizationConfig.GetRelevantTranslation(LocalizationConfig.SortByWeightLabel, nameof(LocalizationConfig.SortByWeightLabel));

                case QSSConfig.SortCriteriaEnum.Type:
                    return LocalizationConfig.GetRelevantTranslation(LocalizationConfig.SortByTypeLabel, nameof(LocalizationConfig.SortByTypeLabel));

                default:
                    return "invalid";
            }
        }

        private void UpdateButtonTextTranslation(Button button, ConfigEntry<string> overrideConfig, string configName)
        {
            if (button != null)
            {
                var text = button.GetComponentInChildren<Text>();

                if (text != null)
                {
                    text.text = LocalizationConfig.GetRelevantTranslation(overrideConfig, configName);
                }
            }
        }

        internal IEnumerator WaitAFrameToUpdateUIElements(InventoryGui instance, bool includeTrashButton)
        {
            yield return null;

            CreateGui(instance);

            if (includeTrashButton)
            {
                TrashModule.TrashItemsPatches.Show_Postfix(instance);
            }
        }
    }
}
