using Rocket.Unturned.Player;

namespace Tavstal.TSafe.Components
{
    public class SafeComponent : UnturnedPlayerComponent
    {
        public bool isSafeOpened = false;
        public string VaultId { get; set; }
    }
}