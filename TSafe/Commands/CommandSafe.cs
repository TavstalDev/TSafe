using System;
using Rocket.API;
using System.Collections.Generic;
using Rocket.Unturned.Player;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Database;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TSafe.Models;
using Tavstal.TSafe.Utils.Helpers;
using Tavstal.TSafe.Utils.Managers;
// ReSharper disable UnusedType.Global

namespace Tavstal.TSafe.Commands
{
    public class CommandSafe : CustomCommandBase
    {
        public override IPlugin Plugin => TSafe.Instance;
        public override bool UseBackgroundThread => false;

        public override AllowedCaller AllowedCaller => AllowedCaller.Player;
        public override string Name => "safe";
        public override string Help => "Manages the player's virtual storage.";
        public override string Syntax => "open | clear";
        public override List<string> Aliases => new List<string> { "locker", "storage", "enderchest", "ec" };
        public override List<string> Permissions => new List<string> { "tsafe.command.safe", "tsafe.commands.safe", };

        // 'help' subcommand is built-in, you don't need to add it
        public override List<ISubcommand> SubCommands => new List<ISubcommand>
        {
            new SubCommand("open", "Opens the virtual storage.", "open", new List<string> { "view" }, new List<string> { "tsafe.command.safe.open" },
                Plugin, AllowedCaller,
                async (caller,  args) =>
                {
                    UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
                    List<Vault>? vaults = await TSafe.DatabaseManager.Vaults.GetAsync(queryParameters: QueryParameter.eq("OwnerId", callerPlayer.CSteamID.m_SteamID));
                    if (vaults != null && vaults.Count > 0)
                    {
                        Vault vault = vaults[0];
                        await VaultManager.OpenVaultAsync(callerPlayer, vault.Id);
                        TSafe.Instance.SendCommandReply(caller, "success_command_safe_open", TSafe.Instance.Config.General.MessageIcon);
                        return;
                    }
                    
                    var vaultSize = VaultHelper.GetVaultSize(callerPlayer);

                    string guid = Guid.NewGuid().ToString();
                    await TSafe.DatabaseManager.Vaults.AddAsync(new Vault(guid, callerPlayer.CharacterName, callerPlayer.CSteamID.m_SteamID, vaultSize.x, vaultSize.y));
                    await VaultManager.OpenVaultAsync(callerPlayer, guid);
                    TSafe.Instance.SendCommandReply(caller, "success_command_safe_open", TSafe.Instance.Config.General.MessageIcon);
                }),
            new SubCommand("clear", "Clears the virtual storage.", "clear", new List<string> { "empty" }, new List<string> { "tsafe.command.safe.clear" }, 
                Plugin, AllowedCaller,
                async (caller,  args) =>
                {
                    UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
                    List<Vault>? vaults = await TSafe.DatabaseManager.Vaults.GetAsync(1, QueryParameter.eq("OwnerId", callerPlayer.CSteamID.m_SteamID));
                    if (vaults != null && vaults.Count > 0)
                    {
                        Vault vault = vaults[0];
                        await TSafe.DatabaseManager.Vaults.DeleteAsync(vault.Id);
                        VaultManager.DestroyVaultNoQueue(vault.Id);
                        TSafe.Instance.SendCommandReply(caller, "success_command_safe_clear", TSafe.Instance.Config.General.MessageIcon);
                        return;
                    }
                    TSafe.Instance.SendCommandReply(caller, "success_command_safe_clear", TSafe.Instance.Config.General.MessageIcon);
                }),
        };

        protected override bool HandleExecute(IRocketPlayer caller, string[] command) => false;
    }
}
