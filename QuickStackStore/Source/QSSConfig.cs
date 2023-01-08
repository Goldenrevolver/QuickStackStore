using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static QuickStackStore.LocalizationConfig;
using static QuickStackStore.QSSConfig.FavoriteConfig;
using static QuickStackStore.QSSConfig.GeneralConfig;
using static QuickStackStore.QSSConfig.QuickStackConfig;
using static QuickStackStore.QSSConfig.RestockConfig;
using static QuickStackStore.QSSConfig.SortConfig;
using static QuickStackStore.QSSConfig.StoreTakeAllConfig;
using static QuickStackStore.QSSConfig.TrashConfig;

namespace QuickStackStore
{
    [HarmonyPatch(typeof(Player))]
    internal static class PlayerPatch
    {
        [HarmonyPatch(nameof(Player.Start))]
        [HarmonyPostfix]
        internal static void StartPatch(Player __instance)
        {
            if (__instance == Player.m_localPlayer)
            {
                QSSConfig.ResetAllFavoritingData_SettingChanged(null, null);
            }
        }
    }

    internal class QSSConfig
    {
        public static ConfigFile Config;

        internal class GeneralConfig
        {
            internal static ConfigEntry<OverrideButtonDisplay> OverrideButtonDisplay;
            internal static ConfigEntry<OverrideKeybindBehavior> OverrideKeybindBehavior;
            internal static ConfigEntry<OverrideHotkeyBarBehavior> OverrideHotkeyBarBehavior;
            internal static ConfigEntry<bool> SuppressContainerSoundAndVisuals;
            internal static ConfigEntry<bool> UseTopDownLogicForEverything;
        }

        internal class FavoriteConfig
        {
            public static ConfigEntry<Color> BorderColorFavoritedItem;
            public static ConfigEntry<Color> BorderColorFavoritedItemOnFavoritedSlot;
            public static ConfigEntry<Color> BorderColorFavoritedSlot;
            public static ConfigEntry<Color> BorderColorTrashFlaggedItem;
            public static ConfigEntry<Color> BorderColorTrashFlaggedItemOnFavoritedSlot;
            public static ConfigEntry<bool> DisplayTooltipHint;
            public static ConfigEntry<KeyCode> FavoritingModifierKey1;
            public static ConfigEntry<KeyCode> FavoritingModifierKey2;
            public static ConfigEntry<FavoritingToggling> DisplayFavoriteToggleButton;
        }

        internal class QuickStackConfig
        {
            public static ConfigEntry<ShowTwoButtons> DisplayQuickStackButtons;
            public static ConfigEntry<QuickStackBehavior> QuickStackHotkeyBehaviorWhenContainerOpen;
            public static ConfigEntry<bool> QuickStackIncludesHotkeyBar;
            public static ConfigEntry<KeyCode> QuickStackKey;
            public static ConfigEntry<float> QuickStackToNearbyRange;
            public static ConfigEntry<bool> QuickStackTrophiesIntoSameContainer;
            public static ConfigEntry<bool> ShowQuickStackResultMessage;
        }

        internal class RestockConfig
        {
            public static ConfigEntry<ShowTwoButtons> DisplayRestockButtons;
            public static ConfigEntry<float> RestockFromNearbyRange;
            public static ConfigEntry<RestockBehavior> RestockHotkeyBehaviorWhenContainerOpen;
            public static ConfigEntry<bool> RestockIncludesHotkeyBar;
            public static ConfigEntry<KeyCode> RestockKey;
            public static ConfigEntry<bool> RestockOnlyAmmoAndConsumables;
            public static ConfigEntry<bool> RestockOnlyFavoritedItems;
            public static ConfigEntry<bool> ShowRestockResultMessage;
        }

        internal class StoreTakeAllConfig
        {
            public static ConfigEntry<bool> ChestsUseImprovedTakeAllLogic;
            public static ConfigEntry<bool> DisplayStoreAllButton;

            public static ConfigEntry<bool> NeverMoveTakeAllButton;

            public static ConfigEntry<bool> StoreAllIncludesEquippedItems;
            public static ConfigEntry<bool> StoreAllIncludesHotkeyBar;
        }

        internal class SortConfig
        {
            public static ConfigEntry<AutoSortBehavior> AutoSort;
            public static ConfigEntry<ShowTwoButtons> DisplaySortButtons;
            public static ConfigEntry<bool> DisplaySortCriteriaInLabel;
            public static ConfigEntry<SortCriteriaEnum> SortCriteria;
            public static ConfigEntry<SortBehavior> SortHotkeyBehaviorWhenContainerOpen;
            public static ConfigEntry<bool> SortInAscendingOrder;
            public static ConfigEntry<bool> SortIncludesHotkeyBar;
            public static ConfigEntry<KeyCode> SortKey;
            public static ConfigEntry<bool> SortLeavesEmptyFavoritedSlotsEmpty;
            public static ConfigEntry<bool> SortMergesStacks;
        }

        internal class TrashConfig
        {
            public static ConfigEntry<bool> AlwaysConsiderTrophiesTrashFlagged;
            public static ConfigEntry<bool> DisplayTrashCanUI;
            public static ConfigEntry<bool> EnableQuickTrash;
            public static ConfigEntry<KeyCode> QuickTrashHotkey;
            public static ConfigEntry<ShowConfirmDialogOption> ShowConfirmDialogForNormalItem;
            public static ConfigEntry<bool> ShowConfirmDialogForQuickTrash;
            public static ConfigEntry<KeyCode> TrashHotkey;
            public static ConfigEntry<bool> TrashingCanAffectHotkeyBar;
            public static ConfigEntry<Color> TrashLabelColor;
        }

        internal class DebugConfig
        {
            public static ConfigEntry<DebugLevel> ShowDebugLogs;
            public static ConfigEntry<DebugSeverity> DebugSeverity;
            public static ConfigEntry<ResetFavoritingData> ResetAllFavoritingData;
        }

        internal static void LoadConfig(QuickStackStorePlugin plugin)
        {
            Config = plugin.Config;

            SetupTranslations();

            string sectionName;

            // keep the entries within a section in alphabetical order for the r2modman config manager

            string overrideButton = $"overridden by {nameof(GeneralConfig.OverrideButtonDisplay)}";
            string overrideHotkey = $"overridden by {nameof(GeneralConfig.OverrideKeybindBehavior)}";
            string overrideHotkeyBar = $"overridden by {nameof(GeneralConfig.OverrideHotkeyBarBehavior)}";
            string hotkey = "What to do when the hotkey is pressed while you have a container open.";
            string twoButtons = $"Which of the two buttons to display ({overrideButton}). Selecting {nameof(ShowTwoButtons.BothButDependingOnContext)} will hide the mini button while a container is open. The hotkey works independently.";
            string range = "How close the searched through containers have to be.";
            string favoriteFunction = "disallowing quick stacking, storing, sorting and trashing";

            sectionName = "0 - General";

            GeneralConfig.OverrideButtonDisplay = Config.Bind(sectionName, nameof(GeneralConfig.OverrideButtonDisplay), OverrideButtonDisplay.UseIndividualConfigOptions, "Override to disable all new UI elements no matter the current individual setting of each of them.");
            GeneralConfig.OverrideButtonDisplay.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonRelevantSettingChanged(plugin, true);

            GeneralConfig.OverrideHotkeyBarBehavior = Config.Bind(sectionName, nameof(GeneralConfig.OverrideHotkeyBarBehavior), OverrideHotkeyBarBehavior.NeverAffectHotkeyBar, "Override to never affect the hotkey bar with any feature no matter the individual setting of each of them. Recommended to turn off if you are actually using favoriting.");
            GeneralConfig.OverrideKeybindBehavior = Config.Bind(sectionName, nameof(GeneralConfig.OverrideKeybindBehavior), OverrideKeybindBehavior.UseIndividualConfigOptions, "Override to disable all new keybinds no matter the current individual setting of each of them.");

            bool oldValue = false;

            if (TryGetOldConfigValue(new ConfigDefinition(sectionName, "DisableAllNewButtons"), ref oldValue))
            {
                GeneralConfig.OverrideButtonDisplay.Value = oldValue ? OverrideButtonDisplay.DisableAllNewButtons : OverrideButtonDisplay.UseIndividualConfigOptions;
            }

            if (TryGetOldConfigValue(new ConfigDefinition(sectionName, "DisableAllNewKeybinds"), ref oldValue))
            {
                GeneralConfig.OverrideKeybindBehavior.Value = oldValue ? OverrideKeybindBehavior.DisableAllNewHotkeys : OverrideKeybindBehavior.UseIndividualConfigOptions;
            }

            if (TryGetOldConfigValue(new ConfigDefinition(sectionName, "NeverAffectHotkeyBar"), ref oldValue))
            {
                GeneralConfig.OverrideHotkeyBarBehavior.Value = oldValue ? OverrideHotkeyBarBehavior.NeverAffectHotkeyBar : OverrideHotkeyBarBehavior.UseIndividualConfigOptions;
            }

            UseTopDownLogicForEverything = Config.Bind(sectionName, nameof(UseTopDownLogicForEverything), false, "Whether to always put items into the top first row (affects the entire game) rather than top or bottom first depending on the item type (base game uses top first only for weapons and tools, bottom first for the rest). Recommended to keep off.");

            sectionName = "1 - Favoriting";

            // valheim yellow/ orange-ish
            BorderColorFavoritedItem = Config.Bind(sectionName, nameof(BorderColorFavoritedItem), new Color(1f, 0.8482759f, 0f), "Color of the border for slots containing favorited items.");
            // dark-ish green
            BorderColorFavoritedItemOnFavoritedSlot = Config.Bind(sectionName, nameof(BorderColorFavoritedItemOnFavoritedSlot), new Color(0.5f, 0.67413795f, 0.5f), "Color of the border of a favorited slot that also contains a favorited item.");

            // light-ish blue
            BorderColorFavoritedSlot = Config.Bind(sectionName, nameof(BorderColorFavoritedSlot), new Color(0f, 0.5f, 1f), "Color of the border for favorited slots.");
            // dark-ish red
            BorderColorTrashFlaggedItem = Config.Bind(sectionName, nameof(BorderColorTrashFlaggedItem), new Color(0.5f, 0f, 0), "Color of the border for slots containing trash flagged items.");
            // black
            BorderColorTrashFlaggedItemOnFavoritedSlot = Config.Bind(sectionName, nameof(BorderColorTrashFlaggedItemOnFavoritedSlot), Color.black, "Color of the border of a favorited slot that also contains a trash flagged item.");

            DisplayFavoriteToggleButton = Config.Bind(sectionName, nameof(DisplayFavoriteToggleButton), FavoritingToggling.Disabled, $"Whether to display a button to toggle favoriting mode on or off, allowing to favorite without holding any hotkey ({overrideButton}). This can also be used to trash flag. The hotkeys work independently.");
            DisplayFavoriteToggleButton.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonRelevantSettingChanged(plugin);

            if (TryGetOldConfigValue(new ConfigDefinition(sectionName, "FavoritingModifierToggles"), ref oldValue))
            {
                DisplayFavoriteToggleButton.Value = oldValue ? FavoritingToggling.EnabledTopButton : FavoritingToggling.Disabled;
            }

            DisplayTooltipHint = Config.Bind(sectionName, nameof(DisplayTooltipHint), true, "Whether to add additional info the item tooltip of a favorited or trash flagged item.");

            string favoritingKey = $"While holding this, left clicking on items or right clicking on slots favorites them, {favoriteFunction}, or trash flags them if you are hovering an item on the trash can.";
            FavoritingModifierKey1 = Config.Bind(sectionName, nameof(FavoritingModifierKey1), KeyCode.LeftAlt, $"{favoritingKey} Identical to {nameof(FavoritingModifierKey2)}.");
            FavoritingModifierKey2 = Config.Bind(sectionName, nameof(FavoritingModifierKey2), KeyCode.RightAlt, $"{favoritingKey} Identical to {nameof(FavoritingModifierKey1)}.");

            sectionName = "2 - Quick Stacking and Restocking";

            SuppressContainerSoundAndVisuals = Config.Bind(sectionName, nameof(SuppressContainerSoundAndVisuals), true, "Whether when a feature checks multiple containers in an area, they actually play opening sounds and visuals. Disable if the suppression causes incompatibilities.");

            sectionName = "2.1 - Quick Stacking";

            DisplayQuickStackButtons = Config.Bind(sectionName, nameof(DisplayQuickStackButtons), ShowTwoButtons.BothButDependingOnContext, twoButtons);
            DisplayQuickStackButtons.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonRelevantSettingChanged(plugin);

            QuickStackHotkeyBehaviorWhenContainerOpen = Config.Bind(sectionName, nameof(QuickStackHotkeyBehaviorWhenContainerOpen), QuickStackBehavior.QuickStackOnlyToCurrentContainer, hotkey);
            QuickStackIncludesHotkeyBar = Config.Bind(sectionName, nameof(QuickStackIncludesHotkeyBar), true, $"Whether to also quick stack items from the hotkey bar ({overrideHotkeyBar}).");
            QuickStackKey = Config.Bind(sectionName, nameof(QuickStackKey), KeyCode.P, $"The hotkey to start quick stacking to the current or nearby containers (depending on {nameof(QuickStackHotkeyBehaviorWhenContainerOpen)}, {overrideHotkey}).");

            QuickStackToNearbyRange = Config.Bind(sectionName, nameof(QuickStackToNearbyRange), 10f, range);
            QuickStackTrophiesIntoSameContainer = Config.Bind(sectionName, nameof(QuickStackTrophiesIntoSameContainer), false, "Whether to put all types of trophies in the container if any trophy is found in that container.");

            ShowQuickStackResultMessage = Config.Bind(sectionName, nameof(ShowQuickStackResultMessage), true, "Whether to show the central screen report message after quick stacking.");

            sectionName = "2.2 - Quick Restocking";

            DisplayRestockButtons = Config.Bind(sectionName, nameof(DisplayRestockButtons), ShowTwoButtons.BothButDependingOnContext, twoButtons);
            DisplayRestockButtons.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonRelevantSettingChanged(plugin);

            RestockFromNearbyRange = Config.Bind(sectionName, nameof(RestockFromNearbyRange), 10f, range);
            RestockHotkeyBehaviorWhenContainerOpen = Config.Bind(sectionName, nameof(RestockHotkeyBehaviorWhenContainerOpen), RestockBehavior.RestockOnlyFromCurrentContainer, hotkey);
            RestockIncludesHotkeyBar = Config.Bind(sectionName, nameof(RestockIncludesHotkeyBar), true, $"Whether to also try to restock items currently in the hotkey bar ({overrideHotkeyBar}).");
            RestockKey = Config.Bind(sectionName, nameof(RestockKey), KeyCode.R, $"The hotkey to start restocking from the current or nearby containers (depending on {nameof(RestockHotkeyBehaviorWhenContainerOpen)}, {overrideHotkey}).");
            RestockOnlyAmmoAndConsumables = Config.Bind(sectionName, nameof(RestockOnlyAmmoAndConsumables), true, $"Whether restocking should only restock ammo and consumable or every stackable item (like materials). Also affected by {nameof(RestockOnlyFavoritedItems)}.");
            RestockOnlyFavoritedItems = Config.Bind(sectionName, nameof(RestockOnlyFavoritedItems), false, $"Whether restocking should only restock favorited items or items on favorited slots or every stackable item. Also affected by {nameof(RestockOnlyAmmoAndConsumables)}.");
            ShowRestockResultMessage = Config.Bind(sectionName, nameof(ShowRestockResultMessage), true, "Whether to show the central screen report message after restocking.");

            sectionName = "3 - Store and Take All";

            ChestsUseImprovedTakeAllLogic = Config.Bind(sectionName, nameof(ChestsUseImprovedTakeAllLogic), true, "Whether to use the improved logic for 'Take All' for non tomb stones. Disable if needed for compatibility.");

            DisplayStoreAllButton = Config.Bind(sectionName, nameof(DisplayStoreAllButton), true, $"Whether to display the 'Store All' button in containers ({overrideButton}).");
            DisplayStoreAllButton.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonRelevantSettingChanged(plugin);

            NeverMoveTakeAllButton = Config.Bind(sectionName, nameof(NeverMoveTakeAllButton), false, "Disallows my mod from moving the 'Take All' button. Enable for compatibility with other mods. If it was already moved, then you need to log out and back in (since I don't even allow to reset the position, since I don't know if that position is valid with your installed mods).");

            StoreAllIncludesEquippedItems = Config.Bind(sectionName, nameof(StoreAllIncludesEquippedItems), false, "Whether to also unequip and store non favorited equipped items or exclude them.");
            StoreAllIncludesHotkeyBar = Config.Bind(sectionName, nameof(StoreAllIncludesHotkeyBar), true, $"Whether to also store all non favorited items from the hotkey bar ({overrideHotkeyBar})");

            sectionName = "4 - Sorting";

            AutoSort = Config.Bind(sectionName, nameof(AutoSort), AutoSortBehavior.Never, "Automatically let the mod sort the player inventory every time you open it, as well as every container you open. This respects your other sorting config options.");

            DisplaySortButtons = Config.Bind(sectionName, nameof(DisplaySortButtons), ShowTwoButtons.Both, twoButtons);
            DisplaySortButtons.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonRelevantSettingChanged(plugin);

            DisplaySortCriteriaInLabel = Config.Bind(sectionName, nameof(DisplaySortCriteriaInLabel), false, "Whether to display the current sort criteria in the inventory sort button as a reminder. The author thinks the button is a bit too small for it to look good.");
            DisplaySortCriteriaInLabel.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonRelevantSettingChanged(plugin);

            SortCriteria = Config.Bind(sectionName, nameof(SortCriteria), SortCriteriaEnum.Type, "The sort criteria the sort button uses. Ties are broken by internal name, quality and stack size.");
            SortHotkeyBehaviorWhenContainerOpen = Config.Bind(sectionName, nameof(SortHotkeyBehaviorWhenContainerOpen), SortBehavior.OnlySortContainer, hotkey);
            SortInAscendingOrder = Config.Bind(sectionName, nameof(SortInAscendingOrder), true, "Whether the current first sort criteria should be used in ascending or descending order.");
            SortIncludesHotkeyBar = Config.Bind(sectionName, nameof(SortIncludesHotkeyBar), true, $"Whether to also sort non favorited items from the hotkey bar ({overrideHotkeyBar}).");
            SortKey = Config.Bind(sectionName, nameof(SortKey), KeyCode.O, $"The hotkey to sort the inventory or the current container or both (depending on {nameof(SortHotkeyBehaviorWhenContainerOpen)}, {overrideHotkey}).");
            SortLeavesEmptyFavoritedSlotsEmpty = Config.Bind(sectionName, nameof(SortLeavesEmptyFavoritedSlotsEmpty), true, "Whether sort treats empty favorited slots as occupied and leaves them empty, so you don't accidentally put items on them.");
            SortMergesStacks = Config.Bind(sectionName, nameof(SortMergesStacks), true, "Whether to merge stacks after sorting or keep them as separate non full stacks.");

            sectionName = "5 - Trashing";

            AlwaysConsiderTrophiesTrashFlagged = Config.Bind(sectionName, nameof(AlwaysConsiderTrophiesTrashFlagged), false, "Whether to always consider trophies as trash flagged, allowing for immediate trashing or to be affected by quick trashing.");

            DisplayTrashCanUI = Config.Bind(sectionName, nameof(DisplayTrashCanUI), true, $"Whether to display the trash can UI element ({overrideButton}). Hotkeys work independently.");
            DisplayTrashCanUI.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonRelevantSettingChanged(plugin, true);

            EnableQuickTrash = Config.Bind(sectionName, nameof(EnableQuickTrash), true, "Whether quick trashing can be called with the hotkey or be clicking on the trash can while not holding anything.");
            QuickTrashHotkey = Config.Bind(sectionName, nameof(QuickTrashHotkey), KeyCode.None, $"The hotkey to perform a quick trash on the player inventory, deleting all trash flagged items ({overrideHotkey}).");
            ShowConfirmDialogForNormalItem = Config.Bind(sectionName, nameof(ShowConfirmDialogForNormalItem), ShowConfirmDialogOption.WhenNotTrashFlagged, "When to show a confirmation dialog while doing a non quick trash.");
            ShowConfirmDialogForQuickTrash = Config.Bind(sectionName, nameof(ShowConfirmDialogForQuickTrash), true, "Whether to show a confirmation dialog while doing a quick trash.");
            TrashHotkey = Config.Bind(sectionName, nameof(TrashHotkey), KeyCode.Delete, $"The hotkey to trash the currently held item ({overrideHotkey}).");
            TrashingCanAffectHotkeyBar = Config.Bind(sectionName, nameof(TrashingCanAffectHotkeyBar), true, $"Whether trashing and quick trashing can trash items that are currently in the hotkey bar ({overrideHotkeyBar}).");
            TrashLabelColor = Config.Bind(sectionName, nameof(TrashLabelColor), new Color(1f, 0.8482759f, 0), "The color of the text below the trash can in the player inventory.");

            sectionName = "8 - Debugging";

            DebugConfig.ShowDebugLogs = Config.Bind(sectionName, nameof(DebugConfig.ShowDebugLogs), DebugLevel.Disabled, "Enable debug logs into the console. Optionally set it to print as warnings, so the yellow color is easier to spot. Some important prints ignore this setting.");
            DebugConfig.ResetAllFavoritingData = Config.Bind(sectionName, nameof(DebugConfig.ResetAllFavoritingData), ResetFavoritingData.No, "This deletes all the favoriting of your items and slots, as well as trash flagging, the next time the mod checks for it (either on loading a character or on config change while ingame), and then resets this config back to 'No'.");
            DebugConfig.ResetAllFavoritingData.SettingChanged -= ResetAllFavoritingData_SettingChanged;
            DebugConfig.ResetAllFavoritingData.SettingChanged += ResetAllFavoritingData_SettingChanged;
            DebugConfig.DebugSeverity = Config.Bind(sectionName, nameof(DebugConfig.DebugSeverity), DebugSeverity.Normal, $"Filters which kind of debug messages are shown when {DebugConfig.ShowDebugLogs} is not disabled. Only use {DebugSeverity.Everything} for testing.");

            sectionName = "9 - Localization";

            TrashLabel = Config.Bind(sectionName, nameof(TrashLabel), string.Empty, string.Empty);
            TrashLabel.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonTextTranslationSettingChanged();

            QuickStackLabel = Config.Bind(sectionName, nameof(QuickStackLabel), string.Empty, string.Empty);
            QuickStackLabel.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonTextTranslationSettingChanged();

            StoreAllLabel = Config.Bind(sectionName, nameof(StoreAllLabel), string.Empty, string.Empty);
            StoreAllLabel.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonTextTranslationSettingChanged();

            TakeAllLabel = Config.Bind(sectionName, nameof(TakeAllLabel), string.Empty, string.Empty);
            TakeAllLabel.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonTextTranslationSettingChanged();

            RestockLabel = Config.Bind(sectionName, nameof(RestockLabel), string.Empty, string.Empty);
            RestockLabel.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonTextTranslationSettingChanged();

            SortLabel = Config.Bind(sectionName, nameof(SortLabel), string.Empty, string.Empty);
            SortLabel.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonTextTranslationSettingChanged();

            QuickStackLabelCharacter = Config.Bind(sectionName, nameof(QuickStackLabelCharacter), string.Empty, string.Empty);
            QuickStackLabelCharacter.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonTextTranslationSettingChanged();

            SortLabelCharacter = Config.Bind(sectionName, nameof(SortLabelCharacter), string.Empty, string.Empty);
            SortLabelCharacter.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonTextTranslationSettingChanged();

            RestockLabelCharacter = Config.Bind(sectionName, nameof(RestockLabelCharacter), string.Empty, string.Empty);
            RestockLabelCharacter.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonTextTranslationSettingChanged();

            SortByInternalNameLabel = Config.Bind(sectionName, nameof(SortByInternalNameLabel), string.Empty, string.Empty);
            SortByInternalNameLabel.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonTextTranslationSettingChanged();

            SortByTranslatedNameLabel = Config.Bind(sectionName, nameof(SortByTranslatedNameLabel), string.Empty, string.Empty);
            SortByTranslatedNameLabel.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonTextTranslationSettingChanged();

            SortByValueLabel = Config.Bind(sectionName, nameof(SortByValueLabel), string.Empty, string.Empty);
            SortByValueLabel.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonTextTranslationSettingChanged();

            SortByWeightLabel = Config.Bind(sectionName, nameof(SortByWeightLabel), string.Empty, string.Empty);
            SortByWeightLabel.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonTextTranslationSettingChanged();

            SortByTypeLabel = Config.Bind(sectionName, nameof(SortByTypeLabel), string.Empty, string.Empty);
            SortByTypeLabel.SettingChanged += (a, b) => ButtonRenderer.manager.OnButtonTextTranslationSettingChanged();

            QuickStackResultMessageNothing = Config.Bind(sectionName, nameof(QuickStackResultMessageNothing), string.Empty, string.Empty);
            QuickStackResultMessageNone = Config.Bind(sectionName, nameof(QuickStackResultMessageNone), string.Empty, string.Empty);
            QuickStackResultMessageOne = Config.Bind(sectionName, nameof(QuickStackResultMessageOne), string.Empty, string.Empty);
            QuickStackResultMessageMore = Config.Bind(sectionName, nameof(QuickStackResultMessageMore), string.Empty, string.Empty);

            RestockResultMessageNothing = Config.Bind(sectionName, nameof(RestockResultMessageNothing), string.Empty, string.Empty);
            RestockResultMessageNone = Config.Bind(sectionName, nameof(RestockResultMessageNone), string.Empty, string.Empty);
            RestockResultMessagePartial = Config.Bind(sectionName, nameof(RestockResultMessagePartial), string.Empty, string.Empty);
            RestockResultMessageFull = Config.Bind(sectionName, nameof(RestockResultMessageFull), string.Empty, string.Empty);

            TrashConfirmationOkayButton = Config.Bind(sectionName, nameof(TrashConfirmationOkayButton), string.Empty, string.Empty);
            QuickTrashConfirmation = Config.Bind(sectionName, nameof(QuickTrashConfirmation), string.Empty, string.Empty);
            CantTrashFavoritedItemWarning = Config.Bind(sectionName, nameof(CantTrashFavoritedItemWarning), string.Empty, string.Empty);
            CantTrashHotkeyBarItemWarning = Config.Bind(sectionName, nameof(CantTrashHotkeyBarItemWarning), string.Empty, string.Empty);
            CantTrashFlagFavoritedItemWarning = Config.Bind(sectionName, nameof(CantTrashFlagFavoritedItemWarning), string.Empty, string.Empty);
            CantFavoriteTrashFlaggedItemWarning = Config.Bind(sectionName, nameof(CantFavoriteTrashFlaggedItemWarning), string.Empty, string.Empty);

            FavoritedItemTooltip = Config.Bind(sectionName, nameof(FavoritedItemTooltip), string.Empty, string.Empty);
            TrashFlaggedItemTooltip = Config.Bind(sectionName, nameof(TrashFlaggedItemTooltip), string.Empty, string.Empty);
        }

        internal static void ResetAllFavoritingData_SettingChanged(object sender, EventArgs e)
        {
            if (DebugConfig.ResetAllFavoritingData?.Value != ResetFavoritingData.YesDeleteAllMyFavoritingData)
            {
                return;
            }

            DebugConfig.ResetAllFavoritingData.SettingChanged -= ResetAllFavoritingData_SettingChanged;
            DebugConfig.ResetAllFavoritingData.Value = ResetFavoritingData.No;
            DebugConfig.ResetAllFavoritingData.SettingChanged += ResetAllFavoritingData_SettingChanged;

            if (Player.m_localPlayer != null)
            {
                var playerConfig = UserConfig.GetPlayerConfig(Player.m_localPlayer.GetPlayerID());

                playerConfig.ResetAllFavoriting();
            }
        }

        public static bool TryGetOldConfigValue<T>(ConfigDefinition configDefinition, ref T oldValue, bool removeIfFound = true)
        {
            if (!TomlTypeConverter.CanConvert(typeof(T)))
            {
                throw new ArgumentException(string.Format("Type {0} is not supported by the config system. Supported types: {1}", typeof(T), string.Join(", ", (from x in TomlTypeConverter.GetSupportedTypes() select x.Name).ToArray())));
            }

            try
            {
                var iolock = AccessTools.FieldRefAccess<ConfigFile, object>("_ioLock").Invoke(Config);
                var orphanedEntries = (Dictionary<ConfigDefinition, string>)AccessTools.PropertyGetter(typeof(ConfigFile), "OrphanedEntries").Invoke(Config, new object[0]);

                lock (iolock)
                {
                    if (orphanedEntries.TryGetValue(configDefinition, out string oldValueString))
                    {
                        oldValue = (T)TomlTypeConverter.ConvertToValue(oldValueString, typeof(T));

                        if (removeIfFound)
                        {
                            orphanedEntries.Remove(configDefinition);
                        }

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Helper.LogO($"Error getting orphaned entry: {e.StackTrace}", DebugLevel.Warning);
            }

            return false;
        }

        #region Display Helper Methods

        public static bool CanDisplayInventorySortButton => DisplaySortButtons.Value != ShowTwoButtons.OnlyContainerButton;

        public static bool CanDisplayContainerSortButton => DisplaySortButtons.Value != ShowTwoButtons.OnlyInventoryButton;

        public static bool CanDisplayInventoryQuickStackButton => DisplayQuickStackButtons.Value != ShowTwoButtons.OnlyContainerButton;

        public static bool CanDisplayContainerQuickStackButton => DisplayQuickStackButtons.Value != ShowTwoButtons.OnlyInventoryButton;

        public static bool CanDisplayInventoryRestockButton => DisplayRestockButtons.Value != ShowTwoButtons.OnlyContainerButton;

        public static bool CanDisplayContainerRestockButton => DisplayRestockButtons.Value != ShowTwoButtons.OnlyInventoryButton;

        public static bool CanDisplayStoreAllButton => DisplayStoreAllButton.Value;

        public static bool CanDisplayTrashCanUI => DisplayTrashCanUI.Value;

        public static bool CanDisplayFavoriteToggleButton => DisplayFavoriteToggleButton.Value != FavoritingToggling.Disabled;

        public static bool CanDisplaySortCriteriaInLabel => DisplaySortCriteriaInLabel.Value;

        public static bool CanMoveTakeAllButton => !NeverMoveTakeAllButton.Value;

        public static bool ShouldAutoSortInventory => AutoSort.Value == AutoSortBehavior.SortPlayerInventoryOnOpen
                                                   || AutoSort.Value == AutoSortBehavior.Both;

        public static bool ShouldAutoSortContainer => AutoSort.Value == AutoSortBehavior.SortContainerOnOpen
                                                   || AutoSort.Value == AutoSortBehavior.Both;
        #endregion

        public enum OverrideButtonDisplay
        {
            DisableAllNewButtons,
            UseIndividualConfigOptions
        }

        public enum OverrideKeybindBehavior
        {
            DisableAllNewHotkeys,
            UseIndividualConfigOptions
        }

        public enum OverrideHotkeyBarBehavior
        {
            NeverAffectHotkeyBar,
            UseIndividualConfigOptions
        }

        public enum ShowConfirmDialogOption
        {
            Never,
            WhenNotTrashFlagged,
            Always
        }

        public enum ShowTwoButtons
        {
            Both,
            OnlyInventoryButton,
            OnlyContainerButton,
            BothButDependingOnContext,
        }

        public enum QuickStackBehavior
        {
            QuickStackOnlyToCurrentContainer,
            QuickStackToBoth
        }

        public enum RestockBehavior
        {
            RestockOnlyFromCurrentContainer,
            RestockFromBoth
        }

        public enum SortBehavior
        {
            OnlySortContainer,
            SortBoth
        }

        public enum SortCriteriaEnum
        {
            InternalName,
            TranslatedName,
            Value,
            Weight,
            Type
        }

        public enum AutoSortBehavior
        {
            Never,
            SortContainerOnOpen,
            SortPlayerInventoryOnOpen,
            Both
        }

        internal enum DebugLevel
        {
            Disabled = 0,
            Log = 1,
            Warning = 2
        }

        internal enum DebugSeverity
        {
            Normal = 0,
            AlsoSpeedTests = 1,
            Everything = 2
        }

        internal enum ResetFavoritingData
        {
            No,
            YesDeleteAllMyFavoritingData
        }

        internal enum FavoritingToggling
        {
            Disabled = 0,
            EnabledTopButton = 1,
            EnabledBottomButton = 2
        }
    }
}