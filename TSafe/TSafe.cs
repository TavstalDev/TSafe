using System;
using System.Collections.Concurrent;
using SDG.Unturned;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Models.Database;
using Tavstal.TLibrary.Models.Logging;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TLibrary.Threading;
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
        public static TSafe Instance { get; private set; } = null!;
        internal static bool IsShuttingDown { get; set; }
        public static DatabaseManager DatabaseManager { get; private set; } = null!;
        public static bool IsConnectionAuthFailed { get; set; }

        private readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1, 1);
        private int _time;

        
        public override void OnPreLoad()
        {
            Instance = this;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("────────────────────────────────────────────────────────");
            sb.AppendLine();
            sb.AppendLine("████████╗███████╗ █████╗ ███████╗███████╗");
            sb.AppendLine("╚══██╔══╝██╔════╝██╔══██╗██╔════╝██╔════╝");
            sb.AppendLine("   ██║   ███████╗███████║█████╗  █████╗  ");
            sb.AppendLine("   ██║   ╚════██║██╔══██║██╔══╝  ██╔══╝  ");
            sb.AppendLine("   ██║   ███████║██║  ██║██║     ███████╗");
            sb.AppendLine("   ╚═╝   ╚══════╝╚═╝  ╚═╝╚═╝     ╚══════╝");
            sb.AppendLine();
            sb.AppendLine("[ About ]");
            sb.AppendLine(" ▸ Developer : Tavstal");
            sb.AppendLine(" ▸ Discord   : @Tavstal");
            sb.AppendLine(" ▸ Website   : https://redstoneplugins.com");
            sb.AppendLine(" ▸ GitHub    : https://github.com/TavstalDev");
            sb.AppendLine();
            sb.AppendLine("[ Build ]");
            sb.AppendLine($" ▸ Version   : {Version}");
            sb.AppendLine($" ▸ Build Date: {BuildDate} UTC");
            sb.AppendLine($" ▸ TLibrary  : {LibraryVersion}");
            sb.AppendLine();
            sb.AppendLine("[ Support ]");
            sb.AppendLine(" ▸ Report issues or request features:");
            sb.AppendLine(" ▸ https://github.com/TavstalDev/TSafe/issues");
            sb.AppendLine();
            sb.AppendLine("────────────────────────────────────────────────────────");
            Logger.Log(ELogLevel.COMMAND, sb.ToString(), includePrefixes: false, color:  ConsoleColor.Cyan);
        }
        
        /// <summary>
        /// Fired when the plugin is loaded.
        /// </summary>
        public override void OnLoad()
        {
            // Attach event, which will be fired when all plugins are loaded.
            Level.onPostLevelLoaded += Event_OnPluginsLoaded;
            // Attach player related events
            PlayerEventHandler.AttachEvents();

            DatabaseManager = new DatabaseManager(this, Config);
            if (IsConnectionAuthFailed)
                return;

            Logger.Info($"# {GetPluginName()} has been loaded.");
        }

        /// <summary>
        /// Fired when the plugin is unloaded.
        /// </summary>
        public override void OnUnLoad()
        {
            Level.onPostLevelLoaded -= Event_OnPluginsLoaded;
            PlayerEventHandler.DetachEvents();
            Logger.Info($"# {GetPluginName()} has been successfully unloaded.");

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

            // TODO:
            BackgroundThreadDispatcher.RunAsync(async () =>
            {
                await _updateLock.WaitAsync();
                try
                {

                    Dictionary<string, List<ItemJar>> vaultItems = new Dictionary<string, List<ItemJar>>();
                    QueryParameter[] parameters = new QueryParameter[VaultManager.VaultList.Count];
                    foreach (var vault in VaultManager.VaultList)
                    {
                        VaultManager.CancelVaultDestroy(vault.Key);
                        InteractableStorage storage = (InteractableStorage)vault.Value.StorageDrop.interactable;
                        vaultItems.Add(vault.Key, storage.items.items);
                        
                        await DatabaseManager.Items.DeleteAsync(QueryParameter.eq("VaultId", vault.Key));
                        await DatabaseManager.AddVaultItemAsync(vault.Key, storage.items.items);
                        await MainThreadDispatcher.RunAsync(() => VaultManager.DestroyVaultNoQueue(vault.Key));
                    }

                    
                }
                finally
                {
                    _updateLock.Release();
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