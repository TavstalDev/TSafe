using Tavstal.TLibrary.Models.Database;
using YamlDotNet.Serialization;

namespace Tavstal.TSafe.Models
{
    public class DatabaseData : DatabaseSettingsBase
    {
        // Note: It starts from 7 because there are 6 defined property in the base class
        [YamlMember(Order = 7)]
        public string TablePrefix { get; set; }
        
        [YamlMember(Order = 8)]
        public int SaveInterval { get; set; }
        
        public DatabaseData(string tablePrefix, int saveInterval)
        {
            TablePrefix = tablePrefix;
            SaveInterval = saveInterval;
        }

        public DatabaseData()
        {
            TablePrefix = "tsafe_";
            SaveInterval = 300;
        }
    }
}
