using System;
using SDG.Unturned;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavstal.TLibrary;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TSafe.Utils.Handlers;
using Tavstal.TSafe.Utils.Managers;

namespace Tavstal.TSafe
{
    /// <summary>
    /// The main plugin class.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class TSafe : PluginBase<TSafeConfig>
    {
        public static TSafe Instance { get; private set; }
        internal static bool IsShuttingDown { get; set; }
        public static DatabaseManager DatabaseManager { get; private set; }
        /// <summary>
        /// Used to prevent error spamming that is related to database configuration.
        /// </summary>
        public static bool IsConnectionAuthFailed { get; set; }

        private int _time;

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

            Logger.Log("████████╗███████╗ █████╗ ███████╗███████╗", ConsoleColor.Cyan, prefix: null);
            Logger.Log("╚══██╔══╝██╔════╝██╔══██╗██╔════╝██╔════╝", ConsoleColor.Cyan, prefix: null);
            Logger.Log("   ██║   ███████╗███████║█████╗  █████╗  ", ConsoleColor.Cyan, prefix: null);
            Logger.Log("   ██║   ╚════██║██╔══██║██╔══╝  ██╔══╝  ", ConsoleColor.Cyan, prefix: null);
            Logger.Log("   ██║   ███████║██║  ██║██║     ███████╗", ConsoleColor.Cyan, prefix: null);
            Logger.Log("   ╚═╝   ╚══════╝╚═╝  ╚═╝╚═╝     ╚══════╝", ConsoleColor.Cyan, prefix: null);
            Logger.Log("#########################################", prefix: null);
            Logger.Log("#       Thanks for using this plugin!   #", prefix: null);
            Logger.Log("#########################################", prefix: null);
            Logger.Log("# Developed By: Tavstal", prefix: null);
            Logger.Log("# Discord:      @Tavstal", prefix: null);
            Logger.Log("# Website:      https://redstoneplugins.com", prefix: null);
            Logger.Log("# My GitHub:    https://tavstaldev.github.io", prefix: null);
            Logger.Log("#########################################", prefix: null);
            Logger.Log($"# Plugin Version:    {Version}", prefix: null);
            Logger.Log($"# Build Date:        {BuildDate}", prefix: null);
            Logger.Log($"# TLibrary Version:  {LibraryVersion}", prefix: null);
            Logger.Log("#########################################", prefix: null);
            Logger.Log("# Found an issue or have a suggestion?", prefix: null);
            Logger.Log("# Report it here: https://github.com/TavstalDev/TSafe/issues", prefix: null); 
            Logger.Log("#########################################", prefix: null);

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

            try
            {
                if (IsShuttingDown)
                    return;
                
                foreach (var vault in VaultManager.VaultList)
                {
                    VaultManager.CancelVaultDestroy(vault.Key);
                    InteractableStorage storage = (InteractableStorage)vault.Value.StorageDrop.interactable;
                    Task.Run(async () =>
                    {
                        await DatabaseManager.RemoveVaultItemsAsync(vault.Key);
                        await DatabaseManager.AddVaultItemAsync(vault.Key, storage.items.items);
                        VaultManager.DestroyVaultNoQueue(vault.Key);
                    });
                }
            }
            catch
            {
                /* Ignore, shutdown might provide some errors */
            }
        }

        private void Event_OnPluginsLoaded(int i)
        {
            if (IsConnectionAuthFailed)
            {
                Logger.Warning($"# Unloading {GetPluginName()} due to database authentication error.");
                this.UnloadPlugin();
            }
        }

        private void FixedUpdate()
        {
            _time++;
            // Every second
            if (_time % 3000 != 0)
                return;
            
            VaultManager.Update();
            
            // (Default SaveInterval is 300)
            // Executes the code every 300 seconds (5 minutes)
            if (_time % (Config.Database.SaveInterval * 50) != 0)
                return;
            
            Task.Run(async () =>
            {
                foreach (var vault in VaultManager.VaultList)
                {
                    VaultManager.CancelVaultDestroy(vault.Key);
                    InteractableStorage storage = (InteractableStorage)vault.Value.StorageDrop.interactable;
                    await DatabaseManager.RemoveVaultItemsAsync(vault.Key);
                    await DatabaseManager.AddVaultItemAsync(vault.Key, storage.items.items);
                    await MainThreadDispatcher.RunOnMainThreadAsync(() => VaultManager.DestroyVaultNoQueue(vault.Key));
                }
            });
        }

        public override Dictionary<string, string> DefaultLocalization =>
           new Dictionary<string, string>
           {
               { "prefix", $"&e[{GetPluginName()}]" },
               { "error_player_not_found", "&cPlayer was not found." },
               { "error_vault_not_found", "&e{0}&c has no vault." },
               { "error_invalid_item", "&cInvalid item id." },
               { "success_command_safe_open", "&aYou have successfully opened your safe." },
               { "success_command_safe_clear", "&aYou have successfully cleared your safe." },
               { "success_command_safeadmin_clear", "&aYou have successfully clear the safe." },
               { "success_command_safeadmin_clearall", "&aYou have successfully cleared all safes." },
           };
    }
}