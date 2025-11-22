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
            LogInfo("Plugin loaded.");
            string[] shadersToCheck = new string[]
        {
            // Standard shaders
            "Standard",
            "Standard (Specular setup)",
            "Standard (Roughness setup)",
            
            // Unlit
            "Unlit/Color",
            "Unlit/Texture",
            "Unlit/Transparent",
            "Unlit/Transparent Cutout",
            
            // Sprites
            "Sprites/Default",
            "Sprites/Diffuse",
            "Sprites/Mask",
            
            // UI
            "UI/Default",
            "UI/Unlit/Transparent",
            "UI/Unlit/Text",
            
            // Particles
            "Particles/Standard Surface",
            "Particles/Standard Unlit",
            "Particles/Alpha Blended",
            "Particles/Multiply",
            "Particles/Additive",
            
            // Legacy shaders
            "Legacy Shaders/Diffuse",
            "Legacy Shaders/Transparent/Diffuse",
            "Legacy Shaders/Transparent/Cutout/Diffuse",
            "Legacy Shaders/VertexLit",
            "Legacy Shaders/Transparent/VertexLit",
            
            // Other common shaders
            "Mobile/Unlit (Supports Lightmap)",
            "Mobile/Particles/VertexLit Blended",
            "FX/Unlit/Transparent",
            "FX/Glass",
            "FX/Water",
            "FX/Flare",
            "FX/Reveal",
            "FX/Skybox",
            "FX/Fire"
        };

            Debug.Log("=== Checking shaders at runtime ===");

            foreach (string shaderName in shadersToCheck)
            {
                Shader s = Shader.Find(shaderName);
                if (s != null)
                    Debug.Log($"Shader found: {shaderName}");
                else
                    Debug.LogWarning($"Shader missing: {shaderName}");
            }
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
