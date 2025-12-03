using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BuildingPlus.Selection;
using HarmonyLib;
using UnityEngine;

namespace BuildingPlus.Patches
{
    internal class PiecePlacementCursorOnAcceptDownPatch
    {
        private static bool isProcessingPlacement = false;
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
            // Block selection events during placement processing
            if (isProcessingPlacement)
                return false;

            if (LobbyManager.instance.AllLocal)
                return Selector.Instance.OnAcceptDown();
            return true;
        }

        private static void OnAcceptDownPostfix(PiecePlacementCursor __instance)
        {
            SelectionManager selection = Selector.Instance.Selection;
            if (selection.Head != null)
            {
                isProcessingPlacement = true; // Block selection events
                Selector.Instance.Lock();

                // Ensure all picked up pieces get placed
                var pickedUpPlaceables = selection.GetPickedUpPlaceables().ToList();
                foreach (var p in pickedUpPlaceables)
                {
                    if (p != null && p.gameObject != null)
                    {
                        p.Place(0);
                    }
                }

                BuildingPlusPlugin.Instance.StartCoroutine(WaitForPlaceablesPlaced());
            }
        }

        private static IEnumerator WaitForPlaceablesPlaced()
        {
            SelectionManager selection = Selector.Instance.Selection;
            var newPlaceables = selection.GetPickedUpPlaceables().ToList();

            // Safety check: ensure we have placeables to wait for
            if (newPlaceables == null || newPlaceables.Count == 0)
            {
                isProcessingPlacement = false; // Re-enable selection
                Selector.Instance.Unlock();
                yield break;
            }

            // Remove any null entries before waiting
            newPlaceables.RemoveAll(p => p == null || p.gameObject == null);

            if (newPlaceables.Count == 0)
            {
                isProcessingPlacement = false; // Re-enable selection
                Selector.Instance.Unlock();
                yield break;
            }

            // Wait until all placeables are placed
            float timeout = 5f;
            float elapsed = 0f;
            yield return new WaitUntil(() =>
            {
                elapsed += Time.deltaTime;
                if (elapsed >= timeout)
                {
                    BuildingPlusPlugin.LogWarning("[WaitForPlaceablesPlaced] Timeout reached. Proceeding anyway.");
                    return true;
                }

                foreach (var p in newPlaceables)
                {
                    if (p == null || p.gameObject == null)
                        continue;
                    if (!p.Placed)
                        return false;
                }
                return true;
            });

            // Delay before detaching
            yield return new WaitForSeconds(
                BuildingPlusConfig.SelectionDetachmentDelay.Value
            );

            // Clear ALL selections before detaching to prevent interference
            selection.DeselectAll();

            // Detach all pieces
            selection.Drop();

            // Wait a frame to ensure deselection is complete
            yield return null;

            // Re-select ONLY the newly placed pieces
            foreach (var p in newPlaceables)
            {
                if (p != null && p.gameObject != null && p.Placed)
                {
                    selection.Select(p);
                }
            }

            // Delay before unlocking
            yield return new WaitForSeconds(
                BuildingPlusConfig.SelectionUnlockDelay.Value
            );

            isProcessingPlacement = false; // Re-enable selection events
            Selector.Instance.Unlock();
        }
    }
}