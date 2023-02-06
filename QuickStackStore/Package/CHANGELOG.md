# Changelog
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
## 1.0
- Initial release