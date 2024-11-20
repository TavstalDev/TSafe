using System;
using Rocket.Unturned.Player;
using Tavstal.TSafe.Models;

namespace Tavstal.TSafe.Components
{
    public class SafeComponent : UnturnedPlayerComponent
    {
        public bool isSafeOpened = false;
        public string VaultId { get; set; }
    }
}