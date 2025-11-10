using System;
using System.Collections.Generic;

namespace FTPSyncer.core.Models
{
    /// <summary>
    /// Settings for job scheduling
    /// </summary>
    public class ScheduleSettings
    {
        public bool Enabled { get; set; } = false;
        public int Interval { get; set; } = 60; // In minutes
        public bool UseSpecificTimes { get; set; } = false;
        public List<TimeSpan> ExecutionTimes { get; set; } = new List<TimeSpan>();
        public int MaxConcurrentJobs { get; set; } = 1;
        public bool RunOnStartup { get; set; } = false;
        public bool SkipIfJobRunning { get; set; } = true;
        public RetrySettings RetrySettings { get; set; } = new RetrySettings();

        public ScheduleSettings()
        {
            ExecutionTimes = new List<TimeSpan>();
        }

        public ScheduleSettings Clone()
        {
            var clone = new ScheduleSettings
            {
                Enabled = this.Enabled,
                Interval = this.Interval,
                UseSpecificTimes = this.UseSpecificTimes,
                MaxConcurrentJobs = this.MaxConcurrentJobs,
                RunOnStartup = this.RunOnStartup,
                SkipIfJobRunning = this.SkipIfJobRunning,
                RetrySettings = this.RetrySettings?.Clone()
            };

            // Deep copy execution times
            foreach (var time in this.ExecutionTimes)
            {
                clone.ExecutionTimes.Add(time);
            }

            return clone;
        }
    }

    /// <summary>
    /// Settings for job retry logic
    /// </summary>
    public class RetrySettings
    {
        public bool Enabled { get; set; } = false;
        public int MaxRetries { get; set; } = 3;
        public int RetryIntervalMinutes { get; set; } = 5;
        public bool OnlyRetryOnError { get; set; } = true;

        public RetrySettings Clone()
        {
            return new RetrySettings
            {
                Enabled = this.Enabled,
                MaxRetries = this.MaxRetries,
                RetryIntervalMinutes = this.RetryIntervalMinutes,
                OnlyRetryOnError = this.OnlyRetryOnError
            };
        }
    }
}





