using System.Collections.Generic;
using System.Threading.Tasks;
using Rocket.API;
using Rocket.Unturned.Player;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;

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
                    
                    
                    
                }),
            new SubCommand("clear", "Clears a specific player's storage or all of a specific item", "clear [player] <item>", new List<string>() { "empty" }, new List<string>() { "tsafe.command.safeadmin.clear" }, 
                async (caller,  args) =>
                {
                    UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
                    
                }),
            new SubCommand("clearall", "Clears every unit of a specific item from all virtual storage.", "clearall <item>", new List<string>() { "emptyall" }, new List<string>() { "tsafe.command.safeadmin.clearall" }, 
                async (caller,  args) =>
                {
                    UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
                    
                }),
        };
        
        protected override Task<bool> ExecutionRequested(IRocketPlayer caller, string[] args)
        {
            return Task.FromResult(false);
        }
    }
}