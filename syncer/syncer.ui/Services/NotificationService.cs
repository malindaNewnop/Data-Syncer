using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace syncer.ui.Services
{
    /// <summary>
    /// Manages system notifications for important application events
    /// </summary>
    public class NotificationService
    {
        private SystemTrayManager _trayManager;
        private ILogService _logService;
        private Queue<NotificationItem> _notificationQueue = new Queue<NotificationItem>();
        private bool _processingNotifications = false;
        private System.Timers.Timer _notificationTimer;
        
        // Settings
        private bool _notificationsEnabled = true;
        private int _notificationDelay = 3000; // 3 seconds between notifications

        /// <summary>
        /// Creates a new instance of the notification service
        /// </summary>
        /// <param name="trayManager">The system tray manager to use for displaying notifications</param>
        public NotificationService(SystemTrayManager trayManager)
        {
            _trayManager = trayManager ?? throw new ArgumentNullException("trayManager");
            
            try
            {
                _logService = ServiceLocator.LogService;
                
                // Load notification settings
                var configService = ServiceLocator.ConfigurationService;
                if (configService != null)
                {
                    _notificationsEnabled = configService.GetSetting("NotificationsEnabled", true);
                    _notificationDelay = configService.GetSetting("NotificationDelay", 3000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error initializing NotificationService: " + ex.Message);
                // Continue with default settings
            }
            
            // Initialize the notification timer
            _notificationTimer = new System.Timers.Timer(_notificationDelay);
            _notificationTimer.Elapsed += OnNotificationTimerElapsed;
            _notificationTimer.AutoReset = false;
            
            // Sync the setting with tray manager
            if (_trayManager != null)
                _trayManager.NotificationsEnabled = _notificationsEnabled;
        }

        /// <summary>
        /// Enables or disables notifications
        /// </summary>
        public bool NotificationsEnabled
        {
            get { return _notificationsEnabled; }
            set 
            { 
                _notificationsEnabled = value; 
                if (_trayManager != null)
                    _trayManager.NotificationsEnabled = value;
                    
                // Save setting if possible
                try
                {
                    var configService = ServiceLocator.ConfigurationService;
                    if (configService != null)
                        configService.SaveSetting("NotificationsEnabled", value);
                }
                catch
                {
                    // Ignore save errors
                }
            }
        }

        /// <summary>
        /// Shows a notification or queues it if another notification is currently showing
        /// </summary>
        /// <param name="title">The notification title</param>
        /// <param name="message">The notification message</param>
        /// <param name="icon">The notification icon</param>
        /// <param name="importance">The notification importance</param>
        public void ShowNotification(string title, string message, 
            ToolTipIcon icon = ToolTipIcon.Info, NotificationImportance importance = NotificationImportance.Normal)
        {
            if (!_notificationsEnabled || _trayManager == null)
                return;
                
            // Create the notification item
            var notification = new NotificationItem
            {
                Title = title,
                Message = message,
                Icon = icon,
                Importance = importance,
                Timestamp = DateTime.Now
            };
            
            // Add to queue and process
            _notificationQueue.Enqueue(notification);
            ProcessNotificationQueue();
            
            // Log the notification
            LogNotification(notification);
        }

        /// <summary>
        /// Shows a notification about a completed job
        /// </summary>
        /// <param name="jobName">The name of the job</param>
        /// <param name="fileCount">The number of files processed</param>
        /// <param name="success">Whether the job was successful</param>
        /// <param name="errorMessage">The error message if the job failed</param>
        public void ShowJobNotification(string jobName, int fileCount, bool success, string errorMessage = null)
        {
            string title = success ? "Job Completed" : "Job Failed";
            string message = success
                ? $"Job '{jobName}' completed successfully. {fileCount} file(s) processed."
                : $"Job '{jobName}' failed: {errorMessage}";
                
            ToolTipIcon icon = success ? ToolTipIcon.Info : ToolTipIcon.Error;
            NotificationImportance importance = success ? NotificationImportance.Normal : NotificationImportance.High;
            
            ShowNotification(title, message, icon, importance);
        }

        /// <summary>
        /// Shows a notification about a file transfer
        /// </summary>
        /// <param name="fileName">The name of the file</param>
        /// <param name="success">Whether the transfer was successful</param>
        /// <param name="errorMessage">The error message if the transfer failed</param>
        public void ShowTransferNotification(string fileName, bool success, string errorMessage = null)
        {
            // Only show transfer notifications for failed transfers to avoid spamming
            if (!success)
            {
                string title = "Transfer Failed";
                string message = $"Failed to transfer file '{fileName}': {errorMessage}";
                
                ShowNotification(title, message, ToolTipIcon.Error, NotificationImportance.Medium);
            }
        }

        /// <summary>
        /// Shows a notification about the service status
        /// </summary>
        /// <param name="running">Whether the service is running</param>
        public void ShowServiceStatusNotification(bool running)
        {
            string title = running ? "Service Started" : "Service Stopped";
            string message = running 
                ? "The Data Syncer service has started. File synchronization will occur according to schedule."
                : "The Data Syncer service has stopped. File synchronization will not occur until the service is restarted.";
                
            ToolTipIcon icon = running ? ToolTipIcon.Info : ToolTipIcon.Warning;
            
            ShowNotification(title, message, icon, NotificationImportance.High);
        }

        /// <summary>
        /// Shows a notification about connection status
        /// </summary>
        /// <param name="connected">Whether the connection is active</param>
        /// <param name="serverName">The name of the server</param>
        /// <param name="errorMessage">The error message if connection failed</param>
        public void ShowConnectionNotification(bool connected, string serverName, string errorMessage = null)
        {
            string title = connected ? "Connection Established" : "Connection Failed";
            string message = connected
                ? $"Successfully connected to {serverName}."
                : $"Failed to connect to {serverName}: {errorMessage}";
                
            ToolTipIcon icon = connected ? ToolTipIcon.Info : ToolTipIcon.Error;
            NotificationImportance importance = connected ? NotificationImportance.Low : NotificationImportance.High;
            
            ShowNotification(title, message, icon, importance);
        }

        #region Private Methods

        private void ProcessNotificationQueue()
        {
            if (_processingNotifications || _notificationQueue.Count == 0 || _trayManager == null)
                return;
                
            _processingNotifications = true;
            
            try
            {
                // Get the next notification from the queue
                var notification = _notificationQueue.Dequeue();
                
                // Display the notification
                _trayManager.ShowNotification(
                    notification.Title, 
                    notification.Message,
                    notification.Icon);
                
                // Start the timer for the next notification
                if (_notificationQueue.Count > 0)
                {
                    _notificationTimer.Start();
                }
                else
                {
                    _processingNotifications = false;
                }
            }
            catch (Exception ex)
            {
                _processingNotifications = false;
                Console.WriteLine("Error processing notification queue: " + ex.Message);
            }
        }

        private void OnNotificationTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _processingNotifications = false;
            ProcessNotificationQueue();
        }

        private void LogNotification(NotificationItem notification)
        {
            if (_logService != null)
            {
                string logMessage = string.Format("Notification [{0}]: {1} - {2}", 
                    notification.Importance, notification.Title, notification.Message);
                
                switch (notification.Importance)
                {
                    case NotificationImportance.High:
                        _logService.LogWarning(logMessage, "UI");
                        break;
                    case NotificationImportance.Low:
                    case NotificationImportance.Normal:
                    case NotificationImportance.Medium:
                    default:
                        _logService.LogInfo(logMessage, "UI");
                        break;
                }
            }
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Represents a notification item
        /// </summary>
        private class NotificationItem
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public ToolTipIcon Icon { get; set; }
            public NotificationImportance Importance { get; set; }
            public DateTime Timestamp { get; set; }
        }

        #endregion
    }

    /// <summary>
    /// Represents the importance of a notification
    /// </summary>
    public enum NotificationImportance
    {
        Low,
        Normal,
        Medium,
        High
    }
}