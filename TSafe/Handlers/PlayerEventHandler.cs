using System.Threading.Tasks;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tavstal.TSafe.Components;
using Tavstal.TSafe.Managers;
using Tavstal.TSafe.Models;

namespace Tavstal.TSafe.Handlers
{
    public static class PlayerEventHandler
    {
        private static bool _isAttached;

        public static void AttachEvents()
        {
            if (_isAttached)
                return;

            _isAttached = true;

            U.Events.OnPlayerConnected += OnPlayerConnected;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            UnturnedPlayerEvents.OnPlayerInventoryResized += OnPlayerInventoryResized;
        }

        public static void DetachEvents()
        {
            if (!_isAttached)
                return;

            _isAttached = false;

            U.Events.OnPlayerConnected -= OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            UnturnedPlayerEvents.OnPlayerInventoryResized -= OnPlayerInventoryResized;
        }

        private static void OnPlayerInventoryResized(UnturnedPlayer player, InventoryGroup inventoryGroup, byte o, byte u)
        {
            if (inventoryGroup != InventoryGroup.Storage)
                return;

            SafeComponent comp = player.GetComponent<SafeComponent>();
            if (player.Inventory.storage == null && comp.isSafeOpened)
            {
                comp.isSafeOpened = false;
                if (!VaultManager.VaultList.TryGetValue(comp.VaultId, out UnturnedVault unturnedVault))
                    return;
                Task.Run(async () =>
                {
                    Vault vault = await TSafe.DatabaseManager.FindVaultAsync(comp.VaultId);
                    if (vault == null)
                        return;

                    await TSafe.DatabaseManager.RemoveVaultItemsAsync(comp.VaultId);
                    InteractableStorage storage = (InteractableStorage)unturnedVault.StorageDrop.interactable;
                    await TSafe.DatabaseManager.AddVaultItemAsync(comp.VaultId,storage.items.items);
                });
                UnturnedChat.Say("Closed safe.");
            }
            else
            {
                UnturnedChat.Say("Did not close safe.");
            }
        }

        private static void OnPlayerConnected(UnturnedPlayer player)
        {
            Task.Run(async () =>
            {
                foreach (var vault in await TSafe.DatabaseManager.GetVaultsAsync(player.CSteamID.m_SteamID))
                {
                    VaultManager.PreventVaultDestroy(vault.Id);
                }
            });
        }
        
        private static void OnPlayerDisconnected(UnturnedPlayer player)
        {
            Task.Run(async () =>
            {
                foreach (var vault in await TSafe.DatabaseManager.GetVaultsAsync(player.CSteamID.m_SteamID))
                {
                    VaultManager.RequestVaultDestroy(vault.Id);
                }
            });
        }
    }
}
