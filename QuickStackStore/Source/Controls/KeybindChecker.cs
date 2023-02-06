using BepInEx.Configuration;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    [HarmonyPatch]
    public static class KeybindChecker
    {
        public static bool IgnoreKeyPresses()
        {
            // removed InventoryGui.IsVisible() because we specifically want to allow that
            return IgnoreKeyPressesDueToPlayer(Player.m_localPlayer)
                || !ZNetScene.instance
                || Minimap.IsOpen()
                || Menu.IsVisible()
                || Console.IsVisible()
                || StoreGui.IsVisible()
                || TextInput.IsVisible()
                || (Chat.instance && Chat.instance.HasFocus())
                || (ZNet.instance && ZNet.instance.InPasswordDialog())
                || (TextViewer.instance && TextViewer.instance.IsVisible());
        }

        private static bool IgnoreKeyPressesDueToPlayer(Player player)
        {
            return !player
                || player.InCutscene()
                || player.IsTeleporting()
                || player.IsDead()
                || player.InPlaceMode();
        }

        // thank you to 'Margmas' for giving me this snippet from VNEI https://github.com/MSchmoecker/VNEI/blob/master/VNEI/Logic/BepInExExtensions.cs#L21
        // since KeyboardShortcut.IsPressed and KeyboardShortcut.IsDown behave unintuitively
        public static bool IsKeyDown(KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKeyDown(shortcut.MainKey) && shortcut.Modifiers.All(Input.GetKey);
        }

        public static bool IsKeyHeld(KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKey(shortcut.MainKey) && shortcut.Modifiers.All(Input.GetKey);
        }

        [HarmonyPatch(typeof(Player))]
        public static class Player_Update_Patch
        {
            [HarmonyPatch(nameof(Player.Update)), HarmonyPostfix]
            public static void Postfix_Patch(Player __instance)
            {
                if (Player.m_localPlayer != __instance)
                {
                    return;
                }

                if (IgnoreKeyPresses())
                {
                    return;
                }

                if (ZInput.IsGamepadActive() && ControllerConfig.UseHardcodedControllerSupport.Value)
                {
                    HandleControllerKeys(__instance);
                }
                else
                {
                    HandleGenericKeys(__instance);
                }
            }

            private static void HandleGenericKeys(Player player)
            {
                if (GeneralConfig.OverrideKeybindBehavior.Value == OverrideKeybindBehavior.DisableAllNewHotkeys)
                {
                    return;
                }

                if (IsKeyDown(QuickStackConfig.QuickStackKeybind.Value))
                {
                    QuickStackModule.DoQuickStack(player);
                }
                else if (IsKeyDown(RestockConfig.RestockKeybind.Value))
                {
                    RestockModule.DoRestock(player);
                }

                if (!InventoryGui.IsVisible())
                {
                    return;
                }

                if (IsKeyDown(SortConfig.SortKeybind.Value))
                {
                    SortModule.DoSort(player);
                }
                else if (IsKeyDown(TrashConfig.QuickTrashKeybind.Value))
                {
                    TrashModule.AttemptQuickTrash();
                }
                else if (IsKeyDown(TrashConfig.TrashKeybind.Value))
                {
                    TrashModule.TrashOrTrashFlagItem(true);
                }
                else if (IsKeyDown(StoreTakeAllConfig.TakeAllKeybind.Value))
                {
                    StoreTakeAllModule.DoTakeAllWithKeybind(player);
                }
                else if (IsKeyDown(StoreTakeAllConfig.StoreAllKeybind.Value))
                {
                    StoreTakeAllModule.DoStoreAllWithKeybind(player);
                }
            }

            private static void HandleControllerKeys(Player player)
            {
                if (!InventoryGui.IsVisible())
                {
                    return;
                }

                if (ZInput.GetButtonDown(joyGetButtonDownPrefix + joySort))
                {
                    SortModule.DoSort(player);
                }
                else if (ZInput.GetButtonDown(joyGetButtonDownPrefix + joyStoreAll))
                {
                    StoreTakeAllModule.DoStoreAllWithKeybind(player);
                }

                if (ControllerConfig.ControllerDPadUsageInInventoryGrid.Value == DPadUsage.InventorySlotMovement)
                {
                    return;
                }

                if (ControllerConfig.ControllerDPadUsageInInventoryGrid.Value == DPadUsage.KeybindsWhileHoldingModifierKey
                    && !IsKeyHeld(ControllerConfig.ControllerDPadUsageModifierKeybind.Value))
                {
                    return;
                }

                if (ZInput.GetButtonDown(joyGetButtonDownPrefix + joyQuickStack))
                {
                    QuickStackModule.DoQuickStack(player);
                }
                else if (ZInput.GetButtonDown(joyGetButtonDownPrefix + joyRestock))
                {
                    RestockModule.DoRestock(player);
                }
                else if (ZInput.GetButtonDown(joyGetButtonDownPrefix + joyFavoriteToggling))
                {
                    FavoritingMode.ToggleFavoriteToggling();
                }
                else if (ZInput.GetButtonDown(joyGetButtonDownPrefix + joyTrash))
                {
                    TrashModule.TrashOrTrashFlagItem();
                }
            }
        }

        internal const string joyTranslationPrefix = "KEY_";
        internal const string joyGetButtonDownPrefix = "Joy";
        internal const string joyQuickStack = "DPadDown";
        internal const string joyRestock = "DPadUp";
        internal const string joySort = "Back";
        internal const string joyStoreAll = "RStick";
        internal const string joyFavoriteToggling = "DPadLeft";
        internal const string joyTrash = "DPadRight";
    }
}