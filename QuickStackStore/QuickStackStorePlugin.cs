using BepInEx;
using HarmonyLib;
using QuickStackStore.UI.Buttons;
using QuickStackStore.UI.Favoriting;
using System.Reflection;
using UnityEngine;

namespace QuickStackStore
{
    [BepInIncompatibility("virtuacode.valheim.trashitems")]
    [BepInDependency(CompatibilitySupport.auga, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CompatibilitySupport.multiUserChest, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class QuickStackStorePlugin : BaseUnityPlugin
    {
        public const string GUID = "goldenrevolver.quick_stack_store";
        public const string NAME = "Quick Stack - Store - Sort - Trash - Restock";
        public const string VERSION = "1.3.3";

        protected void Awake()
        {
            if (CompatibilitySupport.HasOutdatedMUCPlugin())
            {
                Helper.LogO("This mod is not compatible with versions of Multi User Chest earlier than 0.4.0, aborting start", QSSConfig.DebugLevel.Warning);
                return;
            }

            if (CompatibilitySupport.HasAuga())
            {
                ButtonRenderer.instance = new AugaButtonRenderer();
                FavoriteRenderer.instance = new AugaFavoriteRenderer();
            }
            else
            {
                var path = "QuickStackStore.Resources";

                ControllerButtonHintHelper.circleButtonSprite = Helper.LoadSprite($"{path}.circleButton.png", new Rect(0, 0, 36, 36));
                ControllerButtonHintHelper.rectButtonSprite = Helper.LoadSprite($"{path}.rectangleButton.png", new Rect(0, 0, 28, 28));

                FavoriteRenderer.instance.indicator = Helper.LoadSprite($"{path}.border.png", new Rect(0, 0, 1024, 1024));
                TrashModule.trashSprite = Helper.LoadSprite($"{path}.trash.png", new Rect(0, 0, 64, 64));
                TrashModule.bgSprite = Helper.LoadSprite($"{path}.trashmask.png", new Rect(0, 0, 96, 112));

                ButtonRenderer.instance = new DefaultButtonRenderer();
                FavoriteRenderer.instance = new DefaultFavoriteRenderer(Helper.LoadSprite($"{path}.border.png", new Rect(0, 0, 1024, 1024), new Vector2(512, 512)));
            }

            QSSConfig.LoadConfig(this);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(FejdStartup))]
    internal class FejdStartupPatch
    {
        [HarmonyPatch(nameof(FejdStartup.Awake)), HarmonyPostfix]
        private static void FejdStartupAwakePatch()
        {
            LocalizationConfig.SetupTranslations();
            QSSConfig.ConfigTemplate_SettingChanged(null, null);
        }
    }
}