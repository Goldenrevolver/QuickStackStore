using BepInEx;
using HarmonyLib;
using QuickStackStore.Source.UI;
using System.Reflection;
using UnityEngine;

namespace QuickStackStore
{
    [BepInIncompatibility("com.maxsch.valheim.MultiUserChest")]
    [BepInIncompatibility("virtuacode.valheim.trashitems")]
    [BepInDependency("randyknapp.mods.auga", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("goldenrevolver.quick_stack_store", NAME, VERSION)]
    public class QuickStackStorePlugin : BaseUnityPlugin
    {
        public const string NAME = "Quick Stack - Store - Sort - Trash - Restock";
        public const string VERSION = "1.2.5";

        // TODO controller support
        protected void Awake()
        {
            var path = "QuickStackStore.Resources";

            BorderRenderer.border = Helper.LoadSprite($"{path}.border.png", new Rect(0, 0, 1024, 1024), new Vector2(512, 512));
            TrashModule.trashSprite = Helper.LoadSprite($"{path}.trash.png", new Rect(0, 0, 64, 64), new Vector2(32, 32));
            TrashModule.bgSprite = Helper.LoadSprite($"{path}.trashmask.png", new Rect(0, 0, 96, 112), new Vector2(48, 56));

            if (CompatibilitySupport.HasAuga())
            {
                ButtonRenderer.manager = new AugaButtonRendererManager();
                BorderRenderer.border = null;
            }
            else
            {
                ButtonRenderer.manager = new DefaultButtonRendererManager();
            }

            QSSConfig.LoadConfig(this);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }
}