using System;
using System.Collections.Generic;
using System.Linq;

namespace FTPSyncer.core
{
    /// <summary>
    /// Job Scheduler Service - Note: This is a basic implementation
    /// For production use with Quartz.NET, you'll need to install Quartz.NET NuGet package
    /// Install-Package Quartz
    /// </summary>
    public class JobScheduler : IJobScheduler
    {
        private readonly IJobRepository _jobRepository;
        private readonly IJobRunner _jobRunner;
        private readonly ILogService _logService;
        private readonly Dictionary<string, System.Threading.Timer> _scheduledJobs;
        private readonly object _lockObject = new object();

        public event EventHandler<JobScheduledEventArgs> JobScheduled;
        public event EventHandler<JobScheduledEventArgs> JobUnscheduled;
        public event EventHandler<ScheduledJobTriggeredEventArgs> ScheduledJobTriggered;

        public JobScheduler(IJobRepository jobRepository, IJobRunner jobRunner, ILogService logService)
        {
            _jobRepository = jobRepository ?? throw new ArgumentNullException("jobRepository");
            _jobRunner = jobRunner ?? throw new ArgumentNullException("jobRunner");
            _logService = logService ?? throw new ArgumentNullException("logService");
            _scheduledJobs = new Dictionary<string, System.Threading.Timer>();
        }

        public void Start()
        {
            lock (_lockObject)
            {
                _logService.LogInfo(null, "Starting job scheduler");

                // Load all scheduled jobs and set them up
                var jobs = _jobRepository.GetAll();
                foreach (var job in jobs.Where(j => j.IsScheduled && j.IsEnabled))
                {
                    ScheduleJobInternal(job);
                }

                _logService.LogInfo(null, "Job scheduler started with " + _scheduledJobs.Count + " scheduled jobs");
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                _logService.LogInfo(null, "Stopping job scheduler");

                foreach (var timer in _scheduledJobs.Values)
                {
                    timer?.Dispose();
                }
                _scheduledJobs.Clear();

                _logService.LogInfo(null, "Job scheduler stopped");
            }
        }

        public bool ScheduleJob(SyncJob job)
        {
            if (job == null || !job.IsScheduled || job.Schedule == null)
                return false;

            lock (_lockObject)
            {
                try
                {
                    // Unschedule if already scheduled
                    UnscheduleJobInternal(job.Id);

                    // Schedule the job
                    if (ScheduleJobInternal(job))
                    {
                        _logService.LogInfo(job.Id, "Job '" + job.Name + "' scheduled with cron: " + job.Schedule.CronExpression);
                        OnJobScheduled(new JobScheduledEventArgs { Job = job });
                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    _logService.LogError(job.Id, $"Failed to schedule job '{job.Name}': {ex.Message}");
                    return false;
                }
            }
        }

        public bool UnscheduleJob(string jobId)
        {
            lock (_lockObject)
            {
                try
                {
                    if (UnscheduleJobInternal(jobId))
                    {
                        var job = _jobRepository.GetById(jobId);
                        if (job != null)
                        {
                            _logService.LogInfo(jobId, $"Job '{job.Name}' unscheduled");
                            OnJobUnscheduled(new JobScheduledEventArgs { Job = job });
                        }
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    _logService.LogError(jobId, $"Failed to unschedule job: {ex.Message}");
                    return false;
                }
            }
        }

        public bool IsJobScheduled(string jobId)
        {
            lock (_lockObject)
            {
                return _scheduledJobs.ContainsKey(jobId);
            }
        }

        public List<string> GetScheduledJobs()
        {
            lock (_lockObject)
            {
                return _scheduledJobs.Keys.ToList();
            }
        }

        public DateTime? GetNextRunTime(string jobId)
        {
            // This is a simplified implementation
            // With Quartz.NET, you would get the actual next fire time from the trigger
            var job = _jobRepository.GetById(jobId);
            if (job?.Schedule != null)
            {
                return CalculateNextRunTime(job.Schedule.CronExpression);
            }
            return null;
        }

        private bool ScheduleJobInternal(SyncJob job)
        {
            try
            {
                if (string.IsNullOrEmpty(job.Schedule.CronExpression))
                    return false;

                // Parse cron expression and calculate next run time
                var nextRunTime = CalculateNextRunTime(job.Schedule.CronExpression);
                if (!nextRunTime.HasValue)
                    return false;

                var delay = nextRunTime.Value - DateTime.Now;
                if (delay.TotalMilliseconds < 0)
                {
                    // If the time has passed, calculate next occurrence
                    nextRunTime = CalculateNextRunTime(job.Schedule.CronExpression, DateTime.Now.AddMinutes(1));
                    if (!nextRunTime.HasValue)
                        return false;
                    delay = nextRunTime.Value - DateTime.Now;
                }

                // Create timer for the job
                var timer = new System.Threading.Timer(
                    callback: _ => ExecuteScheduledJob(job.Id),
                    state: null,
                    dueTime: (int)delay.TotalMilliseconds,
                    period: System.Threading.Timeout.Infinite);

                _scheduledJobs[job.Id] = timer;
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError(job.Id, $"Failed to schedule job internally: {ex.Message}");
                return false;
            }
        }

        private bool UnscheduleJobInternal(string jobId)
        {
            if (_scheduledJobs.ContainsKey(jobId))
            {
                _scheduledJobs[jobId]?.Dispose();
                _scheduledJobs.Remove(jobId);
                return true;
            }
            return false;
        }

        private void ExecuteScheduledJob(string jobId)
        {
            try
            {
                var job = _jobRepository.GetById(jobId);
                if (job == null)
                {
                    _logService.LogError(jobId, "Scheduled job not found in repository");
                    UnscheduleJobInternal(jobId);
                    return;
                }

                if (!job.IsEnabled)
                {
                    _logService.LogInfo(jobId, "Scheduled job is disabled, skipping execution");
                    return;
                }

                _logService.LogInfo(jobId, $"Executing scheduled job '{job.Name}'");

                // Notify that scheduled job is triggered
                OnScheduledJobTriggered(new ScheduledJobTriggeredEventArgs { Job = job });



                // Reschedule for next occurrence
                if (job.IsScheduled && job.Schedule != null)
                {
                    RescheduleJob(job);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError(jobId, $"Error in scheduled job execution: {ex.Message}");
            }
        }

        private void RescheduleJob(SyncJob job)
        {
            try
            {
                // Remove current timer
                UnscheduleJobInternal(job.Id);

                // Schedule for next occurrence
                ScheduleJobInternal(job);
            }
            catch (Exception ex)
            {
                _logService.LogError(job.Id, $"Failed to reschedule job: {ex.Message}");
            }
        }

        private DateTime? CalculateNextRunTime(string cronExpression, DateTime? fromTime = null)
        {
            // This is a very basic cron parser
            // For production, use Quartz.NET's CronExpression class
            
            try
            {
                var baseTime = fromTime ?? DateTime.Now;
                var parts = cronExpression.Split(' ');
                
                if (parts.Length < 5)
                    return null;

                // Very basic parsing - only handles simple cases
                // Format: minute hour day month dayOfWeek
                
                var minute = parts[0];
                var hour = parts[1];
                var day = parts[2];
                var month = parts[3];
                var dayOfWeek = parts[4];

                // Handle simple cases like "0 9 * * *" (daily at 9 AM)
                if (minute == "*" || hour == "*")
                    return baseTime.AddMinutes(1); // Every minute for testing

                if (int.TryParse(minute, out int min) && int.TryParse(hour, out int hr))
                {
                    var nextRun = new DateTime(baseTime.Year, baseTime.Month, baseTime.Day, hr, min, 0);
                    
                    if (nextRun <= baseTime)
                    {
                        nextRun = nextRun.AddDays(1);
                    }
                    
                    return nextRun;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        protected virtual void OnJobScheduled(JobScheduledEventArgs e)
        {
            JobScheduled?.Invoke(this, e);
        }

        protected virtual void OnJobUnscheduled(JobScheduledEventArgs e)
        {
            JobUnscheduled?.Invoke(this, e);
        }

        protected virtual void OnScheduledJobTriggered(ScheduledJobTriggeredEventArgs e)
        {
            ScheduledJobTriggered?.Invoke(this, e);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}





