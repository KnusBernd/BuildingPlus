# BuildingPlus
**Enhanced building tools for Ultimate Chicken Horse**, featuring multi-piece manipulation, customizable colors, removal of building restrictions, and more!

## Multi-Piece Cursor
In Singleplayer Free Play sessions, the cursor becomes a **Multi-Piece Cursor**, allowing you to:

- **Marquee Select** multiple placeables at once.
- **Pick up** any placeable in the selection to move the entire group.
- **Copy** the selected placeables.
- **Delete** all placeables in the selection.
- **Multi-Select:** Hold the configurable key **(default: Left Control)** to select multiple placeables without deselecting others, or to select/deselect individual pieces.

## Free Color Choice
While in the inventory, hold the configurable key **(default: Left Shift)** and click on a **“Pick a Color” block** to open a dialog and choose any color for your placeables.

## 2D Camera Controller
BuildingPlus now adds a **customizable 2D Camera Controller** for Free Play.

### Features
- **Middle Mouse Drag:** Click and drag the middle mouse button to pan the camera smoothly.
- **Edge Scrolling:** Move the mouse to the screen edges to scroll the camera.
- **Zooming:** Use the mouse scroll wheel in combination with the configurable key **(Left Alt)** to zoom in and out.
- **Double-Click Reset:** Double-click the middle mouse button to reset the camera to its **original position**.
- **Configurable Settings:** Drag speed, edge scroll speed, zoom sensitivity, and min/max zoom levels are all configurable via the plugin's config file.

> **Note:** Holding **Shift** while dragging increases drag speed by **50%**.

### Usage
- Enable the custom 2D camera in the config with **EnableCustomCamera**.
- Press the configurable toggle key **(default: F8)** to toggle between the 2D camera controller and the default one at runtime.

## Faster Placement & Level Loading
- Placeables are now **placed much faster**, allowing rapid placement of many pieces at once.
- Levels **load faster** due to the optimized placement process.

> **Note:** This overlaps with the `LevelLoaderOptimization` mod. You can safely uninstall that mod if using BuildingPlus.

## Configuration Options

### Placement Restrictions
- **IgnorePlacementRules** – Disable all building restrictions.
- **IgnoreBounds** – Ignore placement boundaries for all placeables.
- **BypassLevelFullness** – Ignore level fullness limits.
- **SelectionUnlockDelay** – Delay (in seconds) before a placed selection can be interacted with again.

### Control Keys
- **ControlSelectionKey** – Hold to multi-select without deselecting.
- **ColorPickDialogKey** – Open the color picker dialog.
- **FreePlacementKey** – Hold to freely move placeables without grid snapping.
- **ToggleCameraKey** – Toggle between the custom and the default camera controller (default: F8).

### Camera Settings
- **EnableCustomCamera** – Toggle whether the 2D camera should be added as an Option to the Camera.
- **CameraDragSpeed** – Adjust the speed of camera panning when dragging.
- **CameraEdgeScrollSpeed** – Adjust the speed when moving the camera near screen edges.
- **CameraZoomSensitivity** – Adjust how fast the camera zooms.
- **CameraMinFOV** – Minimum camera zoom.
- **CameraMaxFOV** – Maximum camera zoom.

## Installation / Setup
BuildingPlus is a **BepInEx plugin**, so you must have BepInEx installed for *Ultimate Chicken Horse*.

### 1. Install BepInEx (if you haven’t already)
1. Download **BepInEx 5.x** for your system from [this link](https://github.com/bepinex/bepinex/releases).
2. Extract into your **Ultimate Chicken Horse** game folder.

### 2. Install BuildingPlus
1. Download the latest release from [this link](https://github.com/KnusBernd/BuildingPlus/releases/) or build it yourself.
2. Place the .dll file into: `Ultimate Chicken Horse/BepInEx/plugins/`
3. Start the game — BepInEx will load the plugin automatically.

## Known Issues
- Quickly picking up a placed selection at different placeables can cause crashes if detachment is still processing.
- Sometimes the selector gets locked with an active selection when entering the inventory.
- Attached pieces such as Wires and Glue won't get copied.
- Copying sometimes breaks the Selector logic.
- Copying sometimes creates highlight duplicates.
