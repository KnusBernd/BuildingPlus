using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildingPlus.Selection;
using HarmonyLib;

namespace BuildingPlus.Patches
{
    internal class PiecePlacementCursorOnSprintUpPatch
    {
        public static void ApplyPatch(Harmony harmony)
        {
            var onSprintUpMethod = AccessTools.Method(typeof(PiecePlacementCursor), "OnSprintUp");
            if (onSprintUpMethod != null)
            {
                var onSprintUpPrefix = AccessTools.Method(typeof(PiecePlacementCursorOnSprintUpPatch), nameof(OnSprintUpPrefix));
                harmony.Patch(onSprintUpMethod, prefix: new HarmonyMethod(onSprintUpPrefix));
                BuildingPlusPlugin.LogInfo("Patched PiecePlacementCursor.OnSprintUp successfully");
            }
            else
            {
                BuildingPlusPlugin.LogError("Failed to find PiecePlacementCursor.OnSprintUp");
            }
        }

        private static bool OnSprintUpPrefix(PiecePlacementCursor __instance)
        {
            if (LobbyManager.instance.AllLocal)
                return Selector.Instance.OnSprintUp();
            else return true;
        }
    }
}
