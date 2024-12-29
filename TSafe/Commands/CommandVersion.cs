using Rocket.API;
using System.Collections.Generic;
using System.Reflection;
using Tavstal.TLibrary.Helpers.Unturned;

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
            // Please do not remove this region and its code, because the license require credits to the author.
            #region Credits to Tavstal
            TSafe.Instance.SendPlainCommandReply(caller, "#########################################");
            TSafe.Instance.SendPlainCommandReply(caller, $"# This plugin uses TLibrary.");
            TSafe.Instance.SendPlainCommandReply(caller, $"# TLibrary Created By: Tavstal");
            TSafe.Instance.SendPlainCommandReply(caller, $"# Github: https://github.com/TavstalDev/TLibrary/tree/master");
            #endregion
            TSafe.Instance.SendPlainCommandReply(caller, "#########################################");
            TSafe.Instance.SendPlainCommandReply(caller, $"# Build Version: {TSafe.Version}");
            TSafe.Instance.SendPlainCommandReply(caller, $"# Build Date: {TSafe.BuildDate}");
            TSafe.Instance.SendPlainCommandReply(caller, "#########################################");
        }
    }
}
