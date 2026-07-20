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
        public List<string> Permissions => new List<string> { "tsafe.command.version" };
        
        public void Execute(IRocketPlayer caller, string[] command)
        {
            TSafe.Instance.SendPlainCommandReply(caller, "#########################################");
            TSafe.Instance.SendPlainCommandReply(caller, $"# Build Version: {TSafe.Version}");
            TSafe.Instance.SendPlainCommandReply(caller, $"# Build Date: {TSafe.BuildDate}");
            TSafe.Instance.SendPlainCommandReply(caller, "#########################################");
        }
    }
}
