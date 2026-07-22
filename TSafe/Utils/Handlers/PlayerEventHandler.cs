using System.Collections.Concurrent;
using System.Threading;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using Tavstal.TLibrary.Models.Database;
using Tavstal.TLibrary.Threading;
using Tavstal.TSafe.Utils.Managers;

namespace Tavstal.TSafe.Utils.Handlers
{
    /// <summary>
    /// Provides event handlers for player-related events.
    /// </summary>
    public static class PlayerEventHandler
    {
        private static bool _isAttached;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _playerLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

        /// <summary>
        /// Attaches event handlers for player-related events.
        /// </summary>
        public static void AttachEvents()
        {
            if (_isAttached)
                return;

            _isAttached = true;

            U.Events.OnPlayerConnected += OnPlayerConnected;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
        }
        
        /// <summary>
        /// Detaches event handlers for player-related events.
        /// </summary>
        public static void DetachEvents()
        {
            if (!_isAttached)
                return;

            _isAttached = false;

            U.Events.OnPlayerConnected -= OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
        }

        /// <summary>
        /// Handles the event when a player connects to the game.
        /// </summary>
        /// <param name="player">The player who has connected.</param>
        private static void OnPlayerConnected(UnturnedPlayer player)
        {
            if (TSafe.DatabaseManager.IsAuthenticationFailed)
                return;
            
            var steamIdString = player.CSteamID.m_SteamID.ToString();
            var steamId = player.CSteamID;
            
            BackgroundThreadDispatcher.RunAsync(async () =>
            {
                var playerLock = _playerLocks.GetOrAdd(steamIdString, new SemaphoreSlim(1, 1));
                await playerLock.WaitAsync();
                try
                {
                    var vaults = await TSafe.DatabaseManager.Vaults.GetAsync(queryParameters:
                        QueryParameter.eq("OwnerId", steamId.m_SteamID));
                    if (vaults == null || vaults.Count == 0)
                        return;
                    vaults.ForEach(x => VaultManager.CancelVaultDestroy(x.Id));
                }
                finally
                {
                    playerLock.Release();
                    _playerLocks.TryRemove(steamIdString, out _);
                }
            });
        }
        
        /// <summary>
        /// Handles the event when a player disconnects from the game.
        /// </summary>
        /// <param name="player">The player who has disconnected.</param>
        private static void OnPlayerDisconnected(UnturnedPlayer player)
        {
            if (TSafe.DatabaseManager.IsAuthenticationFailed)
                return;
            
            var steamIdString = player.CSteamID.m_SteamID.ToString();
            var steamId = player.CSteamID;
            
            BackgroundThreadDispatcher.RunAsync(async () =>
            {
                var playerLock = _playerLocks.GetOrAdd(steamIdString, new SemaphoreSlim(1, 1));
                await playerLock.WaitAsync();
                try
                {
                    var vaults = await TSafe.DatabaseManager.Vaults.GetAsync(queryParameters:
                        QueryParameter.eq("OwnerId", steamId.m_SteamID));
                    if (vaults == null || vaults.Count == 0)
                        return;
                    vaults.ForEach(x => VaultManager.RequestVaultDestroy(x.Id));
                }
                finally
                {
                    playerLock.Release();
                    _playerLocks.TryRemove(steamIdString, out _);
                }
            });
        }
    }
}
