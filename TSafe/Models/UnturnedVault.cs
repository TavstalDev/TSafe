using Rocket.Unturned.Player;
using SDG.Unturned;

namespace Tavstal.TSafe.Models
{
    public class UnturnedVault
    {
        public readonly BarricadeDrop StorageDrop;
        public readonly int SizeX;
        public readonly int SizeY;

        public UnturnedVault() { }
        
        public UnturnedVault(BarricadeDrop storageDrop, int sizeX, int sizeY)
        {
            StorageDrop = storageDrop;
            SizeX = sizeX;
            SizeY = sizeY;
        }
    }
}