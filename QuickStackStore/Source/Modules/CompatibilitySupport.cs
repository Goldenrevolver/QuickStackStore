using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    public static class CompatibilitySupport
    {
        private static MethodInfo IsComfyArmorSlot;
        private static FieldInfo IsQuiverEnabled;
        private static FieldInfo QuiverRowIndex;
        private static FieldInfo AedenAddEquipmentRow;
        private static FieldInfo OdinExAddEquipmentRow;
        private static FieldInfo OdinQOLAddEquipmentRow;
        private static FieldInfo RandyQuickSlotsEnabled;

        public static Dictionary<string, bool> cache = null;

        public const string aeden = "aedenthorn.ExtendedPlayerInventory";
        public const string comfy = "com.bruce.valheim.comfyquickslots";
        public const string odinPlus = "com.odinplusqol.mod";
        public const string odinExInv = "odinplusqol.OdinsExtendedInventory";
        public const string randy = "randyknapp.mods.equipmentandquickslots";
        public const string betterArchery = "ishid4.mods.betterarchery";
        public const string smartContainers = "flueno.SmartContainers";
        public const string backpacks = "org.bepinex.plugins.backpacks";
        public const string auga = "randyknapp.mods.auga";

        public static string[] supportedPlugins = new string[]
        {
            aeden,
            comfy,
            odinPlus,
            odinExInv,
            randy,
            betterArchery,
            auga,
        };

        public static void RegenerateCache()
        {
            cache = new Dictionary<string, bool>();

            var plugins = UnityEngine.Object.FindObjectsOfType<BaseUnityPlugin>();

            foreach (var guid in supportedPlugins)
            {
                var found = plugins.Any(plugin => plugin.Info.Metadata.GUID == guid);

                if (found)
                {
                    Helper.Log($"Found supported plugin {guid}. Enabling compatibility for it.");
                }

                cache[guid] = found;
            }
        }

        public static bool HasPlugin(string guid)
        {
            if (cache == null)
            {
                RegenerateCache();
            }

            if (!cache.ContainsKey(guid))
            {
                var plugins = UnityEngine.Object.FindObjectsOfType<BaseUnityPlugin>();
                cache[guid] = plugins.Any(plugin => plugin.Info.Metadata.GUID == guid);
            }

            return cache[guid];
        }

        public enum RandyStatus
        {
            Disabled,
            EnabledWithoutQuickSlots,
            EnabledWithQuickSlots
        }

        public static RandyStatus HasRandyPlugin()
        {
            if (!HasPlugin(randy))
            {
                return RandyStatus.Disabled;
            }

            var status = RandyStatus.EnabledWithQuickSlots;

            if (RandyQuickSlotsEnabled == null)
            {
                var assembly = Assembly.Load("EquipmentAndQuickSlots");

                if (assembly != null)
                {
                    var type = assembly.GetTypes().First(a => a.IsClass && a.Name == "EquipmentAndQuickSlots");
                    var pubStaticFields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                    RandyQuickSlotsEnabled = pubStaticFields.First(t => t.Name == "QuickSlotsEnabled");
                }
            }

            if (RandyQuickSlotsEnabled?.GetValue(null) is ConfigEntry<bool> config && !config.Value)
            {
                status = RandyStatus.EnabledWithoutQuickSlots;
            }

            return status;
        }

        public static bool ShouldBlockChangesToTakeAllButton()
        {
            return StoreTakeAllConfig.NeverMoveTakeAllButton.Value || HasPlugin(smartContainers) || HasPlugin(backpacks);
        }

        public static bool HasPluginThatRequiresMiniButtonVMove()
        {
            return HasPlugin(aeden) || HasPlugin(odinExInv) || HasPlugin(odinPlus);
        }

        public static bool HasAuga()
        {
            if (!HasPlugin(auga))
                return false;

            if (Assembly.Load("AugaAPI") is null)
                return false;

            return true;
            
        }

        private static bool IsEquipOrQuickSlotForAedenLike(ref FieldInfo fieldInfo, string assemblyName, string className, string fieldName, int inventoryHeight, Vector2i itemPos, bool checkForRestockableSlots)
        {
            if (fieldInfo == null)
            {
                var assembly = Assembly.Load(assemblyName);

                if (assembly != null)
                {
                    var type = assembly.GetTypes().First(a => a.IsClass && a.Name == className);
                    var pubStaticFields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                    fieldInfo = pubStaticFields.First(t => t.Name == fieldName);
                }
            }

            if (fieldInfo?.GetValue(null) is ConfigEntry<bool> config && config.Value)
            {
                bool isEquipmentRow = itemPos.y == inventoryHeight - 1;

                if (isEquipmentRow && (checkForRestockableSlots || itemPos.x < 5 || itemPos.x > 7))
                {
                    return true;
                }
            }

            return false;
        }

        //Helper.Log($"Found {mod} and it says {itemPos} is an equip/ quick slot");
        public static bool IsEquipOrQuickSlot(int inventoryHeight, Vector2i itemPos, bool checkForRestockableSlots = true)
        {
            //if (HasPlugin(randy))
            //{
            //    // randyknapps mod ignores everything this mod does anyway, so no need for specific compatibility
            //}

            if (HasPlugin(aeden) && IsEquipOrQuickSlotForAedenLike(ref AedenAddEquipmentRow, "ExtendedPlayerInventory", "BepInExPlugin", "addEquipmentRow", inventoryHeight, itemPos, checkForRestockableSlots))
            {
                return true;
            }

            if (HasPlugin(odinExInv) && IsEquipOrQuickSlotForAedenLike(ref OdinExAddEquipmentRow, "OdinsExtendedInventory", "OdinsExtendedInventoryPlugin", "addEquipmentRow", inventoryHeight, itemPos, checkForRestockableSlots))
            {
                return true;
            }

            if (HasPlugin(odinPlus) && IsEquipOrQuickSlotForAedenLike(ref OdinQOLAddEquipmentRow, "OdinQOL", "QuickAccessBar", "AddEquipmentRow", inventoryHeight, itemPos, checkForRestockableSlots))
            {
                return true;
            }

            if (HasPlugin(comfy))
            {
                if (IsComfyArmorSlot == null)
                {
                    var assembly = Assembly.Load("ComfyQuickSlots");

                    if (assembly != null)
                    {
                        var type = assembly.GetTypes().First(a => a.IsClass && a.Name == "ComfyQuickSlots");
                        var pubStaticMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                        IsComfyArmorSlot = pubStaticMethods.First(t => t.Name == "IsArmorSlot" && t.GetParameters().Length == 1);
                    }
                }

                if (IsComfyArmorSlot?.Invoke(null, new object[] { itemPos }) is bool isArmorSlot && isArmorSlot)
                {
                    return true;
                }

                if (checkForRestockableSlots)
                {
                    // check for quickslot (could also be armor slot)
                    if (IsComfyArmorSlot?.Invoke(null, new object[] { new Vector2i(itemPos.x - 3, itemPos.y) }) is bool isArmorOrQuickSlot && isArmorOrQuickSlot)
                    {
                        return true;
                    }
                }
            }

            if (HasPlugin(betterArchery))
            {
                if (IsQuiverEnabled == null || QuiverRowIndex == null)
                {
                    var assembly = Assembly.Load("BetterArchery");

                    if (assembly != null)
                    {
                        var type = assembly.GetTypes().First(a => a.IsClass && a.Name == "BetterArchery");
                        var pubStaticFields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                        IsQuiverEnabled = pubStaticFields.First(t => t.Name == "configQuiverEnabled");
                        QuiverRowIndex = pubStaticFields.First(t => t.Name == "QuiverRowIndex");
                    }
                }

                if (IsQuiverEnabled?.GetValue(null) is ConfigEntry<bool> config && config.Value)
                {
                    // this would also return the same thing: GetBonusInventoryRowIndex
                    if (QuiverRowIndex?.GetValue(null) is int rowIndex)
                    {
                        // it doesn't make sense for it to be the hotkey bar
                        if (rowIndex != 0)
                        {
                            if (itemPos.y == rowIndex && (checkForRestockableSlots || itemPos.x < 0 || itemPos.x > 2))
                            {
                                return true;
                            }

                            // for some reason 'Better Archery' adds two entire rows and doesn't even use this one (probably for backwards compatibility)
                            if (itemPos.y == rowIndex - 1)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}