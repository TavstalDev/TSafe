using Newtonsoft.Json;

namespace Tavstal.TSafe.Models
{
    public class Group
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("permission")]
        public string Permission { get; set; }
        [JsonProperty("sizeX")]
        public byte SizeX { get; set; }
        [JsonProperty("sizeY")]
        public byte SizeY { get; set; }

        public Group() { }

        public Group(string name, string permission, byte sizeX, byte sizeY)
        {
            Name = name;
            Permission = permission;
            SizeX = sizeX;
            SizeY = sizeY;
        }
    }
}