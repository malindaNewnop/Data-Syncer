using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace syncer.core
{
    /// <summary>
    /// Implementation of IJobRunner that can run multiple jobs in parallel
    /// Each job runs in its own thread
    /// </summary>
    public class ParallelJobRunner : IJobRunner
    {
        private readonly object _lockObject = new object();
        private readonly IJobRepository _jobRepository;
        private readonly ILogService _logService;
        private readonly Dictionary<string, Thread> _runningJobs = new Dictionary<string, Thread>();
        private readonly Dictionary<string, bool> _cancellationFlags = new Dictionary<string, bool>();
        private readonly ITransferProgressTracker _progressTracker;
        
        public event EventHandler<JobStatusEventArgs> JobStatusChanged;
        
        public ParallelJobRunner(IJobRepository jobRepository, ILogService logService, ITransferProgressTracker progressTracker = null)
        {
            _jobRepository = jobRepository;
            _logService = logService;
            _progressTracker = progressTracker ?? new DefaultTransferProgressTracker();
        }
        
        public bool IsRunning(string jobId)
        {
            lock (_lockObject)
            {
                return _runningJobs.ContainsKey(jobId) && _runningJobs[jobId].IsAlive;
            }
        }
        
        public List<string> GetRunningJobIds()
        {
            lock (_lockObject)
            {
                return _runningJobs.Where(kv => kv.Value.IsAlive).Select(kv => kv.Key).ToList();
            }
        }
        
        public bool StartJob(string jobId)
        {
            lock (_lockObject)
            {
                if (_runningJobs.ContainsKey(jobId) && _runningJobs[jobId].IsAlive)
                {
                    _logService.LogWarning($"Job {jobId} is already running", "ParallelJobRunner");
                    return false;
                }
                
                var job = _jobRepository.GetById(jobId);
                if (job == null)
                {
                    _logService.LogError($"Job {jobId} not found", "ParallelJobRunner");
                    return false;
                }
                
                if (!job.IsEnabled)
                {
                    _logService.LogWarning($"Job {jobId} is disabled", "ParallelJobRunner");
                    return false;
                }
                
                // Create a new thread for the job
                var thread = new Thread(JobThreadProc);
                thread.IsBackground = true;
                thread.Name = $"Job-{job.Name}-{jobId}";
                
                // Store the thread and reset cancellation flag
                _runningJobs[jobId] = thread;
                _cancellationFlags[jobId] = false;
                
                // Start the thread
                thread.Start(jobId);
                
                _logService.LogInfo($"Started job {job.Name} ({jobId}) in a new thread", "ParallelJobRunner");
                return true;
            }
        }
        
        public bool CancelJob(string jobId)
        {
            lock (_lockObject)
            {
                if (!_runningJobs.ContainsKey(jobId) || !_runningJobs[jobId].IsAlive)
                {
                    _logService.LogWarning($"Job {jobId} is not running", "ParallelJobRunner");
                    return false;
                }
                
                // Set cancellation flag
                _cancellationFlags[jobId] = true;
                
                _logService.LogInfo($"Cancellation requested for job {jobId}", "ParallelJobRunner");
                return true;
            }
        }
        
        public bool WaitForJob(string jobId, int timeoutMilliseconds = -1)
        {
            Thread jobThread;
            lock (_lockObject)
            {
                if (!_runningJobs.ContainsKey(jobId) || !_runningJobs[jobId].IsAlive)
                {
                    return true; // Job is not running, so we're done waiting
                }
                
                jobThread = _runningJobs[jobId];
            }
            
            // Wait for the thread to complete
            return jobThread.Join(timeoutMilliseconds);
        }
        
        private void JobThreadProc(object state)
        {
            string jobId = (string)state;
            SyncJob job = null;
            
            try
            {
                // Get the job from the repository
                job = _jobRepository.GetById(jobId);
                if (job == null)
                {
                    _logService.LogError($"Job {jobId} not found", "ParallelJobRunner");
                    return;
                }
                
                // Update job status to running
                job.IsRunning = true;
                job.LastStatus = "Running";
                job.LastRun = DateTime.Now;
                _jobRepository.Save(job);
                
                // Notify status change
                OnJobStatusChanged(new JobStatusEventArgs { JobId = jobId, Status = "Running" });
                
                // Log job start
                _logService.LogJobStart(job);
                
                // Create the job worker
                IJobWorker worker = JobWorkerFactory.CreateWorker(job);
                
                // Connect progress events
                worker.ProgressChanged += (sender, e) =>
                {
                    _progressTracker.UpdateProgress(jobId, e);
                    _logService.LogJobProgress(job, $"Progress: {e.PercentComplete}% - {e.CurrentFile}");
                    
                    // Check cancellation
                    bool shouldCancel = false;
                    lock (_lockObject)
                    {
                        if (_cancellationFlags.ContainsKey(jobId))
                        {
                            shouldCancel = _cancellationFlags[jobId];
                        }
                    }
                    
                    if (shouldCancel)
                    {
                        e.Cancel = true;
                    }
                };
                
                // Execute the job
                JobResult result = worker.Execute();
                
                // Update job with results
                job.LastStatus = result.Success ? "Success" : "Failed";
                job.LastError = result.ErrorMessage;
                job.LastTransferCount = result.TransferredFiles;
                job.LastTransferBytes = result.TransferredBytes;
                job.LastDuration = result.Duration;
                job.IsRunning = false;
                
                // Calculate next run time if scheduled
                if (job.IsEnabled && job.IsScheduled && job.Schedule != null)
                {
                    job.NextRun = CalculateNextRunTime(job);
                }
                
                // Save updated job
                _jobRepository.Save(job);
                
                // Log completion
                if (result.Success)
                {
                    _logService.LogJobSuccess(job, $"Transferred {result.TransferredFiles} files ({result.TransferredBytes} bytes) in {result.Duration.TotalSeconds:F1} seconds");
                }
                else
                {
                    _logService.LogJobError(job, $"Failed: {result.ErrorMessage}");
                }
                
                // Notify status change
                OnJobStatusChanged(new JobStatusEventArgs { JobId = jobId, Status = job.LastStatus });
            }
            catch (Exception ex)
            {
                // Log any unhandled exceptions
                _logService.LogError($"Unhandled exception in job thread: {ex.Message}", "ParallelJobRunner", ex);
                
                // Update job status if we have a job object
                if (job != null)
                {
                    job.LastStatus = "Failed";
                    job.LastError = $"Unhandled exception: {ex.Message}";
                    job.IsRunning = false;
                    
                    // Save the job
                    try
                    {
                        _jobRepository.Save(job);
                    }
                    catch
                    {
                        // Ignore errors when saving the job
                    }
                    
                    // Notify status change
                    OnJobStatusChanged(new JobStatusEventArgs { JobId = jobId, Status = "Failed" });
                }
            }
            finally
            {
                // Clean up
                lock (_lockObject)
                {
                    _cancellationFlags.Remove(jobId);
                }
            }
        }
        
        private DateTime? CalculateNextRunTime(SyncJob job)
        {
            if (job.Schedule == null || !job.IsScheduled)
            {
                return null;
            }
            
            DateTime now = DateTime.Now;
            DateTime baseTime = now;
            
            // Daily schedule
            if (job.Schedule.Type == ScheduleType.Daily)
            {
                var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 
                    job.Schedule.Hour, job.Schedule.Minute, 0);
                
                if (scheduledTime <= now)
                {
                    // Today's scheduled time has passed, schedule for tomorrow
                    scheduledTime = scheduledTime.AddDays(1);
                }
                
                return scheduledTime;
            }
            // Weekly schedule
            else if (job.Schedule.Type == ScheduleType.Weekly)
            {
                if (job.Schedule.DaysOfWeek == null || job.Schedule.DaysOfWeek.Count == 0)
                {
                    return null;
                }
                
                var daysOfWeek = job.Schedule.DaysOfWeek.Select(d => (DayOfWeek)d).ToList();
                
                // Find the next day of week that matches the schedule
                DateTime scheduledTime = new DateTime(now.Year, now.Month, now.Day, 
                    job.Schedule.Hour, job.Schedule.Minute, 0);
                
                if (scheduledTime <= now)
                {
                    // Today's scheduled time has passed, so start from tomorrow
                    scheduledTime = scheduledTime.AddDays(1);
                }
                
                // Check up to 7 days forward to find the next scheduled day
                for (int i = 0; i < 7; i++)
                {
                    if (daysOfWeek.Contains(scheduledTime.DayOfWeek))
                    {
                        return scheduledTime;
                    }
                    
                    scheduledTime = scheduledTime.AddDays(1);
                }
            }
            // Monthly schedule
            else if (job.Schedule.Type == ScheduleType.Monthly)
            {
                int day = Math.Min(job.Schedule.DayOfMonth, DateTime.DaysInMonth(now.Year, now.Month));
                var scheduledTime = new DateTime(now.Year, now.Month, day, 
                    job.Schedule.Hour, job.Schedule.Minute, 0);
                
                if (scheduledTime <= now)
                {
                    // This month's scheduled time has passed, schedule for next month
                    if (now.Month == 12)
                    {
                        // December, so schedule for January next year
                        day = Math.Min(job.Schedule.DayOfMonth, DateTime.DaysInMonth(now.Year + 1, 1));
                        scheduledTime = new DateTime(now.Year + 1, 1, day, 
                            job.Schedule.Hour, job.Schedule.Minute, 0);
                    }
                    else
                    {
                        // Schedule for next month
                        day = Math.Min(job.Schedule.DayOfMonth, DateTime.DaysInMonth(now.Year, now.Month + 1));
                        scheduledTime = new DateTime(now.Year, now.Month + 1, day, 
                            job.Schedule.Hour, job.Schedule.Minute, 0);
                    }
                }
                
                return scheduledTime;
            }
            // Interval schedule
            else if (job.Schedule.Type == ScheduleType.Interval)
            {
                // Schedule for the next interval from now
                return now.AddMinutes(job.Schedule.IntervalMinutes);
            }
            
            return null;
        }
        
        protected virtual void OnJobStatusChanged(JobStatusEventArgs e)
        {
            JobStatusChanged?.Invoke(this, e);
        }
        
        #region IDisposable Implementation
        
        private bool _disposed = false;
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    lock (_lockObject)
                    {
                        // Set cancellation flags for all running jobs
                        foreach (var jobId in new List<string>(_runningJobs.Keys))
                        {
                            _cancellationFlags[jobId] = true;
                            _logService.LogInfo($"Cancelling job {jobId} during disposal", "ParallelJobRunner");
                        }
                        
                        // Wait a short time for jobs to respond to cancellation
                        Thread.Sleep(500);
                        
                        // Attempt to abort any threads that didn't respond to cancellation
                        foreach (var kvp in _runningJobs)
                        {
                            try
                            {
                                if (kvp.Value.IsAlive)
                                {
                                    kvp.Value.Abort();
                                }
                            }
                            catch (Exception ex)
                            {
                                _logService.LogError($"Error aborting job thread {kvp.Key}: {ex.Message}", "ParallelJobRunner");
                            }
                        }
                        
                        // Clear collections
                        _runningJobs.Clear();
                        _cancellationFlags.Clear();
                    }
                }
                
                _disposed = true;
            }
        }
        
        ~ParallelJobRunner()
        {
            Dispose(false);
        }
        
        #endregion
    }
    
    public class JobStatusEventArgs : EventArgs
    {
        public string JobId { get; set; }
        public string Status { get; set; }
    }
}
