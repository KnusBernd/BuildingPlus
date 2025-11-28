using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildingPlus.Selection;
using HarmonyLib;

namespace BuildingPlus.Patches
{
    internal class PiecePlacementCursorOnSprintDownPatch
    {
        public static void ApplyPatch(Harmony harmony)
        {
            var onSprintDownMethod = AccessTools.Method(typeof(PiecePlacementCursor), "OnSprintDown");
            if (onSprintDownMethod != null)
            {
                var onSprintDownPrefix = AccessTools.Method(typeof(PiecePlacementCursorOnSprintDownPatch), nameof(OnSprintDownPrefix));
                harmony.Patch(onSprintDownMethod, prefix: new HarmonyMethod(onSprintDownPrefix));
            }
            else
            {
                BuildingPlusPlugin.LogError("Failed to find PiecePlacementCursor.OnSprintUp");
            }
        }

        private static bool OnSprintDownPrefix(PiecePlacementCursor __instance)
        {
            if (LobbyManager.instance.AllLocal)
                return Selector.Instance.OnSprintDown();
            return true;
        }
    }
}
