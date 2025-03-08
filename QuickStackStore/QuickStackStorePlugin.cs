using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace QuickStackStore
{
    [BepInIncompatibility("virtuacode.valheim.trashitems")]
    [BepInDependency(CompatibilitySupport.azuEPI, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CompatibilitySupport.multiUserChest, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class QuickStackStorePlugin : BaseUnityPlugin
    {
        public const string GUID = "goldenrevolver.quick_stack_store";
        public const string NAME = "Quick Stack - Store - Sort - Trash - Restock";
        public const string VERSION = "1.4.13";

        // intentionally not Awake, so the chainloader is done (for compatibility checks, mostly in the config)
        protected void Start()
        {
            if (CompatibilitySupport.HasOutdatedMUCPlugin())
            {
                Helper.LogO("This mod is not compatible with versions of Multi User Chest earlier than 0.4.0, aborting start", QSSConfig.DebugLevel.Warning);
                return;
            }

            if (AzuExtendedPlayerInventory.API.IsLoaded())
            {
                CompatibilitySupport.isUsingAzuEPIWithAPI = true;

                if (CompatibilitySupport.HasAzuEPIWithQuickslotCompatibleAPI())
                {
                    CompatibilitySupport.isUsingAzuEPIWithQuickslotCompatibleAPI = true;
                }
            }

            string path = "QuickStackStore.Resources";

            BorderRenderer.border = Helper.LoadSprite($"{path}.border.png", new Rect(0, 0, 1024, 1024));
            TrashModule.trashSprite = Helper.LoadSprite($"{path}.trash.png", new Rect(0, 0, 64, 64));
            TrashModule.bgSprite = Helper.LoadSprite($"{path}.trashmask.png", new Rect(0, 0, 96, 112));

            QSSConfig.LoadConfig(this);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);
        }
    }

    [HarmonyPatch(typeof(FejdStartup))]
    internal class FejdStartupPatch
    {
        [HarmonyPatch(nameof(FejdStartup.Awake)), HarmonyPostfix]
        private static void FejdStartupAwakePatch()
        {
            LocalizationLoader.SetupTranslations();
            QSSConfig.ConfigTemplate_SettingChanged(null, null);
        }
    }
}