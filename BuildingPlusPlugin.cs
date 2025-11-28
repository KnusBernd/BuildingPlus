using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using BuildingPlus.Patches;
using BuildingPlus.UI;
using HarmonyLib;
using UnityEngine;

namespace BuildingPlus
{

    [BepInPlugin("BuildingPlus", "BuildingPlus", "0.0.1")]
    public class BuildingPlusPlugin : BaseUnityPlugin
    {

        internal static BuildingPlusPlugin Instance { get; private set; }
        internal static ManualLogSource LoggerInstance => Instance.Logger;
        private ColorPickerDialog dialog;

        public ColorPickerDialog Dialog => dialog;

        private Harmony harmony;

        private void Awake()
        {
            Instance = this;

            BuildingPlusConfig.BindConfig(Config);

            // Initialize Harmony
            harmony = new Harmony("BuildingPlus");

            GameControlPatch.ApplyPatch(harmony);
            InventoryBookPatch.ApplyPatch(harmony);
            PickableCustomizationButtonPatch.ApplyPatch(harmony);
            PiecePlacementCursorHandleEventPatch.ApplyPatch(harmony);
            PiecePlacementCursorOnAcceptDownPatch.ApplyPatch(harmony);
            PiecePlacementCursorOnAcceptUpPatch.ApplyPatch(harmony);
            PiecePlacementCursorOnSprintDownPatch.ApplyPatch(harmony);
            PiecePlacementCursorOnSprintUpPatch.ApplyPatch(harmony);
            PiecePlacementCursorStartPatch.ApplyPatch(harmony);
            PiecePlacementCursorPlacePieceDeferredPatch.ApplyPatch(harmony);
            
            //PlaceablePlacePatch.ApplyPatch(harmony);
            LogInfo("Plugin loaded.");

        }
        void Start()
        {
            dialog = new ColorPickerDialog();
        }

        private void OnGUI()
        {
            dialog.Draw();
        }

        private void OnColorPicked(Color pickedColor)
        {
            throw new NotImplementedException();
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
