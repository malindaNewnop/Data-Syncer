using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace syncer.core
{
    /// <summary>
    /// Multi-threaded job runner that can execute multiple jobs concurrently
    /// with proper queue management and dependency handling (.NET 3.5 compatible)
    /// </summary>
    public class MultiJobRunner : IMultiJobRunner
    {
        private readonly IJobRepository _jobRepository;
        private readonly IJobQueueService _jobQueueService;
        private readonly ILogService _logService;
        private readonly ITransferClientFactory _transferClientFactory;
        private readonly IFileEnumerator _fileEnumerator;
        
        private readonly object _lockObject = new object();
        private readonly Dictionary<string, Thread> _runningJobs = new Dictionary<string, Thread>();
        private readonly Dictionary<string, bool> _cancellationTokens = new Dictionary<string, bool>();
        private readonly Dictionary<string, JobExecutionContext> _executionContexts = new Dictionary<string, JobExecutionContext>();
        
        private int _maxConcurrentJobs = 5;
        private bool _isProcessing = false;
        private Timer _queueProcessorTimer;
        
        // Events from IJobRunner
        public event EventHandler<JobStatusEventArgs> JobStatusChanged;
        
        // Events from IMultiJobRunner
        public event EventHandler<MultiJobStatusEventArgs> MultiJobStatusChanged;
        
        // This event is currently unused but kept for future use
        #pragma warning disable 0067
        public event EventHandler<JobBatchCompletedEventArgs> JobBatchCompleted;
        #pragma warning restore 0067

        public MultiJobRunner(
            IJobRepository jobRepository,
            IJobQueueService jobQueueService,
            ILogService logService,
            ITransferClientFactory transferClientFactory,
            IFileEnumerator fileEnumerator)
        {
            if (jobRepository == null) throw new ArgumentNullException("jobRepository");
            if (jobQueueService == null) throw new ArgumentNullException("jobQueueService");
            if (logService == null) throw new ArgumentNullException("logService");
            if (transferClientFactory == null) throw new ArgumentNullException("transferClientFactory");
            if (fileEnumerator == null) throw new ArgumentNullException("fileEnumerator");

            _jobRepository = jobRepository;
            _jobQueueService = jobQueueService;
            _logService = logService;
            _transferClientFactory = transferClientFactory;
            _fileEnumerator = fileEnumerator;

            // Start queue processor timer (check every 5 seconds)
            _queueProcessorTimer = new Timer(ProcessQueues, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            
            _logService.LogInfo("MultiJobRunner initialized with queue processing", "MultiJobRunner");
        }

        #region IJobRunner Implementation

        public bool IsRunning(string jobId)
        {
            lock (_lockObject)
            {
                return _runningJobs.ContainsKey(jobId) && 
                       _runningJobs[jobId].IsAlive;
            }
        }

        public List<string> GetRunningJobIds()
        {
            lock (_lockObject)
            {
                var runningIds = new List<string>();
                foreach (var kvp in _runningJobs)
                {
                    if (kvp.Value.IsAlive)
                        runningIds.Add(kvp.Key);
                }
                return runningIds;
            }
        }

        public bool StartJob(string jobId)
        {
            if (string.IsNullOrEmpty(jobId)) return false;

            lock (_lockObject)
            {
                // Check if job is already running
                if (_runningJobs.ContainsKey(jobId) && _runningJobs[jobId].IsAlive)
                {
                    _logService.LogWarning("Job " + jobId + " is already running", "MultiJobRunner");
                    return false;
                }

                // Check if we've reached the concurrent job limit
                var runningCount = 0;
                foreach (var kvp in _runningJobs)
                {
                    if (kvp.Value.IsAlive) runningCount++;
                }

                if (runningCount >= _maxConcurrentJobs)
                {
                    _logService.LogInfo(string.Format("Cannot start job {0} - concurrent limit reached ({1}/{2})", 
                        jobId, runningCount, _maxConcurrentJobs), "MultiJobRunner");
                    
                    // Queue the job for later execution
                    _jobQueueService.QueueJob(jobId);
                    return false;
                }

                // Get the job from repository
                var job = _jobRepository.GetById(jobId);
                if (job == null)
                {
                    _logService.LogError("Job " + jobId + " not found in repository", "MultiJobRunner");
                    return false;
                }

                if (!job.IsEnabled)
                {
                    _logService.LogWarning("Job " + jobId + " is disabled", "MultiJobRunner");
                    return false;
                }

                // Check dependencies
                if (!IsJobEligibleToRun(jobId))
                {
                    _logService.LogInfo("Job " + jobId + " has unmet dependencies", "MultiJobRunner");
                    return false;
                }

                return StartJobInternal(jobId, job);
            }
        }

        public bool CancelJob(string jobId)
        {
            lock (_lockObject)
            {
                if (!_runningJobs.ContainsKey(jobId) || !_runningJobs[jobId].IsAlive)
                {
                    _logService.LogWarning("Job " + jobId + " is not running", "MultiJobRunner");
                    return false;
                }

                _cancellationTokens[jobId] = true;
                _logService.LogInfo("Cancellation requested for job " + jobId, "MultiJobRunner");
                return true;
            }
        }

        public bool WaitForJob(string jobId, int timeoutMilliseconds = -1)
        {
            Thread jobThread;
            lock (_lockObject)
            {
                if (!_runningJobs.ContainsKey(jobId) || !_runningJobs[jobId].IsAlive)
                    return true; // Job is not running
                
                jobThread = _runningJobs[jobId];
            }

            try
            {
                return jobThread.Join(timeoutMilliseconds);
            }
            catch (Exception ex)
            {
                _logService.LogError("Error waiting for job " + jobId + ": " + ex.Message, "MultiJobRunner");
                return false;
            }
        }

        #endregion

        #region IMultiJobRunner Implementation

        public bool StartMultipleJobs(List<string> jobIds)
        {
            if (jobIds == null || jobIds.Count == 0) return false;

            var startedJobs = new List<string>();
            var failedJobs = new List<string>();

            foreach (var jobId in jobIds)
            {
                if (StartJob(jobId))
                {
                    startedJobs.Add(jobId);
                }
                else
                {
                    failedJobs.Add(jobId);
                }
            }

            _logService.LogInfo(string.Format("Started {0} jobs, failed to start {1} jobs", 
                startedJobs.Count, failedJobs.Count), "MultiJobRunner");
            
            if (failedJobs.Count > 0)
            {
                _logService.LogWarning("Failed to start jobs: " + string.Join(", ", failedJobs.ToArray()), "MultiJobRunner");
            }

            return startedJobs.Count > 0;
        }

        public bool StartJobsInQueue(string queueId)
        {
            var pendingJobs = _jobQueueService.GetPendingJobs(queueId);
            if (pendingJobs.Count == 0)
            {
                _logService.LogInfo("No pending jobs in queue " + queueId, "MultiJobRunner");
                return false;
            }

            // Sort by priority (higher priority first) then by queued time
            var sortedJobs = new List<QueuedJob>();
            foreach (var job in pendingJobs)
            {
                sortedJobs.Add(job);
            }
            sortedJobs.Sort((a, b) => 
            {
                var priorityCompare = b.Priority.CompareTo(a.Priority);
                if (priorityCompare != 0) return priorityCompare;
                return a.QueuedTime.CompareTo(b.QueuedTime);
            });

            var jobIds = new List<string>();
            foreach (var job in sortedJobs)
            {
                jobIds.Add(job.JobId);
            }

            return StartMultipleJobs(jobIds);
        }

        public Dictionary<string, string> GetAllJobStatuses()
        {
            lock (_lockObject)
            {
                var statuses = new Dictionary<string, string>();
                
                foreach (var kvp in _runningJobs)
                {
                    var status = GetJobStatusString(kvp.Value);
                    statuses[kvp.Key] = status;
                }

                return statuses;
            }
        }

        public bool ProcessJobQueue(string queueId)
        {
            var queue = _jobQueueService.GetQueueById(queueId);
            if (queue == null || !queue.IsActive)
            {
                return false;
            }

            lock (_lockObject)
            {
                // Count currently running jobs in this queue
                var runningJobsInQueue = _jobQueueService.GetRunningJobs(queueId).Count;
                var availableSlots = queue.MaxConcurrentJobs - runningJobsInQueue;

                if (availableSlots <= 0)
                {
                    return false; // Queue is at capacity
                }

                // Get eligible jobs
                var eligibleJobs = GetEligibleJobsFromQueue(queueId, availableSlots);
                
                foreach (var queuedJob in eligibleJobs)
                {
                    if (StartJob(queuedJob.JobId))
                    {
                        // Update queued job status
                        queuedJob.Status = QueuedJobStatus.Running;
                    }
                }

                return eligibleJobs.Count > 0;
            }
        }

        public void SetMaxConcurrentJobs(int maxJobs)
        {
            if (maxJobs < 1) maxJobs = 1;
            
            lock (_lockObject)
            {
                _maxConcurrentJobs = maxJobs;
                _logService.LogInfo("Set max concurrent jobs to " + maxJobs, "MultiJobRunner");
            }
        }

        public int GetMaxConcurrentJobs()
        {
            lock (_lockObject)
            {
                return _maxConcurrentJobs;
            }
        }

        public bool StartJobWithDependencies(string jobId, List<string> dependencyJobIds)
        {
            if (string.IsNullOrEmpty(jobId) || dependencyJobIds == null)
                return false;

            // Queue the job with dependencies
            return _jobQueueService.QueueJobWithDependencies(jobId, dependencyJobIds);
        }

        public List<string> GetJobDependencies(string jobId)
        {
            var queuedJob = _jobQueueService.GetQueuedJob(jobId);
            return queuedJob != null ? queuedJob.Dependencies : new List<string>();
        }

        public bool IsJobEligibleToRun(string jobId)
        {
            var queuedJob = _jobQueueService.GetQueuedJob(jobId);
            if (queuedJob == null) return true; // Not queued, can run directly

            // Check if scheduled time has passed
            if (queuedJob.ScheduledTime.HasValue && queuedJob.ScheduledTime.Value > DateTime.Now)
            {
                return false;
            }

            // Check dependencies
            if (queuedJob.Dependencies != null && queuedJob.Dependencies.Count > 0)
            {
                foreach (var dependencyId in queuedJob.Dependencies)
                {
                    var dependencyJob = _jobQueueService.GetQueuedJob(dependencyId);
                    if (dependencyJob == null || dependencyJob.Status != QueuedJobStatus.Completed)
                    {
                        return false; // Dependency not completed
                    }
                }
            }

            return true;
        }

        #endregion

        #region Private Methods

        private bool StartJobInternal(string jobId, SyncJob job)
        {
            try
            {
                var executionContext = new JobExecutionContext
                {
                    JobId = jobId,
                    ThreadId = Thread.CurrentThread.ManagedThreadId,
                    StartTime = DateTime.Now
                };

                _cancellationTokens[jobId] = false;
                _executionContexts[jobId] = executionContext;

                var thread = new Thread(ExecuteJobThread)
                {
                    IsBackground = true,
                    Name = "Job-" + job.Name + "-" + jobId
                };

                _runningJobs[jobId] = thread;
                thread.Start(new JobThreadParameters { JobId = jobId, Job = job });

                _logService.LogInfo(string.Format("Started job '{0}' (ID: {1})", job.Name, jobId), "MultiJobRunner");
                
                if (JobStatusChanged != null)
                    JobStatusChanged(this, new JobStatusEventArgs { JobId = jobId, Status = "Running" });

                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError("Failed to start job " + jobId + ": " + ex.Message, "MultiJobRunner");
                return false;
            }
        }

        private void ExecuteJobThread(object parameter)
        {
            var jobParams = (JobThreadParameters)parameter;
            var jobId = jobParams.JobId;
            var job = jobParams.Job;

            try
            {
                // Update job status
                job.IsRunning = true;
                job.LastRun = DateTime.Now;
                job.LastStatus = "Running";
                _jobRepository.Save(job);

                _logService.LogJobStart(job);

                // Create a job runner to execute the job
                var jobRunner = new JobRunner(_transferClientFactory, _logService, _fileEnumerator);
                
                // Monitor for cancellation during execution
                var executionThread = new Thread(() =>
                {
                    try
                    {
                        jobRunner.RunJob(job);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                })
                {
                    IsBackground = true
                };

                executionThread.Start();

                // Wait for completion or cancellation
                while (executionThread.IsAlive)
                {
                    bool shouldCancel = false;
                    lock (_lockObject)
                    {
                        if (_cancellationTokens.ContainsKey(jobId))
                            shouldCancel = _cancellationTokens[jobId];
                    }

                    if (shouldCancel)
                    {
                        executionThread.Abort();
                        throw new OperationCanceledException("Job was cancelled by user");
                    }

                    Thread.Sleep(100); // Check every 100ms
                }

                // Update success status
                job.IsRunning = false;
                job.LastStatus = "Completed";
                _jobRepository.Save(job);

                _logService.LogJobSuccess(job, "Job completed successfully");
                
                // Update queued job status if it exists
                var queuedJob = _jobQueueService.GetQueuedJob(jobId);
                if (queuedJob != null)
                {
                    queuedJob.Status = QueuedJobStatus.Completed;
                }
            }
            catch (ThreadAbortException)
            {
                // Job was cancelled
                job.IsRunning = false;
                job.LastStatus = "Cancelled";
                job.LastError = "Job was cancelled by user";
                _jobRepository.Save(job);

                _logService.LogInfo("Job " + jobId + " was cancelled", "MultiJobRunner");
                
                // Update queued job status
                var queuedJob = _jobQueueService.GetQueuedJob(jobId);
                if (queuedJob != null)
                {
                    queuedJob.Status = QueuedJobStatus.Cancelled;
                }
            }
            catch (Exception ex)
            {
                // Job failed
                job.IsRunning = false;
                job.LastStatus = "Failed";
                job.LastError = ex.Message;
                _jobRepository.Save(job);

                _logService.LogJobError(job, "Job failed: " + ex.Message, ex);
                
                // Update queued job status and consider retry
                var queuedJob = _jobQueueService.GetQueuedJob(jobId);
                if (queuedJob != null)
                {
                    if (queuedJob.RetryCount < queuedJob.MaxRetries)
                    {
                        // Requeue for retry
                        _jobQueueService.RequeueJob(jobId);
                        queuedJob.Status = QueuedJobStatus.Retrying;
                    }
                    else
                    {
                        queuedJob.Status = QueuedJobStatus.Failed;
                    }
                }
            }
            finally
            {
                // Clean up
                CleanupJobExecution(jobId);
            }
        }

        private void CleanupJobExecution(string jobId)
        {
            lock (_lockObject)
            {
                _runningJobs.Remove(jobId);
                _cancellationTokens.Remove(jobId);
                _executionContexts.Remove(jobId);
            }

            if (JobStatusChanged != null)
                JobStatusChanged(this, new JobStatusEventArgs { JobId = jobId, Status = "Completed" });
            
            // Notify multi-job status change
            var allStatuses = GetAllJobStatuses();
            var runningCount = 0;
            var completedCount = 0;
            foreach (var status in allStatuses.Values)
            {
                if (status == "Running") runningCount++;
                if (status == "Completed") completedCount++;
            }

            if (MultiJobStatusChanged != null)
            {
                MultiJobStatusChanged(this, new MultiJobStatusEventArgs
                {
                    JobStatuses = allStatuses,
                    RunningJobs = runningCount,
                    PendingJobs = _jobQueueService.GetPendingJobs().Count,
                    CompletedJobs = completedCount
                });
            }

            _logService.LogInfo("Cleaned up job execution for " + jobId, "MultiJobRunner");
        }

        private void ProcessQueues(object state)
        {
            if (_isProcessing) return;

            _isProcessing = true;
            try
            {
                var allQueues = _jobQueueService.GetAllQueues();
                var activeQueues = new List<JobQueue>();
                
                foreach (var queue in allQueues)
                {
                    if (queue.IsActive)
                        activeQueues.Add(queue);
                }

                // Sort by priority
                activeQueues.Sort((a, b) => b.Priority.CompareTo(a.Priority));

                foreach (var queue in activeQueues)
                {
                    ProcessJobQueue(queue.Id);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError("Error processing job queues: " + ex.Message, "MultiJobRunner");
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private List<QueuedJob> GetEligibleJobsFromQueue(string queueId, int maxJobs)
        {
            var pendingJobs = _jobQueueService.GetPendingJobs(queueId);
            var eligibleJobs = new List<QueuedJob>();

            // Sort by priority and queued time
            var sortedJobs = new List<QueuedJob>();
            foreach (var job in pendingJobs)
            {
                sortedJobs.Add(job);
            }
            sortedJobs.Sort((a, b) => 
            {
                var priorityCompare = b.Priority.CompareTo(a.Priority);
                if (priorityCompare != 0) return priorityCompare;
                return a.QueuedTime.CompareTo(b.QueuedTime);
            });

            foreach (var queuedJob in sortedJobs)
            {
                if (eligibleJobs.Count >= maxJobs) break;

                if (IsJobEligibleToRun(queuedJob.JobId))
                {
                    eligibleJobs.Add(queuedJob);
                }
            }

            return eligibleJobs;
        }

        private string GetJobStatusString(Thread thread)
        {
            if (thread.IsAlive)
                return "Running";
            else
                return "Completed";
        }

        #endregion

        #region Helper Classes

        private class JobThreadParameters
        {
            public string JobId { get; set; }
            public SyncJob Job { get; set; }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_queueProcessorTimer != null)
            {
                _queueProcessorTimer.Dispose();
            }
            
            lock (_lockObject)
            {
                // Cancel all running jobs
                foreach (var kvp in _cancellationTokens)
                {
                    _cancellationTokens[kvp.Key] = true;
                }

                // Wait a bit for jobs to finish gracefully
                Thread.Sleep(1000);

                // Abort any remaining threads
                foreach (var kvp in _runningJobs)
                {
                    try
                    {
                        if (kvp.Value.IsAlive)
                            kvp.Value.Abort();
                    }
                    catch { }
                }
                
                _cancellationTokens.Clear();
                _runningJobs.Clear();
                _executionContexts.Clear();
            }

            if (_logService != null)
                _logService.LogInfo("MultiJobRunner disposed", "MultiJobRunner");
        }

        #endregion
    }
}
