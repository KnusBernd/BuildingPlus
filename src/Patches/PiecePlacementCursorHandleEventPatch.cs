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

            //BuildingPlusPlugin.LogInfo( $"PickBlockEvent received. PlayerNumber={pickBlockEvent.PlayerNumber}, CursorPlayer={cursorPlayer}");

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

            Placeable placeable = UnityEngine.Object.Instantiate(
                place.PickableBlock.placeablePrefab
            );
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

            var newSel = selection.CopySelectedPlaceablesRelativeTo(
                placeable,
                place
            );

            yield return null;
            yield return null;
            yield return null;

            cursor.SetPiece(placeable, destroyPrevious: true);
            yield return null;

            selection.GetPickedUpPlaceables().Clear();
            selection.GetPickedUpPlaceables().Add(placeable);
            selection.GetPickedUpPlaceables().AddRange(newSel);

            selection.Head = placeable;

            selection.GetOldSelectedPlaceables().Clear();
            selection.GetOldSelectedPlaceables().AddRange(selection.GetSelectedPlaceables());

            yield return new WaitForSeconds(0.1f);
            selection.DeselectAll(); 
        }
    }
}
