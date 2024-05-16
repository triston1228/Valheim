using System;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Common;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ABearCodes.Valheim.CraftingWithContainers.UI
{
        [HarmonyPatch(typeof(InventoryGui), "SetupRequirement", typeof(Transform), typeof(Piece.Requirement), typeof(Player), typeof(bool), typeof(int))]
        [HarmonyPrefix]
        private static bool SetupRequirementTotalItemsIndicatorPatch(Transform elementRoot, Piece.Requirement req, Player player, bool craft, int quality, ref bool __result)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value || !Plugin.Settings.ModifyItemCountIndicator.Value)
                return true;

            if (elementRoot == null || req == null || player == null) return true;

            var iconImage = elementRoot.transform.Find("res_icon")?.GetComponent<Image>();
            var nameText = elementRoot.transform.Find("res_name")?.GetComponent<TMP_Text>();
            var amountText = elementRoot.transform.Find("res_amount")?.GetComponent<TMP_Text>();
            var tooltip = elementRoot.GetComponent<UITooltip>();

            if (iconImage == null || nameText == null || amountText == null || tooltip == null || req.m_resItem == null) return true;

            iconImage.gameObject.SetActive(true);
            nameText.gameObject.SetActive(true);
            amountText.gameObject.SetActive(true);

            var itemData = req.m_resItem.m_itemData;
            iconImage.sprite = itemData.GetIcon();
            iconImage.color = Color.white;
            tooltip.m_text = Localization.instance.Localize(itemData.m_shared.m_name);
            nameText.text = Localization.instance.Localize(itemData.m_shared.m_name);

            // Get the combined item count using the patched CountItems method
            var totalItemCount = player.GetInventory().CountItems(itemData.m_shared.m_name, quality, craft);

            // Log the total item count
            Plugin.Log.LogInfo($"Total item count for {itemData.m_shared.m_name}: {totalItemCount}");

            // Get the amount required from Piece.Requirement
            var amountRequired = req.GetAmount(quality);

            // Log amount required
            Plugin.Log.LogInfo($"Amount required for {itemData.m_shared.m_name}: {amountRequired}");

            if (amountRequired <= 0)
            {
                InventoryGui.HideRequirement(elementRoot);
                __result = false;
                return false;
            }

            // Update the amount text with total item count and amount required
            amountText.text = string.Format($"{totalItemCount}/{amountRequired}");
            amountText.color = totalItemCount < amountRequired ? (Mathf.Sin(Time.time * 10f) > 0.0 ? Color.red : Color.white) : Color.white;

            __result = true;
            return false;


        }

    }
}
