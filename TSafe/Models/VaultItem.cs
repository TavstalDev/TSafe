using System;
using Newtonsoft.Json;
using SDG.Unturned;
using Tavstal.TLibrary.Models.Database.Attributes;

namespace Tavstal.TSafe.Models
{
    public class VaultItem
    {
        [JsonProperty("vaultId")]
        [SqlMember(columnType: "varchar(36)")]
        public string VaultId { get; set; }
        [JsonProperty("itemId")]
        [SqlMember(isUnsigned: true)]
        public ushort ItemId { get; set; }
        [JsonProperty("amount")]
        [SqlMember]
        public byte Amount { get; set; }
        [JsonProperty("quality")]
        [SqlMember]
        public byte Quality { get; set; }
        [JsonProperty("state")]
        [SqlMember]
        public string StateBase64 { get; set; }
        [JsonProperty("x")]
        [SqlMember]
        public byte X { get; set; }
        [JsonProperty("y")]
        [SqlMember]
        public byte Y { get; set; }
        [JsonProperty("rot")]
        [SqlMember]
        public byte Rot { get; set; }
        
        public VaultItem() {}

        public VaultItem(string vaultId, ushort itemId, byte amount, byte quality, byte[] state, byte x, byte y, byte rot)
        {
            VaultId = vaultId;
            ItemId = itemId;
            Amount = amount;
            Quality = quality;
            StateBase64 = Convert.ToBase64String(state);
            X = x;
            Y = y;
            Rot = rot;
        }
        
        public VaultItem(ushort itemId, byte amount, byte quality, byte[] state, byte x, byte y, byte rot)
        {
            VaultId = Guid.NewGuid().ToString();
            ItemId = itemId;
            Amount = amount;
            Quality = quality;
            StateBase64 = Convert.ToBase64String(state);
            X = x;
            Y = y;
            Rot = rot;
        }

        public Item ToItem()
        {
            return new Item(ItemId, Amount, Quality, Convert.FromBase64String(StateBase64));
        }
    }
}