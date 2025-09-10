using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace syncer.ui.Services
{
    /// <summary>
    /// Helper class for managing system tray icons and related functionality
    /// </summary>
    public static class TrayIconHelper
    {
        /// <summary>
        /// Gets the application icon with fallback options
        /// </summary>
        /// <returns>An icon suitable for the system tray</returns>
        public static Icon GetApplicationIcon()
        {
            try
            {
                // First try to get the application icon from the executable
                string exePath = Application.ExecutablePath;
                if (File.Exists(exePath))
                {
                    Icon extractedIcon = Icon.ExtractAssociatedIcon(exePath);
                    if (extractedIcon != null)
                        return extractedIcon;
                }
            }
            catch
            {
                // Continue to fallback options
            }

            try
            {
                // Try to load from embedded resources
                Assembly assembly = Assembly.GetExecutingAssembly();
                string[] resourceNames = assembly.GetManifestResourceNames();
                
                foreach (string resourceName in resourceNames)
                {
                    if (resourceName.EndsWith(".ico") || resourceName.Contains("icon"))
                    {
                        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream != null)
                            {
                                return new Icon(stream);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Continue to system icon fallback
            }

            // Final fallback to system icons
            return SystemIcons.Application;
        }

        /// <summary>
        /// Creates a status-based icon for the system tray
        /// </summary>
        /// <param name="serviceRunning">Whether the service is running</param>
        /// <returns>An icon representing the current status</returns>
        public static Icon GetStatusIcon(bool serviceRunning)
        {
            try
            {
                // For now, return the application icon
                // In a more advanced implementation, you could create colored icons
                // or overlay status indicators
                return GetApplicationIcon();
            }
            catch
            {
                return SystemIcons.Information;
            }
        }

        /// <summary>
        /// Truncates text to fit system tray tooltip requirements (max 63 chars)
        /// </summary>
        /// <param name="text">The text to truncate</param>
        /// <returns>Truncated text suitable for tooltips</returns>
        public static string TruncateTooltipText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            const int maxLength = 63;
            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - 3) + "...";
        }

        /// <summary>
        /// Formats a status message for the system tray tooltip
        /// </summary>
        /// <param name="serviceStatus">The service status</param>
        /// <param name="lastSyncTime">The last synchronization time</param>
        /// <returns>A formatted status message</returns>
        public static string FormatStatusMessage(string serviceStatus, DateTime? lastSyncTime)
        {
            string baseMessage = string.Format("Data Syncer - {0}", serviceStatus);
            
            if (lastSyncTime.HasValue)
            {
                string timeStr = lastSyncTime.Value.ToString("HH:mm");
                string fullMessage = string.Format("{0} (Last: {1})", baseMessage, timeStr);
                return TruncateTooltipText(fullMessage);
            }
            
            return TruncateTooltipText(baseMessage);
        }

        /// <summary>
        /// Shows a balloon tooltip with error handling
        /// </summary>
        /// <param name="notifyIcon">The NotifyIcon to show the tooltip on</param>
        /// <param name="title">The tooltip title</param>
        /// <param name="text">The tooltip text</param>
        /// <param name="icon">The tooltip icon</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        public static void ShowBalloonTip(NotifyIcon notifyIcon, string title, string text, ToolTipIcon icon, int timeout)
        {
            if (notifyIcon == null)
                return;

            try
            {
                // Ensure the title and text are not too long
                string safeTitle = TruncateTooltipText(title);
                string safeText = text;
                if (!string.IsNullOrEmpty(safeText) && safeText.Length > 255)
                {
                    safeText = safeText.Substring(0, 252) + "...";
                }

                notifyIcon.ShowBalloonTip(timeout, safeTitle, safeText, icon);
            }
            catch (Exception)
            {
                // Log error but don't throw - notifications should not crash the app
            }
        }
    }
}
