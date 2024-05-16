using System;
using System.Collections.Generic;
using ABearCodes.Valheim.CraftingWithContainers.Common;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using HarmonyLib;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Inventoring
{
    public static class InventoryUtils
    {
        /// <summary>
        ///     Removes as much as possible from an inventory and returns the amount left to remove
        /// </summary>
        /// <returns>amount of items taken</returns>
        public static int RemoveItemAsMuchAsPossible(this Inventory inventory, string name, int requestedAmount, int itemQuality, bool worldLevelBased)
        {
            var currentInventoryCount = ReversePatches.CountItemsOriginal(inventory, name, itemQuality, worldLevelBased);
            var itemsToTake = currentInventoryCount < requestedAmount ? currentInventoryCount : requestedAmount;

            Plugin.Log.LogDebug($"Attempting to remove {itemsToTake} of {name} from inventory {inventory.GetHashCode()}");

            if (Plugin.Settings.TakeItemsInReverseOrder.Value)
            {
                RemoveItemReversed(inventory, name, itemsToTake, itemQuality, worldLevelBased);
            }
            else
            {
                ReversePatches.RemoveItemOriginal(inventory, name, itemsToTake, itemQuality, worldLevelBased);
            }

            Plugin.Log.LogDebug($"Removed {itemsToTake} of {name} from inventory {inventory.GetHashCode()}");
            return itemsToTake;
        }

        public static void RemoveItemReversed(Inventory inventory, string name, int amount, int itemQuality, bool worldLevelBased)
        {
            Plugin.Log.LogDebug($"Reversed item removal for {name} ({amount}) triggered");
            var m_inventory = (List<ItemDrop.ItemData>)AccessTools.Field(typeof(Inventory), "m_inventory").GetValue(inventory);

            for (var index = m_inventory.Count - 1; index >= 0; index--)
            {
                var itemData = m_inventory[index];
                if (itemData.m_shared.m_name == name && (itemQuality < 0 || itemData.m_quality == itemQuality) && (!worldLevelBased || itemData.m_worldLevel >= Game.m_worldLevel))
                {
                    var num = Mathf.Min(itemData.m_stack, amount);
                    itemData.m_stack -= num;
                    amount -= num;
                    Plugin.Log.LogDebug($"Removed {num} of {name}, remaining to remove: {amount}");
                    if (amount <= 0)
                        break;
                }
            }
            m_inventory.RemoveAll(x => x.m_stack <= 0);
            ReversePatches.Changed(inventory);
        }




    }


}
