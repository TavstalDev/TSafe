using Rocket.Unturned.Player;
using SDG.Unturned;

namespace Tavstal.TSafe.Models
{
    public class UnturnedVault
    {
        public readonly BarricadeDrop StorageDrop;
        public readonly UnturnedPlayer Player;
        public readonly int SizeX;
        public readonly int SizeY;

        public UnturnedVault() { }
        
        public UnturnedVault(BarricadeDrop storageDrop, UnturnedPlayer player, int sizeX, int sizeY)
        {
            StorageDrop = storageDrop;
            Player = player;
            SizeX = sizeX;
            SizeY = sizeY;
        }
    }
}