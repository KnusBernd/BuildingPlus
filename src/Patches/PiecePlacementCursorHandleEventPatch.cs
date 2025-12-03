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
            var method = AccessTools.Method(typeof(PiecePlacementCursor), "handleEvent", new Type[] { typeof(global::GameEvent.GameEvent) });
            if (method != null)
            {
                var prefix = AccessTools.Method(typeof(PiecePlacementCursorHandleEventPatch), nameof(HandleEventPrefix));
                harmony.Patch(method,
                    prefix: new HarmonyMethod(prefix));
            }
            else
            {
                BuildingPlusPlugin.LogError("Failed to find PiecePlacementCursor.handleEvent");
            }
        }

        private static bool HandleEventPrefix(PiecePlacementCursor __instance, global::GameEvent.GameEvent e)
        {
            if (e.GetType() != typeof(PickBlockEvent))
            {
                return true;
            }
            if (LobbyManager.instance.GetLobbyPlayers().Count() > 1)
            {
                return true;
            }
            if (Selector.Instance == null)
            {
                return true;
            }

            PickBlockEvent pickBlockEvent = (PickBlockEvent)e;
            var cursor = Selector.Instance.Cursor;
            int cursorPlayer = cursor.AssociatedGamePlayer.networkNumber;
            var selection = Selector.Instance.Selection;

            if (cursor == null)
            {
                return true;
            }

            Placeable place = cursor.hoveredPiece;

            if (!selection.GetSelectedPlaceables().Contains(place))
            {
                return true;
            }

            BuildingPlusPlugin.Instance.StartCoroutine(Copy());

            return false;
        }

        private static IEnumerator Copy()
        {
            var cursor = Selector.Instance.Cursor;
            var selection = Selector.Instance.Selection;
            Placeable place = cursor.hoveredPiece;

            // Safety check: ensure hovered piece is valid
            if (place == null || place.gameObject == null)
            {
                BuildingPlusPlugin.LogWarning("[Copy] Hovered piece is null or destroyed. Aborting copy.");
                yield break;
            }

            // Instantiate the main piece
            Placeable placeable = UnityEngine.Object.Instantiate(
                place.PickableBlock.placeablePrefab
            );

            // Safety check: ensure instantiation succeeded
            if (placeable == null)
            {
                BuildingPlusPlugin.LogError("[Copy] Failed to instantiate placeable. Aborting.");
                yield break;
            }

            placeable.GenerateIDOnPick(
                placeable.ID,
                cursor.AssociatedGamePlayer.networkNumber
            );
            placeable.SetColor(place.CustomColor);
            placeable.SetInitialDamageLevel(place.damageLevel, true);
            placeable.transform.SetPositionAndRotation(
                place.transform.position,
                place.transform.rotation
            );

            // Wait for the main placeable to be fully initialized
            yield return new WaitForEndOfFrame();

            // Copy all selected pieces relative to the new head
            var newSel = selection.CopySelectedPlaceablesRelativeTo(
                placeable,
                place
            );

            // Wait for all copied pieces to be initialized
            yield return new WaitForEndOfFrame();

            // Set the piece in the cursor
            cursor.SetPiece(placeable, destroyPrevious: true);

            // Wait for cursor to process the new piece
            yield return new WaitForEndOfFrame();

            // Update picked up placeables list
            selection.GetPickedUpPlaceables().Clear();
            selection.GetPickedUpPlaceables().Add(placeable);
            if (newSel != null && newSel.Count > 0)
            {
                selection.GetPickedUpPlaceables().AddRange(newSel);
            }
            selection.Head = placeable;

            // Store old selection for reference
            selection.GetOldSelectedPlaceables().Clear();
            selection.GetOldSelectedPlaceables().AddRange(selection.GetSelectedPlaceables());

            // Brief delay before deselecting
            yield return new WaitForSeconds(0.08f);
            selection.DeselectAll();
        }
    }
}