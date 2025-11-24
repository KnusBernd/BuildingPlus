using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildingPlus.Selection;
using GameEvent;
using HarmonyLib;
using UnityEngine;

namespace BuildingPlus.Patches
{
    internal class PiecePlacementCursorHandleEventPatch
    {

        public static void ApplyPatch(Harmony harmony)
        {
            // Get the original method
            var method = AccessTools.Method(typeof(PiecePlacementCursor), "handleEvent", new Type[] { typeof(global::GameEvent.GameEvent) });
            if (method != null)
            {
                var postfix = AccessTools.Method(typeof(PiecePlacementCursorHandleEventPatch), nameof(HandleEventPostfix));

                // Apply only postfix; prefix is no longer needed
                harmony.Patch(method,
                    postfix: new HarmonyMethod(postfix));

                BuildingPlusPlugin.LogInfo("Patched PiecePlacementCursor.handleEvent with postfix successfully");
            }
            else
            {
                BuildingPlusPlugin.LogError("Failed to find PiecePlacementCursor.handleEvent");
            }
        }

        private static void HandleEventPostfix(PiecePlacementCursor __instance, global::GameEvent.GameEvent e)
        {

            if (Selector.Instance == null)
            {
                return;
            }

            if (e.GetType() != typeof(PickBlockEvent))
            {
                return;
            }

            PickBlockEvent pickBlockEvent = (PickBlockEvent)e;
            var cursor = Selector.Instance.Cursor;

            if (cursor == null)
            {
                BuildingPlusPlugin.LogError("Selector cursor was null!");
                return;
            }

            int cursorPlayer = cursor.AssociatedGamePlayer.networkNumber;

            BuildingPlusPlugin.LogInfo(
                $"PickBlockEvent received. PlayerNumber={pickBlockEvent.PlayerNumber}, CursorPlayer={cursorPlayer}"
            );

            if (pickBlockEvent.PlayerNumber != cursorPlayer)
            {
                BuildingPlusPlugin.LogInfo("PickBlockEvent player does not match cursor player. Ignoring.");
                return;
            }

            if (pickBlockEvent.PickablePiece == null)
            {
                BuildingPlusPlugin.LogWarning("PickBlockEvent.PickablePiece was null. Ignoring.");
                return;
            }

            var selection = Selector.Instance.Selection;
            if (selection == null)
            {
                BuildingPlusPlugin.LogError("Selector.Instance.Selection was null!");
                return;
            }

            var selected = selection.GetSelectedPlaceables();
            if (!selected.Contains(pickBlockEvent.ReuseTransformPlaceable))
            {
                BuildingPlusPlugin.LogInfo("Picked piece not in selected placeables. Ignoring.");
                return;
            }

            BuildingPlusPlugin.LogInfo("Valid pick — starting CopyPaste coroutine.");
            BuildingPlusPlugin.Instance.StartCoroutine(CopyPaste());
        }

        private static IEnumerator CopyPaste()
        {
            var cursor = Selector.Instance.Cursor;
            var selection = Selector.Instance.Selection;

            // 1. Copy pieces relative to cursor
            var newSel = selection.CopySelectedPlaceablesRelativeTo(cursor.Piece);

            if (newSel == null || newSel.Count == 0) yield break;

            // 2. Clear old selection
            selection.DeselectAll();

            // 3. Set picked-up list and head
            selection.GetPickedUpPlaceables().Clear();
            selection.GetPickedUpPlaceables().AddRange(newSel);
            selection.Head = newSel[0]; // new head

            // 4. Highlight new pieces
            foreach (var p in newSel)
            {
                var highlight = p.GetComponent<SelectionHighlight>();
                if (!highlight) highlight = p.gameObject.AddComponent<SelectionHighlight>();
                highlight.Show();
            }

            yield return null;
        }
    }
}
