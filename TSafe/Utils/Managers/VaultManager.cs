using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tavstal.TLibrary;
using Tavstal.TSafe.Models;
using UnityEngine;

namespace Tavstal.TSafe.Utils.Managers
{
    /// <summary>
    /// A static class responsible for managing vault-related operations within the game.
    /// </summary>
    /// <remarks>
    /// This class handles the creation, retrieval, updating, and deletion of vaults in the system. It also provides utilities for interacting with vault data and performing necessary actions on them.
    /// </remarks>
    public static class VaultManager
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Dictionary<string, UnturnedVault> _vaultList = new Dictionary<string, UnturnedVault>();
        public static Dictionary<string, UnturnedVault> VaultList => _vaultList;
        
        // ReSharper disable once InconsistentNaming
        private static readonly Dictionary<string, DateTime> _destroyQueue = new Dictionary<string, DateTime>();

        /// <summary>
        /// Creates a new vault asynchronously.
        /// </summary>
        /// <param name="vaultId">The unique identifier for the vault to be created.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing the created <see cref="UnturnedVault"/> object.
        /// </returns>
        /// <remarks>
        /// This method asynchronously creates a vault with the specified <paramref name="vaultId"/>. It initializes the vault and prepares it for use in the game.
        /// </remarks>
        private static async Task<UnturnedVault> CreateVaultAsync(string vaultId)
        {
            UnturnedVault result = null;

            try
            {
                Vault vault = await TSafe.DatabaseManager.FindVaultAsync(vaultId);
                if (vault == null)
                    return null;
                
                List<VaultItem> vaultItems = await TSafe.DatabaseManager.GetVaultItemsAsync(vault.Id);

                BarricadeDrop drop = null;
                await MainThreadDispatcher.RunOnMainThreadAsync(() =>
                {
                    ItemBarricadeAsset barricadeAsset = Assets.find(EAssetType.ITEM, 328) as ItemBarricadeAsset;
                    Transform transform = BarricadeManager.dropBarricade(new Barricade(barricadeAsset), null,
                        new Vector3(0, -100, 0), 0, 0, 0, vault.OwnerId, 29832);
                    drop = BarricadeManager.FindBarricadeByRootTransform(transform);
                });
                
                if (drop == null)
                    return null;
                
                InteractableStorage interactableStorage = (InteractableStorage)drop.interactable;
                List<VaultItem> itemsToForceAdd = new List<VaultItem>();
                foreach (VaultItem item in vaultItems)
                {
                    try
                    {
                        interactableStorage.items.loadItem(item.X, item.Y, item.Rot, item.ToItem());
                    }
                    catch
                    {
                        itemsToForceAdd.Add(item);
                    }
                }
                foreach (VaultItem item in itemsToForceAdd)
                    interactableStorage.items.tryAddItem(item.ToItem());

                result = new UnturnedVault(drop, vault.SizeX, vault.SizeY);
                _vaultList.Add(vaultId, result);
            }
            catch (Exception ex)
            {
                TSafe.Logger.LogException("Error in CreateVaultAsync");
                TSafe.Logger.LogError(ex);
            }

            return result;
        }
        
        /// <summary>
        /// Opens a vault for the specified player asynchronously.
        /// </summary>
        /// <param name="player">The player requesting to open the vault.</param>
        /// <param name="guid">The unique identifier (GUID) of the vault to be opened.</param>
        /// <returns>
        /// A task that represents the asynchronous operation of opening the vault.
        /// </returns>
        /// <remarks>
        /// This method asynchronously handles the process of opening the vault specified by <paramref name="guid"/> for the given <paramref name="player"/>. 
        /// It allows the player to interact with the vault's contents once the operation is complete.
        /// </remarks>
        public static async Task OpenVaultAsync(UnturnedPlayer player, string guid)
        {
            try
            {
                if (!_vaultList.TryGetValue(guid, out UnturnedVault vaultStorage))
                    vaultStorage = await CreateVaultAsync(guid);

                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    InteractableStorage interactableStorage = (InteractableStorage)vaultStorage.StorageDrop.interactable;

                    interactableStorage.items.resize((byte)vaultStorage.SizeX, (byte)vaultStorage.SizeY);
                    interactableStorage.isOpen = true;

                    interactableStorage.opener = player.Player;
                    player.Inventory.isStoring = false;
                    player.Inventory.storage = interactableStorage;

                    player.Inventory.updateItems(PlayerInventory.STORAGE, interactableStorage.items);
                    player.Inventory.sendStorage();
                });
            }
            catch (Exception ex)
            {
                TSafe.Logger.LogException("Error in OpenVaultAsync:");
                TSafe.Logger.LogError(ex);
            }
        }

        /// <summary>
        /// Requests the destruction of a vault with the specified ID.
        /// </summary>
        /// <param name="vaultId">The unique identifier (ID) of the vault to be destroyed.</param>
        /// <remarks>
        /// This method initiates the process of destroying the vault identified by <paramref name="vaultId"/>. 
        /// The vault will be permanently removed from the game map.
        /// </remarks>
        public static void RequestVaultDestroy(string vaultId)
        {
            if (_destroyQueue.ContainsKey(vaultId))
                return;
            
            _destroyQueue.Add(vaultId, DateTime.Now.AddMinutes(5));
        }

        /// <summary>
        /// Cancels a pending request to destroy a vault with the specified ID.
        /// </summary>
        /// <param name="vaultId">The unique identifier (ID) of the vault for which the destruction request should be canceled.</param>
        /// <remarks>
        /// This method cancels any ongoing or pending destruction request for the vault identified by <paramref name="vaultId"/>. 
        /// The vault will not be destroyed if this method is called before the destruction is finalized.
        /// </remarks>
        public static void CancelVaultDestroy(string vaultId)
        {
            if (_destroyQueue.ContainsKey(vaultId))
                _destroyQueue.Remove(vaultId);
        }

        /// <summary>
        /// Immediately destroys the vault with the specified ID without queuing the operation.
        /// </summary>
        /// <param name="vaultId">The unique identifier (ID) of the vault to be destroyed.</param>
        /// <remarks>
        /// This method performs an immediate destruction of the vault identified by <paramref name="vaultId"/> without waiting for any pending operations. 
        /// The vault and any associated data will be permanently removed from the game map without being queued for processing.
        /// </remarks>
        public static void DestroyVaultNoQueue(string vaultId)
        {
            DestroyVault(vaultId);
        }
        
        /// <summary>
        /// Destroys the vault with the specified ID.
        /// </summary>
        /// <param name="vaultId">The unique identifier (ID) of the vault to be destroyed.</param>
        /// <remarks>
        /// This method performs the destruction of the vault identified by <paramref name="vaultId"/>.
        /// </remarks>
        private static void DestroyVault(string vaultId)
        {
            try
            {
                if (!_vaultList.TryGetValue(vaultId, out UnturnedVault vaultStorage))
                    return;

                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    try
                    {
                        BarricadeData barricadeData = vaultStorage.StorageDrop.GetServersideData();
                        InteractableStorage interactableStorage =
                            (InteractableStorage)vaultStorage.StorageDrop.interactable;
                        interactableStorage.items.clear();
                        barricadeData.barricade.askDamage(ushort.MaxValue);
                        _vaultList.Remove(vaultId);
                    }
                    catch (Exception ex)
                    {
                        TSafe.Logger.LogException("Error in DestroyVault game thread:");
                        TSafe.Logger.LogError(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                TSafe.Logger.LogException("Error in DestroyVault:");
                TSafe.Logger.LogError(ex);
            }
        }

        /// <summary>
        /// Performs the update logic for the system or component on each frame.
        /// </summary>
        /// <remarks>
        /// This method is called periodically to execute the update logic. 
        /// It should be called during each frame or cycle of the application.
        /// </remarks>
        internal static void Update()
        {
            if (_destroyQueue.Count < 1)
               return;

            List<string> toRemove = new List<string>();
            foreach (var elem in _destroyQueue)
            {
                if (elem.Value > DateTime.Now)
                    continue;
                
                toRemove.Add(elem.Key);
                Task.Run(async () =>
                {
                    UnturnedVault vault = _vaultList[elem.Key];
                    InteractableStorage storage = (InteractableStorage)vault.StorageDrop.interactable;
                    await TSafe.DatabaseManager.RemoveVaultItemsAsync(elem.Key);
                    await TSafe.DatabaseManager.AddVaultItemAsync(elem.Key, storage.items.items);
                    await MainThreadDispatcher.RunOnMainThreadAsync(() => DestroyVault(elem.Key));
                });
            }

            foreach (var elem in toRemove)
                _destroyQueue.Remove(elem);
        }
    }
}