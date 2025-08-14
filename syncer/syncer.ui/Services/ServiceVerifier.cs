using System;
using syncer.ui.Services;
using syncer.core;
using System.IO;

namespace syncer.ui
{
    /// <summary>
    /// Utility class to verify that all services are properly connected
    /// </summary>
    public static class ServiceVerifier
    {
        public static bool VerifyServices()
        {
            Console.WriteLine("=== Service Verification ===");
            bool success = true;
            
            try
            {
                // Test LogService
                var logService = ServiceLocator.LogService;
                if (logService == null)
                {
                    Console.WriteLine("ERROR: LogService is null");
                    success = false;
                }
                else if (logService is CoreLogServiceAdapter)
                {
                    Console.WriteLine("SUCCESS: LogService is properly connected to Core");
                }
                else
                {
                    Console.WriteLine("WARNING: LogService is using stub implementation");
                    success = false;
                }
                
                // Test SyncJobService
                var jobService = ServiceLocator.SyncJobService;
                if (jobService == null)
                {
                    Console.WriteLine("ERROR: SyncJobService is null");
                    success = false;
                }
                else if (jobService is CoreSyncJobServiceAdapter)
                {
                    Console.WriteLine("SUCCESS: SyncJobService is properly connected to Core");
                }
                else
                {
                    Console.WriteLine("WARNING: SyncJobService is using stub implementation");
                    success = false;
                }
                
                // Test ConnectionService
                var connService = ServiceLocator.ConnectionService;
                if (connService == null)
                {
                    Console.WriteLine("ERROR: ConnectionService is null");
                    success = false;
                }
                else if (connService is CoreConnectionServiceAdapter)
                {
                    Console.WriteLine("SUCCESS: ConnectionService is properly connected to Core");
                }
                else
                {
                    Console.WriteLine("WARNING: ConnectionService is using stub implementation");
                    success = false;
                }
                
                // Test file system access
                string tempPath = Path.GetTempPath();
                string testFile = Path.Combine(tempPath, "syncer_test.txt");
                
                try
                {
                    File.WriteAllText(testFile, "Test file access");
                    File.Delete(testFile);
                    Console.WriteLine("SUCCESS: File system access working properly");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: File system access failed: " + ex.Message);
                    success = false;
                }
                
                // Test core paths
                try
                {
                    var appDataFolder = syncer.core.Paths.AppDataFolder;
                    if (Directory.Exists(appDataFolder))
                    {
                        Console.WriteLine("SUCCESS: Core Paths initialized properly");
                    }
                    else
                    {
                        Console.WriteLine("WARNING: Core application data folder not found: " + appDataFolder);
                        success = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: Core Paths failed: " + ex.Message);
                    success = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CRITICAL ERROR: Service verification failed: " + ex.Message);
                success = false;
            }
            
            Console.WriteLine("=== Service Verification " + (success ? "PASSED" : "FAILED") + " ===");
            return success;
        }
    }
}
