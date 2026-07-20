using System;
using System.Threading.Tasks;
using Tavstal.TLibrary.Extensions;
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
        public MySqlRepository<string, Vault> Vaults { get;  }
        public MySqlRepository<string, VaultItem> Items { get; }

        public DatabaseManager(IPlugin plugin, TSafeConfig config) : base(plugin, config.Database)
        {
            Vaults = new MySqlRepository<string, Vault>(this, config.Database.TablePrefix);
            Items = new MySqlRepository<string, VaultItem>(this, config.Database.TablePrefix);
        }

        /// <summary>
        /// Checks the schema of the database, creates or modifies the tables if needed
        /// <br/>PS. If you change the Primary Key then you must delete the table.
        /// </summary>
        public override async Task CheckSchemaAsync()
        {
            try
            {
                await using var connection = CreateConnection();
                await Vaults.CheckSchemaAsync(connection);
                await Items.CheckSchemaAsync(connection);
            }
            catch (Exception ex)
            {
                TSafe.Logger.Error("Error in checkSchema:", ex);
            }
        }
    }
}
