using System.Threading.Tasks;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tavstal.TSafe.Utils.Managers;

namespace Tavstal.TSafe.Utils.Handlers
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
            Provider.onCommenceShutdown+= OnShutdown;
        }
        
        public static void DetachEvents()
        {
            if (!_isAttached)
                return;

            _isAttached = false;

            U.Events.OnPlayerConnected -= OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            Provider.onCommenceShutdown -= OnShutdown;
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
        
        private static void OnShutdown()
        {
            TSafe.IsShuttingDown = true;
            foreach (var vault in VaultManager.VaultList)
            {
                VaultManager.PreventVaultDestroy(vault.Key);
                InteractableStorage storage = (InteractableStorage)vault.Value.StorageDrop.interactable;
                Task.Run(async () =>
                {
                    await TSafe.DatabaseManager.RemoveVaultItemsAsync(vault.Key);
                    await TSafe.DatabaseManager.AddVaultItemAsync(vault.Key, storage.items.items);
                    VaultManager.DestroyVaultNoQueue(vault.Key);
                });
            }
        }
    }
}
