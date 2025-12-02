using System;
using BuildingPlus.Selection;
using HarmonyLib;
using UnityEngine;

namespace BuildingPlus.Patches
{
    internal class PiecePlacementCursorFixedUpdatePatch
    {
        private static AccessTools.FieldRef<PiecePlacementCursor, bool> f_tryingToCancel;
        private static AccessTools.FieldRef<PiecePlacementCursor, bool> f_placementPhysicsLock;
        private static AccessTools.FieldRef<PiecePlacementCursor, Vector3> f_heldPositionOffset;

        public static void ApplyPatch(Harmony harmony)
        {
            var original = AccessTools.Method(typeof(PiecePlacementCursor), "FixedUpdate");

            if (original == null)
            {
                BuildingPlusPlugin.LogError("PiecePlacementCursor.FixedUpdate not found; patch failed.");
                return;
            }

            var postfix = AccessTools.Method(
                typeof(PiecePlacementCursorFixedUpdatePatch),
                nameof(FixedUpdatePostfix)
            );

            harmony.Patch(original, postfix: new HarmonyMethod(postfix));

            f_tryingToCancel = AccessTools.FieldRefAccess<PiecePlacementCursor, bool>("tryingToCancel");
            f_placementPhysicsLock = AccessTools.FieldRefAccess<PiecePlacementCursor, bool>("placementPhysicsLock");
            f_heldPositionOffset = AccessTools.FieldRefAccess<PiecePlacementCursor, Vector3>("heldPositionOffset");

        }

        public static void FixedUpdatePostfix(PiecePlacementCursor __instance)
        {
            if (__instance.Piece == null ||
                f_tryingToCancel(__instance) ||
                f_placementPhysicsLock(__instance) ||
                __instance.WaitingForPlaceMessageResponse || 
                Selector.Instance == null || 
                !LobbyManager.instance.AllLocal)
            {
                return;
            }

            if (!Input.GetKey(BuildingPlusConfig.FreePlacementKey.Value) || __instance.Piece == null)
                return;

            Vector2 heldOffset = f_heldPositionOffset(__instance);
            Vector3 cursorPos = __instance.transform.position + (Vector3)heldOffset;
            Vector2 newPos = new Vector2(cursorPos.x, cursorPos.y);

            __instance.Piece.transform.position = newPos;
        }
    }
}
