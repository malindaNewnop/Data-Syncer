using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace syncer.ui
{
    internal static class Program
    {
        // Single instance mutex
        private static Mutex instanceMutex = null;
        private const string MUTEX_NAME = "FTPSyncerApplication_SingleInstance_Mutex_12345";
        
        // Windows API declarations for window activation
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);
        
        // ShowWindow constants
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Check for single instance
            if (!CheckSingleInstance())
            {
                return; // Exit if another instance is already running
            }
            
            try
            {
                // Set application-wide exception handler
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                bool useStubs = false;
                
                try
                {
                    // First ensure CommonApplicationData folder is accessible
                    string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    string dataSyncerFolder = Path.Combine(appDataFolder, "FTPSyncer");
                    
                    try
                    {
                        if (!Directory.Exists(dataSyncerFolder))
                            Directory.CreateDirectory(dataSyncerFolder);
                            
                        // Test write permissions by creating a test file
                        string testFile = Path.Combine(dataSyncerFolder, "test.txt");
                        File.WriteAllText(testFile, "Test");
                        File.Delete(testFile);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error accessing application data folder. The application requires write access to: " + 
                                      dataSyncerFolder + "\n\nError: " + ex.Message, 
                                      "Permission Error", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        useStubs = true;
                    }
                    
                    if (!useStubs)
                    {
                        // Initialize service locator with real services
                        ServiceLocator.Initialize();
                        
                        // Configure startup for auto-restart functionality
                        ConfigureApplicationStartup();
                    }
                }
                catch (Exception initEx)
                {
                    // Show detailed initialization error
                    string errorDetails = "Error initializing services: " + initEx.Message;
                    if (initEx.InnerException != null)
                    {
                        errorDetails += "\n\nInner Exception: " + initEx.InnerException.Message;
                    }
                    
                    DialogResult result = MessageBox.Show(
                        errorDetails + "\n\nDo you want to continue with limited functionality?", 
                        "Service Initialization Error", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Error);
                        
                    if (result == DialogResult.No)
                    {
                        return; // Exit application
                    }
                    
                    // Continue with stub services
                    useStubs = true;
                }
                
                if (useStubs)
                {
                    ServiceLocator.InitializeStubs();
                    MessageBox.Show("The application will run with limited functionality. Some features may not work correctly.",
                        "Limited Functionality Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                // Pre-load notification settings
                try
                {
                    var configService = ServiceLocator.ConfigurationService;
                    if (configService != null)
                    {
                        // Load default notification settings if they don't exist
                        if (!configService.GetSetting("NotificationsInitialized", false))
                        {
                            configService.SaveSetting("NotificationsEnabled", true);
                            configService.SaveSetting("NotificationDuration", 3000);
                            configService.SaveSetting("MinimizeToTray", true);
                            configService.SaveSetting("NotificationsInitialized", true);
                        }
                    }
                }
                catch
                {
                    // Ignore settings errors - use defaults
                }
                
                // Run main dashboard form
                Application.Run(new FormMain());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fatal Error: " + ex.Message, 
                              "Application Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool CheckSingleInstance()
        {
            bool createdNew;
            instanceMutex = new Mutex(true, MUTEX_NAME, out createdNew);
            
            if (!createdNew)
            {
                // Another instance is running, try to find and activate it
                ActivateExistingInstance();
                return false;
            }
            
            return true;
        }

        private static void ActivateExistingInstance()
        {
            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);
                
                foreach (Process process in processes)
                {
                    if (process.Id != currentProcess.Id)
                    {
                        IntPtr hWnd = process.MainWindowHandle;
                        if (hWnd != IntPtr.Zero)
                        {
                            // Restore window if minimized
                            if (IsIconic(hWnd))
                            {
                                ShowWindow(hWnd, 9); // SW_RESTORE = 9
                            }
                            
                            // Bring window to foreground
                            SetForegroundWindow(hWnd);
                            break;
                        }
                    }
                }
            }
            catch
            {
                // If activation fails, just ignore - user can manually switch to existing instance
            }
        }

        static void Application_ApplicationExit(object sender, EventArgs e)
        {
            try
            {
                // Save current state before exiting using ServiceLocator shutdown
                if (ServiceLocator.LogService != null)
                {
                    ServiceLocator.LogService.LogInfo("Application exiting - initiating proper shutdown with state saving");
                }

                // Proper shutdown with state saving
                ServiceLocator.Shutdown();

                if (instanceMutex != null)
                {
                    instanceMutex.ReleaseMutex();
                    instanceMutex.Close();
                    instanceMutex = null;
                }

                if (ServiceLocator.LogService != null)
                {
                    ServiceLocator.LogService.LogInfo("Application exit cleanup completed");
                }
            }
            catch (Exception ex)
            {
                // Try to log the error if possible
                try
                {
                    if (ServiceLocator.LogService != null)
                    {
                        ServiceLocator.LogService.LogError("Error during application exit: " + ex.Message);
                    }
                }
                catch
                {
                    // Last resort - console output
                    Console.WriteLine("Critical error during application exit: " + ex.Message);
                }
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            try
            {
                // Log the exception
                try
                {
                    ServiceLocator.LogService.LogError("Unhandled UI exception: " + e.Exception.Message, "UI");
                }
                catch
                {
                    // Ignore logging errors
                }
                
                // Show error to user
                MessageBox.Show("An error has occurred in the application: " + e.Exception.Message + 
                    "\n\nPlease check the logs for more details.", 
                    "Application Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
            catch
            {
                // Last resort - if error handling itself fails
                MessageBox.Show("A critical error has occurred. The application may need to be restarted.",
                    "Critical Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = e.ExceptionObject as Exception;
                string errorMessage = ex != null ? ex.Message : "Unknown error";
                
                // Log the exception
                try
                {
                    ServiceLocator.LogService.LogError("Unhandled application exception: " + errorMessage, "UI");
                }
                catch
                {
                    // Ignore logging errors
                }
                
                // Show error to user if it's terminating
                if (e.IsTerminating)
                {
                    MessageBox.Show("A fatal error has occurred and the application needs to close: " + 
                        errorMessage + "\n\nPlease check the logs for more details.", 
                        "Fatal Error", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);
                }
            }
            catch
            {
                // Last resort - if error handling itself fails
                MessageBox.Show("A critical error has occurred and the application needs to close.",
                    "Critical Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Configure application for automatic startup and job recovery
        /// </summary>
        private static void ConfigureApplicationStartup()
        {
            try
            {
                // Check command line arguments to see if we're in recovery mode
                string[] args = Environment.GetCommandLineArgs();
                bool isStartupRecovery = false;
                
                foreach (string arg in args)
                {
                    if (arg.Equals("-startup", StringComparison.OrdinalIgnoreCase))
                    {
                        isStartupRecovery = true;
                        break;
                    }
                }

                // Configure automatic startup using registry directly
                var startupConfigured = ConfigureStartupRegistry();
                
                if (startupConfigured && ServiceLocator.LogService != null)
                {
                    ServiceLocator.LogService.LogInfo("Application startup configured successfully");
                }

                // If this is a startup recovery, attempt to restore timer jobs
                if (isStartupRecovery)
                {
                    PerformStartupRecovery();
                }
            }
            catch (Exception ex)
            {
                // Don't let startup configuration errors prevent the app from running
                if (ServiceLocator.LogService != null)
                {
                    ServiceLocator.LogService.LogError("Error configuring application startup: " + ex.Message);
                }
                else
                {
                    // Fallback if logging isn't available
                    System.Diagnostics.EventLog.WriteEntry("FTPSyncer", 
                        "Startup configuration error: " + ex.Message, 
                        System.Diagnostics.EventLogEntryType.Warning);
                }
            }
        }

        /// <summary>
        /// Perform job recovery during application startup
        /// </summary>
        private static void PerformStartupRecovery()
        {
            try
            {
                if (ServiceLocator.LogService != null)
                {
                    ServiceLocator.LogService.LogInfo("Performing startup job recovery...");
                }

                // The TimerJobManager will automatically restore timer jobs during initialization
                // Additional recovery logic can be added here if needed
                
                // For example, check for and restore any saved job configurations
                var serviceManager = ServiceLocator.ServiceManager as IServiceManager;
                if (serviceManager != null)
                {
                    // Start the service manager which will handle job restoration
                    bool started = serviceManager.StartService();
                    
                    if (started && ServiceLocator.LogService != null)
                    {
                        ServiceLocator.LogService.LogInfo("Service manager started successfully during recovery");
                    }
                }

                // Initialize timer jobs manager - restoration happens automatically during initialization
                var timerJobManager = ServiceLocator.TimerJobManager;
                if (timerJobManager != null && ServiceLocator.LogService != null)
                {
                    ServiceLocator.LogService.LogInfo("Timer job manager initialized with job recovery");
                }

                // Wait a moment for jobs to fully initialize
                System.Threading.Thread.Sleep(3000);

                if (ServiceLocator.LogService != null)
                {
                    ServiceLocator.LogService.LogInfo("Startup recovery completed");
                }
            }
            catch (Exception ex)
            {
                if (ServiceLocator.LogService != null)
                {
                    ServiceLocator.LogService.LogError("Error during startup recovery: " + ex.Message);
                }
                else
                {
                    System.Diagnostics.EventLog.WriteEntry("FTPSyncer", 
                        "Startup recovery error: " + ex.Message, 
                        System.Diagnostics.EventLogEntryType.Warning);
                }
            }
        }

        /// <summary>
        /// Configure startup registry entry for automatic restart
        /// </summary>
        private static bool ConfigureStartupRegistry()
        {
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        string executablePath = Application.ExecutablePath;
                        string startupValue = string.Format("\"{0}\" -startup", executablePath);
                        key.SetValue("FTPSyncer", startupValue);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                if (ServiceLocator.LogService != null)
                {
                    ServiceLocator.LogService.LogError("Error configuring startup registry: " + ex.Message);
                }
                return false;
            }
        }
    }
}
