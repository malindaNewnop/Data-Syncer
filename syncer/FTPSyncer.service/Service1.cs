using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Timers;
using System.Diagnostics;
using System.IO;
using Core = FTPSyncer.core;

namespace FTPSyncer.service
{
    public partial class Service1 : ServiceBase
    {
        private Timer _timer;
        private Timer _stateUpdateTimer;
        private bool _running;
        private bool _serviceInitialized;
        private List<string> _currentlyRunningJobs;
        private ServiceCommunicator _communicator;

        private Core.IJobRepository _repo;
        private Core.ILogService _log;
        private Core.ITransferClientFactory _factory;
        private Core.IFileEnumerator _fileEnumerator;
        private Core.IJobRunner _runner;

        public Service1()
        {
            InitializeComponent();
            this.ServiceName = "FTPSyncerService"; // Set the service name
            _currentlyRunningJobs = new List<string>();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _log = Core.ServiceFactory.CreateLogService();
                _log.LogInfo(null, "FTPSyncer Service starting with job recovery support...");

                // Initialize core services
                _repo = Core.ServiceFactory.CreateJobRepository();
                _factory = Core.ServiceFactory.CreateTransferClientFactory();
                _fileEnumerator = Core.ServiceFactory.CreateFileEnumerator();
                
                // Use the new multi-job runner from configuration
                _runner = Core.ServiceFactory.CreateJobRunnerFromConfiguration();
                
                // Subscribe to job status events to track running jobs
                if (_runner != null)
                {
                    _runner.JobStatusChanged += OnJobStatusChanged;
                }

                // Load and resume any previously running jobs
                ResumeInterruptedJobs();

                // Set up main timer for scheduled job execution
                _timer = new Timer();
                _timer.Interval = 60 * 1000; // Check every minute
                _timer.AutoReset = true;
                _timer.Elapsed += OnTick;
                _timer.Start();

                // Set up state update timer to persist running job state
                _stateUpdateTimer = new Timer();
                _stateUpdateTimer.Interval = 30 * 1000; // Update state every 30 seconds
                _stateUpdateTimer.AutoReset = true;
                _stateUpdateTimer.Elapsed += OnStateUpdateTick;
                _stateUpdateTimer.Start();

                // Start service communicator for UI integration
                _communicator = new ServiceCommunicator(_runner, _log);
                _communicator.JobStartRequested += OnJobStartRequestedFromUI;
                _communicator.JobStopRequested += OnJobStopRequestedFromUI;
                _communicator.StartListening();

                _serviceInitialized = true;
                _log.LogInfo(null, "FTPSyncer Service started successfully with multi-job support and recovery");
                
                // Initial tick to check for immediate jobs
                OnTick(this, null);
            }
            catch (Exception ex)
            {
                if (_log != null)
                    _log.LogError(null, "Service start failed: " + ex.Message);
                
                // Write to Windows Event Log as backup
                System.Diagnostics.EventLog.WriteEntry("FTPSyncer Service", 
                    "Service startup error: " + ex.Message, 
                    System.Diagnostics.EventLogEntryType.Error);
                throw; // Re-throw to indicate service start failure
            }
        }

        protected override void OnStop()
        {
            try
            {
                _log?.LogInfo(null, "FTPSyncer Service stopping...");
                
                // Mark service as cleanly stopped
                JobStateManager.MarkServiceStopped();

                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Elapsed -= OnTick;
                    _timer.Dispose();
                    _timer = null;
                }

                if (_stateUpdateTimer != null)
                {
                    _stateUpdateTimer.Stop();
                    _stateUpdateTimer.Elapsed -= OnStateUpdateTick;
                    _stateUpdateTimer.Dispose();
                    _stateUpdateTimer = null;
                }

                // Unsubscribe from job events
                if (_runner != null)
                {
                    _runner.JobStatusChanged -= OnJobStatusChanged;
                }

                // Stop service communicator
                if (_communicator != null)
                {
                    _communicator.JobStartRequested -= OnJobStartRequestedFromUI;
                    _communicator.JobStopRequested -= OnJobStopRequestedFromUI;
                    _communicator.StopListening();
                    _communicator.Dispose();
                    _communicator = null;
                }

                if (_log != null) 
                    _log.LogInfo(null, "FTPSyncer Service stopped cleanly");
            }
            catch (Exception ex)
            {
                if (_log != null)
                    _log.LogError(null, "Service stop error: " + ex.Message);
                
                System.Diagnostics.EventLog.WriteEntry("FTPSyncer Service", 
                    "Service stop error: " + ex.Message, 
                    System.Diagnostics.EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Resume any jobs that were running before service restart
        /// </summary>
        private void ResumeInterruptedJobs()
        {
            try
            {
                List<JobStateManager.RunningJobState> jobsToResume = JobStateManager.LoadJobsForRecovery();
                
                if (jobsToResume.Count > 0)
                {
                    _log.LogInfo(null, string.Format("Found {0} jobs to resume after restart", jobsToResume.Count));
                    
                    int resumedCount = 0;
                    int failedCount = 0;
                    
                    foreach (var jobState in jobsToResume)
                    {
                        try
                        {
                            bool resumed = false;
                            
                            // Handle different types of jobs
                            if (jobState.IsTimerJob)
                            {
                                // Timer jobs are handled by the UI TimerJobManager
                                // Just log that they'll be handled elsewhere
                                _log.LogInfo(jobState.JobId, "Timer job recovery delegated to UI TimerJobManager");
                                resumed = true; // Assume success for timer jobs
                            }
                            else
                            {
                                // Try to resume regular service job
                                resumed = _runner.StartJob(jobState.JobId);
                            }
                            
                            if (resumed)
                            {
                                _currentlyRunningJobs.Add(jobState.JobId);
                                resumedCount++;
                                
                                // Mark recovery attempt as successful
                                JobStateManager.MarkJobRecoveryAttempt(jobState.JobId, true);
                                
                                _log.LogInfo(jobState.JobId, string.Format("Successfully resumed {0} job: {1}",
                                    jobState.IsTimerJob ? "timer" : "regular", jobState.JobName));
                            }
                            else
                            {
                                failedCount++;
                                
                                // Mark recovery attempt as failed
                                JobStateManager.MarkJobRecoveryAttempt(jobState.JobId, false);
                                
                                _log.LogWarning(jobState.JobId, string.Format("Failed to resume job: {0} - job may not exist or be already running", jobState.JobName));
                            }
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            
                            // Mark recovery attempt as failed
                            JobStateManager.MarkJobRecoveryAttempt(jobState.JobId, false);
                            
                            _log.LogError(jobState.JobId, string.Format("Error resuming job {0}: {1}", jobState.JobName, ex.Message));
                        }
                    }
                    
                    _log.LogInfo(null, string.Format("Job resume completed: {0} successful, {1} failed out of {2} total", 
                        resumedCount, failedCount, jobsToResume.Count));
                }
                else
                {
                    _log.LogInfo(null, "No interrupted jobs found to resume - clean startup");
                }
                
                // Clean up old state after handling recovery
                // Wait a bit to ensure jobs are fully started
                System.Threading.Thread.Sleep(2000);
                JobStateManager.ClearJobState();
            }
            catch (Exception ex)
            {
                _log.LogError(null, "Error during job recovery: " + ex.Message);
            }
        }

        /// <summary>
        /// Handle job status change events to track running jobs
        /// </summary>
        private void OnJobStatusChanged(object sender, Core.JobStatusEventArgs e)
        {
            try
            {
                if (e.Status == "Running")
                {
                    if (!_currentlyRunningJobs.Contains(e.JobId))
                    {
                        _currentlyRunningJobs.Add(e.JobId);
                        _log?.LogInfo(e.JobId, "Job started");
                    }
                }
                else if (e.Status == "Completed" || 
                         e.Status == "CompletedWithErrors" || 
                         e.Status == "Failed" || 
                         e.Status == "Cancelled")
                {
                    _currentlyRunningJobs.Remove(e.JobId);
                    _log?.LogInfo(e.JobId, string.Format("Job completed with status: {0}", e.Status));
                }
            }
            catch (Exception ex)
            {
                _log?.LogError(null, "Error handling job status change: " + ex.Message);
            }
        }

        /// <summary>
        /// Periodically update the persistent job state
        /// </summary>
        private void OnStateUpdateTick(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_serviceInitialized && _repo != null)
                {
                    // Update persistent state with currently running jobs
                    JobStateManager.SaveRunningJobState(_currentlyRunningJobs, _repo);
                    
                    // Also save service health status
                    SaveServiceHealthStatus();
                    
                    // Log periodic state save (but not too frequently in logs)
                    if (DateTime.Now.Minute % 5 == 0) // Every 5 minutes
                    {
                        if (_log != null)
                        {
                            _log.LogInfo(null, string.Format("Service state updated - {0} jobs tracked for recovery", _currentlyRunningJobs.Count));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Don't log every state update error to avoid spam
                EventLog.WriteEntry("FTPSyncer Service", 
                    "State update error: " + ex.Message, 
                    EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Save service health and operational status
        /// </summary>
        private void SaveServiceHealthStatus()
        {
            try
            {
                // Create a simple health status file
                string healthFilePath = Path.Combine(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "FTPSyncer"),
                    "ServiceHealth.txt");

                string healthInfo = string.Format(
                    "LastUpdate={0}\nServiceRunning=true\nRunningJobs={1}\nProcessId={2}\nVersion={3}\n",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    _currentlyRunningJobs.Count,
                    Process.GetCurrentProcess().Id,
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
                );

                // Ensure directory exists
                string directory = Path.GetDirectoryName(healthFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(healthFilePath, healthInfo);
            }
            catch (Exception)
            {
                // Don't log this error too frequently to avoid log spam
                // Only log critical health status errors
            }
        }

        /// <summary>
        /// Handle job start request from UI
        /// </summary>
        private void OnJobStartRequestedFromUI(object sender, JobRequestEventArgs e)
        {
            try
            {
                if (!_currentlyRunningJobs.Contains(e.JobId))
                {
                    _currentlyRunningJobs.Add(e.JobId);
                    _log?.LogInfo(e.JobId, "Job started by UI request");
                }
            }
            catch (Exception ex)
            {
                _log?.LogError(null, "Error handling UI job start request: " + ex.Message);
            }
        }

        /// <summary>
        /// Handle job stop request from UI
        /// </summary>
        private void OnJobStopRequestedFromUI(object sender, JobRequestEventArgs e)
        {
            try
            {
                _currentlyRunningJobs.Remove(e.JobId);
                _log?.LogInfo(e.JobId, "Job stopped by UI request");
            }
            catch (Exception ex)
            {
                _log?.LogError(null, "Error handling UI job stop request: " + ex.Message);
            }
        }

        private void OnTick(object sender, ElapsedEventArgs e)
        {
            if (_running) return;
            _running = true;
            try
            {
                List<Core.SyncJob> jobs = _repo.LoadAll();
                DateTime now = DateTime.Now;
                for (int i = 0; i < jobs.Count; i++)
                {
                    Core.SyncJob job = jobs[i];
                    if (!job.IsEnabled) continue;
                    if (!IsDue(job, now)) continue;

                    try
                    {
                        // Use StartJob method which is available in IJobRunner
                        if (_runner.StartJob(job.Id))
                        {
                            job.LastRun = now;
                            _log.LogInfo(job.Id, $"Started job '{job.Name}'");
                        }
                        else
                        {
                            _log.LogWarning(job.Id, $"Failed to start job '{job.Name}' - may be already running or queued");
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(job.Id, "Job execution failed: " + ex.Message);
                    }
                }
                _repo.SaveAll(jobs);
            }
            catch (Exception ex)
            {
                if (_log != null) _log.LogError(null, "Tick error: " + ex.Message);
            }
            finally
            {
                _running = false;
            }
        }

        private static bool IsDue(Core.SyncJob job, DateTime now)
        {
            if (job.Schedule == null) return true;
            if (job.LastRun == DateTime.MinValue) return now >= job.Schedule.StartTime;
            TimeSpan interval = GetInterval(job);
            return job.LastRun.Add(interval) <= now;
        }

        private static TimeSpan GetInterval(Core.SyncJob job)
        {
            int every = (job.Schedule == null) ? 60 : job.Schedule.RepeatEvery;
            Core.TimeUnit unit = (job.Schedule == null) ? Core.TimeUnit.Minutes : job.Schedule.Unit;
            if (every <= 0) every = 60;
            switch (unit)
            {
                case Core.TimeUnit.Hours: return TimeSpan.FromHours(every);
                default: return TimeSpan.FromMinutes(every);
            }
        }
    }
}




