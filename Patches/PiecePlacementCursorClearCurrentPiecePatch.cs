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
                harmony.Patch(method, prefix: new HarmonyMethod(prefix));
                BuildingPlusPlugin.LogInfo("Patched PiecePlacementCursor.ClearCurrentPiece successfully");
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
    }
}
