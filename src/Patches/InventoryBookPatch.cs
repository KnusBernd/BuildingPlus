using System.Collections.Generic;
using BuildingPlus.Selection;
using HarmonyLib;
using UnityEngine;

namespace BuildingPlus.Patches
{
    internal class InventoryBookPatch
    {

        public static readonly Dictionary<string, Color> DefaultColors = new Dictionary<string, Color>();

        public static void ApplyPatch(Harmony harmony)
        {
            var updatePagesMethod = AccessTools.Method(typeof(InventoryBook), "UpdatePages");
            if (updatePagesMethod != null)
            {
                var updatePagesPostfix = AccessTools.Method(typeof(InventoryBookPatch), nameof(UpdatePagesPostfix));
                harmony.Patch(updatePagesMethod, postfix: new HarmonyMethod(updatePagesPostfix));
            }
            else
            {
                BuildingPlusPlugin.LogError("Failed to find InventoryBook.UpdatePages");
            }

            var hideMethod = AccessTools.Method(typeof(InventoryBook), "Hide");
            if (hideMethod != null)
            {
                var hidePostfix = AccessTools.Method(typeof(InventoryBookPatch), nameof(HidePostfix));
                harmony.Patch(hideMethod, postfix: new HarmonyMethod(hidePostfix));
            }
            else
            {
                BuildingPlusPlugin.LogError("Failed to find InventoryBook.Hide");
            }
        }

        private static void UpdatePagesPostfix(InventoryBook __instance, int newPage)
        {
            if (__instance.MainMenuBook || Selector.Instance == null) return;

            BuildingPlusPlugin.Instance.Dialog.Hide(false);
            if (newPage == 0)
            {
                UpdateColors(__instance);
            }
        }

        private static void HidePostfix(InventoryBook __instance)
        {
            if (__instance.MainMenuBook || Selector.Instance == null) return;
            BuildingPlusPlugin.Instance.Dialog.Hide(false);
        }

        private static void UpdateColors(InventoryBook __instance)
        {
            if (__instance.InventoryPages == null || __instance.InventoryPages.Length == 0)
            {
                BuildingPlusPlugin.LogWarning("UpdateColors: InventoryPages is null or empty.");
                return;
            }
            Transform page = __instance.InventoryPages[0].transform;
            Transform buttonsTransform = null;

            for (int i = 0; i < page.childCount; i++)
            {
                if (page.GetChild(i).name == "GameObject")
                {
                    buttonsTransform = page.GetChild(i);
                    break;
                }
            }

            if (buttonsTransform == null)
            {
                return;
            }

            for (int i = 0; i < buttonsTransform.childCount; i++)
            {
                PickableCustomizationButton button = buttonsTransform.GetChild(i).GetComponent<PickableCustomizationButton>();
                if (button != null)
                {
                    DefaultColors[button.name] = button.sprite.color;

                    Color savedColor = BuildingPlusConfig.LoadButtonColor(button.name, button.sprite.color);
                    button.sprite.color = savedColor;
                }
                else
                {
                    BuildingPlusPlugin.LogWarning($"UpdateColors: Child at index {i} does not have a PickableCustomizationButton component.");
                }
            }
        }
    }
}
