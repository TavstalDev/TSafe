using System.Collections.Generic;
using System.Threading.Tasks;
using Rocket.API;
using Rocket.Unturned.Player;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TSafe.Models;
using Tavstal.TSafe.Utils.Managers;

namespace Tavstal.TSafe.Commands
{
    public class CommandSafeAdmin : CommandBase
    {
        protected override IPlugin Plugin => TSafe.Instance; 
        public override AllowedCaller AllowedCaller => AllowedCaller.Player;
        public override string Name => "safeadmin";
        public override string Help => "Manages every virtual storage.";
        public override string Syntax => "list | open | clear | clearall";
        public override List<string> Aliases => new List<string>() { "lockeradmin", "storageadmin", "enderchestadmin", "ecadmin" };
        public override List<string> Permissions => new List<string> { "tsafe.command.safeadmin" };

        protected override List<SubCommand> SubCommands => new List<SubCommand>()
        {
            new SubCommand("open", "Opens a specific player's virtual storage.", "open [player]", new List<string>() { "view" }, new List<string>() { "tsafe.command.safeadmin.open" }, 
                async (caller,  args) =>
                {
                    UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
                    if (args.Length != 1)
                    {
                        await ExecuteHelp(caller, false, "open", args);
                        return;
                    }

                    if (!ulong.TryParse(args[0], out ulong targetId))
                    {
                        UnturnedPlayer targetPlayer = UnturnedPlayer.FromName(args[0]);
                        if (targetPlayer == null)
                        {
                            TSafe.Instance.SendCommandReply(caller, "error_player_not_found");
                            return;
                        }

                        targetId = targetPlayer.CSteamID.m_SteamID;
                    }

                    Vault vault = await TSafe.DatabaseManager.FindVaultAsync(targetId);
                    if (vault == null)
                    {
                        TSafe.Instance.SendCommandReply(caller, "error_vault_not_found", args[0]);
                        return;
                    } 
                    await VaultManager.OpenVaultAsync(callerPlayer, vault.Id);
                    TSafe.Instance.SendCommandReply(caller, "success_command_safe_open");
                }),
            new SubCommand("clear", "Clears a specific player's storage or all of a specific item", "clear [player] <item>", new List<string>() { "empty" }, new List<string>() { "tsafe.command.safeadmin.clear" }, 
                async (caller,  args) =>
                {
                    if (args.Length < 1 || args.Length > 2)
                    {
                        await ExecuteHelp(caller, false, "clear", args);
                        return;
                    }
                    
                    if (!ulong.TryParse(args[0], out ulong targetId))
                    {
                        UnturnedPlayer targetPlayer = UnturnedPlayer.FromName(args[0]);
                        if (targetPlayer == null)
                        {
                            TSafe.Instance.SendCommandReply(caller, "error_player_not_found");
                            return;
                        }

                        targetId = targetPlayer.CSteamID.m_SteamID;
                    }

                    Vault vault = await TSafe.DatabaseManager.FindVaultAsync(targetId);
                    if (vault == null)
                    {
                        TSafe.Instance.SendCommandReply(caller, "error_vault_not_found", args[0]);
                        return;
                    }

                    if (args.Length == 2)
                    {
                        if (!ushort.TryParse(args[1], out ushort itemID))
                        {
                            TSafe.Instance.SendCommandReply(caller, "error_vault_not_found", args[0]);
                            return;
                        }
                        await TSafe.DatabaseManager.RemoveVaultItemsAsync(vault.Id, itemID);
                    }
                    else
                        await TSafe.DatabaseManager.RemoveVaultItemsAsync(vault.Id);
                    
                    VaultManager.DestroyVaultNoQueue(vault.Id);
                    TSafe.Instance.SendCommandReply(caller, "success_command_safeadmin_clear");
                }),
            new SubCommand("clearall", "Clears every unit of a specific item from all virtual storage.", "clearall <item>", new List<string>() { "emptyall" }, new List<string>() { "tsafe.command.safeadmin.clearall" }, 
                async (caller,  args) =>
                {
                    if (args.Length > 1)
                    {
                        await ExecuteHelp(caller, false, "clearall", args);
                        return;
                    }

                    ushort itemID = 0;
                    if (args.Length == 1)
                        if (!ushort.TryParse(args[0], out itemID))
                        {
                            TSafe.Instance.SendCommandReply(caller, "error_invalid_item", args[0]);
                            return;
                        }

                    foreach (Vault vault in await TSafe.DatabaseManager.GetVaultsAsync())
                    {
                        if (itemID == 0)
                            await TSafe.DatabaseManager.RemoveVaultItemsAsync(vault.Id);
                        else
                            await TSafe.DatabaseManager.RemoveVaultItemsAsync(vault.Id, itemID);
                        VaultManager.DestroyVaultNoQueue(vault.Id);
                    }
                    TSafe.Instance.SendCommandReply(caller, "success_command_safeadmin_clearall");
                }),
        };
        
        protected override Task<bool> ExecutionRequested(IRocketPlayer caller, string[] args)
        {
            return Task.FromResult(false);
        }
    }
}