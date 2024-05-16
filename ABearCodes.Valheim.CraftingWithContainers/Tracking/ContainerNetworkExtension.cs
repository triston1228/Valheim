using System;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Common;
using UnityEngine;
using static Mono.Security.X509.X520;

namespace ABearCodes.Valheim.CraftingWithContainers.Tracking
{
    public partial class ContainerNetworkExtension
    {
        private readonly Container _container;
        private readonly ZNetView _zNetView;

        public ContainerNetworkExtension(Container container, ZNetView zNetView)
        {
            _container = container;
            _zNetView = zNetView;
        }

        public void Register()
        {
            Unregister();
            _zNetView.Register<long, string, int>("RemoveItemRequest", RPC_RemoveItemRequest);
        }
        public void RequestItemRemoval(long playerId, string itemName, int amount)
        {
            Plugin.Log.LogDebug($"+RemoveItemRequest: {playerId}:{itemName}:{amount}");
            _zNetView.InvokeRPC("RemoveItemRequest", playerId, itemName, amount);
        }

        private void RPC_RemoveItemRequest(long uid, long playerId, string itemName, int amount)
        {
            Plugin.Log.LogDebug($"Player {uid} wants to remove item {itemName} ({amount}) from container {_zNetView.GetZDO().m_uid.ID}");

            if (!_zNetView.IsOwner())
            {
                Plugin.Log.LogDebug("  but I'm not the owner");
            }
            else
            {
                Plugin.Log.LogDebug($"Removing {amount} of {itemName} from requested container");
                var inventory = _container.GetInventory();

                // Use the RemoveItemOriginal method with appropriate parameters
                ReversePatches.RemoveItemOriginal(inventory, itemName, amount, -1, true);

                inventory.Changed();
                _container.Save();
            }
        }

        public void Unregister()
        {
            _zNetView.Unregister("RemoveItemRequest");
            _zNetView.Unregister("RemoveItemResponse");
        }
    }
}
