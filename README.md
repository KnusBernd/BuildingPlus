# BuildingPlus
Enhanced building tools for Ultimate Chicken Horse, featuring multi-piece manipulation, customizable colors, removal of building restrictions, and more!

## Multi-Piece Cursor
In Singleplayer Free Play sessions, the cursor becomes a Multi-Piece Cursor, allowing you to:

- Marquee Select multiple placeables at once.
- Pick up any placeable in the selection to move the entire group.
- Copy the selected placeables.
- Delete all placeables in the selection.
- Multi-Select: Hold the configurable key (default: Left Control) to select multiple placeables without deselecting others, or to select/deselect individual pieces.

## Free Color Choice
While in the inventory, hold the configurable key (default: Left Shift) and click on a “Pick a Color” block to open a dialog and choose any color for your placeables.

## Faster Placement & Level Loading
- Placeables are now placed much faster, allowing rapid placement of many pieces at once.
- Levels load faster due to the optimized placement process.

> **Note:** This feature overlaps with the `LevelLoaderOptimization` mod. If you have BuildingPlus installed, you can safely uninstall `LevelLoaderOptimization`, though the mod still works as a standalone if desired.

## Configuration Options

### Placement Restrictions
- IgnorePlacementRules – Disable all building restrictions.
- IgnoreBounds – Ignore placement boundaries for all placeables.

### Control Keys
- Selection Key – Hold to multi-select without deselecting.
- Color Change Key – Click with this key to open the color picker dialog.

### Selection Unlock Delay
- Configurable delay (in seconds) before selections can be picked up after placing, preventing accidental early pickups during detachment.

## Known Issues
- Quickly picking up a placed selection at different placeables can cause crashes if the detachment or any inner transform progress has not yet finished. (No fix found; the unlock delay is just a workaround.)
- Sometimes the selector gets locked with an active selection when entering the inventory.
- Attached Pieces such as Wires and Glue wont get copied.
- Copying sometimes breaks the Selector logic.