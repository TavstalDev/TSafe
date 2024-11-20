using System;
using Newtonsoft.Json;
using Tavstal.TLibrary.Models.Database.Attributes;

namespace Tavstal.TSafe.Models
{
    public class Vault
    {
        [JsonProperty("id")]
        [SqlMember(columnType: "varchar(36)", isPrimaryKey: true, isNullable: false)]
        public string Id { get; set; }
        [JsonProperty("name")]
        [SqlMember(isNullable: false)]
        public string Name { get; set; }
        [JsonProperty("ownerId")]
        [SqlMember(isUnsigned: true, isNullable: false)]
        public ulong OwnerId { get; set; }
        [JsonProperty("sizeX")]
        [SqlMember(isNullable: false)]
        public byte SizeX { get; set; }
        [JsonProperty("sizeY")]
        [SqlMember(isNullable: false)]
        public byte SizeY { get; set; }

        public Vault() { }

        public Vault(string id, string name, ulong ownerId, byte sizeX, byte sizeY)
        {
            Id = id;
            Name = name;
            OwnerId = ownerId;
            SizeX = sizeX;
            SizeY = sizeY;
        }
        
        public Vault(string name, ulong ownerId, byte sizeX, byte sizeY)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            OwnerId = ownerId;
            SizeX = sizeX;
            SizeY = sizeY;
        }
    }
}