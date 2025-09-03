using System;
using System.IO;
using System.Threading;
using syncer.ui.Services;
using syncer.ui.Interfaces;
using syncer.ui;

namespace syncer
{
    /// <summary>
    /// Test class to verify timer job delete source functionality
    /// .NET 3.5 Compatible
    /// </summary>
    public class TestTimerJobDeleteSource
    {
        private static ILogService _logService;
        private static IConnectionService _connectionService;
        private static ITimerJobManager _timerJobManager;

        public static void Main(string[] args)
        {
            Console.WriteLine("=== Timer Job Delete Source Test ===");
            
            try
            {
                // Initialize services (mimicking actual application initialization)
                InitializeServices();
                
                // Test 1: Timer job WITHOUT delete source after transfer
                Console.WriteLine("\n--- Test 1: Timer job WITHOUT delete source ---");
                TestTimerJobWithoutDelete();
                
                // Test 2: Timer job WITH delete source after transfer
                Console.WriteLine("\n--- Test 2: Timer job WITH delete source ---");
                TestTimerJobWithDelete();
                
                // Test 3: Verify UpdateTimerJob with delete setting
                Console.WriteLine("\n--- Test 3: Update timer job delete setting ---");
                TestUpdateTimerJobDeleteSetting();
                
                Console.WriteLine("\n=== All Tests Completed ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test failed with error: " + ex.Message);
                Console.WriteLine("Stack trace: " + ex.StackTrace);
            }
            finally
            {
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }
        
        private static void InitializeServices()
        {
            // Initialize service locator and services
            ServiceLocator.Initialize();
            _logService = ServiceLocator.LogService;
            _connectionService = ServiceLocator.ConnectionService;
            _timerJobManager = new TimerJobManager();
            
            Console.WriteLine("Services initialized successfully");
        }
        
        private static void TestTimerJobWithoutDelete()
        {
            string testFolderBase = Path.Combine(Path.GetTempPath(), "TimerJobTest_NoDelete_" + DateTime.Now.Ticks);
            string sourceFolder = Path.Combine(testFolderBase, "Source");
            string remoteFolder = Path.Combine(testFolderBase, "Remote");
            
            try
            {
                // Create test directories and files
                Directory.CreateDirectory(sourceFolder);
                Directory.CreateDirectory(remoteFolder);
                
                string testFile = Path.Combine(sourceFolder, "test_nodelete.txt");
                File.WriteAllText(testFile, "Test content - should NOT be deleted");
                
                Console.WriteLine("Created test file: " + testFile);
                
                // Setup local connection (for testing purposes)
                SetupLocalConnection();
                
                // Register timer job WITHOUT delete source after transfer
                long jobId = DateTime.Now.Ticks;
                bool registered = _timerJobManager.RegisterTimerJob(
                    jobId,
                    "Test Job No Delete",
                    sourceFolder,
                    remoteFolder,
                    5000, // 5 second interval
                    true,  // Include subfolders
                    false  // NO delete source after transfer
                );
                
                if (registered)
                {
                    Console.WriteLine("Timer job registered successfully (no delete)");
                    
                    // Start the job
                    if (_timerJobManager.StartTimerJob(jobId))
                    {
                        Console.WriteLine("Timer job started - waiting for first cycle...");
                        Thread.Sleep(7000); // Wait for first upload cycle
                        
                        // Check if source file still exists (should exist)
                        if (File.Exists(testFile))
                        {
                            Console.WriteLine("✓ SUCCESS: Source file preserved (as expected)");
                        }
                        else
                        {
                            Console.WriteLine("✗ FAILURE: Source file was deleted (unexpected)");
                        }
                        
                        // Stop the job
                        _timerJobManager.StopTimerJob(jobId);
                        _timerJobManager.RemoveTimerJob(jobId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test 1 error: " + ex.Message);
            }
            finally
            {
                // Cleanup
                try
                {
                    if (Directory.Exists(testFolderBase))
                        Directory.Delete(testFolderBase, true);
                }
                catch { /* Ignore cleanup errors */ }
            }
        }
        
        private static void TestTimerJobWithDelete()
        {
            string testFolderBase = Path.Combine(Path.GetTempPath(), "TimerJobTest_WithDelete_" + DateTime.Now.Ticks);
            string sourceFolder = Path.Combine(testFolderBase, "Source");
            string remoteFolder = Path.Combine(testFolderBase, "Remote");
            
            try
            {
                // Create test directories and files
                Directory.CreateDirectory(sourceFolder);
                Directory.CreateDirectory(remoteFolder);
                
                string testFile = Path.Combine(sourceFolder, "test_withdelete.txt");
                File.WriteAllText(testFile, "Test content - should be deleted");
                
                Console.WriteLine("Created test file: " + testFile);
                
                // Setup local connection
                SetupLocalConnection();
                
                // Register timer job WITH delete source after transfer
                long jobId = DateTime.Now.Ticks;
                bool registered = _timerJobManager.RegisterTimerJob(
                    jobId,
                    "Test Job With Delete",
                    sourceFolder,
                    remoteFolder,
                    5000, // 5 second interval
                    true, // Include subfolders
                    true  // DELETE source after transfer
                );
                
                if (registered)
                {
                    Console.WriteLine("Timer job registered successfully (with delete)");
                    
                    // Start the job
                    if (_timerJobManager.StartTimerJob(jobId))
                    {
                        Console.WriteLine("Timer job started - waiting for first cycle...");
                        Thread.Sleep(7000); // Wait for first upload cycle
                        
                        // Check if source file was deleted (should be deleted)
                        if (!File.Exists(testFile))
                        {
                            Console.WriteLine("✓ SUCCESS: Source file was deleted after transfer");
                        }
                        else
                        {
                            Console.WriteLine("✗ FAILURE: Source file was NOT deleted (unexpected)");
                        }
                        
                        // Check if remote file exists (should exist)
                        string remoteFile = Path.Combine(remoteFolder, "test_withdelete.txt");
                        if (File.Exists(remoteFile))
                        {
                            Console.WriteLine("✓ SUCCESS: File transferred to destination");
                        }
                        else
                        {
                            Console.WriteLine("✗ FAILURE: File not found at destination");
                        }
                        
                        // Stop the job
                        _timerJobManager.StopTimerJob(jobId);
                        _timerJobManager.RemoveTimerJob(jobId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test 2 error: " + ex.Message);
            }
            finally
            {
                // Cleanup
                try
                {
                    if (Directory.Exists(testFolderBase))
                        Directory.Delete(testFolderBase, true);
                }
                catch { /* Ignore cleanup errors */ }
            }
        }
        
        private static void TestUpdateTimerJobDeleteSetting()
        {
            try
            {
                string testFolderBase = Path.Combine(Path.GetTempPath(), "TimerJobTest_Update_" + DateTime.Now.Ticks);
                string sourceFolder = Path.Combine(testFolderBase, "Source");
                string remoteFolder = Path.Combine(testFolderBase, "Remote");
                
                Directory.CreateDirectory(sourceFolder);
                Directory.CreateDirectory(remoteFolder);
                
                SetupLocalConnection();
                
                // Register timer job without delete initially
                long jobId = DateTime.Now.Ticks;
                bool registered = _timerJobManager.RegisterTimerJob(
                    jobId,
                    "Test Update Job",
                    sourceFolder,
                    remoteFolder,
                    10000, // 10 second interval
                    true,  // Include subfolders
                    false  // NO delete initially
                );
                
                if (registered)
                {
                    Console.WriteLine("Initial timer job registered (no delete)");
                    
                    // Update the job to enable delete
                    bool updated = _timerJobManager.UpdateTimerJob(
                        jobId,
                        "Test Update Job - Delete Enabled",
                        sourceFolder,
                        remoteFolder,
                        8000,  // 8 second interval
                        true,  // Include subfolders
                        true   // ENABLE delete
                    );
                    
                    if (updated)
                    {
                        Console.WriteLine("✓ SUCCESS: Timer job updated with delete enabled");
                    }
                    else
                    {
                        Console.WriteLine("✗ FAILURE: Failed to update timer job");
                    }
                    
                    // Cleanup
                    _timerJobManager.RemoveTimerJob(jobId);
                }
                
                // Cleanup folder
                try
                {
                    if (Directory.Exists(testFolderBase))
                        Directory.Delete(testFolderBase, true);
                }
                catch { /* Ignore cleanup errors */ }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test 3 error: " + ex.Message);
            }
        }
        
        private static void SetupLocalConnection()
        {
            // Setup a local connection for testing
            var connectionSettings = new ConnectionSettings
            {
                Protocol = "Local",
                IsLocalConnection = true,
                Host = "localhost",
                Port = 0,
                Username = "",
                Password = "",
                SshKeyPath = "",
                Timeout = 30000,
                IsRemoteConnection = false
            };
            
            _connectionService.SaveConnectionSettings(connectionSettings);
            Console.WriteLine("Local connection configured for testing");
        }
    }
}
