using System;
using System.Windows.Forms;

namespace FTPSyncer.ui.Services
{
    /// <summary>
    /// Helper class for integrating tray notifications throughout the application
    /// </summary>
    public static class TrayNotificationHelper
    {
        private static SystemTrayManager _trayManager;
        
        /// <summary>
        /// Initializes the tray notification helper with a tray manager instance
        /// </summary>
        /// <param name="trayManager">The SystemTrayManager to use for notifications</param>
        public static void Initialize(SystemTrayManager trayManager)
        {
            _trayManager = trayManager;
        }
        
        /// <summary>
        /// Shows a notification about a successful sync operation
        /// </summary>
        /// <param name="jobName">The name of the sync job</param>
        /// <param name="fileCount">Number of files synced</param>
        public static void ShowSyncSuccess(string jobName, int fileCount = 0)
        {
            if (_trayManager == null) return;
            
            string message = fileCount > 0 ? 
                string.Format("Successfully synced {0} files", fileCount) : 
                "Sync completed successfully";
                
            _trayManager.ShowSyncNotification(jobName, true, message);
        }
        
        /// <summary>
        /// Shows a notification about a failed sync operation
        /// </summary>
        /// <param name="jobName">The name of the sync job</param>
        /// <param name="errorMessage">The error that occurred</param>
        public static void ShowSyncError(string jobName, string errorMessage)
        {
            if (_trayManager == null) return;
            
            _trayManager.ShowSyncNotification(jobName, false, errorMessage);
        }
        
        /// <summary>
        /// Shows a general information notification
        /// </summary>
        /// <param name="title">The notification title</param>
        /// <param name="message">The notification message</param>
        public static void ShowInfo(string title, string message)
        {
            if (_trayManager == null) return;
            
            _trayManager.ShowNotification(title, message, ToolTipIcon.Info);
        }
        
        /// <summary>
        /// Shows a warning notification
        /// </summary>
        /// <param name="title">The notification title</param>
        /// <param name="message">The notification message</param>
        public static void ShowWarning(string title, string message)
        {
            if (_trayManager == null) return;
            
            _trayManager.ShowNotification(title, message, ToolTipIcon.Warning);
        }
        
        /// <summary>
        /// Shows an error notification
        /// </summary>
        /// <param name="title">The notification title</param>
        /// <param name="message">The notification message</param>
        public static void ShowError(string title, string message)
        {
            if (_trayManager == null) return;
            
            _trayManager.ShowNotification(title, message, ToolTipIcon.Error);
        }
        
        /// <summary>
        /// Updates the service status in the tray
        /// </summary>
        /// <param name="isRunning">Whether the service is running</param>
        /// <param name="lastSyncTime">The last sync time (optional)</param>
        public static void UpdateServiceStatus(bool isRunning, DateTime? lastSyncTime = null)
        {
            if (_trayManager == null) return;
            
            _trayManager.UpdateStatus(isRunning, lastSyncTime);
        }
        
        /// <summary>
        /// Shows a service status change notification
        /// </summary>
        /// <param name="isRunning">Whether the service is now running</param>
        public static void ShowServiceStatusChange(bool isRunning)
        {
            if (_trayManager == null) return;
            
            string title = isRunning ? "Service Started" : "Service Stopped";
            string message = isRunning ? 
                "FTPSyncer service is now running and monitoring for changes" :
                "FTPSyncer service has been stopped - no automatic syncing will occur";
            
            ToolTipIcon icon = isRunning ? ToolTipIcon.Info : ToolTipIcon.Warning;
            _trayManager.ShowNotification(title, message, icon);
        }
    }
}





