using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Models.Database;
using Tavstal.TLibrary.Threading;
using Tavstal.TSafe.Models;
using UnityEngine;

namespace Tavstal.TSafe.Utils.Managers
{
    /// <summary>
    /// A static class responsible for managing vault-related operations within the game.
    /// </summary>
    public static class VaultManager
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ConcurrentDictionary<string, UnturnedVault> _vaultList = new ConcurrentDictionary<string, UnturnedVault>();
        public static ConcurrentDictionary<string, UnturnedVault> VaultList => _vaultList;
        
        // ReSharper disable once InconsistentNaming
        private static readonly ConcurrentDictionary<string, DateTime> _destroyQueue = new ConcurrentDictionary<string, DateTime>();

        /// <summary>
        /// Creates a new vault asynchronously.
        /// </summary>
        /// <param name="vaultId">The unique identifier for the vault to be created.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing the created <see cref="UnturnedVault"/> object.
        /// </returns>
        private static async Task<UnturnedVault?> CreateVaultAsync(string vaultId)
        {
            UnturnedVault? result = null;

            try
            {
                Vault? vault = await TSafe.DatabaseManager.Vaults.GetAsync(vaultId);
                if (vault == null)
                    return null;
                
                List<VaultItem>? vaultItems = await TSafe.DatabaseManager.Items.GetAsync(queryParameters: QueryParameter.eq("VaultId", vault.Id));

                BarricadeDrop? drop = null;
                await MainThreadDispatcher.RunAsync(() =>
                {
                    ItemBarricadeAsset? barricadeAsset = Assets.find(EAssetType.ITEM, 328) as ItemBarricadeAsset;
                    if (barricadeAsset == null)
                        return;
                    
                    Transform transform = BarricadeManager.dropBarricade(new Barricade(barricadeAsset), null,
                        new Vector3(0, -100, 0), 0, 0, 0, vault.OwnerId, 29832);
                    drop = BarricadeManager.FindBarricadeByRootTransform(transform);
                });
                
                if (drop == null)
                    return null;
                
                InteractableStorage interactableStorage = (InteractableStorage)drop.interactable;
                List<VaultItem> itemsToForceAdd = new List<VaultItem>();
                if (vaultItems != null)
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
                _vaultList.TryAdd(vaultId, result);
            }
            catch (Exception ex)
            {
                TSafe.Logger.Error("Error in CreateVaultAsync", ex);
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
        public static async Task OpenVaultAsync(UnturnedPlayer player, string guid)
        {
            try
            {
                if (!_vaultList.TryGetValue(guid, out UnturnedVault? vaultStorage))
                    vaultStorage = await CreateVaultAsync(guid);

                if (vaultStorage == null)
                {
                    TSafe.Logger.Error($"Failed to open vault for {player.CharacterName}.");
                    return;
                }

                await MainThreadDispatcher.RunAsync(() =>
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
                TSafe.Logger.Error("Error in OpenVaultAsync:", ex);
            }
        }

        /// <summary>
        /// Requests the destruction of a vault with the specified ID.
        /// </summary>
        /// <param name="vaultId">The unique identifier (ID) of the vault to be destroyed.</param>
        public static void RequestVaultDestroy(string vaultId)
        {
            if (_destroyQueue.ContainsKey(vaultId))
                return;
            
            _destroyQueue.TryAdd(vaultId, DateTime.Now.AddMinutes(5));
        }

        /// <summary>
        /// Cancels a pending request to destroy a vault with the specified ID.
        /// </summary>
        /// <param name="vaultId">The unique identifier (ID) of the vault for which the destruction request should be canceled.</param>
        public static void CancelVaultDestroy(string vaultId)
        {
            if (_destroyQueue.ContainsKey(vaultId))
                _destroyQueue.TryRemove(vaultId, out _);
        }

        /// <summary>
        /// Immediately destroys the vault with the specified ID without queuing the operation.
        /// </summary>
        /// <param name="vaultId">The unique identifier (ID) of the vault to be destroyed.</param>
        public static void DestroyVaultNoQueue(string vaultId) =>
            DestroyVault(vaultId);
        
        /// <summary>
        /// Destroys the vault with the specified ID.
        /// </summary>
        /// <param name="vaultId">The unique identifier (ID) of the vault to be destroyed.</param>
        private static void DestroyVault(string vaultId)
        {
            try
            {
                if (!_vaultList.TryGetValue(vaultId, out UnturnedVault vaultStorage))
                    return;

                MainThreadDispatcher.Run(() =>
                {
                    try
                    {
                        BarricadeData barricadeData = vaultStorage.StorageDrop.GetServersideData();
                        InteractableStorage interactableStorage =
                            (InteractableStorage)vaultStorage.StorageDrop.interactable;
                        interactableStorage.items.clear();
                        barricadeData.barricade.askDamage(ushort.MaxValue);
                        _vaultList.TryRemove(vaultId, out _);
                    }
                    catch (Exception ex)
                    {
                        TSafe.Logger.Error("Error in DestroyVault game thread:", ex);
                    }
                });
            }
            catch (Exception ex)
            {
                TSafe.Logger.Error("Error in DestroyVault:", ex);
            }
        }

        /// <summary>
        /// Performs the update logic for the system or component on each frame.
        /// </summary>
        internal static void Update()
        {
            if (_destroyQueue.Count < 1)
               return;

            List<string> toRemove = new List<string>();
            List<string> vaultIds = new List<string>();
            List<VaultItem> vaultItems = new List<VaultItem>();
            foreach (var elem in _destroyQueue)
            {
                if (elem.Value > DateTime.Now)
                    continue;
                
                toRemove.Add(elem.Key);
                UnturnedVault vault = _vaultList[elem.Key];
                InteractableStorage storage = (InteractableStorage)vault.StorageDrop.interactable;
                vaultIds.Add(elem.Key);
                foreach (var item in storage.items.items)
                    vaultItems.Add(new VaultItem(elem.Key, item));
            }

            BackgroundThreadDispatcher.Run(async () =>
            {
                var ids = new List<object>();
                ids.AddRange(vaultIds);
                await TSafe.DatabaseManager.Items.DeleteRangeAsync("VaultId", ids);
                await TSafe.DatabaseManager.Items.AddRangeAsync(vaultItems);
                await MainThreadDispatcher.RunAsync(() =>
                {
                    foreach (var vaultId in vaultIds)
                        DestroyVault(vaultId);
                });
            });

            toRemove.ForEach(x => _destroyQueue.TryRemove(x, out _));
        }
    }
}