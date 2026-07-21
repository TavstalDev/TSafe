using System.Collections.Generic;
using System.Reflection;
using Tavstal.TLibrary.Helpers.Unturned;
using Rocket.API;
// ReSharper disable UnusedType.Global

namespace Tavstal.TSafe.Commands
{
    public class CommandVersion : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => ("v" + Assembly.GetExecutingAssembly().GetName().Name);
        public string Help => "Gets the version of the plugin";
        public string Syntax => "";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "tsafe.command.version", "tsafe.commands.version" };
        
        public void Execute(IRocketPlayer caller, string[] command)
        {
            var instance = TSafe.Instance;
            var config = instance.Config.General;
            var icon = config.MessageIcon;
            string message = string.Join(System.Environment.NewLine, 
                $"&b&l[{instance.GetPluginName()}]&r System Info:",
                $"&b • Version: &r{TSafe.Version}",
                $"&b • Build Date: &r{TSafe.BuildDate}",
                "&b • Developer: &rTavstal");
            
            instance.SendPlainCommandReply(caller, message, icon);
        }
    }
}
