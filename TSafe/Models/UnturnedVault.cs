using SDG.Unturned;

namespace Tavstal.TSafe.Models
{
    public class UnturnedVault
    {
        public BarricadeDrop StorageDrop { get; set; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }

        public UnturnedVault()
        {
            StorageDrop = null!;
        }
        
        public UnturnedVault(BarricadeDrop storageDrop, int sizeX, int sizeY)
        {
            StorageDrop = storageDrop;
            SizeX = sizeX;
            SizeY = sizeY;
        }
    }
}