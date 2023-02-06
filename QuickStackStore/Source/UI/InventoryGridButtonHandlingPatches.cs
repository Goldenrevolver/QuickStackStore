using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    [HarmonyPatch(typeof(InventoryGrid))]
    internal class InventoryGridButtonHandlingPatches
    {
        [HarmonyPatch(nameof(InventoryGrid.OnRightClick)), HarmonyPrefix]
        internal static bool OnRightClick(InventoryGrid __instance, UIInputHandler element)
        {
            return HandleClick(__instance, element, false);
        }

        [HarmonyPatch(nameof(InventoryGrid.OnLeftClick)), HarmonyPrefix]
        internal static bool OnLeftClick(InventoryGrid __instance, UIInputHandler clickHandler)
        {
            return HandleClick(__instance, clickHandler, true);
        }

        internal static bool HandleClick(InventoryGrid __instance, UIInputHandler clickHandler, bool isLeftClick)
        {
            Vector2i buttonPos = __instance.GetButtonPos(clickHandler.gameObject);

            return HandleClick(__instance, buttonPos, isLeftClick);
        }

        [HarmonyPatch(nameof(InventoryGrid.UpdateGamepad))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                var list = instructions.ToList();

                var info = typeof(InventoryGridButtonHandlingPatches).GetMethod(nameof(GetButtonDownWithConfig));

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].opcode == OpCodes.Call && list[i].operand.ToString().ToLower().Contains("getbuttondown"))
                    {
                        list[i] = new CodeInstruction(OpCodes.Call, info);
                    }
                }

                return list;
            }
            catch (Exception e)
            {
                Helper.LogO($"There was an exception while transpiling {e}\n{e.StackTrace}");
                return instructions;
            }
        }

        [HarmonyPatch(nameof(InventoryGrid.UpdateGamepad))]
        private static void Postfix(InventoryGrid __instance)
        {
            if (__instance != InventoryGui.instance.m_playerGrid)
            {
                return;
            }

            if (!__instance.m_uiGroup.IsActive())
            {
                return;
            }

            if (ZInput.GetButtonDown("JoyButtonA"))
            {
                HandleClick(__instance, __instance.m_selected, true);
            }
            else if (ZInput.GetButtonDown("JoyButtonX"))
            {
                HandleClick(__instance, __instance.m_selected, false);
            }
        }

        public static bool GetButtonDownWithConfig(string key)
        {
            bool pressed = ZInput.GetButtonDown(key);

            if (key == "JoyButtonA" || key == "JoyButtonX")
            {
                // we don't know which grid we are right here, so make some educated guessed on
                // whether we should dispose of this button press or not (based on HandleClick).
                // afterwards the actual player grid can check for the key in postfix and handle it
                var playerGrid = InventoryGui.instance.m_playerGrid;

                if (!playerGrid.m_uiGroup.IsActive())
                {
                    return pressed;
                }

                if (Player.m_localPlayer.IsTeleporting())
                {
                    return pressed;
                }

                if (InventoryGui.instance.m_dragGo)
                {
                    return pressed;
                }

                if (!FavoritingMode.IsInFavoritingMode())
                {
                    return pressed;
                }

                return false;
            }

            if (!key.ToLower().Contains("dpad"))
            {
                return pressed;
            }

            switch (ControllerConfig.ControllerDPadUsageInInventoryGrid.Value)
            {
                case DPadUsage.InventorySlotMovement:
                    return pressed;

                case DPadUsage.KeybindsWhileHoldingModifierKey:
                    return pressed && !KeybindChecker.IsKeyHeld(ControllerConfig.ControllerDPadUsageModifierKeybind.Value);

                case DPadUsage.Keybinds:
                default:
                    return false;
            }
        }

        internal static bool HandleClick(InventoryGrid __instance, Vector2i buttonPos, bool isLeftClick)
        {
            if (InventoryGui.instance.m_playerGrid != __instance)
            {
                return true;
            }

            Player localPlayer = Player.m_localPlayer;

            if (localPlayer.IsTeleporting())
            {
                return true;
            }

            if (InventoryGui.instance.m_dragGo)
            {
                return true;
            }

            if (!FavoritingMode.IsInFavoritingMode())
            {
                return true;
            }

            if (buttonPos == new Vector2i(-1, -1))
            {
                return true;
            }

            if (!isLeftClick)
            {
                UserConfig.GetPlayerConfig(localPlayer.GetPlayerID()).ToggleSlotFavoriting(buttonPos);
            }
            else
            {
                ItemDrop.ItemData itemAt = __instance.m_inventory.GetItemAt(buttonPos.x, buttonPos.y);

                if (itemAt == null)
                {
                    return true;
                }

                bool wasToggleSuccessful = UserConfig.GetPlayerConfig(localPlayer.GetPlayerID()).ToggleItemNameFavoriting(itemAt.m_shared);

                if (!wasToggleSuccessful)
                {
                    localPlayer.Message(MessageHud.MessageType.Center, LocalizationConfig.GetRelevantTranslation(LocalizationConfig.CantFavoriteTrashFlaggedItemWarning, nameof(LocalizationConfig.CantFavoriteTrashFlaggedItemWarning)), 0, null);
                }
            }

            return false;
        }
    }
}