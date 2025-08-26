using System;
using System.Collections.Generic;
using System.Threading;
using syncer.core.Models;

namespace syncer.core.Services
{
    /// <summary>
    /// Provides scheduling functionality for automatic job execution at specified intervals
    /// </summary>
    public class SchedulerService : ISchedulerService
    {
        private readonly ILogService _logService;
        private readonly IJobRepository _jobRepository;
        private readonly TransferEngine _transferEngine;
        private readonly Dictionary<string, Timer> _jobTimers;
        private readonly object _timerLock = new object();
        private readonly object _executionLock = new object();
        private bool _isRunning;

        public event EventHandler JobScheduled;
        public event EventHandler JobTriggered;

        public SchedulerService(
            ILogService logService, 
            IJobRepository jobRepository, 
            TransferEngine transferEngine)
        {
            _logService = logService ?? throw new ArgumentNullException("logService");
            _jobRepository = jobRepository ?? throw new ArgumentNullException("jobRepository");
            _transferEngine = transferEngine ?? throw new ArgumentNullException("transferEngine");
            _jobTimers = new Dictionary<string, Timer>();
            _isRunning = false;
        }

        /// <summary>
        /// Start the scheduler service
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _logService.LogInfo("Scheduler service started");

            // Set up all scheduled jobs
            RefreshScheduledJobs();
        }

        /// <summary>
        /// Stop the scheduler service
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;

            // Clean up all timers
            lock (_timerLock)
            {
                foreach (var timer in _jobTimers.Values)
                {
                    try
                    {
                        timer.Change(Timeout.Infinite, Timeout.Infinite);
                        timer.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logService.LogError($"Error disposing timer: {ex.Message}");
                    }
                }
                _jobTimers.Clear();
            }

            _logService.LogInfo("Scheduler service stopped");
        }

        /// <summary>
        /// Load all scheduled jobs from the repository and set up timers
        /// </summary>
        public void RefreshScheduledJobs()
        {
            if (!_isRunning)
                return;

            // First, clear existing timers
            lock (_timerLock)
            {
                foreach (var timer in _jobTimers.Values)
                {
                    try
                    {
                        timer.Change(Timeout.Infinite, Timeout.Infinite);
                        timer.Dispose();
                    }
                    catch (Exception ex)
                    {
                        // Log timer disposal error but continue cleanup
                        System.Diagnostics.Debug.WriteLine("Failed to dispose timer during cleanup: " + ex.Message);
                    }
                }
                _jobTimers.Clear();
            }

            // Load all jobs
            List<SyncJob> jobs = _jobRepository.GetAll();
            
            foreach (var job in jobs)
            {
                if (job.IsEnabled && job.IsScheduled && job.Schedule != null)
                {
                    ScheduleJob(job);
                }
            }

            _logService.LogInfo($"Scheduler refreshed: {_jobTimers.Count} jobs scheduled");
        }

        /// <summary>
        /// Schedule a specific job
        /// </summary>
        public void ScheduleJob(SyncJob job)
        {
            if (job == null || !job.IsEnabled || !job.IsScheduled || job.Schedule == null)
                return;

            lock (_timerLock)
            {
                // Remove existing timer if any
                if (_jobTimers.ContainsKey(job.Id))
                {
                    try
                    {
                        _jobTimers[job.Id].Change(Timeout.Infinite, Timeout.Infinite);
                        _jobTimers[job.Id].Dispose();
                        _jobTimers.Remove(job.Id);
                    }
                    catch (Exception ex)
                    {
                        // Log timer cleanup error but continue
                        System.Diagnostics.Debug.WriteLine("Failed to cleanup job timer for " + job.Id + ": " + ex.Message);
                        // Still try to remove from dictionary
                        _jobTimers.Remove(job.Id);
                    }
                }

                // Calculate next run time
                TimeSpan delay = CalculateNextRunDelay(job);

                if (delay.TotalMilliseconds <= 0)
                {
                    // Invalid schedule or job should not run
                    _logService.LogWarning($"Job {job.Name} has an invalid schedule - not scheduled");
                    return;
                }

                // Create new timer
                Timer timer = new Timer(OnTimerElapsed, job.Id, delay, Timeout.InfiniteTimeSpan);
                _jobTimers.Add(job.Id, timer);

                // Update job's next run time
                job.NextRun = DateTime.Now.Add(delay);
                _jobRepository.Save(job);

                _logService.LogInfo($"Job {job.Name} scheduled to run in {(int)delay.TotalMinutes} minutes");
                OnJobScheduled(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Unschedule a specific job
        /// </summary>
        public void UnscheduleJob(string jobId)
        {
            lock (_timerLock)
            {
                if (_jobTimers.ContainsKey(jobId))
                {
                    try
                    {
                        _jobTimers[jobId].Change(Timeout.Infinite, Timeout.Infinite);
                        _jobTimers[jobId].Dispose();
                        _jobTimers.Remove(jobId);
                    }
                    catch (Exception ex)
                    {
                        _logService.LogError($"Error unscheduling job: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the delay until the next scheduled run
        /// </summary>
        private TimeSpan CalculateNextRunDelay(SyncJob job)
        {
            if (job.Schedule.Interval <= 0)
                return TimeSpan.Zero; // Invalid interval

            // Convert interval from minutes to milliseconds
            double intervalMs = job.Schedule.Interval * 60 * 1000;
            
            // Default to running after the interval
            TimeSpan delay = TimeSpan.FromMilliseconds(intervalMs);

            // If the job has specific start times, calculate next occurrence
            if (job.Schedule.UseSpecificTimes && job.Schedule.ExecutionTimes.Count > 0)
            {
                DateTime now = DateTime.Now;
                DateTime? nextRun = null;

                // Find the next execution time today
                foreach (TimeSpan execTime in job.Schedule.ExecutionTimes)
                {
                    DateTime todayExecTime = now.Date.Add(execTime);
                    if (todayExecTime > now)
                    {
                        if (nextRun == null || todayExecTime < nextRun.Value)
                            nextRun = todayExecTime;
                    }
                }

                // If no time today, check tomorrow
                if (nextRun == null)
                {
                    DateTime tomorrow = now.Date.AddDays(1);
                    nextRun = tomorrow.Add(job.Schedule.ExecutionTimes[0]);
                }

                delay = nextRun.Value - now;
            }

            return delay;
        }

        /// <summary>
        /// Timer callback method
        /// </summary>
        private void OnTimerElapsed(object state)
        {
            string jobId = state as string;
            if (string.IsNullOrEmpty(jobId))
                return;

            // Re-query the job from repository to get latest settings
            SyncJob job = _jobRepository.GetById(jobId);
            if (job == null || !job.IsEnabled || !job.IsScheduled)
                return;

            // Prevent concurrent execution of the same job
            if (Monitor.TryEnter(_executionLock))
            {
                try
                {
                    // Check if job is already running
                    if (_transferEngine.IsJobRunning(jobId))
                    {
                        _logService.LogWarning($"Job {job.Name} is already running - skipping scheduled execution");
                        return;
                    }

                    _logService.LogInfo($"Executing scheduled job: {job.Name}");
                    OnJobTriggered(EventArgs.Empty);

                    // Execute job
                    var result = _transferEngine.ExecuteJob(job);

                    // Update job with last run details
                    job.LastRun = DateTime.Now;
                    job.LastStatus = result.Success ? "Success" : "Failed";
                    job.LastTransferCount = result.SuccessfulFiles;
                    job.LastTransferBytes = result.TransferredBytes;
                    job.LastDuration = result.Duration;
                    job.LastError = result.LastError ?? "";
                    _jobRepository.Save(job);
                }
                catch (Exception ex)
                {
                    _logService.LogError($"Error executing scheduled job {job.Name}: {ex.Message}");
                    
                    // Update job with failure info
                    job.LastRun = DateTime.Now;
                    job.LastStatus = "Error";
                    job.LastError = ex.Message;
                    _jobRepository.Save(job);
                }
                finally
                {
                    Monitor.Exit(_executionLock);
                }
            }
            else
            {
                _logService.LogWarning($"Cannot execute job {job.Name} - another job is currently running");
            }

            // Schedule next run
            ScheduleJob(job);
        }

        protected virtual void OnJobScheduled(EventArgs e)
        {
            JobScheduled?.Invoke(this, e);
        }

        protected virtual void OnJobTriggered(EventArgs e)
        {
            JobTriggered?.Invoke(this, e);
        }
    }
}
