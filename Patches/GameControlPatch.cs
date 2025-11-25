using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildingPlus.Selection;
using HarmonyLib;
using UnityEngine;

namespace BuildingPlus.Patches
{
    internal class GameControlPatch
    {
        public static void ApplyPatch(Harmony harmony)
        {
            var original = AccessTools.Method(typeof(FreePlayControl), "SetupStart", new Type[] { typeof(GameState.GameMode) });
            if (original == null)
            {
                BuildingPlusPlugin.LogError("Failed to find GameControl.SetupStart");
                return;
            }

            var postfix = AccessTools.Method(typeof(GameControlPatch), nameof(SetupStartPostfix));
            harmony.Patch(original, postfix: new HarmonyMethod(postfix));
            BuildingPlusPlugin.LogInfo("Patched GameControl.SetupStart successfully");

            var onDestroy = AccessTools.Method(typeof(GameControl), "OnDestroy");

            if (onDestroy == null)
            {
                BuildingPlusPlugin.LogError("GameControl.OnDestroy not found; destroy patch skipped.");
                return;
            }

            postfix = AccessTools.Method(typeof(GameControlPatch), nameof(OnDestroyPostfix));
            harmony.Patch(onDestroy, postfix: new HarmonyMethod(postfix));

            BuildingPlusPlugin.LogInfo("Patched GameControl.OnDestroy successfully");

        }

        public static void SetupStartPostfix(GameControl __instance, GameState.GameMode mode)
        {
            BuildingPlusPlugin.LogInfo($"SetupStart called with mode {mode}");
            if (!(__instance is FreePlayControl)) return;

            if (__instance != null && __instance.gameObject != null)
            {
                if (__instance.gameObject.GetComponent<Selector>() == null)
                {
                    __instance.gameObject.AddComponent<Selector>();
                    BuildingPlusPlugin.LogInfo("Selector added.");
                }
            }
            foreach (Placeable p in Placeable.AllPlaceables)
            {
                p.IgnorePlacementRules = BuildingPlusConfig.IgnorePlacementRules.Value;
                p.IgnoreBounds = BuildingPlusConfig.IgnoreBounds.Value;
            }
            foreach (GameObject g in PlaceableMetadataList.Instance.allBlockPrefabs)
            {
                Placeable p = g.GetComponent<Placeable>();
                p.IgnorePlacementRules = BuildingPlusConfig.IgnorePlacementRules.Value;
                p.IgnoreBounds = BuildingPlusConfig.IgnoreBounds.Value;
            }
            foreach (GameObject g in PlaceableMetadataList.Instance.extraBlocks)
            {
                Placeable p = g.GetComponent<Placeable>();
                p.IgnorePlacementRules = BuildingPlusConfig.IgnorePlacementRules.Value;
                p.IgnoreBounds = BuildingPlusConfig.IgnoreBounds.Value;
            }
        }

        public static void OnDestroyPostfix(GameControl __instance)
        {
            BuildingPlusPlugin.LogInfo("GameControl.OnDestroy called.");

            var selector = __instance?.gameObject?.GetComponent<Selector>();
            if (selector != null)
            {
                UnityEngine.Object.Destroy(selector);
                BuildingPlusPlugin.LogInfo("Selector removed on GameControl destroy.");
            }
            foreach (GameObject g in PlaceableMetadataList.Instance.allBlockPrefabs)
            {
                Placeable p = g.GetComponent<Placeable>();
                p.IgnorePlacementRules = false;
                p.IgnoreBounds = false; 
            }
            foreach (GameObject g in PlaceableMetadataList.Instance.extraBlocks)
            {
                Placeable p = g.GetComponent<Placeable>();
                p.IgnorePlacementRules = false;
                p.IgnoreBounds = false;
            }
        }
    }
}
