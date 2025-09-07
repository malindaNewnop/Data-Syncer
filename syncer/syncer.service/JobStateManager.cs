using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace syncer.service
{
    /// <summary>
    /// Manages persistence of running job state to allow recovery after system restart
    /// </summary>
    public class JobStateManager
    {
        private const string REGISTRY_KEY = @"SOFTWARE\FTPSyncer\Service";
        private const string RUNNING_JOBS_VALUE = "RunningJobs";
        private const string SERVICE_STATE_FILE = "ServiceState.xml";

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
        }

        /// <summary>
        /// Container for all service state information
        /// </summary>
        [Serializable]
        [XmlRoot("ServiceState")]
        public class ServiceState
        {
            public List<RunningJobState> RunningJobs { get; set; }
            public DateTime LastServiceStart { get; set; }
            public DateTime LastStateUpdate { get; set; }
            public bool ServiceWasRunning { get; set; }

            public ServiceState()
            {
                RunningJobs = new List<RunningJobState>();
                LastServiceStart = DateTime.Now;
                LastStateUpdate = DateTime.Now;
                ServiceWasRunning = true;
            }
        }

        private static readonly string StateFilePath = Path.Combine(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "FTPSyncer"), 
            SERVICE_STATE_FILE);

        /// <summary>
        /// Save current running job state to persistent storage
        /// </summary>
        public static void SaveRunningJobState(List<string> runningJobIds, syncer.core.IJobRepository jobRepository)
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
                            ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id
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
                System.Diagnostics.EventLog.WriteEntry("FTPSyncer Service", 
                    "Failed to save job state: " + ex.Message, 
                    System.Diagnostics.EventLogEntryType.Warning);
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
    }
}
