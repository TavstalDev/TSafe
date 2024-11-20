using Newtonsoft.Json;
using Tavstal.TLibrary.Models.Database;

namespace Tavstal.TSafe.Models
{
    public class DatabaseData : DatabaseSettingsBase
    {
        // Note: It starts from 7 because there are 6 defined property in the base class
        [JsonProperty(Order = 7)]
        public string VaultTable { get; set; }
        [JsonProperty(Order = 8)]
        public string VaultItemsTable { get; set; }
        [JsonProperty(Order = 9)]
        public int SaveInterval { get; set; }
        
        public DatabaseData(string vaultTable, string vaultItemsTable, int saveInterval)
        {
            VaultTable = vaultTable;
            VaultItemsTable = vaultItemsTable;
            SaveInterval = saveInterval;
        }
    }
}
