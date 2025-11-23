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
    internal class PlaceablePlacePatch
    {
        public static int DebugWaitFrames = 60; 

        public static void ApplyPatch(Harmony harmony)
        {
            var method = AccessTools.Method(typeof(Placeable), "Place",
                new Type[] { typeof(int), typeof(bool), typeof(bool) });

            if (method != null)
            {
                var prefix = AccessTools.Method(typeof(PlaceablePlacePatch), nameof(PlacePrefix));
                harmony.Patch(method, prefix: new HarmonyMethod(prefix));
                BuildingPlusPlugin.LogInfo("Patched Placeable.Place successfully");
            }
            else
            {
                BuildingPlusPlugin.LogError("Failed to find Placeable.Place");
            }
        }

        public static void PlacePrefix(Placeable __instance, int playerNumber, bool sendEvent, bool force)
        {
            if (LobbyManager.instance.AllLocal)
            {
                if (__instance == null || Selector.Instance == null || Selector.Instance.Selection.Head == null || Selector.Instance.Selection.Head.ID != __instance.ID) return;
                Selector.Instance.Lock();
                BuildingPlusPlugin.Instance.StartCoroutine(WaitForPlaceablesPlaced());
            }
        }

        private static System.Collections.IEnumerator WaitForPlaceablesPlaced()
        {
            // Capture selected set as array (so it doesn’t update mid-wait)
            var selected = Selector.Instance?.Selection.GetPickedUpPlaceables().ToArray();
           // BuildingPlusPlugin.LogInfo($"[Coroutine] Waiting for {selected.Length} Placeables to become Placed...");

            // Wait until all selected pieces are placed
            yield return new UnityEngine.WaitUntil(() =>
            {
                foreach (var p in selected)
                {
                    if (!p.Placed) return false;
                }
                return true;
            });

            yield return null;
            yield return null;


            yield return new WaitForSeconds(0.1f);

            Selector.Instance.Selection.Drop();
            yield return null;
            yield return null;
            Selector.Instance.Unlock();

            //BuildingPlusPlugin.LogInfo("[Coroutine] Dropped selection.");
        }
    }
}
