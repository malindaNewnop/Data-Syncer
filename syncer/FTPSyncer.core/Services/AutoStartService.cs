using System;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace FTPSyncer.core.Services
{
    /// <summary>
    /// Manages Windows startup registration for the application
    /// Compatible with .NET 3.5
    /// </summary>
    public class AutoStartService
    {
        private readonly string _applicationName;
        private readonly string _executablePath;
        private readonly ILogService _logService;

        public AutoStartService(string applicationName, ILogService logService)
        {
            _applicationName = applicationName ?? throw new ArgumentNullException("applicationName");
            _logService = logService ?? throw new ArgumentNullException("logService");
            _executablePath = Assembly.GetEntryAssembly().Location;
        }

        /// <summary>
        /// Enable auto-start for the application
        /// </summary>
        public bool EnableAutoStart()
        {
            try
            {
                // Use Registry run key for Windows
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (key != null)
                    {
                        key.SetValue(_applicationName, _executablePath);
                        _logService.LogInfo("Auto-start enabled for " + _applicationName, "AutoStartService");
                        return true;
                    }
                }
                _logService.LogError("Failed to enable auto-start: Cannot access registry key");
                return false;
            }
            catch (Exception ex)
            {
                _logService.LogError("Failed to enable auto-start: " + ex.Message, "AutoStartService");
                return false;
            }
        }

        /// <summary>
        /// Disable auto-start for the application
        /// </summary>
        public bool DisableAutoStart()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(_applicationName, false);
                        _logService.LogInfo("Auto-start disabled for " + _applicationName, "AutoStartService");
                        return true;
                    }
                }
                _logService.LogError("Failed to disable auto-start: Cannot access registry key");
                return false;
            }
            catch (Exception ex)
            {
                _logService.LogError("Failed to disable auto-start: " + ex.Message, "AutoStartService");
                return false;
            }
        }

        /// <summary>
        /// Check if auto-start is enabled
        /// </summary>
        public bool IsAutoStartEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false))
                {
                    if (key != null)
                    {
                        var value = key.GetValue(_applicationName);
                        return value != null && value.ToString() == _executablePath;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logService.LogError("Failed to check auto-start status: " + ex.Message, "AutoStartService");
                return false;
            }
        }

        /// <summary>
        /// Alternative method using startup folder instead of registry
        /// </summary>
        public bool EnableAutoStartViaStartupFolder()
        {
            try
            {
                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupFolder, _applicationName + ".lnk");
                
                if (!File.Exists(shortcutPath))
                {
                    // Create shortcut using shell script since .NET 3.5 doesn't have direct shortcut creation
                    string vbsScript = Path.Combine(Path.GetTempPath(), "CreateShortcut.vbs");
                    using (StreamWriter writer = new StreamWriter(vbsScript))
                    {
                        writer.WriteLine("Set WshShell = WScript.CreateObject(\"WScript.Shell\")");
                        writer.WriteLine("Set shortcut = WshShell.CreateShortcut(\"" + shortcutPath + "\")");
                        writer.WriteLine("shortcut.TargetPath = \"" + _executablePath + "\"");
                        writer.WriteLine("shortcut.WorkingDirectory = \"" + Path.GetDirectoryName(_executablePath) + "\"");
                        writer.WriteLine("shortcut.Description = \"" + _applicationName + " Auto Start\"");
                        writer.WriteLine("shortcut.Save");
                    }

                    // Run the script
                    ProcessStartInfo psi = new ProcessStartInfo("wscript.exe", "\"" + vbsScript + "\"")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    using (Process process = Process.Start(psi))
                    {
                        process.WaitForExit();
                    }

                    // Delete the temporary script
                    File.Delete(vbsScript);
                }
                
                _logService.LogInfo("Auto-start via startup folder enabled for " + _applicationName, "AutoStartService");
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError("Failed to enable auto-start via startup folder: " + ex.Message, "AutoStartService");
                return false;
            }
        }
    }
}





