using YamlDotNet.Serialization;

namespace Tavstal.TSafe.Models
{
    public class Group
    {
        [YamlMember(Order = 0)]
        public string Name { get; set; }
        
        [YamlMember(Order = 1)]
        public string Permission { get; set; }
        
        [YamlMember(Order = 2)]
        public byte SizeX { get; set; }
        
        [YamlMember(Order = 3)]
        public byte SizeY { get; set; }

        public Group()
        {
            Name = string.Empty;
            Permission = string.Empty;
        }

        public Group(string name, string permission, byte sizeX, byte sizeY)
        {
            Name = name;
            Permission = permission;
            SizeX = sizeX;
            SizeY = sizeY;
        }
    }
}