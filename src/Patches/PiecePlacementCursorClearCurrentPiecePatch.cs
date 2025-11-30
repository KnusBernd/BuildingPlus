using System;
using HarmonyLib;
using BuildingPlus.Selection;
using UnityEngine;

namespace BuildingPlus.Patches
{
    internal class PiecePlacementCursorClearCurrentPiecePatch
    {
        public static void ApplyPatch(Harmony harmony)
        {
            var method = AccessTools.Method(typeof(PiecePlacementCursor), "ClearCurrentPiece");

            if (method != null)
            {
                var prefix = AccessTools.Method(typeof(PiecePlacementCursorClearCurrentPiecePatch), nameof(ClearPrefix));
                var postfix = AccessTools.Method(typeof(PiecePlacementCursorClearCurrentPiecePatch), nameof(ClearPostfix));

                harmony.Patch(method,
                    prefix: new HarmonyMethod(prefix),
                    postfix: new HarmonyMethod(postfix)
                );
            }
            else
            {
                BuildingPlusPlugin.LogError("Failed to find PiecePlacementCursor.ClearCurrentPiece");
            }
        }

        public static void ClearPrefix(PiecePlacementCursor __instance)
        {
            if (LobbyManager.instance.AllLocal && Selector.Instance != null)
            {
                Selector.Instance.Unlock();
            }
        }

        public static void ClearPostfix(PiecePlacementCursor __instance)
        {
            if (LobbyManager.instance.AllLocal && Selector.Instance != null)
            {
                if (Selector.Instance.Selection.GetSelectedPlaceables().Count == 0) 
                { 
                    foreach (var p in Selector.Instance.Selection.GetOldSelectedPlaceables()) 
                    { 
                        Selector.Instance.Selection.Select(p);
                    }
                    Selector.Instance.Selection.GetOldSelectedPlaceables().Clear();
                }
            }
        }
    }
}
