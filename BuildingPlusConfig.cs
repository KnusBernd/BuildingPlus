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

        private static readonly Dictionary<string, ConfigEntry<string>> ButtonColorEntries = new Dictionary<string, ConfigEntry<string>>();
        public static void BindConfig(ConfigFile config)
        {
            BuildingPlusConfig.config = config;

            IgnorePlacementRules = config.Bind(
                "BuildingPlus",
                "IgnorePlacementRules",
                false,
                "If true, all placeables ignore placement rules."
            );

            IgnoreBounds = config.Bind(
                "BuildingPlus",
                "IgnoreBounds",
                false,
                "If true, all placeables ignore bounds checks."
            );
            SelectionUnlockDelay = config.Bind(
                "BuildingPlus",
                "SelectionUnlockDelay",
                0f,
                "Time (in seconds) after placing a picked up Placeable before interaction is re-enabled. Acts as a rework to wait for the detachment process to finish. If you experience crashes increase this value slightly."
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
