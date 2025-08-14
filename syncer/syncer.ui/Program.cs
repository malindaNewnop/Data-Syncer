using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

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
    }
}
