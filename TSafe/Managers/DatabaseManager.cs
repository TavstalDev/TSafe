using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDG.Unturned;
using Tavstal.TLibrary.Models.Database;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers.General;
using Tavstal.TLibrary.Managers;
using Tavstal.TSafe.Models;

namespace Tavstal.TSafe.Managers
{
    public class DatabaseManager : DatabaseManagerBase
    {
#pragma warning disable IDE1006 //
        private static TSafeConfig PluginConfig => TSafe.Instance.Config;
#pragma warning restore IDE1006 //

        public DatabaseManager(IPlugin plugin, IConfigurationBase config) : base(plugin, config)
        {

        }

        /// <summary>
        /// Checks the schema of the database, creates or modifies the tables if needed
        /// <br/>PS. If you change the Primary Key then you must delete the table.
        /// </summary>
        public override async Task CheckSchemaAsync()
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    if (!await connection.OpenSafeAsync())
                        TSafe.IsConnectionAuthFailed = true;
                    if (connection.State != System.Data.ConnectionState.Open)
                        throw new Exception("# Failed to connect to the database. Please check the plugin's config file.");

                    // Vault Table
                    if (await connection.DoesTableExistAsync<Vault>(PluginConfig.Database.VaultTable))
                        await connection.CheckTableAsync<Vault>(PluginConfig.Database.VaultTable);
                    else
                        await connection.CreateTableAsync<Vault>(PluginConfig.Database.VaultTable);
                    
                    // Vault Items Table
                    if (await connection.DoesTableExistAsync<VaultItem>(PluginConfig.Database.VaultItemsTable))
                        await connection.CheckTableAsync<VaultItem>(PluginConfig.Database.VaultItemsTable);
                    else
                        await connection.CreateTableAsync<VaultItem>(PluginConfig.Database.VaultItemsTable);

                    if (connection.State != System.Data.ConnectionState.Closed)
                        await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                TSafe.Logger.LogException("Error in checkSchema:");
                TSafe.Logger.LogError(ex);
            }
        }

        #region Vault Table
        public async Task<bool> AddVaultAsync(string id, string name, ulong ownerId, byte sizeX, byte sizeY)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.AddTableRowAsync(tableName: PluginConfig.Database.VaultTable, value: new Vault(id, name, ownerId, sizeX, sizeY));
        }

        public async Task<bool> RemoveVaultAsync(string id)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return  await mySqlConnection.RemoveTableRowAsync<Vault>(tableName: PluginConfig.Database.VaultTable, whereClause: $"Id='{id}'", parameters: null);
        }

        public async Task<bool> UpdateVaultAsync(string id, byte sizeX, byte sizeY)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.UpdateTableRowAsync<Vault>(tableName: PluginConfig.Database.VaultTable, $"Id='{id}'", new List<SqlParameter>
            {
                SqlParameter.Get<Vault>(x => x.SizeX, sizeX),
                SqlParameter.Get<Vault>(x => x.SizeY, sizeY)
            });
        }

        public async Task<List<Vault>> GetVaultsAsync()
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.GetTableRowsAsync<Vault>(tableName: PluginConfig.Database.VaultTable, whereClause: string.Empty, null);
        }
        
        public async Task<List<Vault>> GetVaultsAsync(ulong ownerId)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.GetTableRowsAsync<Vault>(tableName: PluginConfig.Database.VaultTable, whereClause: $"OwnerId='{ownerId}'", null);
        }

        public async Task<Vault> FindVaultAsync(string id)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.GetTableRowAsync<Vault>(tableName: PluginConfig.Database.VaultTable, whereClause: $"Id='{id}'", null);
        }
        
        public async Task<Vault> FindVaultAsync(ulong ownerId)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.GetTableRowAsync<Vault>(tableName: PluginConfig.Database.VaultTable, whereClause: $"OwnerId='{ownerId}'", null);
        }
        #endregion
        
        #region Vault Items Table
        public async Task<bool> AddVaultItemAsync(string vaultId, ushort itemId, byte amount, byte quality, byte[] state, byte x, byte y, byte rot)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.AddTableRowAsync(tableName: PluginConfig.Database.VaultItemsTable, value: new VaultItem(vaultId, itemId, amount, quality, state, x, y, rot));
        }
        
        public async Task<bool> AddVaultItemAsync(string vaultId, Item item, byte x, byte y, byte rot)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.AddTableRowAsync(tableName: PluginConfig.Database.VaultItemsTable, value: new VaultItem(vaultId, item.id, item.amount, item.quality, item.state, x, y, rot));
        }
        
        public async Task<bool> AddVaultItemAsync(string vaultId, List<ItemJar> itemJars)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            List<VaultItem> vaultItems = new List<VaultItem>();
            foreach (var item in itemJars)
                vaultItems.Add(new VaultItem(vaultId, item.item.id, item.item.amount, item.item.quality, item.item.state, item.x, item.y, item.rot));
            return await mySqlConnection.AddTableRowsAsync(tableName: PluginConfig.Database.VaultItemsTable, values: vaultItems);
        }

        public async Task<bool> RemoveVaultItemAsync(string vaultId, ushort itemId)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return  await mySqlConnection.RemoveTableRowAsync<VaultItem>(tableName: PluginConfig.Database.VaultItemsTable, whereClause: $"VaultId='{vaultId}' AND ItemId='{itemId}'", parameters: null);
        }
        
        public async Task<bool> RemoveVaultItemsAsync(string vaultId, ushort itemId)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return  await mySqlConnection.RemoveTableRowsAsync<VaultItem>(tableName: PluginConfig.Database.VaultItemsTable, whereClause: $"VaultId='{vaultId}' AND ItemId='{itemId}'", parameters: null);
        }
        
        public async Task<bool> RemoveVaultItemsAsync(string vaultId)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return  await mySqlConnection.RemoveTableRowsAsync<VaultItem>(tableName: PluginConfig.Database.VaultItemsTable, whereClause: $"VaultId='{vaultId}'", parameters: null);
        }

        public async Task<List<VaultItem>> GetVaultItemsAsync(string vaultId)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.GetTableRowsAsync<VaultItem>(tableName: PluginConfig.Database.VaultItemsTable, whereClause: $"VaultId='{vaultId}'", null);
        }

        public async Task<VaultItem> FindVaultItemAsync(string vaultId, ushort itemId)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.GetTableRowAsync<VaultItem>(tableName: PluginConfig.Database.VaultItemsTable, whereClause: $"VaultId='{vaultId}' AND ItemId='{itemId}'", null);
        }
        #endregion
    }
}
