using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace syncer.ui
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // Set application-wide exception handler
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                bool useStubs = false;
                
                try
                {
                    // First ensure CommonApplicationData folder is accessible
                    string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    string dataSyncerFolder = Path.Combine(appDataFolder, "DataSyncer");
                    
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
                        
                        // Verify services
                        if (!ServiceVerifier.VerifyServices())
                        {
                            DialogResult result = MessageBox.Show(
                                "Warning: Some services could not be initialized correctly. The application may not function properly.\n\n" +
                                "Do you want to continue with limited functionality?", 
                                "Service Initialization Warning", 
                                MessageBoxButtons.YesNo, 
                                MessageBoxIcon.Warning);
                                
                            if (result == DialogResult.No)
                            {
                                return; // Exit application
                            }
                        }
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
    }
}
