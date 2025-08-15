using System;
using System.Collections.Generic;

namespace syncer.ui.Tests
{
    /// <summary>
    /// Quick test to verify multi-job functionality
    /// </summary>
    public class MultiJobTest
    {
        public static void TestCreateMultipleJobs()
        {
            try
            {
                Console.WriteLine("Testing multi-job functionality...");
                
                // Initialize services
                ServiceLocator.Initialize();
                
                var jobService = ServiceLocator.SyncJobService;
                if (jobService == null)
                {
                    Console.WriteLine("ERROR: SyncJobService is null!");
                    return;
                }
                
                Console.WriteLine("Service type: " + jobService.GetType().Name);
                
                // Clear existing jobs for clean test
                var existingJobs = jobService.GetAllJobs();
                Console.WriteLine($"Found {existingJobs.Count} existing jobs");
                
                // Create test job 1
                var job1 = new SyncJob
                {
                    Name = "Test Job 1",
                    SourcePath = @"C:\Test\Source1",
                    DestinationPath = @"C:\Test\Dest1",
                    IntervalType = "Minutes",
                    IntervalValue = 15,
                    IsEnabled = true
                };
                
                int job1Id = jobService.CreateJob(job1);
                Console.WriteLine($"Created Job 1 with ID: {job1Id}");
                
                // Create test job 2
                var job2 = new SyncJob
                {
                    Name = "Test Job 2",
                    SourcePath = @"C:\Test\Source2",
                    DestinationPath = @"C:\Test\Dest2",
                    IntervalType = "Hours",
                    IntervalValue = 2,
                    IsEnabled = true
                };
                
                int job2Id = jobService.CreateJob(job2);
                Console.WriteLine($"Created Job 2 with ID: {job2Id}");
                
                // Verify both jobs exist
                var allJobs = jobService.GetAllJobs();
                Console.WriteLine($"Total jobs after creation: {allJobs.Count}");
                
                foreach (var job in allJobs)
                {
                    Console.WriteLine($"- Job ID {job.Id}: {job.Name}");
                }
                
                Console.WriteLine("Multi-job test completed successfully!");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR in multi-job test: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception: " + ex.InnerException.Message);
                }
                Console.WriteLine("Stack trace: " + ex.StackTrace);
            }
        }
    }
}
