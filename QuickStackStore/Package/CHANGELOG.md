# Changelog

## 1.4.13
- Fixed crossplay multiplayer not working (updated ServerSync)
## 1.4.12
- Fixed trash can no longer working with the newest public release 0.220.3
- Added dynamic placement support for the mini buttons when using the newest version of Currency Pocket by Azumatt
- Added new option 'PreventAutoPickupOfTrashFlaggedItems', disabled by default, added by request by Y4NN1CK
## 1.4.11
- Fixed compatibility with ComfyQuickSlots 1.7.0
## 1.4.10
- Update for current public release 0.219.14
## 1.4.9
- Added support for Dynamic Storage Piles mod to allow quick stacking the restricted item into a pile even when it's empty
- Added Ukrainian translation
## 1.4.8
- Fixed compatibility support for Better Archery
## 1.4.7
- Made sure that when the favoriting borders are spawned no child elements of it are copied. This is the same fix AzuAutoStore applied and once Recycle_N_Reclaim also applies it, it fixes favoriting borders not disappearing from items when unfavoriting.
## 1.4.6
- Fixed a visual glitch with the favoriting borders when using the mod Recycle_N_Reclaim (thanks for the heads up, Azu)
## 1.4.5
- Fixed critical bug when using Adventure Backpacks with 'AllowAreaStackingToNonPhysicalContainers' which deleted other players if you did an area stack/restock close to them (possible since version 1.4.0)
- 'AllowAreaStackingToNonPhysicalContainers' is no longer required to detect Item Drawers
- Added Korean translation, provided by hanawa07
- Added Italian translation, provided by Marcuz08
## 1.4.4
- Fixed trash can not reacting to clicks when using both AzuEPI 1.3 and Jewelcrafting
- Replaced controller key check with a more modern version for better compatibility
- Reenabled controller key hints that were disabled in 1.3.9 as their base game performance has since been improved
## 1.4.3
- Added full compatibility for AzuEPI 1.3 using its API (thank you, Azu!)
- Fixed backwards compatibility for AzuEPI 1.2 due to config type change
## 1.4.2
- Update for current public release 0.217.24
- Added option to hide the base game 'Place Stacks' button (enabled by default)
- Added option to override the behavior of quick stacking by holding the open button on a chest to use the modded quick stacking logic (enabled by default)
- Fixed layouting problems with the base game 'Place Stacks' button, if it's not disabled
## 1.4.1
- Update for current public release 0.217.14 (thank you to JuniorAndria for the pull request on github)
## 1.4.0
- Update for current public release 0.216.9 (thank you to Mistermycelium for the pull request on github)
- Added config option to exclude non player built containers (like dungeon chests) from area quick stacking/ restocking (enabled by default)
- Added config option to exclude containers without a physical build piece (like backpacks) from area quick stacking/ restocking (disabled by default)
- Edited the display config option for every inventory + container button pair (quick stack, restock, sort) to allow disabling both of them (I thought I added this already)
- Fixed the sort inventory button not respecting the BothButDependingOnContext config setting
- Updated simplified chinese translation, provided by taotao
- Made sure controller key hints don't even get spawned, instead of immediately getting disabled, to not trigger another base game visual bug (which affects other mods that spawn buttons too)
## 1.3.10
- Fixed controller key hints for the trash button not being completely disabled, causing a small visual bug (independent of whether you used a controller or not)
## 1.3.9
- Recompiled for newest version of BepInEx. Temporarily disabled the new controller key hints completely, as they caused a significant fps drop when opening the inventory
## 1.3.8
- Rebuilt for 0.214.2, adapted to new controller key hints
## 1.3.7
- Fixed an error and a warning when using Auga (still only supported using keybinds)
## 1.3.6
- Fixed repeated log messages about adding a canvas component multiple times
## 1.3.5
- Fixed area quick stacking potentially causing issues when affecting a container at the edge of the loading range due to a very high range setting
- Added Swedish translation
- Updated Polish translation
## 1.3.4
- Added compatibility for AzuExtendedPlayerInventory
- All trashing features are now disabled when using RecyclePlus
- The ingame config menu now changes the display of settings that are overridden for compatibility reasons
- Updated BR portuguese translation
## 1.3.3
- Full controller support (including keybind hints and favoriting)
- Fixed ServerSync messing up button layout
- Fixed trash hotkey doing quick trash and vice versa (introduced in 1.3.1)
- Added French, BR Portuguese and Polish translation
- Added config template feature
- Added 'Take All' and 'Store All' keybind
## 1.3.2
- Non public testing version of 1.3.3
## 1.3.1
- Multi User Chest support (requires MUC 0.4)
- Area quick stacking and restocking in multiplayer can now be disabled when not using MUC (default). Ships are excluded from area quick stacking in multiplayer without MUC
- Area quick stacking and restocking buttons are now hidden when their range is set to zero, or when disabled by the above config setting
- Added ServerSync for area quick stacking and restocking config values (it's not required for everyone to use the mod)
- You can now set custom restocking stack size limits (individual configs for ammo, consumables and general items)
- Converted all KeyCode configs to KeyboardShortcut configs (including backwards compatibility), fixed default restock keybind
- Improved translation system to read json files (or internal files as a fallback)
- Fixed loading translations too early (better compatibility)
- Added Russian translation by Opik7
- The 'Take All' button changes no longer break certain custom containers (like jewelry bags)
- The new container buttons are now hidden when a custom container is open that is not supported (like jewelry bags)
- Improved start up performance by 300%
## 1.3.0
- Non public testing version of 1.3.1
## 1.2.5
- Stacks of items with custom data can no longer get merged by sorting or get restocked to improve compatibility. Overhauled translation system, added chinese translation by Tiomer. Option to override the Take All button label
## 1.2.4
- Fixed quick stacking sometimes not starting since version 1.2, because the game thinks there is a held item when there isn't. "Quick stack only one item" feature disabled until better implementation is found
## 1.2.3
- Increased performance for quickstacking when using other mods that listen for Inventory.Changed calls (by reducing the amount of calls, related: xkcd.com/1691)
## 1.2.2
- Reverted 1.2.1 due to logic error
## 1.2.0
- Replaced favoriting mode hotkey with favoriting mode UI button (disabled by default, just like the old hotkey)
- Added way to quick stack only one item by dragging it onto the mini quick stack button
- Fixed compatibility with SmarterContainers and Backpacks by excluding my changes to the 'Take All' Button, and added config option to do this regardless of mod setup for compatibility
- When disabling quick slots in EAQS the mini buttons will now revert to being horizontal rather than vertical
- Sorting stack merging now respects quality of modded items (base game stackables don't have quality)
- Lessened likelihood of issue with the trash can UI button when using Better UI Reforged and changing display configs while ingame (the proper fix will be included in Better UI Reforged)
- Flagged 'Multi User Chest' as incompatible until they add compatibility for this mod (which we are working on)
## 1.1.1
- Fixed trash button not being clickable for some people. Added config options for auto sorting and favorite toggling
## 1.1.0
- Compatibility for Better Archery, even better compatibility for other equipment and quick slot mods (see compatibility section), new config options for debugging
## 1.0.3
- Fixed config option for quick stacking trophies together
## 1.0.2
- Sorting now respects ComfyQuickSlots. QuickStack and Restock hotkeys no longer require the inventory to be open
## 1.0.1
- Fixed sort merging leaving empty slots after removing stacks. Renamed 3 config options that were double negatives
## 1.0.0
- Initial release