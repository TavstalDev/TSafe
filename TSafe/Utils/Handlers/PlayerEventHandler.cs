using System.Threading.Tasks;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tavstal.TSafe.Utils.Managers;

namespace Tavstal.TSafe.Utils.Handlers
{
    /// <summary>
    /// Provides event handlers for player-related events.
    /// </summary>
    /// <remarks>
    /// This class contains static methods that handle various player-related events, such as player connection, disconnection,
    /// damage requests, item interactions, and other player actions within the game.
    /// </remarks>
    public static class PlayerEventHandler
    {
        private static bool _isAttached;

        /// <summary>
        /// Attaches event handlers for player-related events.
        /// </summary>
        /// <remarks>
        /// This method subscribes the appropriate event handler methods to their respective events.
        /// It ensures that player-related actions such as connections, disconnections, item interactions, and other events are properly handled.
        /// </remarks>
        public static void AttachEvents()
        {
            if (_isAttached)
                return;

            _isAttached = true;

            U.Events.OnPlayerConnected += OnPlayerConnected;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            Provider.onCommenceShutdown+= OnShutdown;
        }
        
        /// <summary>
        /// Detaches event handlers for player-related events.
        /// </summary>
        /// <remarks>
        /// This method unsubscribes the previously attached event handlers from their respective events.
        /// It ensures that player-related actions are no longer handled when the event handlers are detached.
        /// </remarks>
        public static void DetachEvents()
        {
            if (!_isAttached)
                return;

            _isAttached = false;

            U.Events.OnPlayerConnected -= OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            Provider.onCommenceShutdown -= OnShutdown;
        }

        /// <summary>
        /// Handles the event when a player connects to the game.
        /// </summary>
        /// <param name="player">The player who has connected.</param>
        /// <remarks>
        /// This method is triggered when a player successfully connects to the game.
        /// It subscribes the player's events, such as item drop, equipment equip, and dequip requests, to the appropriate handlers.
        /// </remarks>
        private static void OnPlayerConnected(UnturnedPlayer player)
        {
            Task.Run(async () =>
            {
                foreach (var vault in await TSafe.DatabaseManager.GetVaultsAsync(player.CSteamID.m_SteamID))
                    VaultManager.CancelVaultDestroy(vault.Id);
            });
        }
        
        /// <summary>
        /// Handles the event when a player disconnects from the game.
        /// </summary>
        /// <param name="player">The player who has disconnected.</param>
        /// <remarks>
        /// This method is triggered when a player disconnects from the game.
        /// It unsubscribes the player's events, such as item drop, equipment equip, and dequip requests, to ensure that the player no longer triggers those handlers.
        /// </remarks>
        private static void OnPlayerDisconnected(UnturnedPlayer player)
        {
            Task.Run(async () =>
            {
                foreach (var vault in await TSafe.DatabaseManager.GetVaultsAsync(player.CSteamID.m_SteamID))
                    VaultManager.RequestVaultDestroy(vault.Id);
            });
        }
        
        /// <summary>
        /// Handles the event when the server shuts down.
        /// </summary>
        /// <remarks>
        /// This method is triggered when the server is shutting down.
        /// It can be used to perform any necessary cleanup, such as saving data, releasing resources, or disconnecting players gracefully.
        /// </remarks>
        private static void OnShutdown()
        {
            TSafe.IsShuttingDown = true;
            foreach (var vault in VaultManager.VaultList)
            {
                VaultManager.CancelVaultDestroy(vault.Key);
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
