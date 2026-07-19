using System;
using Tavstal.TLibrary.Models.Database.Attributes;
using YamlDotNet.Serialization;

namespace Tavstal.TSafe.Models
{
    public class Vault
    {
        [YamlMember(Order = 0)]
        [SqlMember(columnType: "varchar(36)", isPrimaryKey: true, isNullable: false)]
        public string Id { get; set; }
        
        [YamlMember(Order = 1)]
        [SqlMember(isNullable: false)]
        public string Name { get; set; }
        
        [YamlMember(Order = 2)]
        [SqlMember(isUnsigned: true, isNullable: false)]
        public ulong OwnerId { get; set; }
        
        [YamlMember(Order = 3)]
        [SqlMember(isNullable: false)]
        public byte SizeX { get; set; }
        
        [YamlMember(Order = 4)]
        [SqlMember(isNullable: false)]
        public byte SizeY { get; set; }

        public Vault()
        {
            Id = string.Empty;
            Name = string.Empty;
        }

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