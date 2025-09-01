using System;
using System.IO;
using syncer.core;
using syncer.ui.Services;

namespace syncer
{
    /// <summary>
    /// Simple test class to verify real-time logging functionality
    /// </summary>
    public class RealTimeLoggingTest
    {
        public static void TestRealTimeLogging()
        {
            // Create a test CSV file path
            string testCsvPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
                                            "test_realtime_log.csv");
            
            // Get the log service
            var logService = new CoreLogServiceAdapter();
            
            try
            {
                // Enable real-time logging
                logService.EnableRealTimeLogging(testCsvPath);
                Console.WriteLine("Real-time logging enabled to: " + testCsvPath);
                
                // Test various log types
                logService.LogInfo("System startup completed", "System");
                logService.LogWarning("Memory usage is high", "System");
                
                // Test job logging
                var testJob = new SyncJob
                {
                    Id = "TEST001",
                    Name = "Test Sync Job",
                    SourcePath = @"C:\TestSource",
                    DestinationPath = @"C:\TestDestination"
                };
                
                logService.LogJobStart(testJob);
                logService.LogJobProgress(testJob, "Processing files...");
                
                // Test enhanced transfer logging
                logService.LogEnhancedTransfer("Test Sync Job", "test.pdf", 1024000, 
                                              TimeSpan.FromSeconds(2.5), 
                                              @"C:\TestSource\test.pdf", 
                                              @"C:\TestDestination\test.pdf", 
                                              true, null);
                
                logService.LogJobSuccess(testJob, "All files transferred successfully");
                
                // Test error logging
                logService.LogError("Test error message", "TestModule");
                
                Console.WriteLine("Test logging completed. Check the CSV file for results.");
                
                // Disable real-time logging
                logService.DisableRealTimeLogging();
                Console.WriteLine("Real-time logging disabled.");
                
                // Display the CSV content
                if (File.Exists(testCsvPath))
                {
                    Console.WriteLine("\nCSV Content:");
                    Console.WriteLine(new string('=', 80));
                    Console.WriteLine(File.ReadAllText(testCsvPath));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during test: " + ex.Message);
            }
        }
    }
}
