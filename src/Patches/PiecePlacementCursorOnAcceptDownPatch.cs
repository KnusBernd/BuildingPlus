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
    internal class PiecePlacementCursorOnAcceptDownPatch
    {

        public static void ApplyPatch(Harmony harmony)
        {
            var onAcceptDownMethod = AccessTools.Method(typeof(PiecePlacementCursor), "OnAcceptDown");
            if (onAcceptDownMethod != null)
            {
                var prefix = AccessTools.Method(typeof(PiecePlacementCursorOnAcceptDownPatch), nameof(OnAcceptDownPrefix));
                var postfix = AccessTools.Method(typeof(PiecePlacementCursorOnAcceptDownPatch), nameof(OnAcceptDownPostfix));

                harmony.Patch(onAcceptDownMethod,
                    prefix: new HarmonyMethod(prefix),
                    postfix: new HarmonyMethod(postfix));

            }
            else
            {
                BuildingPlusPlugin.LogError("Failed to find PiecePlacementCursor.OnAcceptDown");
            }
        }

        private static bool OnAcceptDownPrefix(PiecePlacementCursor __instance)
        {
            if (LobbyManager.instance.AllLocal)
                return Selector.Instance.OnAcceptDown();
            return true;
        }

        private static void OnAcceptDownPostfix(PiecePlacementCursor __instance)
        {
            SelectionManager selection = Selector.Instance.Selection;
            //BuildingPlusPlugin.LogInfo("postfix accept down ");
            if (selection.Head != null) 
            {
                //BuildingPlusPlugin.LogInfo("postfix cleaning up drop");
                Selector.Instance.Lock();
                // just make sure each child gets placed
                foreach (var p in selection.GetPickedUpPlaceables()) 
                {
                    p.Place(0);
                }
                BuildingPlusPlugin.Instance.StartCoroutine(WaitForPlaceablesPlaced());
            }
        }

        private static System.Collections.IEnumerator WaitForPlaceablesPlaced()
        {
            // Capture selected set as array (so it doesn’t update mid-wait)
            SelectionManager selection = Selector.Instance.Selection;
            var selected = Selector.Instance?.Selection.GetPickedUpPlaceables().ToArray();
            // BuildingPlusPlugin.LogInfo($"[Coroutine] Waiting for {selected.Length} Placeables to become Placed...");
            yield return new UnityEngine.WaitUntil(() =>
            {
                foreach (var p in selected)
                {
                    if (!p.Placed) return false;
                }
                return true;
            });

            yield return new WaitForSeconds(BuildingPlusConfig.SelectionDetachmentDelay.Value);
            List<Placeable> placed = new List<Placeable>(selected);
            placed.Add(selection.Head);
            selection.Drop();
            foreach (var p in placed)
            {
                selection.Select(p);
            }
            yield return new WaitForSeconds(BuildingPlusConfig.SelectionUnlockDelay.Value);
            Selector.Instance.Unlock();
        }
    }
}
