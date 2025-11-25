using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace BuildingPlus.Patches
{
    internal class PickableCustomizationButtonPatch
    {
        public static void ApplyPatch(Harmony harmony)
        {
            var original = AccessTools.Method(typeof(PickableCustomizationButton), "OnAccept");

            if (original != null)
            {
                var prefix = AccessTools.Method(typeof(PickableCustomizationButtonPatch),
                                                nameof(OnAcceptPrefix));

                harmony.Patch(original, prefix: new HarmonyMethod(prefix));

                BuildingPlusPlugin.LogInfo("Patched PickableCustomizationButton.OnAccept successfully");
            }
            else
            {
                BuildingPlusPlugin.LogError("Failed to find PickableCustomizationButton.OnAccept");
            }
        }

        public static bool OnAcceptPrefix(PickableCustomizationButton __instance, PickCursor pickCursor)
        {
            if (Input.GetKey(KeyCode.LeftShift) && __instance.customizationType == CustomizationType.BlockColors) 
            {
                SpriteRenderer r = __instance.GetComponentInChildren<SpriteRenderer>();

                BuildingPlusPlugin.Instance.Dialog.Show(r.color, (accepted, color) =>
                {
                    if (accepted)
                    {
                        r.color = color;
                        BuildingPlusConfig.SaveButtonColor(__instance.name, color);
                        Debug.Log("Picked color: " + color);
                    }
                    else
                        Debug.Log("User canceled");
                });
                return false;
            }

            return true; 
        }

    }
}
