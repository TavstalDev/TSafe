using System.Collections.Generic;
using Newtonsoft.Json;
using Tavstal.TSafe.Models;
using Tavstal.TLibrary.Models.Plugin;

namespace Tavstal.TSafe
{
    // ReSharper disable once InconsistentNaming
    public class TSafeConfig : ConfigurationBase
    {
        [JsonProperty(Order = 3)]
        public DatabaseData Database { get; set; }
        [JsonProperty(Order = 4)]
        public byte DefaultRowX;
        [JsonProperty(Order = 5)]
        public byte DefaultRowY;
        [JsonProperty(Order = 6)]
        public List<Group> Groups { get; set; }

        public override void LoadDefaults()
        {
            DebugMode = false;
            Locale = "en";
            DownloadLocalePacks = true;
            Database = new DatabaseData("tsafe_vault", "tsafe_vaultitems");
            DefaultRowX = 5;
            DefaultRowY = 5;
            Groups = new List<Group>
            {
                new Group { Name = "VIP", Permission = "tsafe.safe.vip", SizeX = 8, SizeY = 5 },
                new Group { Name = "MVP", Permission = "tsafe.safe.mvp", SizeX = 10, SizeY = 10 },
                new Group { Name = "Donor", Permission = "tsafe.safe.donor", SizeX = 10, SizeY = 15 },
                new Group { Name = "Admin", Permission = "tsafe.safe.admin", SizeX = 10, SizeY = 20 }
            };
        }

        // Required because of the library
        public TSafeConfig() { }
        public TSafeConfig(string fileName, string path) : base(fileName, path) { }
    }
}
