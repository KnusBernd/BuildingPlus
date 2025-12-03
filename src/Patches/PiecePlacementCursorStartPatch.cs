using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildingPlus.Selection;
using HarmonyLib;
using UnityEngine;

namespace BuildingPlus.Patches
{
    internal class PiecePlacementCursorStartPatch
    {

        public static void ApplyPatch(Harmony harmony)
        {
            var startMethod = AccessTools.Method(typeof(PiecePlacementCursor), "Start", Type.EmptyTypes);
            if (startMethod != null)
            {
                var startPostfix = AccessTools.Method(typeof(PiecePlacementCursorStartPatch), nameof(StartPostfix));
                harmony.Patch(startMethod, postfix: new HarmonyMethod(startPostfix));
            }
            else
            {
                BuildingPlusPlugin.LogError("Failed to find PiecePlacementCursor.Start");
            }

        }

        public static void StartPostfix(PiecePlacementCursor __instance)
        {
            BuildingPlusPlugin.Instance.StartCoroutine(WaitForPlayer(__instance));
        }

        private static IEnumerator WaitForPlayer(PiecePlacementCursor cursor)
        {
            int maxWaitFrames = 300;
            int waitFrames = 0;

            while (cursor.AssociatedGamePlayer == null)
            {
                yield return null;
                if (++waitFrames > maxWaitFrames) yield break;
            }

            if (cursor.AssociatedGamePlayer.isLocalPlayer)
            {
                if (!LobbyManager.instance.AllLocal) yield return null;

                waitFrames = 0;
                while (Selector.Instance == null)
                {
                    yield return null;
                    if (++waitFrames > maxWaitFrames) yield break;
                }

                Selector.Instance.Cursor = cursor;
                //BuildingPlusPlugin.LogInfo("Cursor found of player " + cursor.AssociatedGamePlayer.playerName);
            }
        }
    }
}
