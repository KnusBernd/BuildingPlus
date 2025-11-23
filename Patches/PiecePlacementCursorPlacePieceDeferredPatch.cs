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
    internal class PiecePlacementCursorPlacePieceDeferredPatch
    {
        public static int DebugWaitFrames = 60;   // default: wait 30 frames before Drop()
        public static void ApplyPatch(Harmony harmony)
        {
            var original = AccessTools.Method(
                typeof(PiecePlacementCursor),
                "PlacePieceDeferred",
                new System.Type[] { typeof(MsgPiecePlaced), typeof(Placeable), typeof(bool) }
            );

            if (original != null)
            {

                var postfix = AccessTools.Method(
                    typeof(PiecePlacementCursorPlacePieceDeferredPatch),
                    nameof(PlacePieceDeferredPostfix)
                );

                harmony.Patch(original,
                   postfix: new HarmonyMethod(postfix)
                );

                BuildingPlusPlugin.LogInfo("Patched PiecePlacementCursor.PlacePieceDeferred with Prefix + Postfix");
            }
            else
            {
                BuildingPlusPlugin.LogError("Failed to find PiecePlacementCursor.PlacePieceDeferred");
            }
        }


        private static void PlacePieceDeferredPostfix(
         PiecePlacementCursor __instance,
         MsgPiecePlaced placeMsg,
         Placeable piece,
         bool pieceWasPickedUp
)
        {
           // BuildingPlusPlugin.LogInfo("[Postfix] PlacePieceDeferred called");

            if (piece == null)
                return;

            var sel = Selector.Instance?.Selection;
            if (sel == null)
                return;

            if (sel.Head == null)
                return;

            bool headPlaced = piece.ID == sel.Head.ID;

            if (!headPlaced)
                return;
            Selector.Instance.Lock();
            BuildingPlusPlugin.Instance.StartCoroutine(WaitForPlaceablesPlaced());
            //BuildingPlusPlugin.LogInfo("Dropping Selection");

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

            yield return new WaitForSeconds(0.2f); // idk maybe do something different but this work for me.

            Selector.Instance.Selection.Drop();
            yield return null;
            yield return null;
            Selector.Instance.Unlock();

           // BuildingPlusPlugin.LogInfo("[Coroutine] Dropped and unlocked selection.");
        }
    }
}
