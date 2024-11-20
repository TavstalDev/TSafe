using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tavstal.TLibrary;
using Tavstal.TSafe.Components;
using Tavstal.TSafe.Models;
using UnityEngine;

namespace Tavstal.TSafe.Managers
{
    public static class VaultManager
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Dictionary<string, UnturnedVault> _vaultList = new Dictionary<string, UnturnedVault>();
        public static Dictionary<string, UnturnedVault> VaultList => _vaultList;
        
        // ReSharper disable once InconsistentNaming
        private static readonly Dictionary<string, DateTime> _destroyQueue = new Dictionary<string, DateTime>();

        private static async Task<UnturnedVault> CreateVaultAsync(UnturnedPlayer player, string vaultId)
        {
            UnturnedVault result = null;

            try
            {
                Vault vault = await TSafe.DatabaseManager.FindVaultAsync(vaultId);
                if (vault == null)
                    return null;
                
                List<VaultItem> vaultItems = await TSafe.DatabaseManager.GetVaultItemsAsync(vault.Id);
                
                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    try
                    {
                        ItemBarricadeAsset barricadeAsset = Assets.find(EAssetType.ITEM, 328) as ItemBarricadeAsset;
                        Transform transform = BarricadeManager.dropBarricade(new Barricade(barricadeAsset), null,
                            new Vector3(0, -100, 0), 0, 0, 0, player.CSteamID.m_SteamID, 29832);
                        BarricadeDrop drop = BarricadeManager.FindBarricadeByRootTransform(transform);
                        if (drop == null)
                            return;

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

                        result = new UnturnedVault(drop, player, vault.SizeX, vault.SizeY);
                        _vaultList.Add(vaultId, result);
                    }
                    catch (Exception ex)
                    {
                        TSafe.Logger.LogException("Error in CreateVaultAsync game thread:");
                        TSafe.Logger.LogError(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                TSafe.Logger.LogException("Error in CreateVaultAsync");
                TSafe.Logger.LogError(ex);
            }

            return result;
        }
        
        public static async Task OpenVaultAsync(UnturnedPlayer player, string guid)
        {
            try
            {
                if (!_vaultList.TryGetValue(guid, out UnturnedVault vaultStorage))
                    vaultStorage = await CreateVaultAsync(player, guid);

                InteractableStorage interactableStorage = (InteractableStorage)vaultStorage.StorageDrop.interactable;
                interactableStorage.items.resize((byte)vaultStorage.SizeX, (byte)vaultStorage.SizeY);
                interactableStorage.isOpen = true;
                interactableStorage.opener = player.Player;
                player.Inventory.isStoring = false;
                player.Inventory.storage = interactableStorage;
                player.Inventory.updateItems(PlayerInventory.STORAGE, interactableStorage.items);
                player.Inventory.sendStorage();
                var comp = player.GetComponent<SafeComponent>();
                comp.isSafeOpened = true;
                comp.VaultId = guid;
            }
            catch (Exception ex)
            {
                TSafe.Logger.LogException("Error in OpenVaultAsync:");
                TSafe.Logger.LogError(ex);
            }
        }

        public static void RequestVaultDestroy(string vaultId)
        {
            if (_destroyQueue.ContainsKey(vaultId))
                return;
            
            _destroyQueue.Add(vaultId, DateTime.Now.AddMinutes(5));
        }

        public static void PreventVaultDestroy(string vaultId)
        {
            if (_destroyQueue.ContainsKey(vaultId))
                _destroyQueue.Remove(vaultId);
        }

        public static void DestroyVaultNoQueue(string vaultId)
        {
            DestroyVault(vaultId);
        }
        
        private static void DestroyVault(string vaultId)
        {
            try
            {
                if (!_vaultList.TryGetValue(vaultId, out UnturnedVault vaultStorage))
                    return;

                MainThreadDispatcher.RunOnMainThread(() =>
                {
                    BarricadeData barricadeData = vaultStorage.StorageDrop.GetServersideData();
                    InteractableStorage interactableStorage =
                        (InteractableStorage)vaultStorage.StorageDrop.interactable;
                    interactableStorage.items.clear();
                    barricadeData.barricade.askDamage(ushort.MaxValue);
                    _vaultList.Remove(vaultId);
                });
            }
            catch (Exception ex)
            {
                TSafe.Logger.LogException("Error in DestroyVault:");
                TSafe.Logger.LogError(ex);
            }
        }

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
                DestroyVault(elem.Key);
            }

            foreach (var elem in toRemove)
                _destroyQueue.Remove(elem);
        }
    }
}