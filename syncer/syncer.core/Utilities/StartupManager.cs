using System;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Security;

namespace syncer.core.Utilities
{
    /// <summary>
    /// Manages Windows startup registry entries and service auto-start configuration
    /// Compatible with .NET Framework 3.5
    /// </summary>
    public static class StartupManager
    {
        private const string STARTUP_REGISTRY_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string STARTUP_SERVICE_KEY = @"SYSTEM\CurrentControlSet\Services\FTPSyncerService";
        private const string APP_NAME = "FTPSyncer";
        private const string SERVICE_NAME = "FTPSyncerService";

        /// <summary>
        /// Add application to Windows startup (current user)
        /// </summary>
        public static bool AddToStartup(string executablePath)
        {
            try
            {
                if (string.IsNullOrEmpty(executablePath) || !File.Exists(executablePath))
                {
                    EventLog.WriteEntry("FTPSyncer", 
                        "Cannot add to startup - executable path is invalid: " + executablePath,
                        EventLogEntryType.Warning);
                    return false;
                }

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(STARTUP_REGISTRY_KEY, true))
                {
                    if (key != null)
                    {
                        // Add with startup parameters for recovery mode
                        string startupValue = string.Format("\"{0}\" -startup", executablePath);
                        key.SetValue(APP_NAME, startupValue);
                        
                        EventLog.WriteEntry("FTPSyncer", 
                            "Successfully added to Windows startup", 
                            EventLogEntryType.Information);
                        return true;
                    }
                }
                return false;
            }
            catch (SecurityException ex)
            {
                EventLog.WriteEntry("FTPSyncer", 
                    "Permission denied adding to startup: " + ex.Message,
                    EventLogEntryType.Warning);
                return false;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("FTPSyncer", 
                    "Error adding to startup: " + ex.Message,
                    EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// Remove application from Windows startup
        /// </summary>
        public static bool RemoveFromStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(STARTUP_REGISTRY_KEY, true))
                {
                    if (key != null && IsInStartup())
                    {
                        key.DeleteValue(APP_NAME, false);
                        EventLog.WriteEntry("FTPSyncer", 
                            "Successfully removed from Windows startup", 
                            EventLogEntryType.Information);
                        return true;
                    }
                }
                return true; // Consider success if it wasn't there
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("FTPSyncer", 
                    "Error removing from startup: " + ex.Message,
                    EventLogEntryType.Warning);
                return false;
            }
        }

        /// <summary>
        /// Check if application is configured for Windows startup
        /// </summary>
        public static bool IsInStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(STARTUP_REGISTRY_KEY, false))
                {
                    if (key != null)
                    {
                        object value = key.GetValue(APP_NAME);
                        return value != null;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Configure Windows service for automatic startup
        /// </summary>
        public static bool ConfigureServiceAutoStart()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(STARTUP_SERVICE_KEY, true))
                {
                    if (key != null)
                    {
                        // Set service start type to automatic (2)
                        key.SetValue("Start", 2, RegistryValueKind.DWord);
                        
                        // Configure service recovery options
                        ConfigureServiceRecovery();
                        
                        EventLog.WriteEntry("FTPSyncer Service", 
                            "Service configured for automatic startup", 
                            EventLogEntryType.Information);
                        return true;
                    }
                }
                return false;
            }
            catch (SecurityException ex)
            {
                EventLog.WriteEntry("FTPSyncer Service", 
                    "Permission denied configuring service startup: " + ex.Message,
                    EventLogEntryType.Warning);
                return false;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("FTPSyncer Service", 
                    "Error configuring service startup: " + ex.Message,
                    EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// Configure service recovery options for automatic restart
        /// </summary>
        private static void ConfigureServiceRecovery()
        {
            try
            {
                // Use sc command to configure service recovery
                string scCommand = string.Format(
                    "sc failure {0} reset= 86400 actions= restart/5000/restart/10000/restart/30000", 
                    SERVICE_NAME);
                
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = string.Format(
                        "failure {0} reset= 86400 actions= restart/5000/restart/10000/restart/30000",
                        SERVICE_NAME),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        process.WaitForExit(10000); // 10 second timeout
                        if (process.ExitCode == 0)
                        {
                            EventLog.WriteEntry("FTPSyncer Service", 
                                "Service recovery options configured successfully", 
                                EventLogEntryType.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("FTPSyncer Service", 
                    "Warning: Could not configure service recovery options: " + ex.Message,
                    EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Check if service is configured for automatic startup
        /// </summary>
        public static bool IsServiceAutoStart()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(STARTUP_SERVICE_KEY, false))
                {
                    if (key != null)
                    {
                        object startValue = key.GetValue("Start");
                        if (startValue != null)
                        {
                            int startType = (int)startValue;
                            return startType == 2; // Automatic startup
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get the current executable path
        /// </summary>
        public static string GetCurrentExecutablePath()
        {
            try
            {
                return System.Reflection.Assembly.GetExecutingAssembly().Location;
            }
            catch
            {
                return System.Windows.Forms.Application.ExecutablePath;
            }
        }

        /// <summary>
        /// Configure both application and service for startup based on availability
        /// </summary>
        public static StartupConfigurationResult ConfigureStartup()
        {
            var result = new StartupConfigurationResult();

            try
            {
                // Try to configure service first (preferred method)
                if (IsServiceInstalled())
                {
                    result.ServiceConfigured = ConfigureServiceAutoStart();
                    result.PreferredMethod = "Windows Service";
                }

                // Configure application startup as fallback
                string exePath = GetCurrentExecutablePath();
                if (!string.IsNullOrEmpty(exePath))
                {
                    result.ApplicationConfigured = AddToStartup(exePath);
                    if (!result.ServiceConfigured)
                    {
                        result.PreferredMethod = "Application Startup";
                    }
                }

                result.Success = result.ServiceConfigured || result.ApplicationConfigured;
                
                if (result.Success)
                {
                    EventLog.WriteEntry("FTPSyncer", 
                        string.Format("Startup configured successfully using: {0}", result.PreferredMethod), 
                        EventLogEntryType.Information);
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                EventLog.WriteEntry("FTPSyncer", 
                    "Error configuring startup: " + ex.Message,
                    EventLogEntryType.Error);
                return result;
            }
        }

        /// <summary>
        /// Check if Windows service is installed
        /// </summary>
        private static bool IsServiceInstalled()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(STARTUP_SERVICE_KEY, false))
                {
                    return key != null;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Result of startup configuration operation
    /// </summary>
    public class StartupConfigurationResult
    {
        public bool Success { get; set; }
        public bool ServiceConfigured { get; set; }
        public bool ApplicationConfigured { get; set; }
        public string PreferredMethod { get; set; }
        public string ErrorMessage { get; set; }

        public StartupConfigurationResult()
        {
            Success = false;
            ServiceConfigured = false;
            ApplicationConfigured = false;
            PreferredMethod = "None";
        }
    }
}
