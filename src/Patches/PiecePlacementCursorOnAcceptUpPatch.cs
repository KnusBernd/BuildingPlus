using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildingPlus.Selection;
using HarmonyLib;

namespace BuildingPlus.Patches
{
    internal class PiecePlacementCursorOnAcceptUpPatch
    {
        public static void ApplyPatch(Harmony harmony)
        {
            var onAcceptUpMethod = AccessTools.Method(typeof(PiecePlacementCursor), "OnAcceptUp");
            if (onAcceptUpMethod != null)
            {
                var onAcceptUpPrefix = AccessTools.Method(typeof(PiecePlacementCursorOnAcceptUpPatch), nameof(OnAcceptUpPrefix));
                harmony.Patch(onAcceptUpMethod,
                    prefix: new HarmonyMethod(onAcceptUpPrefix));
                BuildingPlusPlugin.LogInfo("Patched PiecePlacementCursor.OnAcceptUp with prefix and postfix successfully");
            }
            else
            {
                BuildingPlusPlugin.LogError("Failed to find PiecePlacementCursor.OnAcceptUp");
            }
        }

        private static bool OnAcceptUpPrefix(PiecePlacementCursor __instance)
        {
            if (LobbyManager.instance.AllLocal)
                return Selector.Instance.OnAcceptUp();
            return true;
        }
    }
}
