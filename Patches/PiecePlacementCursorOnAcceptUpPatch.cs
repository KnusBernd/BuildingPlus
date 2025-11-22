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
                var onAcceptUpPostfix = AccessTools.Method(typeof(PiecePlacementCursorOnAcceptUpPatch), nameof(OnAcceptUpPostfix));
                harmony.Patch(onAcceptUpMethod,
                    prefix: new HarmonyMethod(onAcceptUpPrefix),
                    postfix: new HarmonyMethod(onAcceptUpPostfix));
                BuildingPlusPlugin.LogInfo("Patched PiecePlacementCursor.OnAcceptUp with prefix and postfix successfully");
            }
            else
            {
                BuildingPlusPlugin.LogError("Failed to find PiecePlacementCursor.OnAcceptUp");
            }
        }

        private static bool OnAcceptUpPrefix(PiecePlacementCursor __instance)
        {
            return Selector.Instance.OnAcceptUp();
        }

        private static void OnAcceptUpPostfix(PiecePlacementCursor __instance)
        {
        }
    }
}
