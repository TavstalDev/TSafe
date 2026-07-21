using System;
using SDG.Unturned;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Models.Logging;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TLibrary.Threading;
using Tavstal.TSafe.Models;
using Tavstal.TSafe.Utils.Handlers;
using Tavstal.TSafe.Utils.Managers;

namespace Tavstal.TSafe
{
    // ReSharper disable once InconsistentNaming
    public class TSafe : PluginBase<TSafeConfig>
    {
        public static TSafe Instance { get; private set; } = null!;
        internal static bool IsShuttingDown { get; set; }
        public static DatabaseManager DatabaseManager { get; private set; } = null!;

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
            Level.onPostLevelLoaded += OnPluginsLoaded;
            Provider.onCommenceShutdown += OnShutdown;
            // Attach player related events
            PlayerEventHandler.AttachEvents();

            DatabaseManager = new DatabaseManager(this, Config);
            if (DatabaseManager.IsAuthenticationFailed)
                return;

            Logger.Info($"# {GetPluginName()} has been loaded.");
        }

        /// <summary>
        /// Fired when the plugin is unloaded.
        /// </summary>
        public override void OnUnLoad()
        {
            Level.onPostLevelLoaded -= OnPluginsLoaded;
            Provider.onCommenceShutdown -= OnShutdown;
            PlayerEventHandler.DetachEvents();
            Logger.Info($"# {GetPluginName()} has been successfully unloaded.");

            try
            {
                if (DatabaseManager.IsAuthenticationFailed || IsShuttingDown)
                    return;
                BackgroundThreadDispatcher.RunAsync(async () => await BackupAsync());
            }
            catch
            {
                /* Ignore, shutdown might provide some errors */
            }
        }

        private void OnPluginsLoaded(int i)
        {
            if (!DatabaseManager.IsAuthenticationFailed)
                return;
            Logger.Warning($"# Unloading {GetPluginName()} due to database authentication error.");
            this.UnloadPlugin();
        }

        private void OnShutdown()
        {
            if (DatabaseManager.IsAuthenticationFailed || IsShuttingDown)
                return;
            IsShuttingDown = true;
            BackgroundThreadDispatcher.RunAsync(async () => await BackupAsync());
        }

        private void FixedUpdate()
        {
            try
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (DatabaseManager == null)
                    return;

                if (DatabaseManager.IsAuthenticationFailed)
                    return;

                _time++;
                // Every second
                if (_time % 3000 != 0)
                    return;

                VaultManager.Update();

                // (Default SaveInterval is 300)
                // Executes the code every 300 seconds (5 minutes)
                if (_time % (Config.Database.SaveInterval * 50) != 0)
                    return;

                BackgroundThreadDispatcher.RunAsync(async () => await BackupAsync());
            }
            catch (Exception ex)
            {
                Logger.Error("An unexpected error occured in FixedUpdate.", ex);
            }
        }

        private async Task BackupAsync()
        {
            await _updateLock.WaitAsync();
            try
            {
                List<string> vaultIds = new List<string>();
                List<VaultItem> vaultItems = new List<VaultItem>();
                foreach (var vault in VaultManager.VaultList)
                {
                    VaultManager.CancelVaultDestroy(vault.Key);
                    InteractableStorage storage = (InteractableStorage)vault.Value.StorageDrop.interactable;
                    vaultIds.Add(vault.Key);
                    foreach (var item in storage.items.items)
                        vaultItems.Add(new VaultItem(vault.Key, item));
                }

                var ids = new List<object>();
                ids.AddRange(vaultIds);
                if (ids.Count > 0)
                    await DatabaseManager.Items.DeleteRangeAsync("VaultId", ids);
                if (vaultItems.Count > 0)
                    await DatabaseManager.Items.AddRangeAsync(vaultItems);
                await MainThreadDispatcher.RunAsync(() =>
                {
                    foreach (var vaultId in vaultIds)
                        VaultManager.DestroyVaultNoQueue(vaultId);
                });
            }
            finally
            {
                _updateLock.Release();
            }
        }

        public override Dictionary<string, string> DefaultLocalization =>
           new Dictionary<string, string>
           {
               { "prefix", $"&e[{GetPluginName()}] " },
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