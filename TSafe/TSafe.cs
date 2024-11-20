using System;
using SDG.Unturned;
using System.Collections.Generic;
using Tavstal.TSafe.Handlers;
using Tavstal.TSafe.Managers;
using Tavstal.TLibrary.Models.Plugin;

namespace Tavstal.TSafe
{
    /// <summary>
    /// The main plugin class.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class TSafe : PluginBase<TSafeConfig>
    {
        public static TSafe Instance { get; private set; }
        public new static readonly TLogger Logger = new TLogger("TSafe", false);
        public static DatabaseManager DatabaseManager { get; private set; }
        /// <summary>
        /// Used to prevent error spamming that is related to database configuration.
        /// </summary>
        public static bool IsConnectionAuthFailed { get; set; }

        private int _time = 0;

        /// <summary>
        /// Fired when the plugin is loaded.
        /// </summary>
        public override void OnLoad()
        {
            Instance = this;
            // Attach event, which will be fired when all plugins are loaded.
            Level.onPostLevelLoaded += Event_OnPluginsLoaded;
            // Attach player related events
            PlayerEventHandler.AttachEvents();

            Logger.LogWarning("████████╗███████╗ █████╗ ███████╗███████╗");
            Logger.LogWarning("╚══██╔══╝██╔════╝██╔══██╗██╔════╝██╔════╝");
            Logger.LogWarning("   ██║   ███████╗███████║█████╗  █████╗  ");
            Logger.LogWarning("   ██║   ╚════██║██╔══██║██╔══╝  ██╔══╝  ");
            Logger.LogWarning("   ██║   ███████║██║  ██║██║     ███████╗");
            Logger.LogWarning("   ╚═╝   ╚══════╝╚═╝  ╚═╝╚═╝     ╚══════╝");
            Logger.Log("#########################################");
            Logger.Log("# Thanks for using my plugin");
            Logger.Log("# Plugin Created By Tavstal");
            Logger.Log("# Discord: Tavstal");
            Logger.Log("# Website: https://redstoneplugins.com");
            // Please do not remove this region and its code, because the license require credits to the author.
            #region Credits to Tavstal
            Logger.Log("#########################################");
            Logger.Log($"# This plugin uses TLibrary.");
            Logger.Log($"# TLibrary Created By: Tavstal"); 
            Logger.Log($"# Github: https://github.com/TavstalDev/TLibrary/tree/master");
            #endregion
            Logger.Log("#########################################");
            Logger.Log($"# Build Version: {Version}");
            Logger.Log($"# Build Date: {BuildDate}");
            Logger.Log("#########################################");

            DatabaseManager = new DatabaseManager(this, Config);
            if (IsConnectionAuthFailed)
                return;

            Logger.Log($"# {GetPluginName()} has been loaded.");
        }

        /// <summary>
        /// Fired when the plugin is unloaded.
        /// </summary>
        public override void OnUnLoad()
        {
            Level.onPostLevelLoaded -= Event_OnPluginsLoaded;
            PlayerEventHandler.DetachEvents();
            Logger.Log($"# {GetPluginName()} has been successfully unloaded.");

            foreach (var vault in VaultManager.VaultList)
            {
                VaultManager.PreventVaultDestroy(vault.Key);
                VaultManager.DestroyVaultNoQueue(vault.Key);
            }
        }

        private void Event_OnPluginsLoaded(int i)
        {
            if (IsConnectionAuthFailed)
            {
                Logger.LogWarning($"# Unloading {GetPluginName()} due to database authentication error.");
                this?.UnloadPlugin();
                return;
            }
        }

        private void Update()
        {
            _time++;
            if (_time % 60 == 0)
                return;
            
            VaultManager.Update();
        }

        public override Dictionary<string, string> DefaultLocalization =>
           new Dictionary<string, string>
           {
               { "prefix", $"&e[{GetPluginName()}]" },
               { "error_player_not_found", "&cPlayer was not found." },
           };
    }
}