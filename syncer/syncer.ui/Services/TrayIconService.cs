using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using syncer.core.Models;

namespace syncer.core.Services
{
    /// <summary>
    /// Manages the system tray icon and context menu for the application
    /// </summary>
    public class TrayIconService : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ILogService _logService;
        private readonly EnhancedSettingsService _settingsService;
        private bool _disposed;

        // Events
        public event EventHandler ShowMainWindowRequested;
        public event EventHandler ExitApplicationRequested;
        public event EventHandler<SyncJob> RunJobRequested;
        public event EventHandler ConfigureSettingsRequested;
        
        public TrayIconService(ILogService logService, EnhancedSettingsService settingsService)
        {
            _logService = logService ?? throw new ArgumentNullException("logService");
            _settingsService = settingsService ?? throw new ArgumentNullException("settingsService");
            
            // Set up notify icon
            _notifyIcon = new NotifyIcon
            {
                Icon = GetApplicationIcon(),
                Text = "Data Syncer",
                Visible = true
            };
            
            // Set up context menu
            InitializeContextMenu();
            
            // Add event handlers
            _notifyIcon.DoubleClick += (s, e) => OnShowMainWindowRequested();
            
            _logService.LogInfo("Tray icon service initialized", "TrayIconService");
        }

        /// <summary>
        /// Sets up the context menu with standard items
        /// </summary>
        private void InitializeContextMenu()
        {
            var contextMenu = new ContextMenu();
            
            // Open application
            var openItem = new MenuItem("Open Data Syncer", (s, e) => OnShowMainWindowRequested());
            contextMenu.MenuItems.Add(openItem);
            
            // Settings item
            var settingsItem = new MenuItem("Settings", (s, e) => OnConfigureSettingsRequested());
            contextMenu.MenuItems.Add(settingsItem);
            
            // Add separator
            contextMenu.MenuItems.Add(new MenuItem("-"));
            
            // Exit application
            var exitItem = new MenuItem("Exit", (s, e) => OnExitApplicationRequested());
            contextMenu.MenuItems.Add(exitItem);
            
            _notifyIcon.ContextMenu = contextMenu;
        }

        /// <summary>
        /// Updates the context menu to include jobs that can be run directly
        /// </summary>
        public void UpdateJobMenu(SyncJob[] jobs)
        {
            if (_notifyIcon.ContextMenu == null)
                return;
                
            // Remove existing job menu items
            for (int i = _notifyIcon.ContextMenu.MenuItems.Count - 1; i >= 0; i--)
            {
                var item = _notifyIcon.ContextMenu.MenuItems[i];
                if (item.Tag is SyncJob)
                    _notifyIcon.ContextMenu.MenuItems.RemoveAt(i);
            }
            
            // If there are jobs, add them between settings and exit
            if (jobs != null && jobs.Length > 0)
            {
                int insertIndex = _notifyIcon.ContextMenu.MenuItems.Count - 2; // Before separator and exit
                
                _notifyIcon.ContextMenu.MenuItems.Add(insertIndex, new MenuItem("-"));
                _notifyIcon.ContextMenu.MenuItems.Add(insertIndex + 1, new MenuItem("Run Jobs"));
                
                for (int i = 0; i < jobs.Length; i++)
                {
                    var job = jobs[i];
                    var menuItem = new MenuItem(job.Name, (s, e) => OnRunJobRequested(job))
                    {
                        Tag = job
                    };
                    _notifyIcon.ContextMenu.MenuItems.Add(insertIndex + 2 + i, menuItem);
                }
                
                _notifyIcon.ContextMenu.MenuItems.Add(insertIndex + 2 + jobs.Length, new MenuItem("-"));
            }
        }

        /// <summary>
        /// Show a notification balloon
        /// </summary>
        public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info, int timeout = 3000)
        {
            _notifyIcon.ShowBalloonTip(timeout, title, message, icon);
            _logService.LogInfo($"Notification shown: {title} - {message}", "TrayIconService");
        }

        /// <summary>
        /// Show a transfer result notification
        /// </summary>
        public void ShowTransferResultNotification(TransferResultEnhanced result)
        {
            if (result == null)
                return;
                
            string title;
            string message;
            ToolTipIcon icon;
            
            if (result.Success && result.FailedFiles == 0)
            {
                title = "Transfer Successful";
                message = $"Job '{result.JobName}' completed successfully. " +
                         $"Transferred {result.SuccessfulFiles} files ({result.GetFormattedDataSize()}).";
                icon = ToolTipIcon.Info;
            }
            else if (result.Success) // Partial success
            {
                title = "Transfer Completed with Warnings";
                message = $"Job '{result.JobName}' completed with warnings. " +
                         $"Transferred {result.SuccessfulFiles} files, {result.FailedFiles} failed.";
                icon = ToolTipIcon.Warning;
            }
            else
            {
                title = "Transfer Failed";
                message = $"Job '{result.JobName}' failed. Error: {result.LastError}";
                icon = ToolTipIcon.Error;
            }
            
            ShowNotification(title, message, icon);
        }

        /// <summary>
        /// Gets the application icon
        /// </summary>
        private Icon GetApplicationIcon()
        {
            try
            {
                // Try to load from embedded resources
                var assembly = Assembly.GetEntryAssembly();
                var iconStream = assembly.GetManifestResourceStream("syncer.ui.Resources.appicon.ico");
                
                if (iconStream != null)
                    return new Icon(iconStream);
                
                // Fall back to extracting from the executable
                return Icon.ExtractAssociatedIcon(assembly.Location);
            }
            catch
            {
                // Last resort - return default system icon
                return SystemIcons.Application;
            }
        }

        // Event handlers
        protected virtual void OnShowMainWindowRequested()
        {
            ShowMainWindowRequested?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnExitApplicationRequested()
        {
            ExitApplicationRequested?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRunJobRequested(SyncJob job)
        {
            RunJobRequested?.Invoke(this, job);
        }

        protected virtual void OnConfigureSettingsRequested()
        {
            ConfigureSettingsRequested?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing && _notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }

            _disposed = true;
        }
    }
}
