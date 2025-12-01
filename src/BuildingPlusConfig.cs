using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;
using UnityEngine;

namespace BuildingPlus
{
    internal class BuildingPlusConfig
    {
        private static ConfigFile config;
        public static ConfigEntry<bool> IgnorePlacementRules { get; private set; }
        public static ConfigEntry<bool> IgnoreBounds { get; private set; }
        public static ConfigEntry<float> SelectionUnlockDelay { get; private set; }
        public static ConfigEntry<KeyCode> ControlSelectionKey { get; private set; }
        public static ConfigEntry<KeyCode> ColorPickDialogKey { get; private set; }
        public static ConfigEntry<KeyCode> FreePlacementKey { get; private set; }
        public static ConfigEntry<bool> EnableCustomCamera { get; private set; }
        public static ConfigEntry<KeyCode> ToggleCameraKey { get; private set; }
        public static ConfigEntry<bool> BypassLevelFullness { get; private set; }

        private static readonly Dictionary<string, ConfigEntry<string>> ButtonColorEntries = new Dictionary<string, ConfigEntry<string>>();

        public static void BindConfig(ConfigFile config)
        {
            BuildingPlusConfig.config = config;

            IgnorePlacementRules = config.Bind(
                "Placement",
                "IgnorePlacementRules",
                false,
                "If true, all placeables ignore placement rules."
            );

            IgnoreBounds = config.Bind(
                "Placement",
                "IgnoreBounds",
                false,
                "If true, all placeables ignore bounds checks."
            );

            BypassLevelFullness = config.Bind(
                "Placement",
                "BypassLevelFullness",
                false,
                "If true, level fullness limits will be ignored."
            );

            SelectionUnlockDelay = config.Bind(
                "Placement",
                "SelectionUnlockDelay",
                0f,
                "Time (in seconds) after placing a picked-up Placeable before interaction is re-enabled."
            );
            ControlSelectionKey = config.Bind(
                "Controls",
                "ControlSelectionKey",
                KeyCode.LeftControl,
                "Hold this key to select multiple placeables without deselecting others."
            );

            ColorPickDialogKey = config.Bind(
                "Controls",
                "ColorPickDialogKey",
                KeyCode.LeftShift,
                "Press this key to open the color pick dialog for the selected placeable."
            );

            FreePlacementKey = config.Bind(
                "Controls",
                "FreePlacementKey",
                KeyCode.LeftAlt,
                "Hold this key to freely move placeables without grid snapping."
            );

            EnableCustomCamera = config.Bind(
                "Camera",
                "EnableCustomCamera",
                true,
                "If true, the custom 2D camera controller is enabled."
            );

            ToggleCameraKey = config.Bind(
                "Camera",
                "ToggleCameraKey",
                KeyCode.F8,
                "Press this key to toggle between normal camera and custom 2D camera controller."
            );
        }

        public static void SaveButtonColor(string buttonName, Color color)
        {
            string value = $"{color.r},{color.g},{color.b}";

            if (!ButtonColorEntries.TryGetValue(buttonName, out var entry))
            {
                entry = config.Bind("ButtonColors", buttonName, value, "Saved RGB color for customization button");
                ButtonColorEntries[buttonName] = entry;
            }
            else
            {
                entry.Value = value;
            }
        }

        public static Color LoadButtonColor(string buttonName, Color defaultColor)
        {
            if (!ButtonColorEntries.TryGetValue(buttonName, out var entry))
            {
                entry = config.Bind("ButtonColors", buttonName, $"{defaultColor.r},{defaultColor.g},{defaultColor.b}", "Saved RGB color for customization button");
                ButtonColorEntries[buttonName] = entry;
            }

            string[] parts = entry.Value.Split(',');
            if (parts.Length == 3 &&
                float.TryParse(parts[0], out float r) &&
                float.TryParse(parts[1], out float g) &&
                float.TryParse(parts[2], out float b))
            {
                return new Color(r, g, b);
            }

            // fallback to default
            return defaultColor;
        }
    }
}
