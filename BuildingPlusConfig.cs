using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;

namespace BuildingPlus
{
    internal class BuildingPlusConfig
    {
        private static ConfigFile config;
        public static ConfigEntry<bool> IgnorePlacementRules { get; private set; }
        public static ConfigEntry<bool> IgnoreBounds { get; private set; }
        public static ConfigEntry<float> SelectionUnlockDelay { get; private set; }
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
    }
}
