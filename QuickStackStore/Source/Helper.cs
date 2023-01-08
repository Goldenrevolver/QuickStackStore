using HarmonyLib;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    public static class Helper
    {
        /// <summary>
        /// Fairly useful bit of code for sussing out the layout of the ui tree.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="maxDepth"></param>
        public static void DumpComponents(Component parent, int maxDepth = 3)
        {
            Debug.Log(DumpComponentsInner(parent, 0, maxDepth));
        }

        private static string DumpComponentsInner(Component parent, int depth, int maxDepth)
        {
            string result = "";
            string tabs = "";

            for (int i = 0; i < depth; i++)
                tabs = $"{tabs}\t";

            Component[] children = parent.GetComponentsInChildren<Component>()
                .Where(c => c.transform.parent.GetInstanceID() == parent.GetInstanceID())
                .ToArray();

            if (parent.transform is RectTransform pRect)
            {
                result = $"{parent}:{parent.GetInstanceID()}[{children?.Length ?? 0}] -> {parent.gameObject.name} ^- {parent.transform.parent}:{parent.transform.parent.GetInstanceID()} [pos:{pRect.rect.position} -- size:{pRect.rect.size}]\n";
            }
            else
            {
                result = $"{parent}:{parent.GetInstanceID()}[{children?.Length ?? 0}] -> {parent.gameObject.name} ^- {parent.transform.parent}:{parent.transform.parent.GetInstanceID()} [pos:{parent.transform.position}]\n";
            }

            if (depth < maxDepth)
                foreach (Component child in children)
                    result = $"{result}{tabs}{DumpComponentsInner(child, depth + 1, maxDepth)}";

            return result;
        }

        internal static void Log(object s, DebugSeverity debugSeverity = DebugSeverity.Normal)
        {
            if ((int)debugSeverity > (int)(DebugConfig.DebugSeverity?.Value ?? 0))
            {
                return;
            }

            var toPrint = $"{QuickStackStorePlugin.NAME} {QuickStackStorePlugin.VERSION}: {(s != null ? s.ToString() : "null")}";

            if (DebugConfig.ShowDebugLogs?.Value == DebugLevel.Log)
            {
                Debug.Log(toPrint);
            }
            else if (DebugConfig.ShowDebugLogs?.Value == DebugLevel.Warning)
            {
                Debug.LogWarning(toPrint);
            }
        }

        internal static void LogO(object s, DebugLevel OverrideLevel)
        {
            var toPrint = $"{QuickStackStorePlugin.NAME} {QuickStackStorePlugin.VERSION}: {(s != null ? s.ToString() : "null")}";

            if (OverrideLevel == DebugLevel.Log)
            {
                Debug.Log(toPrint);
            }
            else if (OverrideLevel == DebugLevel.Warning)
            {
                Debug.LogWarning(toPrint);
            }
        }

        internal static int CompareSlotOrder(Vector2i a, Vector2i b)
        {
            // Bottom left to top right
            var yPosCompare = -a.y.CompareTo(b.y);

            if (GeneralConfig.UseTopDownLogicForEverything.Value)
            {
                // Top left to bottom right
                yPosCompare *= -1;
            }

            return yPosCompare != 0 ? yPosCompare : a.x.CompareTo(b.x);
        }

        private static bool hasCurrentlyToggledFavoriting = false;

        private const string blackStar = "\u2605";
        private const string whiteStar = "\u2606";

        internal static bool HasCurrentlyToggledFavoriting
        {
            get => hasCurrentlyToggledFavoriting;
            set
            {
                hasCurrentlyToggledFavoriting = value;

                if (ButtonRenderer.manager.favoritingTogglingButtonText != null)
                {
                    var color = ColorUtility.ToHtmlStringRGB(FavoriteConfig.BorderColorFavoritedItem.Value);

                    ButtonRenderer.manager.favoritingTogglingButtonText.text = $"<color=#{color}>{(value ? blackStar : whiteStar)}</color>";
                }
            }
        }

        internal static bool IsInFavoritingMode()
        {
            return HasCurrentlyToggledFavoriting
                || Input.GetKey(FavoriteConfig.FavoritingModifierKey1.Value)
                || Input.GetKey(FavoriteConfig.FavoritingModifierKey2.Value);
        }

        internal static Color GetMixedColor(Color color1, Color color2)
        {
            float r = (color1.r + color2.r) / 2f;
            float g = (color1.g + color2.g) / 2f;
            float b = (color1.b + color2.b) / 2f;
            float a = (color1.a + color2.a) / 2f;

            return new Color(r, g, b, a);
        }

        // originally from 'Trash Items' mod, as allowed in their permission settings on nexus
        // https://www.nexusmods.com/valheim/mods/441
        // https://github.com/virtuaCode/valheim-mods/tree/main/TrashItems
        public static Sprite LoadSprite(string path, Rect size, Vector2 pivot, int units = 100)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream img = asm.GetManifestResourceStream(path);

            Texture2D tex = new Texture2D((int)size.width, (int)size.height, TextureFormat.RGBA32, false, true);

            using (MemoryStream mStream = new MemoryStream())
            {
                img.CopyTo(mStream);
                tex.LoadImage(mStream.ToArray());
                tex.Apply();
                return Sprite.Create(tex, size, pivot, units);
            }
        }

        // taken from https://github.com/aedenthorn/ValheimMods AedenthornUtils.IgnoreKeyPresses, public domain
        public static bool IgnoreKeyPresses()
        {
            // removed InventoryGui.IsVisible() because we specifically want that to be the case
            return ZNetScene.instance == null || Player.m_localPlayer == null || Minimap.IsOpen() || Console.IsVisible() || TextInput.IsVisible() || ZNet.instance.InPasswordDialog() || Chat.instance?.HasFocus() == true || StoreGui.IsVisible() || Menu.IsVisible() || TextViewer.instance?.IsVisible() == true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    public static class HotkeyListener
    {
        public static void Postfix(Player __instance)
        {
            if (GeneralConfig.OverrideKeybindBehavior.Value == OverrideKeybindBehavior.DisableAllNewHotkeys)
            {
                return;
            }

            if (Player.m_localPlayer != __instance)
            {
                return;
            }

            if (Helper.IgnoreKeyPresses())
            {
                return;
            }

            if (Input.GetKeyDown(QuickStackConfig.QuickStackKey.Value))
            {
                QuickStackRestockModule.DoQuickStack(__instance);
            }
            else if (Input.GetKeyDown(RestockConfig.RestockKey.Value))
            {
                QuickStackRestockModule.DoRestock(__instance);
            }

            if (!InventoryGui.IsVisible())
            {
                return;
            }

            if (Input.GetKeyDown(SortConfig.SortKey.Value))
            {
                SortModule.DoSort(__instance);
            }
            else if (Input.GetKeyDown(TrashConfig.TrashHotkey.Value))
            {
                TrashModule.TrashOrTrashFlagItem(true);
            }
            else if (Input.GetKeyDown(TrashConfig.QuickTrashHotkey.Value))
            {
                TrashModule.AttemptQuickTrash();
            }
        }
    }
}