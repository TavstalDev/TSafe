using System;
using SDG.Unturned;
using Tavstal.TLibrary.Models.Database.Attributes;
using YamlDotNet.Serialization;

namespace Tavstal.TSafe.Models
{
    public class VaultItem
    {
        [YamlMember(Order = 0)]
        [SqlMember(columnType: "varchar(36)")]
        public string Id { get; set; }
        
        [YamlMember(Order = 1)]
        [SqlMember(columnType: "varchar(36)")]
        public string VaultId { get; set; }
        
        [YamlMember(Order = 2)]
        [SqlMember(isUnsigned: true)]
        public ushort ItemId { get; set; }
        
        [YamlMember(Order = 3)]
        [SqlMember]
        public byte Amount { get; set; }
        
        [YamlMember(Order = 4)]
        [SqlMember]
        public byte Quality { get; set; }
        
        [YamlMember(Order = 5)]
        [SqlMember]
        public string StateBase64 { get; set; }
        
        [YamlMember(Order = 6)]
        [SqlMember]
        public byte X { get; set; }
        
        [YamlMember(Order = 7)]
        [SqlMember]
        public byte Y { get; set; }
        
        [YamlMember(Order = 8)]
        [SqlMember]
        public byte Rot { get; set; }

        public VaultItem()
        {
            Id = Guid.NewGuid().ToString();
            VaultId = string.Empty;
            StateBase64 = string.Empty;
        }
        
        public VaultItem(string vaultId, ItemJar itemJar)
        {
            Id = Guid.NewGuid().ToString();
            VaultId = vaultId;
            ItemId = itemJar.item.id;
            Amount = itemJar.item.amount;
            Quality = itemJar.item.quality;
            StateBase64 = Convert.ToBase64String(itemJar.item.metadata);
            X = itemJar.x;
            Y = itemJar.y;
            Rot = itemJar.rot;
        }

        public VaultItem(string id, string vaultId, ushort itemId, byte amount, byte quality, byte[] state, byte x, byte y, byte rot)
        {
            Id = id;
            VaultId = vaultId;
            ItemId = itemId;
            Amount = amount;
            Quality = quality;
            StateBase64 = Convert.ToBase64String(state);
            X = x;
            Y = y;
            Rot = rot;
        }

        public Item ToItem() =>
            new Item(ItemId, Amount, Quality, Convert.FromBase64String(StateBase64));
    }
}