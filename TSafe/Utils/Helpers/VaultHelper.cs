using System.Linq;
using Rocket.API;
using Rocket.Unturned.Player;
using Tavstal.TSafe.Models;

namespace Tavstal.TSafe.Utils.Helpers
{
    /// <summary>
    /// Provides helper methods for vault size calculations.
    /// </summary>
    public static class VaultHelper
    {
        /// <summary>
        /// Determines the vault grid size for a player based on their highest matching permission group.
        /// </summary>
        /// <param name="player">The player to evaluate.</param>
        /// <returns>
        /// A tuple of (<c>x</c>, <c>y</c>) representing the vault column and row count.
        /// Returns the highest-priority matching group's size, or the configured defaults if no group matches.
        /// </returns>
        public static (byte x, byte y) GetVaultSize(UnturnedPlayer player)
        {
            byte x = TSafe.Instance.Config.DefaultRowX;
            byte y = TSafe.Instance.Config.DefaultRowY;
            var localGroups = TSafe.Instance.Config.Groups.ToList();
            localGroups.Reverse();
                        
            foreach (Group group in localGroups)
            {
                if (player.HasPermission(group.Permission))
                {
                    x = group.SizeX;
                    y = group.SizeY;
                    break;
                }
            }
            
            return (x, y);
        }
    }
}