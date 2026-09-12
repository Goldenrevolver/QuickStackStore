using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    public static class Helper
    {
        private static MethodInfo LoadImage;

        internal static void Log(object s, DebugSeverity debugSeverity = DebugSeverity.Normal)
        {
            if ((int)debugSeverity > (int)(DebugConfig.DebugSeverity?.Value ?? 0))
            {
                return;
            }

            string toPrint = $"{QuickStackStorePlugin.NAME} {QuickStackStorePlugin.VERSION}: {(s != null ? s.ToString() : "null")}";

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
            string toPrint = $"{QuickStackStorePlugin.NAME} {QuickStackStorePlugin.VERSION}: {(s != null ? s.ToString() : "null")}";

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
            int yPosCompare = -a.y.CompareTo(b.y);

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

                // texture.LoadImage(mStream.ToArray());
                // prevents a conflict between .Net Standard 2.1 and Unity/.NET Framework 4.8
                // the correct method is available at runtime
                if (LoadImage == null)
                {
                    var imageConversionAssembly =
                        Assembly.Load("UnityEngine.ImageConversionModule");
                    var imageConversionType =
                        imageConversionAssembly.GetType("UnityEngine.ImageConversion");

                    LoadImage = imageConversionType.GetMethod(
                        "LoadImage",
                        new[] { typeof(Texture2D), typeof(byte[]) }
                    );

                    if (LoadImage == null)
                    {
                        throw new MissingMethodException(
                            "Could not find UnityEngine.ImageConversion.LoadImage(Texture2D, byte[])"
                        );
                    }
                }

                LoadImage.Invoke(null, new object[] { texture, mStream.ToArray() });

                texture.Apply();
                return Sprite.Create(texture, size, pivot.Value, units);
            }
        }
    }
}