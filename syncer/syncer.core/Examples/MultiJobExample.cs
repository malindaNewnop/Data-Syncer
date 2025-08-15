using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using syncer.core;

namespace syncer.examples
{
    /// <summary>
    /// Example demonstrating how to use the new multi-job functionality
    /// </summary>
    public class MultiJobExample
    {
        private readonly IJobRepository _jobRepository;
        private readonly IJobQueueService _jobQueueService;
        private readonly IMultiJobRunner _multiJobRunner;
        private readonly MultiJobConfigurationService _configService;
        private readonly ILogService _logService;

        public MultiJobExample()
        {
            // Initialize services using the factory
            _logService = ServiceFactory.CreateLogService();
            _jobRepository = ServiceFactory.CreateJobRepository();
            _jobQueueService = ServiceFactory.CreateJobQueueService();
            _multiJobRunner = ServiceFactory.CreateMultiJobRunner();
            _configService = ServiceFactory.CreateMultiJobConfigurationService();

            _logService.LogInfo("Multi-Job Example initialized", "MultiJobExample");
        }

        /// <summary>
        /// Example 1: Basic multi-job execution
        /// </summary>
        public void Example1_BasicMultiJobExecution()
        {
            Console.WriteLine("\n=== Example 1: Basic Multi-Job Execution ===");

            try
            {
                // Create some sample jobs
                var jobs = CreateSampleJobs(3);
                
                // Get list of job IDs
                var jobIds = new List<string>();
                foreach (var job in jobs)
                {
                    _jobRepository.Save(job);
                    jobIds.Add(job.Id);
                    Console.WriteLine($"Created job: {job.Name} (ID: {job.Id})");
                }

                // Start multiple jobs at once
                Console.WriteLine($"\nStarting {jobIds.Count} jobs concurrently...");
                bool success = _multiJobRunner.StartMultipleJobs(jobIds);
                
                if (success)
                {
                    Console.WriteLine("Jobs started successfully!");
                    
                    // Monitor job statuses
                    MonitorJobExecution(jobIds, TimeSpan.FromMinutes(2));
                }
                else
                {
                    Console.WriteLine("Failed to start jobs.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Example 1: {ex.Message}");
                _logService.LogError($"Example 1 failed: {ex.Message}", "MultiJobExample");
            }
        }

        /// <summary>
        /// Example 2: Job queue management with priorities
        /// </summary>
        public void Example2_JobQueueWithPriorities()
        {
            Console.WriteLine("\n=== Example 2: Job Queue with Priorities ===");

            try
            {
                // Create a high-priority queue for critical jobs
                var criticalQueue = _jobQueueService.CreateQueue("Critical Jobs", 2, 100);
                Console.WriteLine($"Created critical queue: {criticalQueue.Name} (ID: {criticalQueue.Id})");

                // Create a normal priority queue
                var normalQueue = _jobQueueService.CreateQueue("Normal Jobs", 3, 50);
                Console.WriteLine($"Created normal queue: {normalQueue.Name} (ID: {normalQueue.Id})");

                // Create sample jobs
                var criticalJobs = CreateSampleJobs(2, "Critical");
                var normalJobs = CreateSampleJobs(4, "Normal");

                // Save jobs to repository
                foreach (var job in criticalJobs)
                {
                    _jobRepository.Save(job);
                }
                foreach (var job in normalJobs)
                {
                    _jobRepository.Save(job);
                }

                // Queue critical jobs with high priority
                foreach (var job in criticalJobs)
                {
                    _jobQueueService.QueueJob(job.Id, criticalQueue.Id, priority: 10);
                    Console.WriteLine($"Queued critical job: {job.Name}");
                }

                // Queue normal jobs with lower priority
                foreach (var job in normalJobs)
                {
                    _jobQueueService.QueueJob(job.Id, normalQueue.Id, priority: 5);
                    Console.WriteLine($"Queued normal job: {job.Name}");
                }

                // Process queues
                Console.WriteLine("\nProcessing job queues...");
                _multiJobRunner.StartJobsInQueue(criticalQueue.Id);
                _multiJobRunner.StartJobsInQueue(normalQueue.Id);

                // Monitor execution
                var allJobIds = new List<string>();
                criticalJobs.ForEach(j => allJobIds.Add(j.Id));
                normalJobs.ForEach(j => allJobIds.Add(j.Id));
                
                MonitorJobExecution(allJobIds, TimeSpan.FromMinutes(3));

                // Show queue statistics
                ShowQueueStatistics();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Example 2: {ex.Message}");
                _logService.LogError($"Example 2 failed: {ex.Message}", "MultiJobExample");
            }
        }

        /// <summary>
        /// Example 3: Job dependencies
        /// </summary>
        public void Example3_JobDependencies()
        {
            Console.WriteLine("\n=== Example 3: Job Dependencies ===");

            try
            {
                // Create jobs with dependencies
                var jobs = CreateSampleJobs(4, "Dependent");
                
                // Save jobs to repository
                foreach (var job in jobs)
                {
                    _jobRepository.Save(job);
                }

                Console.WriteLine("Created dependent jobs:");
                for (int i = 0; i < jobs.Count; i++)
                {
                    Console.WriteLine($"  Job {i + 1}: {jobs[i].Name} (ID: {jobs[i].Id})");
                }

                // Set up dependencies: Job2 depends on Job1, Job3 depends on Job2, Job4 depends on Job3
                var dependencies = new List<string>();
                
                // Job 1 has no dependencies (will run first)
                _jobQueueService.QueueJob(jobs[0].Id);
                Console.WriteLine($"Queued {jobs[0].Name} with no dependencies");

                // Job 2 depends on Job 1
                dependencies.Clear();
                dependencies.Add(jobs[0].Id);
                _jobQueueService.QueueJobWithDependencies(jobs[1].Id, dependencies);
                Console.WriteLine($"Queued {jobs[1].Name} depending on {jobs[0].Name}");

                // Job 3 depends on Job 2
                dependencies.Clear();
                dependencies.Add(jobs[1].Id);
                _jobQueueService.QueueJobWithDependencies(jobs[2].Id, dependencies);
                Console.WriteLine($"Queued {jobs[2].Name} depending on {jobs[1].Name}");

                // Job 4 depends on Job 3
                dependencies.Clear();
                dependencies.Add(jobs[2].Id);
                _jobQueueService.QueueJobWithDependencies(jobs[3].Id, dependencies);
                Console.WriteLine($"Queued {jobs[3].Name} depending on {jobs[2].Name}");

                Console.WriteLine("\nStarting dependent job chain...");
                
                // Start the first job (others will follow automatically as dependencies are met)
                _multiJobRunner.StartJob(jobs[0].Id);

                // Monitor execution
                var jobIds = jobs.ConvertAll(j => j.Id);
                MonitorJobExecution(jobIds, TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Example 3: {ex.Message}");
                _logService.LogError($"Example 3 failed: {ex.Message}", "MultiJobExample");
            }
        }

        /// <summary>
        /// Example 4: Configuration management
        /// </summary>
        public void Example4_ConfigurationManagement()
        {
            Console.WriteLine("\n=== Example 4: Configuration Management ===");

            try
            {
                // Get current configuration
                var config = _configService.GetConfiguration();
                Console.WriteLine("Current Multi-Job Configuration:");
                Console.WriteLine($"  Global Max Concurrent Jobs: {config.GlobalMaxConcurrentJobs}");
                Console.WriteLine($"  Default Queue Max Concurrent Jobs: {config.DefaultQueueMaxConcurrentJobs}");
                Console.WriteLine($"  Queue Processing Interval: {config.QueueProcessingIntervalSeconds} seconds");
                Console.WriteLine($"  Auto Retry Enabled: {config.EnableAutoRetry}");
                Console.WriteLine($"  Max Retries: {config.DefaultMaxRetries}");
                Console.WriteLine($"  Dependency Checking: {config.EnableDependencyChecking}");
                Console.WriteLine($"  Job Prioritization: {config.EnableJobPrioritization}");

                // Update configuration for testing
                var newConfig = new MultiJobConfiguration
                {
                    GlobalMaxConcurrentJobs = 8,
                    DefaultQueueMaxConcurrentJobs = 4,
                    QueueProcessingIntervalSeconds = 3,
                    EnableAutoRetry = true,
                    DefaultMaxRetries = 5,
                    RetryDelayMinutes = 2,
                    EnableDependencyChecking = true,
                    EnableJobPrioritization = true,
                    JobTimeoutMinutes = 30,
                    EnableDetailedLogging = true
                };

                Console.WriteLine("\nUpdating configuration...");
                if (_configService.UpdateConfiguration(newConfig))
                {
                    Console.WriteLine("Configuration updated successfully!");
                    
                    // Update the multi-job runner with new settings
                    _multiJobRunner.SetMaxConcurrentJobs(newConfig.GlobalMaxConcurrentJobs);
                    Console.WriteLine($"Updated max concurrent jobs to: {_multiJobRunner.GetMaxConcurrentJobs()}");
                }
                else
                {
                    Console.WriteLine("Failed to update configuration.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Example 4: {ex.Message}");
                _logService.LogError($"Example 4 failed: {ex.Message}", "MultiJobExample");
            }
        }

        /// <summary>
        /// Run all examples
        /// </summary>
        public void RunAllExamples()
        {
            Console.WriteLine("=== DataSyncer Multi-Job Functionality Examples ===");
            Console.WriteLine("This demonstration shows the new concurrent job execution capabilities.\n");

            Example1_BasicMultiJobExecution();
            Thread.Sleep(2000);

            Example2_JobQueueWithPriorities();
            Thread.Sleep(2000);

            Example3_JobDependencies();
            Thread.Sleep(2000);

            Example4_ConfigurationManagement();

            Console.WriteLine("\n=== All examples completed ===");
        }

        #region Helper Methods

        private List<SyncJob> CreateSampleJobs(int count, string prefix = "Sample")
        {
            var jobs = new List<SyncJob>();
            
            for (int i = 1; i <= count; i++)
            {
                var job = new SyncJob
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"{prefix} Job {i}",
                    Description = $"This is {prefix.ToLower()} job number {i} for demonstration",
                    IsEnabled = true,
                    SourcePath = $@"C:\temp\source{i}",
                    DestinationPath = $@"C:\temp\dest{i}",
                    IncludeSubfolders = true,
                    OverwriteExisting = true,
                    TransferMode = TransferMode.Mirror,
                    Connection = new ConnectionSettings { Protocol = ProtocolType.Local },
                    CreatedDate = DateTime.Now,
                    MaxRetries = 3
                };

                jobs.Add(job);
            }

            return jobs;
        }

        private void MonitorJobExecution(List<string> jobIds, TimeSpan timeout)
        {
            var startTime = DateTime.Now;
            var lastStatusUpdate = DateTime.Now;

            Console.WriteLine("Monitoring job execution...");
            Console.WriteLine("Press any key to stop monitoring early.\n");

            while (DateTime.Now - startTime < timeout)
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                    Console.WriteLine("\nMonitoring stopped by user.");
                    break;
                }

                // Update status every 5 seconds
                if (DateTime.Now - lastStatusUpdate >= TimeSpan.FromSeconds(5))
                {
                    var statuses = _multiJobRunner.GetAllJobStatuses();
                    int runningCount = 0;
                    int completedCount = 0;
                    int failedCount = 0;

                    foreach (var jobId in jobIds)
                    {
                        if (statuses.ContainsKey(jobId))
                        {
                            var status = statuses[jobId];
                            if (status == "Running") runningCount++;
                            else if (status == "Completed") completedCount++;
                            else if (status == "Failed") failedCount++;
                        }
                    }

                    Console.WriteLine($"Status Update - Running: {runningCount}, Completed: {completedCount}, Failed: {failedCount}");
                    lastStatusUpdate = DateTime.Now;

                    // Check if all jobs are done
                    if (runningCount == 0 && (completedCount + failedCount) >= jobIds.Count)
                    {
                        Console.WriteLine("All jobs completed!");
                        break;
                    }
                }

                Thread.Sleep(1000);
            }

            Console.WriteLine("\nFinal job statuses:");
            var finalStatuses = _multiJobRunner.GetAllJobStatuses();
            foreach (var jobId in jobIds)
            {
                var job = _jobRepository.GetById(jobId);
                var status = finalStatuses.ContainsKey(jobId) ? finalStatuses[jobId] : "Unknown";
                Console.WriteLine($"  {job?.Name ?? jobId}: {status}");
            }
        }

        private void ShowQueueStatistics()
        {
            Console.WriteLine("\nQueue Statistics:");
            var allStats = _jobQueueService.GetAllQueueStatistics();
            
            foreach (var stats in allStats)
            {
                Console.WriteLine($"\nQueue: {stats.QueueName}");
                Console.WriteLine($"  Total Jobs: {stats.TotalJobs}");
                Console.WriteLine($"  Pending: {stats.PendingJobs}");
                Console.WriteLine($"  Running: {stats.RunningJobs}");
                Console.WriteLine($"  Completed: {stats.CompletedJobs}");
                Console.WriteLine($"  Failed: {stats.FailedJobs}");
                Console.WriteLine($"  Success Rate: {stats.SuccessRate:F1}%");
                Console.WriteLine($"  Max Concurrent: {stats.MaxConcurrentJobs}");
                Console.WriteLine($"  Active: {stats.IsActive}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Program entry point for running the multi-job examples
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var example = new MultiJobExample();
                example.RunAllExamples();
                
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
