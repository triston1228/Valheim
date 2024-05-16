using System;
using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers.Common
{
    [HarmonyPatch]
    public static class ReversePatches
    {

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(InventoryGui), "DoCrafting")]
        public static void DoCraftingOriginal(InventoryGui __instance, Player player)
        {
            // This will be replaced by Harmony with the original method
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Inventory), "CountItems", typeof(string), typeof(int), typeof(bool))]
        public static int CountItemsOriginal(Inventory instance, string name, int quality, bool matchWorldLevel)
        {
            // This will be filled by Harmony
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Inventory), "RemoveItem", typeof(string), typeof(int), typeof(int), typeof(bool))]
        public static void RemoveItemOriginal(Inventory instance, string name, int amount, int itemQuality, bool worldLevelBased)
        {
            // This will be filled by Harmony
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Container), "CheckAccess", typeof(long))]
        public static bool CheckAccess(this Container instance, long playerID)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Container), "Save")]
        public static void Save(this Container instance)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Inventory), "Changed")]
        public static void Changed(this Inventory instance)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Inventory), "HaveItem", typeof(string), typeof(bool))]
        public static bool HaveItemOriginal(this Inventory __instance, string name, bool matchWorldLevel)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(InventoryGui), "UpdateCraftingPanel", typeof(bool))]
        public static void UpdateCraftingPanel(this InventoryGui instance, bool focus)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Terminal), "AddString", typeof(string), typeof(string), typeof(Talker.Type), typeof(bool))]
        public static void AddString(this Terminal instance, string user, string text, Talker.Type type, bool timestamp = false)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Smelter), "GetFuel")]
        public static float GetFuel(this Smelter instance)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Smelter), "FindCookableItem", typeof(Inventory))]
        public static ItemDrop.ItemData FindCookableItem(this Smelter instance, Inventory inventory)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Smelter), "IsItemAllowed", typeof(string))]
        public static bool IsItemAllowed(this Smelter instance, string itemName)
        {
            throw new NotImplementedException("stub");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Smelter), "GetQueueSize")]
        public static int GetQueueSize(this Smelter instance)
        {
            throw new NotImplementedException("");
        }
    }
}
