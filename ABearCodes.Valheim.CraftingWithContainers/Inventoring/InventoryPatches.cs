using System.Collections.Generic;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Common;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using HarmonyLib;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Inventoring
{
    [HarmonyPatch]
    public class InventoryPatches
    {


        [HarmonyPatch(typeof(Inventory))]
        [HarmonyPatch("GetItem", typeof(string), typeof(int), typeof(bool))]
        [HarmonyBefore("randyknapp.mods.equipmentandquickslots")]
        public static bool GetItemReversed(Inventory __instance, string name, int quality, bool isPrefabName, List<ItemDrop.ItemData> ___m_inventory, ref ItemDrop.ItemData __result)
        {
            if (!Plugin.Settings.TakeItemsInReverseOrder.Value) return true;

            for (var index = ___m_inventory.Count - 1; index >= 0; index--)
            {
                var itemData = ___m_inventory[index];
                if ((isPrefabName && itemData.m_dropPrefab.name == name) || (!isPrefabName && itemData.m_shared.m_name == name))
                {
                    if (quality < 0 || quality == itemData.m_quality)
                    {
                        __result = itemData;
                        return false;
                    }
                }
            }

            __result = null;
            return false; // Skip the original method
        }

        [HarmonyPatch(typeof(Inventory), "CountItems", typeof(string), typeof(int), typeof(bool))]
        [HarmonyPostfix]
        private static void CountItemsPatch(Inventory __instance, string name, int quality, bool matchWorldLevel, ref int __result)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value ||
                !ContainerTracker.PlayerByInventoryDict.TryGetValue(__instance.GetHashCode(), out var player))
                return;

            // Call the original CountItems method
            var playerItemCount = ReversePatches.CountItemsOriginal(__instance, name, quality, matchWorldLevel);

            // Get items from nearby containers
            var containers = ContainerTracker.GetViableContainersInRangeForPlayer(player, Plugin.Settings.ContainerLookupRange.Value);
            var containerItemCount = containers.Sum(container => ReversePatches.CountItemsOriginal(container.Container.GetInventory(), name, quality, matchWorldLevel));

            // Combine player and container item counts
            __result = playerItemCount + containerItemCount;
        }

        [HarmonyPatch(typeof(Inventory))]
        [HarmonyPatch("RemoveItem", typeof(string), typeof(int), typeof(int), typeof(bool))]
        [HarmonyBefore("randyknapp.mods.equipmentandquickslots")]
        [HarmonyPrefix]
        public static bool RemoveItemPatch(Inventory __instance, string name, int amount, int itemQuality, bool worldLevelBased)
        {
            Plugin.Log.LogDebug($"Trying to remove item from {__instance.GetHashCode()}");

            if (!Plugin.Settings.CraftingWithContainersEnabled.Value ||
                !ContainerTracker.PlayerByInventoryDict.TryGetValue(__instance.GetHashCode(), out var player))
            {
                Plugin.Log.LogDebug($"Not tracked {__instance.GetHashCode()}");
                return true;
            }

            Plugin.Log.LogDebug($"player: {player.GetPlayerName()} ({player.GetInstanceID()} via {__instance.GetHashCode()})");

            var containers = ContainerTracker.GetViableContainersInRangeForPlayer(player, Plugin.Settings.ContainerLookupRange.Value);
            Plugin.Log.LogDebug($"RemoveItem got {containers.Count} containers");

            InventoryItemRemover.IterateAndRemoveItemsFromInventories(player, containers, name, amount, itemQuality, worldLevelBased, out var removalReport);

            if (Plugin.Settings.AddExtractionEffectWhenCrafting.Value)
            {
                foreach (var removal in removalReport.Removals.Where(removal => removal.TrackedContainer.HasValue))
                {
                    InventoryItemRemover.SpawnEffect(player, removal.TrackedContainer.Value);
                }
            }

            Plugin.Log.LogDebug(removalReport.GetReportString());

            if (Plugin.Settings.LogItemRemovalsToConsole.Value)
            {
                Console.instance.Print($"{removalReport.GetReportString(true)}");
            }

            __instance.Changed();

            return false;
        }

        [HarmonyPatch(typeof(Inventory))]
        [HarmonyPatch("HaveItem", typeof(string), typeof(bool))]
        [HarmonyBefore("randyknapp.mods.equipmentandquickslots")]
        [HarmonyPostfix]
        public static void HaveItemPatch(Inventory __instance, string name, bool matchWorldLevel, ref bool __result)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value
                || !ContainerTracker.PlayerByInventoryDict.TryGetValue(__instance.GetHashCode(), out var player)
                || __result)
                return;

            var containers = ContainerTracker
                .GetViableContainersInRangeForPlayer(player, Plugin.Settings.ContainerLookupRange.Value);
            var containersHaveAny = containers
                .Any(container => container.Container.GetInventory().HaveItemOriginal(name, matchWorldLevel));
            Plugin.Log.LogDebug(
                $"Player ${player.GetPlayerID()} found {containersHaveAny} of {name} via {containers.Count} containers");
            __result = containersHaveAny;
        }

       
    }

    [HarmonyPatch(typeof(Player), "ConsumeResources")]
    public static class Player_ConsumeResources_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Player __instance, Piece.Requirement[] requirements, int qualityLevel, int itemQuality = -1)
        {
            ConsumeResourcesFromInventories(__instance, requirements, qualityLevel, itemQuality);
            return false; // Skip the original method
        }

        public static void ConsumeResourcesFromInventories(Player player, Piece.Requirement[] requirements, int qualityLevel, int itemQuality)
        {
            foreach (Piece.Requirement requirement in requirements)
            {
                if (requirement.m_resItem != null)
                {
                    int amount = requirement.GetAmount(qualityLevel);
                    if (amount > 0)
                    {
                        // Try to remove items from the player's inventory first
                        int removedFromPlayer = player.GetInventory().RemoveItemAsMuchAsPossible(requirement.m_resItem.m_itemData.m_shared.m_name, amount, itemQuality, false);
                        int leftToRemove = amount - removedFromPlayer;

                        // If there are still items left to remove, try to remove them from nearby containers
                        if (leftToRemove > 0)
                        {
                            List<TrackedContainer> containers = ContainerTracker.GetViableContainersInRangeForPlayer(player, Plugin.Settings.ContainerLookupRange.Value);
                            InventoryItemRemover.IterateAndRemoveItemsFromInventories(player, containers, requirement.m_resItem.m_itemData.m_shared.m_name, leftToRemove, itemQuality, false, out var removalReport);

                            Plugin.Log.LogDebug(removalReport.GetReportString());
                        }
                    }
                }
            }
        }
    }



}
