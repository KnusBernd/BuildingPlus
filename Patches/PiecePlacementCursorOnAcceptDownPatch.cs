using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildingPlus.Selection;
using HarmonyLib;

namespace BuildingPlus.Patches
{
    internal class PiecePlacementCursorOnAcceptDownPatch
    {

        public static void ApplyPatch(Harmony harmony)
        {
            var onAcceptDownMethod = AccessTools.Method(typeof(PiecePlacementCursor), "OnAcceptDown");
            if (onAcceptDownMethod != null)
            {
                var prefix = AccessTools.Method(typeof(PiecePlacementCursorOnAcceptDownPatch), nameof(OnAcceptDownPrefix));
                var postfix = AccessTools.Method(typeof(PiecePlacementCursorOnAcceptDownPatch), nameof(OnAcceptDownPostfix));

                harmony.Patch(onAcceptDownMethod,
                    prefix: new HarmonyMethod(prefix),
                    postfix: new HarmonyMethod(postfix));

                BuildingPlusPlugin.LogInfo("Patched PiecePlacementCursor.OnAcceptDown successfully");
            }
            else
            {
                BuildingPlusPlugin.LogError("Failed to find PiecePlacementCursor.OnAcceptDown");
            }
        }

        private static bool OnAcceptDownPrefix(PiecePlacementCursor __instance)
        {
            if (LobbyManager.instance.AllLocal)
                return Selector.Instance.OnAcceptDown();
            return true;
        }

        private static void OnAcceptDownPostfix(PiecePlacementCursor __instance)
        {
            BuildingPlusPlugin.LogInfo("PiecePlacementCursor.OnAcceptDown postfix fired");
            SelectionManager selection = Selector.Instance.Selection;
            BuildingPlusPlugin.LogInfo("selection: " + selection.GetSelectedPlaceables().Count);
            BuildingPlusPlugin.LogInfo("picked up: " + selection.GetPickedUpPlaceables().Count);
        }

    }
}
