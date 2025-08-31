using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using syncer.core.Services;

namespace syncer.ui.Services
{
    /// <summary>
    /// Manages the system tray icon, notifications, and context menu for the application
    /// </summary>
    public class SystemTrayManager : IDisposable
    {
        private NotifyIcon _notifyIcon;
        private ContextMenu _contextMenu;
        private Form _mainForm;
        private ILogService _logService;
        private IServiceManager _serviceManager;
        private syncer.core.Services.AutoStartService _autoStartService;
        private bool _disposed = false;
        private System.Timers.Timer _statusUpdateTimer;
        
        // Notification settings
        private bool _notificationsEnabled = true;
        private int _notificationDuration = 3000; // 3 seconds

        /// <summary>
        /// Occurs when a notification is clicked
        /// </summary>
        public event EventHandler NotificationClicked;

        /// <summary>
        /// Creates a new instance of the SystemTrayManager
        /// </summary>
        /// <param name="mainForm">The main application form</param>
        public SystemTrayManager(Form mainForm)
        {
            _mainForm = mainForm ?? throw new ArgumentNullException("mainForm");
            
            try
            {
                // Initialize services
                _logService = ServiceLocator.LogService;
                _serviceManager = ServiceLocator.ServiceManager;
                
                // Initialize auto-start service using core log service
                var coreLogService = new syncer.core.FileLogService();
                _autoStartService = new syncer.core.Services.AutoStartService("Data Syncer", coreLogService);
                
                // Load configuration settings
                var configService = ServiceLocator.ConfigurationService;
                if (configService != null)
                {
                    _notificationsEnabled = configService.GetSetting("NotificationsEnabled", true);
                    _notificationDuration = configService.GetSetting("NotificationDelay", 3000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error initializing SystemTrayManager: " + ex.Message);
                // Continue even if services aren't available - we'll just have limited functionality
            }
            
            InitializeContextMenu();
            InitializeNotifyIcon();
            RegisterMainFormEvents();
            StartStatusUpdater();
        }

        /// <summary>
        /// Starts the background timer that updates tray status
        /// </summary>
        private void StartStatusUpdater()
        {
            try
            {
                _statusUpdateTimer = new System.Timers.Timer(30000); // Update every 30 seconds
                _statusUpdateTimer.Elapsed += OnStatusUpdateTimer;
                _statusUpdateTimer.AutoReset = true;
                _statusUpdateTimer.Start();
                
                if (_logService != null)
                    _logService.LogInfo("Tray status updater started", "UI");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting status updater: " + ex.Message);
            }
        }

        /// <summary>
        /// Timer event handler for updating tray status
        /// </summary>
        private void OnStatusUpdateTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (_disposed || _notifyIcon == null || _serviceManager == null)
                    return;

                // Update service status in the UI thread
                if (_mainForm != null && !_mainForm.IsDisposed)
                {
                    _mainForm.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            bool serviceRunning = _serviceManager.IsServiceRunning();
                            UpdateStatus(serviceRunning);
                        }
                        catch
                        {
                            // Ignore errors during status update
                        }
                    }));
                }
            }
            catch
            {
                // Ignore timer errors
            }
        }

        #region Public Methods

        /// <summary>
        /// Shows the application in the system tray
        /// </summary>
        public void Show()
        {
            if (_notifyIcon != null)
                _notifyIcon.Visible = true;
        }

        /// <summary>
        /// Hides the application from the system tray
        /// </summary>
        public void Hide()
        {
            if (_notifyIcon != null)
                _notifyIcon.Visible = false;
        }

        /// <summary>
        /// Displays a notification in the system tray
        /// </summary>
        /// <param name="title">The notification title</param>
        /// <param name="message">The notification message</param>
        /// <param name="icon">The notification icon</param>
        /// <param name="duration">How long to display the notification (in milliseconds)</param>
        public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info, int? duration = null)
        {
            if (!_notificationsEnabled || _notifyIcon == null)
                return;
                
            try
            {
                int showDuration = duration ?? _notificationDuration;
                
                // Ensure title and message are not too long
                string safeTitle = StringExtensions.Truncate(title ?? "Data Syncer", 63);
                string safeMessage = message ?? "";
                if (safeMessage.Length > 255)
                {
                    safeMessage = safeMessage.Substring(0, 252) + "...";
                }
                
                _notifyIcon.ShowBalloonTip(showDuration, safeTitle, safeMessage, icon);
                
                // Log the notification
                if (_logService != null)
                {
                    string logMessage = string.Format("Notification displayed: {0} - {1}", safeTitle, safeMessage);
                    _logService.LogInfo(logMessage, "UI");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error showing notification: " + ex.Message);
            }
        }

        /// <summary>
        /// Enables or disables system tray notifications
        /// </summary>
        public bool NotificationsEnabled
        {
            get { return _notificationsEnabled; }
            set { _notificationsEnabled = value; }
        }

        /// <summary>
        /// Updates the tooltip text displayed when hovering over the system tray icon
        /// </summary>
        /// <param name="toolTipText">The text to display</param>
        public void UpdateToolTip(string toolTipText)
        {
            if (_notifyIcon != null)
                _notifyIcon.Text = StringExtensions.Truncate(toolTipText, 63);
        }

        /// <summary>
        /// Updates the icon displayed in the system tray
        /// </summary>
        /// <param name="icon">The icon to display</param>
        public void UpdateIcon(Icon icon)
        {
            if (_notifyIcon != null && icon != null)
                _notifyIcon.Icon = icon;
        }

        /// <summary>
        /// Updates both the icon and tooltip based on service status
        /// </summary>
        /// <param name="serviceRunning">Whether the service is currently running</param>
        /// <param name="lastSyncTime">The last synchronization time (optional)</param>
        public void UpdateStatus(bool serviceRunning, DateTime? lastSyncTime = null)
        {
            try
            {
                // Update tooltip
                string status = serviceRunning ? "Running" : "Stopped";
                string tooltip = "Data Syncer - Service: " + status;
                
                if (lastSyncTime.HasValue)
                {
                    string timeStr = lastSyncTime.Value.ToString("HH:mm");
                    tooltip += " (Last: " + timeStr + ")";
                }
                
                UpdateToolTip(tooltip);
                
                // Update menu text if context menu is available
                UpdateServiceMenuText();
                
                // Log status update
                if (_logService != null)
                {
                    _logService.LogInfo("Tray icon status updated - Service: " + status, "UI");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating tray status: " + ex.Message);
            }
        }

        /// <summary>
        /// Shows a sync-specific notification from external callers
        /// </summary>
        /// <param name="jobName">The name of the sync job</param>
        /// <param name="success">Whether the sync was successful</param>
        /// <param name="message">Additional message details</param>
        public void ShowSyncNotification(string jobName, bool success, string message)
        {
            if (!_notificationsEnabled || _notifyIcon == null)
                return;
                
            try
            {
                string title = success ? "Sync Completed" : "Sync Failed";
                ToolTipIcon icon = success ? ToolTipIcon.Info : ToolTipIcon.Error;
                string fullMessage = string.Format("{0}: {1}", jobName, message);
                
                ShowNotification(title, fullMessage, icon);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error showing sync notification: " + ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.ContextMenu = _contextMenu;
            _notifyIcon.Text = "Data Syncer";
            _notifyIcon.Icon = GetApplicationIcon();
            _notifyIcon.Visible = true;

            // Add event handlers
            _notifyIcon.DoubleClick += OnNotifyIconDoubleClick;
            _notifyIcon.BalloonTipClicked += OnBalloonTipClicked;
        }

        private void InitializeContextMenu()
        {
            _contextMenu = new ContextMenu();

            // Open/Restore window
            MenuItem openItem = new MenuItem("Open Data Syncer", OnMenuOpenClick);
            openItem.DefaultItem = true;
            _contextMenu.MenuItems.Add(openItem);
            
            _contextMenu.MenuItems.Add(new MenuItem("-")); // Separator
            
            // Service control
            MenuItem serviceItem = new MenuItem("Service Control");
            serviceItem.MenuItems.Add(new MenuItem("Start Service", OnStartServiceClicked));
            serviceItem.MenuItems.Add(new MenuItem("Stop Service", OnStopServiceClicked));
            serviceItem.MenuItems.Add(new MenuItem("Restart Service", OnRestartServiceClicked));
            _contextMenu.MenuItems.Add(serviceItem);
            
            _contextMenu.MenuItems.Add(new MenuItem("-")); // Separator
            
            // Job control (new functionality)
            MenuItem jobControlItem = new MenuItem("Job Control");
            jobControlItem.MenuItems.Add(new MenuItem("Stop All Running Jobs", OnStopAllJobsClicked));
            jobControlItem.MenuItems.Add(new MenuItem("Resume All Jobs", OnResumeAllJobsClicked));
            jobControlItem.MenuItems.Add(new MenuItem("Show Running Jobs", OnShowRunningJobsClicked));
            _contextMenu.MenuItems.Add(jobControlItem);
            
            _contextMenu.MenuItems.Add(new MenuItem("-")); // Separator
            
            // Quick actions
            MenuItem actionsItem = new MenuItem("Quick Actions");
            actionsItem.MenuItems.Add(new MenuItem("View Logs", OnViewLogsClicked));
            actionsItem.MenuItems.Add(new MenuItem("Refresh Status", OnRefreshStatusClicked));
            actionsItem.MenuItems.Add(new MenuItem("Show Last Sync", OnShowLastSyncClicked));
            _contextMenu.MenuItems.Add(actionsItem);
            
            _contextMenu.MenuItems.Add(new MenuItem("-")); // Separator
            
            // Settings
            MenuItem settingsItem = new MenuItem("Settings");
            MenuItem connectionItem = new MenuItem("Connection...", OnMenuConnectionClick);
            MenuItem filtersItem = new MenuItem("Filters...", OnMenuFiltersClick);
            MenuItem traySettingsItem = new MenuItem("Tray Settings...", OnMenuTraySettingsClick);
            MenuItem notificationsItem = new MenuItem("Notifications", OnMenuNotificationsClick);
            notificationsItem.Checked = _notificationsEnabled;
            
            // Auto-start menu item
            MenuItem autoStartItem = new MenuItem("Auto-start with Windows", OnMenuAutoStartClick);
            if (_autoStartService != null)
            {
                autoStartItem.Checked = _autoStartService.IsAutoStartEnabled();
            }
            
            settingsItem.MenuItems.Add(connectionItem);
            settingsItem.MenuItems.Add(filtersItem);
            settingsItem.MenuItems.Add(new MenuItem("-")); // Separator
            settingsItem.MenuItems.Add(traySettingsItem);
            settingsItem.MenuItems.Add(notificationsItem);
            settingsItem.MenuItems.Add(autoStartItem);
            _contextMenu.MenuItems.Add(settingsItem);
            
            _contextMenu.MenuItems.Add(new MenuItem("-")); // Separator
            
            // Exit
            _contextMenu.MenuItems.Add(new MenuItem("Exit", OnMenuExitClick));
        }

        private void RegisterMainFormEvents()
        {
            if (_mainForm != null)
            {
                // Unregister any existing events first to prevent duplicates
                _mainForm.Resize -= OnMainFormResize;
                _mainForm.FormClosing -= OnMainFormClosing;
                
                // Register the events
                _mainForm.Resize += OnMainFormResize;
                _mainForm.FormClosing += OnMainFormClosing;
                
                if (_logService != null)
                    _logService.LogInfo("System tray events registered successfully", "UI");
            }
        }

        private string GetServiceToggleText()
        {
            bool serviceRunning = false;
            try
            {
                serviceRunning = _serviceManager != null && _serviceManager.IsServiceRunning();
            }
            catch
            {
                // Ignore errors checking service status
            }
            return serviceRunning ? "Stop Service" : "Start Service";
        }

        private Icon GetApplicationIcon()
        {
            try
            {
                // Try to get the application icon
                return Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                // Fall back to the system information icon if we can't get the application icon
                return SystemIcons.Information;
            }
        }

        private void UpdateServiceMenuText()
        {
            try
            {
                if (_contextMenu != null && _serviceManager != null)
                {
                    // Find the Service Control submenu (index 1 in new structure)
                    if (_contextMenu.MenuItems.Count > 1)
                    {
                        MenuItem serviceControlItem = _contextMenu.MenuItems[1];
                        if (serviceControlItem.MenuItems.Count >= 2)
                        {
                            bool isRunning = _serviceManager.IsServiceRunning();
                            
                            // Enable/disable menu items based on service state
                            serviceControlItem.MenuItems[0].Enabled = !isRunning; // Start Service
                            serviceControlItem.MenuItems[1].Enabled = isRunning;  // Stop Service
                            serviceControlItem.MenuItems[2].Enabled = isRunning;  // Restart Service
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating service menu text: " + ex.Message);
            }
        }

        #endregion

        #region Event Handlers

        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            RestoreMainForm();
        }

        private void OnBalloonTipClicked(object sender, EventArgs e)
        {
            try
            {
                if (NotificationClicked != null)
                    NotificationClicked(this, EventArgs.Empty);
                
                // Default action is to restore the main form
                RestoreMainForm();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error handling notification click: " + ex.Message);
            }
        }

        private void OnMenuOpenClick(object sender, EventArgs e)
        {
            RestoreMainForm();
        }

        private void OnMenuViewLogsClick(object sender, EventArgs e)
        {
            try
            {
                using (FormLogs logsForm = new FormLogs())
                {
                    logsForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening logs: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnMenuStartStopClick(object sender, EventArgs e)
        {
            try
            {
                if (_serviceManager.IsServiceRunning())
                {
                    if (_serviceManager.StopService())
                    {
                        if (_logService != null)
                            _logService.LogInfo("Service stopped by user from tray menu", "UI");
                            
                        ShowNotification("Service Stopped", 
                            "The Data Syncer service has been stopped. File synchronization will not occur until the service is restarted.", 
                            ToolTipIcon.Info);
                    }
                    else
                    {
                        MessageBox.Show("Failed to stop service", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    if (_serviceManager.StartService())
                    {
                        if (_logService != null)
                            _logService.LogInfo("Service started by user from tray menu", "UI");
                            
                        ShowNotification("Service Started", 
                            "The Data Syncer service has been started. File synchronization will now occur according to schedule.",
                            ToolTipIcon.Info);
                    }
                    else
                    {
                        MessageBox.Show("Failed to start service", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                
                // Update the menu item text
                UpdateServiceMenuText();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error managing service: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (_logService != null)
                    _logService.LogError("Error managing service from tray menu: " + ex.Message, "UI");
            }
        }

        private void OnMenuConnectionClick(object sender, EventArgs e)
        {
            try
            {
                using (FormConnection connForm = new FormConnection())
                {
                    connForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening connection settings: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnMenuFiltersClick(object sender, EventArgs e)
        {
            try
            {
                using (FormFilters filtersForm = new FormFilters())
                {
                    filtersForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening filter settings: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnMenuTraySettingsClick(object sender, EventArgs e)
        {
            try
            {
                // For now, show a simple message dialog until the form compilation issue is resolved
                MessageBox.Show("Tray settings configuration will be available in the next update.\n\n" +
                    "Current Settings:\n" +
                    "• Notifications: " + (_notificationsEnabled ? "Enabled" : "Disabled") + "\n" +
                    "• Duration: " + _notificationDuration + "ms\n\n" +
                    "Use the 'Notifications' menu item to toggle notifications on/off.", 
                    "Tray Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error showing tray settings: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnMenuNotificationsClick(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                // Toggle notification setting
                _notificationsEnabled = !_notificationsEnabled;
                menuItem.Checked = _notificationsEnabled;
                
                // Show feedback to the user
                if (_notificationsEnabled)
                {
                    ShowNotification("Notifications Enabled", 
                        "You will now receive notifications about file synchronization events.",
                        ToolTipIcon.Info);
                }
                else
                {
                    MessageBox.Show("Notifications have been disabled.", "Notifications", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                // Save setting if possible
                try
                {
                    var configService = ServiceLocator.ConfigurationService;
                    if (configService != null)
                    {
                        configService.SaveSetting("NotificationsEnabled", _notificationsEnabled);
                    }
                }
                catch
                {
                    // Ignore setting save failures
                }
            }
        }

        private void OnMenuExitClick(object sender, EventArgs e)
        {
            try
            {
                if (_logService != null)
                    _logService.LogInfo("Application exit requested from tray menu", "UI");
                    
                // Exit the application completely
                Application.Exit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error exiting application: " + ex.Message);
                Environment.Exit(1); // Force exit on error
            }
        }

        private void OnMenuAutoStartClick(object sender, EventArgs e)
        {
            try
            {
                if (_autoStartService == null)
                {
                    ShowNotification("Auto-start Error", "Auto-start service is not available", ToolTipIcon.Error);
                    return;
                }

                MenuItem menuItem = sender as MenuItem;
                if (menuItem == null) return;

                bool currentState = _autoStartService.IsAutoStartEnabled();
                bool success = false;

                if (currentState)
                {
                    // Disable auto-start
                    success = _autoStartService.DisableAutoStart();
                    if (success)
                    {
                        menuItem.Checked = false;
                        ShowNotification("Auto-start Disabled", "Data Syncer will no longer start with Windows", ToolTipIcon.Info);
                        if (_logService != null)
                            _logService.LogInfo("Auto-start disabled from system tray", "UI");
                    }
                    else
                    {
                        ShowNotification("Auto-start Error", "Failed to disable auto-start", ToolTipIcon.Error);
                    }
                }
                else
                {
                    // Enable auto-start
                    success = _autoStartService.EnableAutoStart();
                    if (success)
                    {
                        menuItem.Checked = true;
                        ShowNotification("Auto-start Enabled", "Data Syncer will now start with Windows", ToolTipIcon.Info);
                        if (_logService != null)
                            _logService.LogInfo("Auto-start enabled from system tray", "UI");
                    }
                    else
                    {
                        ShowNotification("Auto-start Error", "Failed to enable auto-start", ToolTipIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Auto-start Error", "Error toggling auto-start: " + ex.Message, ToolTipIcon.Error);
                if (_logService != null)
                    _logService.LogError("Failed to toggle auto-start from tray: " + ex.Message, "UI");
            }
        }

        #region Enhanced Service Control Event Handlers

        private void OnStartServiceClicked(object sender, EventArgs e)
        {
            try
            {
                if (_serviceManager != null)
                {
                    bool success = _serviceManager.StartService();
                    if (success)
                    {
                        UpdateStatus(true);
                        ShowNotification("Service Started", "Data Syncer service is now running", ToolTipIcon.Info);
                        if (_logService != null)
                            _logService.LogInfo("Service started from system tray", "UI");
                    }
                    else
                    {
                        ShowNotification("Service Error", "Failed to start service", ToolTipIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Service Error", "Error starting service: " + ex.Message, ToolTipIcon.Error);
                if (_logService != null)
                    _logService.LogError("Failed to start service from tray: " + ex.Message, "UI");
            }
        }

        private void OnStopServiceClicked(object sender, EventArgs e)
        {
            try
            {
                if (_serviceManager != null)
                {
                    bool success = _serviceManager.StopService();
                    if (success)
                    {
                        UpdateStatus(false);
                        ShowNotification("Service Stopped", "Data Syncer service has been stopped", ToolTipIcon.Warning);
                        if (_logService != null)
                            _logService.LogInfo("Service stopped from system tray", "UI");
                    }
                    else
                    {
                        ShowNotification("Service Error", "Failed to stop service", ToolTipIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Service Error", "Error stopping service: " + ex.Message, ToolTipIcon.Error);
                if (_logService != null)
                    _logService.LogError("Failed to stop service from tray: " + ex.Message, "UI");
            }
        }

        private void OnRestartServiceClicked(object sender, EventArgs e)
        {
            try
            {
                if (_serviceManager != null)
                {
                    ShowNotification("Service Restarting", "Restarting Data Syncer service...", ToolTipIcon.Info);
                    
                    _serviceManager.StopService();
                    System.Threading.Thread.Sleep(1000); // Wait 1 second
                    bool success = _serviceManager.StartService();
                    
                    if (success)
                    {
                        UpdateStatus(true);
                        ShowNotification("Service Restarted", "Data Syncer service has been restarted", ToolTipIcon.Info);
                        if (_logService != null)
                            _logService.LogInfo("Service restarted from system tray", "UI");
                    }
                    else
                    {
                        ShowNotification("Service Error", "Failed to restart service", ToolTipIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Service Error", "Error restarting service: " + ex.Message, ToolTipIcon.Error);
                if (_logService != null)
                    _logService.LogError("Failed to restart service from tray: " + ex.Message, "UI");
            }
        }

        #endregion

        #region Job Control Event Handlers

        private void OnStopAllJobsClicked(object sender, EventArgs e)
        {
            try
            {
                var timerJobManager = ServiceLocator.TimerJobManager;
                if (timerJobManager != null)
                {
                    int stoppedCount = 0;
                    var runningJobs = timerJobManager.GetRunningJobs();
                    
                    foreach (var jobId in runningJobs.Keys)
                    {
                        if (timerJobManager.StopTimerJob(jobId))
                        {
                            stoppedCount++;
                        }
                    }
                    
                    if (stoppedCount > 0)
                    {
                        ShowNotification("Jobs Stopped", 
                            $"{stoppedCount} running job(s) have been stopped", ToolTipIcon.Warning);
                        if (_logService != null)
                            _logService.LogInfo($"Stopped {stoppedCount} jobs from system tray", "UI");
                    }
                    else
                    {
                        ShowNotification("No Jobs Running", "No running jobs to stop", ToolTipIcon.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Job Control Error", "Error stopping jobs: " + ex.Message, ToolTipIcon.Error);
                if (_logService != null)
                    _logService.LogError("Failed to stop jobs from tray: " + ex.Message, "UI");
            }
        }

        private void OnResumeAllJobsClicked(object sender, EventArgs e)
        {
            try
            {
                var timerJobManager = ServiceLocator.TimerJobManager;
                if (timerJobManager != null)
                {
                    int resumedCount = 0;
                    var allJobs = timerJobManager.GetAllJobs();
                    
                    foreach (var jobId in allJobs.Keys)
                    {
                        if (!timerJobManager.IsTimerJobRunning(jobId))
                        {
                            if (timerJobManager.StartTimerJob(jobId))
                            {
                                resumedCount++;
                            }
                        }
                    }
                    
                    if (resumedCount > 0)
                    {
                        ShowNotification("Jobs Resumed", 
                            $"{resumedCount} job(s) have been resumed", ToolTipIcon.Info);
                        if (_logService != null)
                            _logService.LogInfo($"Resumed {resumedCount} jobs from system tray", "UI");
                    }
                    else
                    {
                        ShowNotification("All Jobs Running", "All jobs are already running", ToolTipIcon.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Job Control Error", "Error resuming jobs: " + ex.Message, ToolTipIcon.Error);
                if (_logService != null)
                    _logService.LogError("Failed to resume jobs from tray: " + ex.Message, "UI");
            }
        }

        private void OnShowRunningJobsClicked(object sender, EventArgs e)
        {
            try
            {
                var timerJobManager = ServiceLocator.TimerJobManager;
                if (timerJobManager != null)
                {
                    var runningJobs = timerJobManager.GetRunningJobs();
                    
                    if (runningJobs.Count > 0)
                    {
                        string jobList = "";
                        foreach (var job in runningJobs)
                        {
                            string jobName = timerJobManager.GetTimerJobName(job.Key);
                            jobList += "• " + (jobName ?? "Job " + job.Key) + "\n";
                        }
                        
                        MessageBox.Show($"Currently Running Jobs ({runningJobs.Count}):\n\n{jobList}", 
                            "Running Jobs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("No jobs are currently running.", 
                            "Running Jobs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Job Control Error", "Error retrieving running jobs: " + ex.Message, ToolTipIcon.Error);
                if (_logService != null)
                    _logService.LogError("Failed to show running jobs from tray: " + ex.Message, "UI");
            }
        }

        #endregion

        #region Quick Actions Event Handlers

        private void OnViewLogsClicked(object sender, EventArgs e)
        {
            try
            {
                using (FormLogs logsForm = new FormLogs())
                {
                    logsForm.ShowDialog();
                }
                
                if (_logService != null)
                    _logService.LogInfo("Logs accessed from system tray", "UI");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening logs: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnRefreshStatusClicked(object sender, EventArgs e)
        {
            try
            {
                if (_serviceManager != null)
                {
                    bool isRunning = _serviceManager.IsServiceRunning();
                    string status = _serviceManager.GetServiceStatus();
                    
                    UpdateStatus(isRunning);
                    UpdateToolTip("Data Syncer - Service: " + status);
                    
                    ShowNotification("Status Refreshed", "Service is currently: " + status, ToolTipIcon.Info);
                    
                    if (_logService != null)
                        _logService.LogInfo("Status refreshed from system tray - Service: " + status, "UI");
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Error", "Failed to refresh status: " + ex.Message, ToolTipIcon.Error);
            }
        }

        private void OnShowLastSyncClicked(object sender, EventArgs e)
        {
            try
            {
                if (_logService != null)
                {
                    // Get recent logs
                    var logs = _logService.GetLogs();
                    if (logs.Rows.Count > 0)
                    {
                        var recentLogs = new List<string>();
                        int maxLogs = Math.Min(3, logs.Rows.Count);
                        
                        for (int i = logs.Rows.Count - maxLogs; i < logs.Rows.Count; i++)
                        {
                            var row = logs.Rows[i];
                            string logEntry = string.Format("{0}: {1}", 
                                row["DateTime"], 
                                row["Message"].ToString().Substring(0, Math.Min(50, row["Message"].ToString().Length)));
                            recentLogs.Add(logEntry);
                        }
                        
                        string message = "Recent activity:\n" + string.Join("\n", recentLogs.ToArray());
                        ShowNotification("Recent Activity", message, ToolTipIcon.Info);
                    }
                    else
                    {
                        ShowNotification("No Activity", "No sync activities recorded yet", ToolTipIcon.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Error", "Failed to get recent activity: " + ex.Message, ToolTipIcon.Error);
            }
        }

        #endregion

        private void OnMainFormResize(object sender, EventArgs e)
        {
            // When the form is minimized, hide it and show a notification
            if (_mainForm != null && _mainForm.WindowState == FormWindowState.Minimized)
            {
                _mainForm.Hide();
                
                if (_notificationsEnabled)
                {
                    ShowNotification("Data Syncer Running", 
                        "The application is still running in the background. Double-click the tray icon to restore.",
                        ToolTipIcon.Info);
                }
            }
        }

        private void OnMainFormClosing(object sender, FormClosingEventArgs e)
        {
            // Intercept the close operation to minimize to tray instead
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Cancel the close operation
                _mainForm.WindowState = FormWindowState.Minimized; // Minimize the form
                _mainForm.Hide(); // Explicitly hide the form (don't rely only on resize)
                
                if (_notificationsEnabled)
                {
                    ShowNotification("Data Syncer Running", 
                        "The application is still running in the background. Double-click the tray icon to restore.",
                        ToolTipIcon.Info);
                }
            }
        }
        
        private void RestoreMainForm()
        {
            try
            {
                if (_mainForm != null && !_mainForm.IsDisposed)
                {
                    // Show the form and bring it to the foreground
                    _mainForm.Show();
                    _mainForm.WindowState = FormWindowState.Normal;
                    _mainForm.Activate();
                    
                    // Logging
                    if (_logService != null)
                        _logService.LogInfo("Main form restored from tray icon", "UI");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error restoring main form: " + ex.Message);
            }
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Stop and dispose the status update timer
                    if (_statusUpdateTimer != null)
                    {
                        _statusUpdateTimer.Stop();
                        _statusUpdateTimer.Dispose();
                        _statusUpdateTimer = null;
                    }
                    
                    // Dispose managed resources
                    if (_notifyIcon != null)
                    {
                        _notifyIcon.Visible = false;
                        _notifyIcon.Dispose();
                        _notifyIcon = null;
                    }
                    
                    if (_contextMenu != null)
                    {
                        _contextMenu.Dispose();
                        _contextMenu = null;
                    }
                    
                    // Unregister event handlers
                    if (_mainForm != null)
                    {
                        _mainForm.Resize -= OnMainFormResize;
                        _mainForm.FormClosing -= OnMainFormClosing;
                        _mainForm = null;
                    }
                }
                
                _disposed = true;
            }
        }

        #endregion
    }
}