using System;
using System.Windows.Forms;

namespace syncer.ui
{
    /// <summary>
    /// This class helps test the connection between the UI and core components
    /// </summary>
    public static class BackendConnector
    {
        public static bool TestConnection()
        {
            try
            {
                // Initialize services
                ServiceLocator.Initialize();
                
                // Test log service
                var logService = ServiceLocator.LogService;
                logService.LogInfo("Testing backend connection");
                
                // Test job service with a basic operation
                var jobService = ServiceLocator.SyncJobService;
                var jobs = jobService.GetAllJobs();
                
                // Create a temporary test directory
                string tempDir = System.IO.Path.GetTempPath();
                string testSourceDir = System.IO.Path.Combine(tempDir, "DataSyncer_TestSource");
                string testDestDir = System.IO.Path.Combine(tempDir, "DataSyncer_TestDest");
                
                try 
                {
                    // Create test directories
                    if (!System.IO.Directory.Exists(testSourceDir))
                        System.IO.Directory.CreateDirectory(testSourceDir);
                    if (!System.IO.Directory.Exists(testDestDir))
                        System.IO.Directory.CreateDirectory(testDestDir);
                        
                    // Create a test file
                    string testFile = System.IO.Path.Combine(testSourceDir, "test.txt");
                    System.IO.File.WriteAllText(testFile, "Test content");
                    
                    // Test the file transfer
                    var job = new SyncJob
                    {
                        Name = "Test Job",
                        SourcePath = testSourceDir,
                        DestinationPath = testDestDir,
                        IncludeSubFolders = true,
                        OverwriteExisting = true
                    };
                    
                    int jobId = jobService.CreateJob(job);
                    
                    // Start the job to test actual file transfer
                    bool started = jobService.StartJob(jobId);
                    
                    // Verify file was copied
                    string destFile = System.IO.Path.Combine(testDestDir, "test.txt");
                    bool fileTransferred = System.IO.File.Exists(destFile);
                    
                    // Clean up
                    jobService.DeleteJob(jobId);
                    
                    if (!fileTransferred)
                    {
                        MessageBox.Show("Connection to backend established but file transfer failed. Please check logs for details.",
                            "Partial Connection Success", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Warning);
                        return false;
                    }
                }
                finally
                {
                    // Clean up test directories
                    try
                    {
                        if (System.IO.Directory.Exists(testSourceDir))
                            System.IO.Directory.Delete(testSourceDir, true);
                        if (System.IO.Directory.Exists(testDestDir))
                            System.IO.Directory.Delete(testDestDir, true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
                
                MessageBox.Show("Successfully connected to backend and transferred test file!", 
                    "Connection Test Passed", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to connect to backend: " + ex.Message,
                    "Connection Test Failed", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
