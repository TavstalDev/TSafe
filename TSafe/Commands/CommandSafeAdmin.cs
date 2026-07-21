using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Player;
using Tavstal.TLibrary.Extensions.Unturned;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Database;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TSafe.Models;
using Tavstal.TSafe.Utils.Managers;
// ReSharper disable UnusedType.Global

namespace Tavstal.TSafe.Commands
{
    public class CommandSafeAdmin : CustomCommandBase
    {
        public override IPlugin Plugin => TSafe.Instance; 
        public override bool UseBackgroundThread => false;
        
        public override AllowedCaller AllowedCaller => AllowedCaller.Player;
        public override string Name => "safeadmin";
        public override string Help => "Manages every virtual storage.";
        public override string Syntax => "list | open | clear | clearall";
        public override List<string> Aliases => new List<string> { "lockeradmin", "storageadmin", "enderchestadmin", "ecadmin" };
        public override List<string> Permissions => new List<string> { "tsafe.command.safeadmin", "tsafe.commands.safeadmin" };

        public override List<ISubcommand> SubCommands => new List<ISubcommand>
        {
            new SubCommand("open", "Opens a specific player's virtual storage.", "open [player]", new List<string> { "view" }, new List<string> { "tsafe.command.safeadmin.open" }, 
                Plugin, AllowedCaller,
                async (caller,  args) =>
                {
                    UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
                    if (args.Length != 1)
                    {
                        this.ExecuteHelp(caller, false, "open");
                        return;
                    }

                    if (!ulong.TryParse(args[0], out ulong targetId))
                    {
                        UnturnedPlayer targetPlayer = UnturnedPlayer.FromName(args[0]);
                        if (targetPlayer == null)
                        {
                            TSafe.Instance.SendCommandReply(caller, "error_player_not_found", TSafe.Instance.Config.General.MessageIcon);
                            return;
                        }

                        targetId = targetPlayer.CSteamID.m_SteamID;
                    }

                    List<Vault>? vaults = await TSafe.DatabaseManager.Vaults.GetAsync(queryParameters: QueryParameter.eq("OwnerId", targetId));
                    if (vaults == null || vaults.Count == 0)
                    {
                        TSafe.Instance.SendCommandReply(caller, "error_vault_not_found", TSafe.Instance.Config.General.MessageIcon, args[0]);
                        return;
                    } 
                    await VaultManager.OpenVaultAsync(callerPlayer, vaults[0].Id);
                    TSafe.Instance.SendCommandReply(caller, "success_command_safe_open", TSafe.Instance.Config.General.MessageIcon);
                }),
            new SubCommand("clear", "Clears a specific player's storage or all of a specific item", "clear [player]", new List<string> { "empty" }, new List<string> { "tsafe.command.safeadmin.clear" }, 
                Plugin, AllowedCaller,
                async (caller,  args) =>
                {
                    if (args.Length < 1 || args.Length > 2)
                    {
                        this.ExecuteHelp(caller, false, "clear");
                        return;
                    }
                    
                    if (!ulong.TryParse(args[0], out ulong targetId))
                    {
                        UnturnedPlayer targetPlayer = UnturnedPlayer.FromName(args[0]);
                        if (targetPlayer == null)
                        {
                            TSafe.Instance.SendCommandReply(caller, "error_player_not_found", TSafe.Instance.Config.General.MessageIcon);
                            return;
                        }

                        targetId = targetPlayer.CSteamID.m_SteamID;
                    }

                    List<Vault>? vaults = await TSafe.DatabaseManager.Vaults.GetAsync(queryParameters: QueryParameter.eq("OwnerId", targetId));
                    if (vaults == null || vaults.Count == 0)
                    {
                        TSafe.Instance.SendCommandReply(caller, "error_vault_not_found", TSafe.Instance.Config.General.MessageIcon, args[0]);
                        return;
                    }

                    Vault vault = vaults[0];
                    await TSafe.DatabaseManager.Items.DeleteRangeAsync("VaultId", new List<object> { vault.Id });
                    
                    VaultManager.DestroyVaultNoQueue(vault.Id);
                    TSafe.Instance.SendCommandReply(caller, "success_command_safeadmin_clear", TSafe.Instance.Config.General.MessageIcon);
                }),
            new SubCommand("clearall", "Clears every unit of a specific item from all virtual storage.", "clearall", new List<string> { "emptyall" }, new List<string> { "tsafe.command.safeadmin.clearall" }, 
                Plugin, AllowedCaller,
                async (caller,  args) =>
                {
                    if (args.Length > 1)
                    {
                        this.ExecuteHelp(caller, false, "clearall");
                        return;
                    }

                    // TODO: Allow adjustable limit
                    foreach (Vault vault in await TSafe.DatabaseManager.Vaults.GetAsync(queryParameters: QueryParameter.not("VaultId", "0")) ?? new List<Vault>())
                    {
                        await TSafe.DatabaseManager.Items.DeleteRangeAsync("Id", new List<object> { vault.Id });
                        VaultManager.DestroyVaultNoQueue(vault.Id);
                    }
                    TSafe.Instance.SendCommandReply(caller, "success_command_safeadmin_clearall", TSafe.Instance.Config.General.MessageIcon);
                }),
        };
        
        protected override bool HandleExecute(IRocketPlayer caller, string[] command) => false;
    }
}