using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using QuickStackStore.Source.UI;

namespace QuickStackStore
{
    internal class LocalizationConfig
    {
        private const string keyPrefix = "quickstackstore_";

        public static ConfigEntry<string> RestockLabelCharacter;
        public static ConfigEntry<string> QuickStackLabelCharacter;
        public static ConfigEntry<string> SortLabelCharacter;

        public static ConfigEntry<string> QuickStackResultMessageNothing;
        public static ConfigEntry<string> QuickStackResultMessageNone;
        public static ConfigEntry<string> QuickStackResultMessageOne;
        public static ConfigEntry<string> QuickStackResultMessageMore;

        public static ConfigEntry<string> RestockResultMessageNothing;
        public static ConfigEntry<string> RestockResultMessageNone;
        public static ConfigEntry<string> RestockResultMessagePartial;
        public static ConfigEntry<string> RestockResultMessageFull;

        public static ConfigEntry<string> QuickStackLabel;
        public static ConfigEntry<string> StoreAllLabel;
        public static ConfigEntry<string> SortLabel;
        public static ConfigEntry<string> RestockLabel;
        public static ConfigEntry<string> TrashLabel;
        public static ConfigEntry<string> TakeAllLabel;

        public static ConfigEntry<string> SortByInternalNameLabel;
        public static ConfigEntry<string> SortByTranslatedNameLabel;
        public static ConfigEntry<string> SortByValueLabel;
        public static ConfigEntry<string> SortByWeightLabel;
        public static ConfigEntry<string> SortByTypeLabel;

        public static ConfigEntry<string> TrashConfirmationOkayButton;
        public static ConfigEntry<string> QuickTrashConfirmation;
        public static ConfigEntry<string> CantTrashFavoritedItemWarning;
        public static ConfigEntry<string> CantTrashFlagFavoritedItemWarning;
        public static ConfigEntry<string> CantTrashHotkeyBarItemWarning;
        public static ConfigEntry<string> CantFavoriteTrashFlaggedItemWarning;

        public static ConfigEntry<string> FavoritedItemTooltip;
        public static ConfigEntry<string> TrashFlaggedItemTooltip;

        private static Dictionary<string, Tuple<string, ConfigEntry<string>>> _ConfigLookup = new Dictionary<string, Tuple<string, ConfigEntry<string>>>();

        static LocalizationConfig()
        {
            _ConfigLookup.Add(nameof(ButtonRendererManagerBase.quickStackAreaButton), new Tuple<string, ConfigEntry<string>>(nameof(QuickStackLabel), QuickStackLabel));
            _ConfigLookup.Add(nameof(ButtonRendererManagerBase.quickStackToContainerButton), new Tuple<string, ConfigEntry<string>>(nameof(QuickStackLabel), QuickStackLabel));
            _ConfigLookup.Add(nameof(ButtonRendererManagerBase.restockAreaButton), new Tuple<string, ConfigEntry<string>>(nameof(RestockLabel), RestockLabel));
            _ConfigLookup.Add(nameof(ButtonRendererManagerBase.restockFromContainerButton), new Tuple<string, ConfigEntry<string>>(nameof(RestockLabel), RestockLabel));
            _ConfigLookup.Add(nameof(ButtonRendererManagerBase.sortContainerButton), new Tuple<string, ConfigEntry<string>>(nameof(SortLabel), SortLabel));
            _ConfigLookup.Add(nameof(ButtonRendererManagerBase.sortInventoryButton), new Tuple<string, ConfigEntry<string>>(nameof(SortLabel), SortLabel));
            _ConfigLookup.Add(nameof(ButtonRendererManagerBase.storeAllButton), new Tuple<string, ConfigEntry<string>>(nameof(StoreAllLabel), StoreAllLabel));
        }

        internal static string GetRelevantTranslation(string objectName)
        {
            if (_ConfigLookup.TryGetValue(objectName, out Tuple<string, ConfigEntry<string>> config))
            {
                if (config.Item2 != null)
                    return config.Item2.Value;
                return Localization.instance.Translate($"quickstackstore_{config.Item1.ToLower()}");
            }
            return Localization.instance.Translate($"quickstackstore_{objectName.ToLower()}");
        }

        internal static string GetRelevantTranslation(ConfigEntry<string> config, string configName)
        {
            return !(config?.Value).IsNullOrWhiteSpace() ? config.Value : Localization.instance.Translate($"quickstackstore_{configName.ToLower()}");
        }

        internal static void SetupTranslations()
        {
            if (Localization.instance.GetSelectedLanguage() == "English")
            {
                if (Localization.instance.m_translations.ContainsKey("inventory_takeall"))
                {
                    Localization.instance.m_translations["inventory_takeall"] = "Take All";
                }
            }

            Chinese(nameof(TrashLabel).ToLower(), "废纸篓");
            Chinese(nameof(QuickStackLabel).ToLower(), "快速堆叠(Q)");
            Chinese(nameof(StoreAllLabel).ToLower(), "全部存进");
            Chinese(nameof(RestockLabel).ToLower(), "补货(R)");
            Chinese(nameof(SortLabel).ToLower(), "整理(S)");

            Chinese(nameof(SortByInternalNameLabel).ToLower(), "内部名称");
            Chinese(nameof(SortByTranslatedNameLabel).ToLower(), "翻译名称");
            Chinese(nameof(SortByValueLabel).ToLower(), "价值");
            Chinese(nameof(SortByWeightLabel).ToLower(), "重量");
            Chinese(nameof(SortByTypeLabel).ToLower(), "类型");

            Chinese(nameof(QuickStackResultMessageNothing).ToLower(), "无需快速堆叠");
            Chinese(nameof(QuickStackResultMessageNone).ToLower(), "存入 0 个物品");
            Chinese(nameof(QuickStackResultMessageOne).ToLower(), "存入 1 个物品");
            Chinese(nameof(QuickStackResultMessageMore).ToLower(), "存入 {0} 个物品");

            Chinese(nameof(RestockResultMessageNothing).ToLower(), "无需补货");
            Chinese(nameof(RestockResultMessageNone).ToLower(), "无法补货 (0/{0})");
            Chinese(nameof(RestockResultMessagePartial).ToLower(), "已部分补货 ({0}/{1})");
            Chinese(nameof(RestockResultMessageFull).ToLower(), "已补货 (共: {0})");

            Chinese(nameof(TrashConfirmationOkayButton).ToLower(), "废纸篓");
            Chinese(nameof(QuickTrashConfirmation).ToLower(), "快速丢弃?");
            Chinese(nameof(CantTrashFavoritedItemWarning).ToLower(), "不能丢弃收藏的物品!");
            Chinese(nameof(CantTrashHotkeyBarItemWarning).ToLower(), "无法将收藏物品标记为垃圾!");
            Chinese(nameof(CantTrashFlagFavoritedItemWarning).ToLower(), "设为不允许丢弃在快速热键栏中的项目!");
            Chinese(nameof(CantFavoriteTrashFlaggedItemWarning).ToLower(), "无法收藏被标记为垃圾的项目!");

            Chinese(nameof(FavoritedItemTooltip).ToLower(), "不会被快速堆叠、分类、存放或丢弃");
            Chinese(nameof(TrashFlaggedItemTooltip).ToLower(), "可以快速丢弃");

            English(nameof(QuickStackLabelCharacter).ToLower(), "Q");
            English(nameof(SortLabelCharacter).ToLower(), "S");
            English(nameof(RestockLabelCharacter).ToLower(), "R");

            English(nameof(TrashLabel).ToLower(), "Trash");
            English(nameof(QuickStackLabel).ToLower(), "Quick Stack");
            English(nameof(StoreAllLabel).ToLower(), "Store All");
            English(nameof(RestockLabel).ToLower(), "Restock");
            English(nameof(SortLabel).ToLower(), "Sort");

            English(nameof(SortByInternalNameLabel).ToLower(), "i. name");
            English(nameof(SortByTranslatedNameLabel).ToLower(), "t. name");
            English(nameof(SortByValueLabel).ToLower(), "value");
            English(nameof(SortByWeightLabel).ToLower(), "weight");
            English(nameof(SortByTypeLabel).ToLower(), "type");

            English(nameof(QuickStackResultMessageNothing).ToLower(), "Nothing to quick stack");
            English(nameof(QuickStackResultMessageNone).ToLower(), "Stacked 0 items");
            English(nameof(QuickStackResultMessageOne).ToLower(), "Stacked 1 item");
            English(nameof(QuickStackResultMessageMore).ToLower(), "Stacked {0} items");

            English(nameof(RestockResultMessageNothing).ToLower(), "Nothing to restock");
            English(nameof(RestockResultMessageNone).ToLower(), "Couldn't restock (0/{0})");
            English(nameof(RestockResultMessagePartial).ToLower(), "Partially restocked ({0}/{1})");
            English(nameof(RestockResultMessageFull).ToLower(), "Fully restocked (total: {0})");

            English(nameof(TrashConfirmationOkayButton).ToLower(), "Trash");
            English(nameof(QuickTrashConfirmation).ToLower(), "Quick trash?");
            English(nameof(CantTrashFavoritedItemWarning).ToLower(), "Can't trash favorited item!");
            English(nameof(CantTrashHotkeyBarItemWarning).ToLower(), "Settings disallow trashing of item in hotkey bar!");
            English(nameof(CantTrashFlagFavoritedItemWarning).ToLower(), "Can't trash flag a favorited item!");
            English(nameof(CantFavoriteTrashFlaggedItemWarning).ToLower(), "Can't favorite a trash flagged item!");

            English(nameof(FavoritedItemTooltip).ToLower(), "Will not be quick stacked, sorted,\nstore all'd or trashed");
            English(nameof(TrashFlaggedItemTooltip).ToLower(), "Can be quick trashed");
        }

        internal static void AddForLang(string lang, string key, string value)
        {
            if (Localization.instance.GetSelectedLanguage() == lang)
            {
                Localization.instance.AddWord(key, value);
            }
            else if (lang == "English" && !Localization.instance.m_translations.ContainsKey(key))
            {
                Localization.instance.AddWord(key, value);
            }
        }

        internal static void English(string key, string value) => AddForLang("English", keyPrefix + key, value);

        internal static void Chinese(string key, string value) => AddForLang("Chinese", keyPrefix + key, value);
    }
}