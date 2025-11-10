using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Win32;
using System.Diagnostics;

namespace FTPSyncer.service
{
    /// <summary>
    /// Manages persistence of running job state to allow recovery after system restart
    /// </summary>
    public class JobStateManager
    {
        private const string REGISTRY_KEY = @"SOFTWARE\FTPSyncer\Service";
        private const string RUNNING_JOBS_VALUE = "RunningJobs";
        private const string SERVICE_STATE_FILE = "ServiceState.xml";
        private const string TIMER_JOBS_STATE_FILE = "TimerJobsState.xml";
        private const int MAX_RESTART_ATTEMPTS = 3;
        private const int RESTART_ATTEMPT_DELAY_MINUTES = 5;

        /// <summary>
        /// Represents a running job state that can be persisted
        /// </summary>
        [Serializable]
        public class RunningJobState
        {
            public string JobId { get; set; }
            public string JobName { get; set; }
            public DateTime StartTime { get; set; }
            public bool IsManualStart { get; set; }
            public string UserId { get; set; }
            public int ProcessId { get; set; }
            public string JobType { get; set; } // Upload, Download, LocalToLocal, etc.
            public string SourcePath { get; set; }
            public string DestinationPath { get; set; }
            public long IntervalMs { get; set; } // For timer jobs
            public bool IsTimerJob { get; set; }
            public DateTime LastActivity { get; set; }
            public bool CanAutoRestart { get; set; }
            public int RestartAttempts { get; set; }
            public DateTime? LastRestartAttempt { get; set; }
            public string RecoveryData { get; set; } // JSON serialized recovery info
            
            public RunningJobState()
            {
                StartTime = DateTime.Now;
                LastActivity = DateTime.Now;
                CanAutoRestart = true;
                RestartAttempts = 0;
                ProcessId = Process.GetCurrentProcess().Id;
            }
        }

        /// <summary>
        /// Container for all service state information
        /// </summary>
        [Serializable]
        [XmlRoot("ServiceState")]
        public class ServiceState
        {
            public List<RunningJobState> RunningJobs { get; set; }
            public List<RunningJobState> TimerJobs { get; set; }
            public DateTime LastServiceStart { get; set; }
            public DateTime LastStateUpdate { get; set; }
            public bool ServiceWasRunning { get; set; }
            public bool UnexpectedShutdown { get; set; }
            public string ServiceVersion { get; set; }
            public int ServiceProcessId { get; set; }
            public DateTime SystemBootTime { get; set; }

            public ServiceState()
            {
                RunningJobs = new List<RunningJobState>();
                TimerJobs = new List<RunningJobState>();
                LastServiceStart = DateTime.Now;
                LastStateUpdate = DateTime.Now;
                ServiceWasRunning = true;
                ServiceVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                ServiceProcessId = Process.GetCurrentProcess().Id;
                SystemBootTime = GetSystemBootTime();
            }
            
            private DateTime GetSystemBootTime()
            {
                try
                {
                    using (var uptime = new PerformanceCounter("System", "System Up Time"))
                    {
                        uptime.NextValue();
                        return DateTime.Now.AddSeconds(-uptime.NextValue());
                    }
                }
                catch
                {
                    return DateTime.Now.AddHours(-1); // Fallback
                }
            }
        }

        private static readonly string StateFilePath = Path.Combine(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "FTPSyncer"), 
            SERVICE_STATE_FILE);

        /// <summary>
        /// Save current running job state to persistent storage
        /// </summary>
        public static void SaveRunningJobState(List<string> runningJobIds, FTPSyncer.core.IJobRepository jobRepository)
        {
            try
            {
                ServiceState state = new ServiceState();
                state.LastStateUpdate = DateTime.Now;
                state.ServiceWasRunning = true;

                // Get job details for each running job
                var allJobs = jobRepository.LoadAll();
                foreach (string jobId in runningJobIds)
                {
                    var job = allJobs.Find(j => j.Id == jobId);
                    if (job != null)
                    {
                        state.RunningJobs.Add(new RunningJobState
                        {
                            JobId = jobId,
                            JobName = job.Name,
                            StartTime = DateTime.Now,
                            IsManualStart = true, // Assume manual start for recovery
                            UserId = Environment.UserName,
                            ProcessId = Process.GetCurrentProcess().Id,
                            JobType = DetermineJobType(job),
                            SourcePath = job.SourcePath,
                            DestinationPath = job.DestinationPath,
                            CanAutoRestart = true,
                            LastActivity = DateTime.Now
                        });
                    }
                }

                // Save to file
                SaveStateToFile(state);

                // Save to registry as backup
                SaveStateToRegistry(state);
            }
            catch (Exception ex)
            {
                // Log error but don't throw to avoid disrupting service
                EventLog.WriteEntry("FTPSyncer Service", 
                    "Failed to save job state: " + ex.Message, 
                    EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Save timer job state for recovery after restart
        /// </summary>
        public static void SaveTimerJobState(Dictionary<long, object> timerJobs)
        {
            try
            {
                ServiceState state = LoadCurrentState() ?? new ServiceState();
                state.TimerJobs.Clear();
                state.LastStateUpdate = DateTime.Now;

                // Convert timer jobs to persistable state
                foreach (var kvp in timerJobs)
                {
                    try
                    {
                        // Use reflection to extract job info (safe for .NET 3.5)
                        var jobInfo = kvp.Value;
                        var jobType = jobInfo.GetType();
                        
                        var timerJobState = new RunningJobState
                        {
                            JobId = kvp.Key.ToString(),
                            JobName = GetPropertyValue(jobInfo, "JobName") ?? "Timer Job",
                            IsTimerJob = true,
                            IntervalMs = GetLongPropertyValue(jobInfo, "IntervalMs"),
                            SourcePath = GetPropertyValue(jobInfo, "FolderPath") ?? "",
                            DestinationPath = GetPropertyValue(jobInfo, "RemotePath") ?? "",
                            JobType = "Timer",
                            CanAutoRestart = true,
                            LastActivity = DateTime.Now
                        };

                        state.TimerJobs.Add(timerJobState);
                    }
                    catch (Exception jobEx)
                    {
                        EventLog.WriteEntry("FTPSyncer Service",
                            $"Error saving timer job {kvp.Key}: {jobEx.Message}",
                            EventLogEntryType.Warning);
                    }
                }

                SaveStateToFile(state);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("FTPSyncer Service",
                    "Failed to save timer job state: " + ex.Message,
                    EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Enhanced method to load jobs that need recovery with detailed state
        /// </summary>
        public static List<RunningJobState> LoadJobsForRecovery()
        {
            var jobsToRecover = new List<RunningJobState>();

            try
            {
                ServiceState state = LoadCurrentState();
                if (state == null)
                    return jobsToRecover;

                DateTime systemBootTime = state.SystemBootTime;
                bool wasUnexpectedShutdown = IsUnexpectedShutdown(state);

                // Process regular running jobs
                foreach (var job in state.RunningJobs)
                {
                    if (ShouldRecoverJob(job, wasUnexpectedShutdown))
                    {
                        jobsToRecover.Add(job);
                    }
                }

                // Process timer jobs
                foreach (var timerJob in state.TimerJobs)
                {
                    if (ShouldRecoverJob(timerJob, wasUnexpectedShutdown))
                    {
                        jobsToRecover.Add(timerJob);
                    }
                }

                return jobsToRecover;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("FTPSyncer Service",
                    "Error loading jobs for recovery: " + ex.Message,
                    EventLogEntryType.Warning);
                return jobsToRecover;
            }
        }

        /// <summary>
        /// Mark a job recovery attempt
        /// </summary>
        public static void MarkJobRecoveryAttempt(string jobId, bool successful)
        {
            try
            {
                ServiceState state = LoadCurrentState();
                if (state == null) return;

                var job = state.RunningJobs.Find(j => j.JobId == jobId) ?? 
                         state.TimerJobs.Find(j => j.JobId == jobId);

                if (job != null)
                {
                    job.RestartAttempts++;
                    job.LastRestartAttempt = DateTime.Now;

                    if (!successful && job.RestartAttempts >= MAX_RESTART_ATTEMPTS)
                    {
                        job.CanAutoRestart = false;
                    }

                    SaveStateToFile(state);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("FTPSyncer Service",
                    "Error marking job recovery attempt: " + ex.Message,
                    EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Load previously running jobs that need to be resumed
        /// </summary>
        public static List<string> LoadJobsToResume()
        {
            List<string> jobsToResume = new List<string>();
            
            try
            {
                ServiceState state = LoadStateFromFile();
                if (state == null)
                {
                    state = LoadStateFromRegistry();
                }

                if (state != null && state.ServiceWasRunning && state.RunningJobs != null)
                {
                    // Check if the service was interrupted (not cleanly shut down)
                    TimeSpan timeSinceLastUpdate = DateTime.Now - state.LastStateUpdate;
                    
                    // If more than 2 minutes have passed, assume the service was interrupted
                    if (timeSinceLastUpdate.TotalMinutes > 2)
                    {
                        foreach (var runningJob in state.RunningJobs)
                        {
                            jobsToResume.Add(runningJob.JobId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("FTPSyncer Service", 
                    "Failed to load job state for recovery: " + ex.Message, 
                    System.Diagnostics.EventLogEntryType.Warning);
            }

            return jobsToResume;
        }

        /// <summary>
        /// Mark service as cleanly shut down
        /// </summary>
        public static void MarkServiceStopped()
        {
            try
            {
                ServiceState state = new ServiceState();
                state.ServiceWasRunning = false;
                state.LastStateUpdate = DateTime.Now;
                state.RunningJobs.Clear();

                SaveStateToFile(state);
                SaveStateToRegistry(state);
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("FTPSyncer Service", 
                    "Failed to mark service stopped: " + ex.Message, 
                    System.Diagnostics.EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Clear all persisted job state
        /// </summary>
        public static void ClearJobState()
        {
            try
            {
                if (File.Exists(StateFilePath))
                {
                    File.Delete(StateFilePath);
                }

                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(REGISTRY_KEY))
                {
                    if (key.GetValue(RUNNING_JOBS_VALUE) != null)
                    {
                        key.DeleteValue(RUNNING_JOBS_VALUE);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("FTPSyncer Service", 
                    "Failed to clear job state: " + ex.Message, 
                    System.Diagnostics.EventLogEntryType.Warning);
            }
        }

        private static void SaveStateToFile(ServiceState state)
        {
            // Ensure directory exists
            string directory = Path.GetDirectoryName(StateFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Serialize to XML
            XmlSerializer serializer = new XmlSerializer(typeof(ServiceState));
            using (FileStream stream = new FileStream(StateFilePath, FileMode.Create))
            {
                serializer.Serialize(stream, state);
            }
        }

        private static ServiceState LoadStateFromFile()
        {
            if (!File.Exists(StateFilePath))
                return null;

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ServiceState));
                using (FileStream stream = new FileStream(StateFilePath, FileMode.Open))
                {
                    return (ServiceState)serializer.Deserialize(stream);
                }
            }
            catch
            {
                return null;
            }
        }

        private static void SaveStateToRegistry(ServiceState state)
        {
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(REGISTRY_KEY))
            {
                // Save as comma-separated job IDs for simplicity
                string jobIds = string.Join(",", state.RunningJobs.ConvertAll(j => j.JobId).ToArray());
                key.SetValue(RUNNING_JOBS_VALUE, jobIds);
                key.SetValue("LastUpdate", state.LastStateUpdate.ToBinary());
                key.SetValue("ServiceWasRunning", state.ServiceWasRunning);
            }
        }

        private static ServiceState LoadStateFromRegistry()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(REGISTRY_KEY))
                {
                    if (key == null)
                        return null;

                    ServiceState state = new ServiceState();
                    
                    string jobIds = key.GetValue(RUNNING_JOBS_VALUE) as string;
                    if (!string.IsNullOrEmpty(jobIds))
                    {
                        string[] ids = jobIds.Split(',');
                        foreach (string id in ids)
                        {
                            if (!string.IsNullOrEmpty(id.Trim()))
                            {
                                state.RunningJobs.Add(new RunningJobState
                                {
                                    JobId = id.Trim(),
                                    JobName = "Unknown",
                                    StartTime = DateTime.Now,
                                    IsManualStart = true
                                });
                            }
                        }
                    }

                    object lastUpdateValue = key.GetValue("LastUpdate");
                    if (lastUpdateValue != null)
                    {
                        state.LastStateUpdate = DateTime.FromBinary((long)lastUpdateValue);
                    }

                    object serviceRunningValue = key.GetValue("ServiceWasRunning");
                    if (serviceRunningValue != null)
                    {
                        state.ServiceWasRunning = (bool)serviceRunningValue;
                    }

                    return state;
                }
            }
            catch
            {
                return null;
            }
        }

        #region Helper Methods

        /// <summary>
        /// Load current state from file or registry
        /// </summary>
        private static ServiceState LoadCurrentState()
        {
            ServiceState state = LoadStateFromFile();
            if (state == null)
            {
                state = LoadStateFromRegistry();
            }
            return state;
        }

        /// <summary>
        /// Determine job type from job configuration
        /// </summary>
        private static string DetermineJobType(FTPSyncer.core.SyncJob job)
        {
            try
            {
                if (string.IsNullOrEmpty(job.SourcePath) || string.IsNullOrEmpty(job.DestinationPath))
                    return "Unknown";

                bool isSourceLocal = job.SourcePath.Contains(@"\") || job.SourcePath.Contains(":");
                bool isDestLocal = job.DestinationPath.Contains(@"\") || job.DestinationPath.Contains(":");

                if (isSourceLocal && isDestLocal)
                    return "LocalToLocal";
                else if (isSourceLocal)
                    return "Upload";
                else if (isDestLocal)
                    return "Download";
                else
                    return "RemoteToRemote";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Get property value using reflection (safe for .NET 3.5)
        /// </summary>
        private static string GetPropertyValue(object obj, string propertyName)
        {
            try
            {
                if (obj == null) return null;
                
                var propertyInfo = obj.GetType().GetProperty(propertyName);
                if (propertyInfo == null) return null;

                var value = propertyInfo.GetValue(obj, null);
                return value?.ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get long property value using reflection (safe for .NET 3.5)
        /// </summary>
        private static long GetLongPropertyValue(object obj, string propertyName)
        {
            try
            {
                if (obj == null) return 0;
                
                var propertyInfo = obj.GetType().GetProperty(propertyName);
                if (propertyInfo == null) return 0;

                var value = propertyInfo.GetValue(obj, null);
                if (value == null) return 0;

                return Convert.ToInt64(value);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Check if shutdown was unexpected
        /// </summary>
        private static bool IsUnexpectedShutdown(ServiceState state)
        {
            if (state == null) return false;

            // Check if service was marked as properly stopped
            if (!state.ServiceWasRunning) return false;

            // Check if system was rebooted since last service run
            DateTime currentBootTime = GetCurrentSystemBootTime();
            if (currentBootTime > state.LastServiceStart)
                return true;

            // Check time gap - if more than 10 minutes since last update, consider unexpected
            TimeSpan timeSinceUpdate = DateTime.Now - state.LastStateUpdate;
            return timeSinceUpdate.TotalMinutes > 10;
        }

        /// <summary>
        /// Determine if a job should be recovered
        /// </summary>
        private static bool ShouldRecoverJob(RunningJobState job, bool wasUnexpectedShutdown)
        {
            if (job == null || !job.CanAutoRestart) return false;

            // Don't restart if we've exceeded max attempts
            if (job.RestartAttempts >= MAX_RESTART_ATTEMPTS) return false;

            // Check if enough time has passed since last restart attempt
            if (job.LastRestartAttempt.HasValue)
            {
                TimeSpan timeSinceRestart = DateTime.Now - job.LastRestartAttempt.Value;
                if (timeSinceRestart.TotalMinutes < RESTART_ATTEMPT_DELAY_MINUTES)
                    return false;
            }

            // For timer jobs, always try to recover if conditions are met
            if (job.IsTimerJob) return true;

            // For regular jobs, only recover if shutdown was unexpected
            return wasUnexpectedShutdown;
        }

        /// <summary>
        /// Get current system boot time
        /// </summary>
        private static DateTime GetCurrentSystemBootTime()
        {
            try
            {
                using (var uptime = new PerformanceCounter("System", "System Up Time"))
                {
                    uptime.NextValue();
                    return DateTime.Now.AddSeconds(-uptime.NextValue());
                }
            }
            catch
            {
                return DateTime.Now.AddHours(-1); // Fallback
            }
        }

        #endregion
    }
}





