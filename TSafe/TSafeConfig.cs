using System.Collections.Generic;
using Tavstal.TLibrary.Models.Config;
using Tavstal.TLibrary.Models.Logging;
using Tavstal.TSafe.Models;
using YamlDotNet.Serialization;

namespace Tavstal.TSafe
{
    // ReSharper disable once InconsistentNaming
    public class TSafeConfig : YamlConfiguration
    {
        [YamlMember(Order = 3)]
        public DatabaseData Database { get; set; }
        [YamlMember(Order = 4)]
        public byte DefaultRowX;
        [YamlMember(Order = 5)]
        public byte DefaultRowY;
        [YamlMember(Order = 6)]
        public List<Group> Groups { get; set; }

        public override void LoadDefaults()
        {
            Locale = "en";
            LogLevel = ELogLevel.INFO;
            DownloadLocalePacks = true;
            Database = new DatabaseData("tsafe_", 300);
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
        public TSafeConfig()
        {
            Database = new DatabaseData("tsafe_", 300);
            Groups = new List<Group>();
        }

        public TSafeConfig(string fileName, string path) : base(fileName, path)
        {
            Database = new DatabaseData("tsafe_", 300);
            Groups = new List<Group>();
        }
    }
}
