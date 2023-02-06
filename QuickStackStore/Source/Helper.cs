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

        internal static void LogO(object s, DebugLevel OverrideLevel = DebugLevel.Warning)
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

        // originally from 'Trash Items' mod, as allowed in their permission settings on nexus
        // https://www.nexusmods.com/valheim/mods/441
        // https://github.com/virtuaCode/valheim-mods/tree/main/TrashItems
        public static Sprite LoadSprite(string path, Rect size, Vector2? pivot = null, int units = 100)
        {
            if (pivot == null)
            {
                pivot = new Vector2(0.5f, 0.5f);
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream imageStream = assembly.GetManifestResourceStream(path);

            Texture2D texture = new Texture2D((int)size.width, (int)size.height, TextureFormat.RGBA32, false, true);

            using (MemoryStream mStream = new MemoryStream())
            {
                imageStream.CopyTo(mStream);
                texture.LoadImage(mStream.ToArray());
                texture.Apply();
                return Sprite.Create(texture, size, pivot.Value, units);
            }
        }
    }
}