using System;
using Rocket.API;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rocket.Unturned.Player;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TSafe.Models;
using Tavstal.TSafe.Utils.Managers;

namespace Tavstal.TSafe.Commands
{
    public class CommandSafe : CommandBase
    {
        protected override IPlugin Plugin => TSafe.Instance; 
        public override AllowedCaller AllowedCaller => AllowedCaller.Player;
        public override string Name => "safe";
        public override string Help => "Manages the player's virtual storage.";
        public override string Syntax => "open | clear";
        public override List<string> Aliases => new List<string>() { "locker", "storage", "enderchest", "ec" };
        public override List<string> Permissions => new List<string> { "tsafe.command.safe" };

        // 'help' subcommand is built-in, you don't need to add it
        protected override List<SubCommand> SubCommands => new List<SubCommand>()
        {
            new SubCommand("open", "Opens the virtual storage.", "open", new List<string>() { "view" }, new List<string>() { "tsafe.command.safe.open" }, 
                async (caller,  args) =>
                {
                    UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
                    Vault vault = await TSafe.DatabaseManager.FindVaultAsync(callerPlayer.CSteamID.m_SteamID);
                    if (vault == null)
                    {
                        byte x = TSafe.Instance.Config.DefaultRowX;
                        byte y = TSafe.Instance.Config.DefaultRowY;
                        var localGroups = TSafe.Instance.Config.Groups;
                        localGroups.Reverse();
                        
                        foreach (Group group in localGroups)
                        {
                            if (callerPlayer.HasPermission(group.Permission))
                            {
                                x = group.SizeX;
                                y = group.SizeY;
                                break;
                            }
                        }

                        string guid = Guid.NewGuid().ToString();
                        await TSafe.DatabaseManager.AddVaultAsync(guid, callerPlayer.CharacterName, callerPlayer.CSteamID.m_SteamID, x, y);
                        await VaultManager.OpenVaultAsync(callerPlayer, guid);
                        TSafe.Instance.SendCommandReply(caller, "success_command_safe_open");
                        return;
                    }
                    await VaultManager.OpenVaultAsync(callerPlayer, vault.Id);
                    TSafe.Instance.SendCommandReply(caller, "success_command_safe_open");
                }),
            new SubCommand("clear", "Clears the virtual storage.", "clear", new List<string>() { "empty" }, new List<string>() { "tsafe.command.safe.clear" }, 
                async (caller,  args) =>
                {
                    UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
                    Vault vault = await TSafe.DatabaseManager.FindVaultAsync(callerPlayer.CSteamID.m_SteamID);
                    if (vault == null)
                    {
                        byte x = TSafe.Instance.Config.DefaultRowX;
                        byte y = TSafe.Instance.Config.DefaultRowY;
                        var localGroups = TSafe.Instance.Config.Groups;
                        localGroups.Reverse();
                        
                        foreach (Group group in localGroups)
                        {
                            if (callerPlayer.HasPermission(group.Permission))
                            {
                                x = group.SizeX;
                                y = group.SizeY;
                                break;
                            }
                        }
                        
                        await TSafe.DatabaseManager.AddVaultAsync(Guid.NewGuid().ToString(), callerPlayer.CharacterName, callerPlayer.CSteamID.m_SteamID, x, y);
                        TSafe.Instance.SendCommandReply(caller, "success_command_safe_clear");
                        return;
                    }

                    await TSafe.DatabaseManager.RemoveVaultItemsAsync(vault.Id);
                    VaultManager.DestroyVaultNoQueue(vault.Id);
                    TSafe.Instance.SendCommandReply(caller, "success_command_safe_clear");
                }),
        };

        protected override Task<bool> ExecutionRequested(IRocketPlayer caller, string[] args)
        {
            return Task.FromResult(false);
        }
    }
}
