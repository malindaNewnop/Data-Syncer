using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

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
        private bool _disposed = false;
        
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
                _logService = ServiceLocator.LogService;
                _serviceManager = ServiceLocator.ServiceManager;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error initializing SystemTrayManager: " + ex.Message);
                // Continue even if services aren't available - we'll just have limited functionality
            }
            
            InitializeContextMenu();
            InitializeNotifyIcon();
            RegisterMainFormEvents();
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
                _notifyIcon.ShowBalloonTip(showDuration, title, message, icon);
                
                // Log the notification
                if (_logService != null)
                {
                    string logMessage = string.Format("Notification displayed: {0} - {1}", title, message);
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

            // Create menu items
            MenuItem openItem = new MenuItem("Open", OnMenuOpenClick);
            MenuItem openLogsItem = new MenuItem("View Logs", OnMenuViewLogsClick);
            MenuItem startStopItem = new MenuItem(GetServiceToggleText(), OnMenuStartStopClick);
            MenuItem settingsItem = new MenuItem("Settings");
            MenuItem exitItem = new MenuItem("Exit", OnMenuExitClick);

            // Create settings submenu
            MenuItem connectionItem = new MenuItem("Connection...", OnMenuConnectionClick);
            MenuItem filtersItem = new MenuItem("Filters...", OnMenuFiltersClick);
            MenuItem notificationsItem = new MenuItem("Notifications", OnMenuNotificationsClick);
            notificationsItem.Checked = _notificationsEnabled;

            settingsItem.MenuItems.Add(connectionItem);
            settingsItem.MenuItems.Add(filtersItem);
            settingsItem.MenuItems.Add(new MenuItem("-")); // Separator
            settingsItem.MenuItems.Add(notificationsItem);

            // Add items to main context menu
            _contextMenu.MenuItems.Add(openItem);
            _contextMenu.MenuItems.Add(openLogsItem);
            _contextMenu.MenuItems.Add(startStopItem);
            _contextMenu.MenuItems.Add(new MenuItem("-")); // Separator
            _contextMenu.MenuItems.Add(settingsItem);
            _contextMenu.MenuItems.Add(new MenuItem("-")); // Separator
            _contextMenu.MenuItems.Add(exitItem);
        }

        private void RegisterMainFormEvents()
        {
            if (_mainForm != null)
            {
                _mainForm.Resize += OnMainFormResize;
                _mainForm.FormClosing += OnMainFormClosing;
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
            if (_contextMenu != null && _contextMenu.MenuItems.Count > 2)
            {
                _contextMenu.MenuItems[2].Text = GetServiceToggleText();
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
                // The resize event will handle hiding the form
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