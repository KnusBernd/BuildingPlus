using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using BuildingPlus.Patches;
using HarmonyLib;
using UnityEngine;

namespace BuildingPlus
{

    [BepInPlugin("BuildingPlus", "BuildingPlus", "0.0.1")]
    public class BuildingPlusPlugin : BaseUnityPlugin
    {

        internal static BuildingPlusPlugin Instance { get; private set; }
        internal static ManualLogSource LoggerInstance => Instance.Logger;
        

        private Harmony harmony;

        private void Awake()
        {
            Instance = this;

            // Initialize Harmony
            harmony = new Harmony("BuildingPlus");

            FreePlayControlPatch.ApplyPatch(harmony);
            PiecePlacementCursorOnAcceptDownPatch.ApplyPatch(harmony);
            PiecePlacementCursorOnAcceptUpPatch.ApplyPatch(harmony);
            PiecePlacementCursorOnSprintDownPatch.ApplyPatch(harmony);
            PiecePlacementCursorOnSprintUpPatch.ApplyPatch(harmony);
            PiecePlacementCursorStartPatch.ApplyPatch(harmony);
            PlaceablePlacePatch.ApplyPatch(harmony);
            LogInfo("Plugin loaded.");


        }

        public static void LogInfo(string message)
        {
            LoggerInstance.LogInfo($"[BuildingPlus] {message}");
        }

        public static void LogWarning(string message)
        {
            LoggerInstance.LogWarning($"[BuildingPlus] {message}");
        }

        public static void LogError(string message)
        {
            LoggerInstance.LogError($"[BuildingPlus] {message}");
        }

    }
}
