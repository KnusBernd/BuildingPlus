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
    internal class FreePlayControlPatch
    {
        public static void ApplyPatch(Harmony harmony)
        {
            var original = AccessTools.Method(typeof(FreePlayControl), "SetupStart", new Type[] { typeof(GameState.GameMode) });
            if (original == null)
            {
                BuildingPlusPlugin.LogError("Failed to find FreePlayControl.SetupStart");
                return;
            }

            var postfix = AccessTools.Method(typeof(FreePlayControlPatch), nameof(SetupStartPostfix));
            harmony.Patch(original, postfix: new HarmonyMethod(postfix));
            BuildingPlusPlugin.LogInfo("Patched FreePlayControl.SetupStart successfully");
        }

        public static void SetupStartPostfix(FreePlayControl __instance, GameState.GameMode mode)
        {
            BuildingPlusPlugin.LogInfo($"SetupStart called with mode {mode}");

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
                { //p.IgnorePlacementRules  = true; p.IgnoreBounds = true;
                }
            }
            foreach (GameObject g in PlaceableMetadataList.Instance.allBlockPrefabs)
            {
                Placeable p = g.GetComponent<Placeable>();
                //p.IgnorePlacementRules = true; p.IgnoreBounds = true; 
            }
        }
    }
}
