using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using YamlDotNet.Serialization;

namespace QuickStackStore
{
    [HarmonyPatch(typeof(Localization))]
    internal class LocalizationPatch
    {
        [HarmonyPatch(nameof(Localization.SetupLanguage)), HarmonyPostfix]
        private static void SetupLanguagePatch(Localization __instance, string language)
        {
            LocalizationConfig.FixTakeAllDefaultText(__instance, language);
        }
    }

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

        public static string[] supportedEmbeddedLanguages = new[] { "English", "Chinese", "Russian", "French", "Portuguese_Brazilian", "Polish" };

        private const string embeddedLanguagePathFormat = "QuickStackStore.Translations.QuickStackStore.{0}.json";

        private const string loadingLog = "Loading {0} translation file for language: {1}";
        private const string failedLoadLog = "Loading {0} translation file for language: {1}";
        private const string external = "external";
        private const string embedded = "embedded";

        public const string takeAllKey = "inventory_takeall";

        internal static string GetRelevantTranslation(ConfigEntry<string> config, string configName)
        {
            return !(config?.Value).IsNullOrWhiteSpace() ? config.Value : Localization.instance.Translate($"quickstackstore_{configName.ToLower()}");
        }

        internal static void FixTakeAllDefaultText(Localization localization, string language)
        {
            if (localization.m_translations.ContainsKey(takeAllKey))
            {
                if (language == "English")
                {
                    localization.m_translations[takeAllKey] = "Take All";
                }
                else if (language == "Russian")
                {
                    localization.m_translations[takeAllKey] = "взять всё";
                }
                else if (language == "French")
                {
                    localization.m_translations[takeAllKey] = "Tout Prendre";
                }
            }
        }

        internal static void SetupTranslations()
        {
            var currentLanguage = Localization.instance.GetSelectedLanguage();

            FixTakeAllDefaultText(Localization.instance, currentLanguage);

            var languageFilesFound = Directory.GetFiles(Path.GetDirectoryName(Paths.PluginPath), "QuickStackStore.*.json", SearchOption.AllDirectories);

            bool externalFileLoaded = false;

            foreach (var languageFilePath in languageFilesFound)
            {
                var languageKey = Path.GetFileNameWithoutExtension(languageFilePath).Split('.')[1];

                if (languageKey == currentLanguage)
                {
                    Helper.Log(string.Format(loadingLog, external, currentLanguage), QSSConfig.DebugSeverity.Everything);

                    if (!LoadExternalLanguageFile(currentLanguage, languageFilePath))
                    {
                        Helper.LogO(string.Format(failedLoadLog, external, currentLanguage), QSSConfig.DebugLevel.Warning);
                    }
                    else
                    {
                        externalFileLoaded = true;
                    }

                    break;
                }
            }

            if (!externalFileLoaded && currentLanguage != "English" && supportedEmbeddedLanguages.Contains(currentLanguage))
            {
                Helper.Log(string.Format(loadingLog, embedded, currentLanguage), QSSConfig.DebugSeverity.Everything);

                if (!LoadEmbeddedLanguageFile(currentLanguage))
                {
                    Helper.LogO(string.Format(failedLoadLog, embedded, currentLanguage), QSSConfig.DebugLevel.Warning);
                }
            }

            Helper.Log(string.Format(loadingLog, embedded, "English"), QSSConfig.DebugSeverity.Everything);

            // always load embedded english at the end to fill potential missing translations
            if (!LoadEmbeddedLanguageFile("English"))
            {
                Helper.LogO(string.Format(failedLoadLog, embedded, "English"), QSSConfig.DebugLevel.Warning);
            }
        }

        internal static bool LoadExternalLanguageFile(string language, string path)
        {
            string translationAsString = File.ReadAllText(path);

            if (translationAsString == null)
            {
                return false;
            }

            return ParseStringToLanguage(language, translationAsString);
        }

        internal static bool LoadEmbeddedLanguageFile(string language)
        {
            string translationAsString = ReadEmbeddedTextFile(string.Format(embeddedLanguagePathFormat, language));

            if (translationAsString == null)
            {
                return false;
            }

            return ParseStringToLanguage(language, translationAsString);
        }

        internal static bool ParseStringToLanguage(string language, string translationAsString)
        {
            Dictionary<string, string> parsedTranslationDict = new DeserializerBuilder().IgnoreFields().Build().Deserialize<Dictionary<string, string>>(translationAsString);

            if (parsedTranslationDict == null || parsedTranslationDict.Count == 0)
            {
                return false;
            }

            foreach (var pair in parsedTranslationDict)
            {
                AddForLanguage(language, pair.Key, pair.Value);
            }

            return true;
        }

        internal static void AddForLanguage(string language, string key, string value)
        {
            string actualKey = keyPrefix + key.ToLower();

            bool isCurrentLanguage = Localization.instance.GetSelectedLanguage() == language;
            bool isDefaultLanguageAndNotYetSet = language == "English" && !Localization.instance.m_translations.ContainsKey(actualKey);

            if (isCurrentLanguage || isDefaultLanguageAndNotYetSet)
            {
                Localization.instance.AddWord(actualKey, value);
            }
        }

        public static string ReadEmbeddedTextFile(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);

            if (stream == null)
            {
                return null;
            }

            using (MemoryStream memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);

                var bytes = memStream.Length > 0 ? memStream.ToArray() : null;

                return bytes != null ? Encoding.UTF8.GetString(bytes) : null;
            }
        }
    }
}