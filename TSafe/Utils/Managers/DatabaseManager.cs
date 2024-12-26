using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SDG.Unturned;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers.General;
using Tavstal.TLibrary.Managers;
using Tavstal.TLibrary.Models.Database;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TSafe.Models;

namespace Tavstal.TSafe.Utils.Managers
{
    /// <summary>
    /// Represents a manager responsible for handling database operations.
    /// </summary>
    /// <remarks>
    /// This class inherits from <see cref="DatabaseManagerBase"/> and provides additional functionality or overrides specific methods to handle database-related tasks, 
    /// such as managing records, executing queries, and interacting with the database system.
    /// </remarks>
    public class DatabaseManager : DatabaseManagerBase
    {
        private static TSafeConfig PluginConfig => TSafe.Instance.Config;

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
        /// <summary>
        /// Asynchronously adds a new vault with the specified parameters.
        /// </summary>
        /// <param name="id">The unique identifier for the vault to be added.</param>
        /// <param name="name">The name of the vault to be added.</param>
        /// <param name="ownerId">The unique identifier of the owner of the vault.</param>
        /// <param name="sizeX">The width of the vault.</param>
        /// <param name="sizeY">The height of the vault.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing a boolean value indicating whether the vault was successfully added.
        /// </returns>
        /// <remarks>
        /// This method asynchronously adds a new vault with the given <paramref name="id"/>, <paramref name="name"/>, <paramref name="ownerId"/>, 
        /// <paramref name="sizeX"/>, and <paramref name="sizeY"/> to the system. It returns a boolean indicating the success or failure of the operation.
        /// </remarks>
        public async Task<bool> AddVaultAsync(string id, string name, ulong ownerId, byte sizeX, byte sizeY)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.AddTableRowAsync(tableName: PluginConfig.Database.VaultTable, value: new Vault(id, name, ownerId, sizeX, sizeY));
        }

        /// <summary>
        /// Asynchronously removes the vault with the specified ID.
        /// </summary>
        /// <param name="id">The unique identifier of the vault to be removed.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing a boolean value indicating whether the vault was successfully removed.
        /// </returns>
        /// <remarks>
        /// This method asynchronously removes the vault identified by <paramref name="id"/> from the system. It returns a boolean value that indicates whether the operation 
        /// was successful or if the vault could not be found or removed.
        /// </remarks>
        public async Task<bool> RemoveVaultAsync(string id)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return  await mySqlConnection.RemoveTableRowAsync<Vault>(tableName: PluginConfig.Database.VaultTable, whereClause: $"Id='{id}'", parameters: null);
        }

        /// <summary>
        /// Asynchronously updates the size of the vault with the specified ID.
        /// </summary>
        /// <param name="id">The unique identifier of the vault to be updated.</param>
        /// <param name="sizeX">The new width of the vault.</param>
        /// <param name="sizeY">The new height of the vault.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing a boolean value indicating whether the vault was successfully updated.
        /// </returns>
        /// <remarks>
        /// This method asynchronously updates the dimensions of the vault identified by <paramref name="id"/>. The vault's size will be changed according to the provided 
        /// <paramref name="sizeX"/> and <paramref name="sizeY"/> values. It returns a boolean value indicating whether the update operation was successful.
        /// </remarks>
        public async Task<bool> UpdateVaultAsync(string id, byte sizeX, byte sizeY)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.UpdateTableRowAsync<Vault>(tableName: PluginConfig.Database.VaultTable, $"Id='{id}'", new List<SqlParameter>
            {
                SqlParameter.Get<Vault>(x => x.SizeX, sizeX),
                SqlParameter.Get<Vault>(x => x.SizeY, sizeY)
            });
        }

        /// <summary>
        /// Asynchronously retrieves a list of all vaults.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation, containing a list of <see cref="Vault"/> objects representing all available vaults.
        /// </returns>
        /// <remarks>
        /// This method asynchronously fetches all vaults from the system and returns them as a list of <see cref="Vault"/> objects. 
        /// It can be used to retrieve a comprehensive list of all vaults managed by the system.
        /// </remarks>
        public async Task<List<Vault>> GetVaultsAsync()
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.GetTableRowsAsync<Vault>(tableName: PluginConfig.Database.VaultTable, whereClause: string.Empty, null);
        }
        
        /// <summary>
        /// Asynchronously retrieves a list of vaults owned by the specified owner.
        /// </summary>
        /// <param name="ownerId">The unique identifier of the vault owner.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing a list of <see cref="Vault"/> objects owned by the specified owner.
        /// </returns>
        /// <remarks>
        /// This method asynchronously fetches all vaults that belong to the owner identified by <paramref name="ownerId"/>. 
        /// It returns a list of <see cref="Vault"/> objects representing the owner's vaults.
        /// </remarks>
        public async Task<List<Vault>> GetVaultsAsync(ulong ownerId)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.GetTableRowsAsync<Vault>(tableName: PluginConfig.Database.VaultTable, whereClause: $"OwnerId='{ownerId}'", null);
        }

        /// <summary>
        /// Asynchronously retrieves a vault by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the vault to be retrieved.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing the <see cref="Vault"/> object if found, or null if no vault is found with the specified ID.
        /// </returns>
        /// <remarks>
        /// This method asynchronously searches for a vault in the database using the provided <paramref name="id"/>. 
        /// If a matching vault is found, it returns the corresponding <see cref="Vault"/> object; otherwise, it returns null.
        /// </remarks>
        public async Task<Vault> FindVaultAsync(string id)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.GetTableRowAsync<Vault>(tableName: PluginConfig.Database.VaultTable, whereClause: $"Id='{id}'", null);
        }
        
        /// <summary>
        /// Asynchronously retrieves a vault owned by the specified owner.
        /// </summary>
        /// <param name="ownerId">The unique identifier of the vault owner.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing the <see cref="Vault"/> object if found, or null if no vault is found for the specified owner.
        /// </returns>
        /// <remarks>
        /// This method asynchronously searches for a vault in the database that is owned by the specified <paramref name="ownerId"/>. 
        /// If a matching vault is found, it returns the corresponding <see cref="Vault"/> object; otherwise, it returns null.
        /// </remarks>
        public async Task<Vault> FindVaultAsync(ulong ownerId)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.GetTableRowAsync<Vault>(tableName: PluginConfig.Database.VaultTable, whereClause: $"OwnerId='{ownerId}'", null);
        }
        #endregion
        
        #region Vault Items Table
        /// <summary>
        /// Asynchronously adds an item to a vault.
        /// </summary>
        /// <param name="vaultId">The unique identifier of the vault where the item will be added.</param>
        /// <param name="itemId">The unique identifier of the item to be added to the vault.</param>
        /// <param name="amount">The amount of the item to be added to the vault.</param>
        /// <param name="quality">The quality of the item being added.</param>
        /// <param name="state">The state of the item (e.g., its condition or modifications).</param>
        /// <param name="x">The x-coordinate of the item's position within the vault.</param>
        /// <param name="y">The y-coordinate of the item's position within the vault.</param>
        /// <param name="rot">The rotation of the item within the vault.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing a boolean value indicating whether the item was successfully added to the vault.
        /// </returns>
        /// <remarks>
        /// This method asynchronously adds an item to the specified vault, including the item's ID, amount, quality, state, and position information.
        /// It returns a boolean value indicating whether the operation was successful.
        /// </remarks>
        public async Task<bool> AddVaultItemAsync(string vaultId, ushort itemId, byte amount, byte quality, byte[] state, byte x, byte y, byte rot)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.AddTableRowAsync(tableName: PluginConfig.Database.VaultItemsTable, value: new VaultItem(vaultId, itemId, amount, quality, state, x, y, rot));
        }
        
        /// <summary>
        /// Asynchronously adds an item to a vault.
        /// </summary>
        /// <param name="vaultId">The unique identifier of the vault where the item will be added.</param>
        /// <param name="item">The <see cref="Item"/> object to be added to the vault.</param>
        /// <param name="x">The x-coordinate of the item's position within the vault.</param>
        /// <param name="y">The y-coordinate of the item's position within the vault.</param>
        /// <param name="rot">The rotation of the item within the vault.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing a boolean value indicating whether the item was successfully added to the vault.
        /// </returns>
        /// <remarks>
        /// This method asynchronously adds an item to the specified vault, including the item's properties and position information.
        /// It returns a boolean value indicating whether the operation was successful.
        /// </remarks>
        public async Task<bool> AddVaultItemAsync(string vaultId, Item item, byte x, byte y, byte rot)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.AddTableRowAsync(tableName: PluginConfig.Database.VaultItemsTable, value: new VaultItem(vaultId, item.id, item.amount, item.quality, item.state, x, y, rot));
        }
        
        /// <summary>
        /// Asynchronously adds multiple items to a vault.
        /// </summary>
        /// <param name="vaultId">The unique identifier of the vault where the items will be added.</param>
        /// <param name="itemJars">A list of <see cref="ItemJar"/> objects representing the items to be added to the vault.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing a boolean value indicating whether the items were successfully added to the vault.
        /// </returns>
        /// <remarks>
        /// This method asynchronously adds multiple items, represented as a list of <see cref="ItemJar"/> objects, to the specified vault.
        /// It returns a boolean value indicating whether the operation was successful.
        /// </remarks>
        public async Task<bool> AddVaultItemAsync(string vaultId, List<ItemJar> itemJars)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            List<VaultItem> vaultItems = new List<VaultItem>();
            foreach (var item in itemJars)
                vaultItems.Add(new VaultItem(vaultId, item.item.id, item.item.amount, item.item.quality, item.item.state, item.x, item.y, item.rot));
            return await mySqlConnection.AddTableRowsAsync(tableName: PluginConfig.Database.VaultItemsTable, values: vaultItems);
        }

        /// <summary>
        /// Asynchronously removes an item from a vault.
        /// </summary>
        /// <param name="vaultId">The unique identifier of the vault from which the item will be removed.</param>
        /// <param name="itemId">The unique identifier of the item to be removed from the vault.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing a boolean value indicating whether the item was successfully removed from the vault.
        /// </returns>
        /// <remarks>
        /// This method asynchronously removes an item from the specified vault based on the item's unique identifier.
        /// It returns a boolean value indicating whether the operation was successful.
        /// </remarks>
        public async Task<bool> RemoveVaultItemAsync(string vaultId, ushort itemId)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return  await mySqlConnection.RemoveTableRowAsync<VaultItem>(tableName: PluginConfig.Database.VaultItemsTable, whereClause: $"VaultId='{vaultId}' AND ItemId='{itemId}'", parameters: null);
        }
        
        /// <summary>
        /// Asynchronously removes all instances of a specific item from a vault.
        /// </summary>
        /// <param name="vaultId">The unique identifier of the vault from which the items will be removed.</param>
        /// <param name="itemId">The unique identifier of the item to be removed from the vault.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing a boolean value indicating whether the items were successfully removed from the vault.
        /// </returns>
        /// <remarks>
        /// This method asynchronously removes all instances of the specified item from the vault based on its unique identifier.
        /// It returns a boolean value indicating whether the operation was successful.
        /// </remarks>
        public async Task<bool> RemoveVaultItemsAsync(string vaultId, ushort itemId)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return  await mySqlConnection.RemoveTableRowsAsync<VaultItem>(tableName: PluginConfig.Database.VaultItemsTable, whereClause: $"VaultId='{vaultId}' AND ItemId='{itemId}'", parameters: null);
        }
        
        /// <summary>
        /// Asynchronously removes all items from a vault.
        /// </summary>
        /// <param name="vaultId">The unique identifier of the vault from which all items will be removed.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing a boolean value indicating whether all items were successfully removed from the vault.
        /// </returns>
        /// <remarks>
        /// This method asynchronously removes all items from the specified vault.
        /// It returns a boolean value indicating whether the operation was successful.
        /// </remarks>
        public async Task<bool> RemoveVaultItemsAsync(string vaultId)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return  await mySqlConnection.RemoveTableRowsAsync<VaultItem>(tableName: PluginConfig.Database.VaultItemsTable, whereClause: $"VaultId='{vaultId}'", parameters: null);
        }

        /// <summary>
        /// Asynchronously retrieves a list of items from a vault.
        /// </summary>
        /// <param name="vaultId">The unique identifier of the vault from which the items will be retrieved.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing a list of <see cref="VaultItem"/> objects representing the items in the vault.
        /// </returns>
        /// <remarks>
        /// This method asynchronously fetches all the items stored in the specified vault.
        /// It returns a list of <see cref="VaultItem"/> objects representing the items, or an empty list if no items are found.
        /// </remarks>
        public async Task<List<VaultItem>> GetVaultItemsAsync(string vaultId)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.GetTableRowsAsync<VaultItem>(tableName: PluginConfig.Database.VaultItemsTable, whereClause: $"VaultId='{vaultId}'", null);
        }

        /// <summary>
        /// Asynchronously retrieves a specific item from a vault based on its unique item identifier.
        /// </summary>
        /// <param name="vaultId">The unique identifier of the vault where the item is stored.</param>
        /// <param name="itemId">The unique identifier of the item to be retrieved from the vault.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing the <see cref="VaultItem"/> object representing the found item.
        /// If the item is not found, the result will be null.
        /// </returns>
        /// <remarks>
        /// This method asynchronously searches for a specific item in the vault by its unique item identifier.
        /// If the item is found, it returns a <see cref="VaultItem"/> object representing the item.
        /// Otherwise, it returns null if the item does not exist in the vault.
        /// </remarks>
        public async Task<VaultItem> FindVaultItemAsync(string vaultId, ushort itemId)
        {
            MySqlConnection mySqlConnection = CreateConnection();
            return await mySqlConnection.GetTableRowAsync<VaultItem>(tableName: PluginConfig.Database.VaultItemsTable, whereClause: $"VaultId='{vaultId}' AND ItemId='{itemId}'", null);
        }
        #endregion
    }
}
