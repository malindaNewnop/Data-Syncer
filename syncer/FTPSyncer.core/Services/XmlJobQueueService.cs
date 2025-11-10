using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace FTPSyncer.core
{
    /// <summary>
    /// XML-based implementation of job queue service for persistent queue management
    /// </summary>
    public class XmlJobQueueService : IJobQueueService
    {
        private readonly ILogService _logService;
        private readonly object _lockObject = new object();
        private readonly string _queuesFilePath;
        private List<JobQueue> _queues;
        
        public event EventHandler<JobQueuedEventArgs> JobQueued;
        public event EventHandler<JobDequeuedEventArgs> JobDequeued;
        public event EventHandler<QueueStatusChangedEventArgs> QueueStatusChanged;

        public XmlJobQueueService(ILogService logService = null)
        {
            _logService = logService ?? new FileLogService();
            _queuesFilePath = Path.Combine(Paths.AppDataFolder, "JobQueues.xml");
            _queues = LoadQueues();
            
            // Ensure default queue exists
            EnsureDefaultQueue();
        }

        #region Queue Management

        public List<JobQueue> GetAllQueues()
        {
            lock (_lockObject)
            {
                return _queues.ToList(); // Return copy to prevent external modification
            }
        }

        public JobQueue GetQueueById(string queueId)
        {
            lock (_lockObject)
            {
                return _queues.FirstOrDefault(q => q.Id == queueId);
            }
        }

        public JobQueue CreateQueue(string name, int maxConcurrentJobs = 3, int priority = 0)
        {
            lock (_lockObject)
            {
                // Check if queue with same name exists
                if (_queues.Any(q => q.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    _logService?.LogWarning($"Queue with name '{name}' already exists", "XmlJobQueueService");
                    return null;
                }

                var queue = new JobQueue
                {
                    Name = name,
                    MaxConcurrentJobs = Math.Max(1, maxConcurrentJobs),
                    Priority = priority,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };

                _queues.Add(queue);
                SaveQueues();

                _logService?.LogInfo($"Created new job queue '{name}' with ID {queue.Id}", "XmlJobQueueService");
                return queue;
            }
        }

        public bool UpdateQueue(JobQueue queue)
        {
            if (queue == null) return false;

            lock (_lockObject)
            {
                var existingQueue = _queues.FirstOrDefault(q => q.Id == queue.Id);
                if (existingQueue == null)
                {
                    _logService?.LogWarning($"Queue with ID {queue.Id} not found for update", "XmlJobQueueService");
                    return false;
                }

                // Update properties
                existingQueue.Name = queue.Name;
                existingQueue.MaxConcurrentJobs = Math.Max(1, queue.MaxConcurrentJobs);
                existingQueue.Priority = queue.Priority;
                existingQueue.IsActive = queue.IsActive;
                existingQueue.ModifiedDate = DateTime.Now;

                SaveQueues();
                _logService?.LogInfo($"Updated job queue '{existingQueue.Name}' (ID: {existingQueue.Id})", "XmlJobQueueService");
                return true;
            }
        }

        public bool DeleteQueue(string queueId)
        {
            lock (_lockObject)
            {
                var queue = _queues.FirstOrDefault(q => q.Id == queueId);
                if (queue == null)
                {
                    _logService?.LogWarning($"Queue with ID {queueId} not found for deletion", "XmlJobQueueService");
                    return false;
                }

                // Don't delete default queue
                if (queue.Name == "Default")
                {
                    _logService?.LogWarning("Cannot delete the default queue", "XmlJobQueueService");
                    return false;
                }

                // Check if queue has pending or running jobs
                var activeJobs = queue.QueuedJobs.Where(j => j.Status == QueuedJobStatus.Pending || j.Status == QueuedJobStatus.Running).ToList();
                if (activeJobs.Any())
                {
                    _logService?.LogWarning($"Cannot delete queue '{queue.Name}' - it has {activeJobs.Count} active jobs", "XmlJobQueueService");
                    return false;
                }

                _queues.Remove(queue);
                SaveQueues();

                _logService?.LogInfo($"Deleted job queue '{queue.Name}' (ID: {queueId})", "XmlJobQueueService");
                return true;
            }
        }

        #endregion

        #region Job Queueing Operations

        public bool QueueJob(string jobId, string queueId = null, int priority = 0, DateTime? scheduledTime = null)
        {
            if (string.IsNullOrEmpty(jobId)) return false;

            lock (_lockObject)
            {
                // Use default queue if none specified
                if (string.IsNullOrEmpty(queueId))
                {
                    queueId = GetDefaultQueue().Id;
                }

                var queue = _queues.FirstOrDefault(q => q.Id == queueId);
                if (queue == null)
                {
                    _logService?.LogError($"Queue with ID {queueId} not found", "XmlJobQueueService");
                    return false;
                }

                // Check if job is already queued
                if (queue.QueuedJobs.Any(j => j.JobId == jobId && j.Status != QueuedJobStatus.Completed && j.Status != QueuedJobStatus.Failed && j.Status != QueuedJobStatus.Cancelled))
                {
                    _logService?.LogWarning($"Job {jobId} is already queued in queue '{queue.Name}'", "XmlJobQueueService");
                    return false;
                }

                var queuedJob = new QueuedJob(jobId)
                {
                    Priority = priority,
                    ScheduledTime = scheduledTime,
                    Status = scheduledTime.HasValue ? QueuedJobStatus.Scheduled : QueuedJobStatus.Pending
                };

                queue.QueuedJobs.Add(queuedJob);
                queue.ModifiedDate = DateTime.Now;
                SaveQueues();

                _logService?.LogInfo($"Queued job {jobId} in queue '{queue.Name}' with priority {priority}", "XmlJobQueueService");
                
                JobQueued?.Invoke(this, new JobQueuedEventArgs 
                { 
                    JobId = jobId, 
                    QueueId = queueId, 
                    QueuedJob = queuedJob 
                });

                return true;
            }
        }

        public bool QueueJobWithDependencies(string jobId, List<string> dependencyJobIds, string queueId = null, int priority = 0)
        {
            if (string.IsNullOrEmpty(jobId) || dependencyJobIds == null) return false;

            lock (_lockObject)
            {
                // Use default queue if none specified
                if (string.IsNullOrEmpty(queueId))
                {
                    queueId = GetDefaultQueue().Id;
                }

                var queue = _queues.FirstOrDefault(q => q.Id == queueId);
                if (queue == null)
                {
                    _logService?.LogError($"Queue with ID {queueId} not found", "XmlJobQueueService");
                    return false;
                }

                // Check if job is already queued
                if (queue.QueuedJobs.Any(j => j.JobId == jobId && j.Status != QueuedJobStatus.Completed && j.Status != QueuedJobStatus.Failed && j.Status != QueuedJobStatus.Cancelled))
                {
                    _logService?.LogWarning($"Job {jobId} is already queued", "XmlJobQueueService");
                    return false;
                }

                var queuedJob = new QueuedJob(jobId)
                {
                    Priority = priority,
                    Dependencies = dependencyJobIds.ToList(),
                    Status = QueuedJobStatus.WaitingForDependencies
                };

                queue.QueuedJobs.Add(queuedJob);
                queue.ModifiedDate = DateTime.Now;
                SaveQueues();

                _logService?.LogInfo($"Queued job {jobId} with {dependencyJobIds.Count} dependencies", "XmlJobQueueService");
                
                JobQueued?.Invoke(this, new JobQueuedEventArgs 
                { 
                    JobId = jobId, 
                    QueueId = queueId, 
                    QueuedJob = queuedJob 
                });

                return true;
            }
        }

        public bool DequeueJob(string jobId)
        {
            lock (_lockObject)
            {
                foreach (var queue in _queues)
                {
                    var job = queue.QueuedJobs.FirstOrDefault(j => j.JobId == jobId);
                    if (job != null)
                    {
                        queue.QueuedJobs.Remove(job);
                        queue.ModifiedDate = DateTime.Now;
                        SaveQueues();

                        _logService?.LogInfo($"Dequeued job {jobId} from queue '{queue.Name}'", "XmlJobQueueService");
                        
                        JobDequeued?.Invoke(this, new JobDequeuedEventArgs 
                        { 
                            JobId = jobId, 
                            QueueId = queue.Id, 
                            Reason = "Manual dequeue" 
                        });

                        return true;
                    }
                }

                _logService?.LogWarning($"Job {jobId} not found in any queue for dequeue operation", "XmlJobQueueService");
                return false;
            }
        }

        public bool RequeueJob(string jobId, string queueId = null)
        {
            lock (_lockObject)
            {
                // Find and remove the job from any queue
                QueuedJob jobToRequeue = null;
                foreach (var queue in _queues)
                {
                    var job = queue.QueuedJobs.FirstOrDefault(j => j.JobId == jobId);
                    if (job != null)
                    {
                        jobToRequeue = job;
                        queue.QueuedJobs.Remove(job);
                        break;
                    }
                }

                if (jobToRequeue == null)
                {
                    _logService?.LogWarning($"Job {jobId} not found for requeue operation", "XmlJobQueueService");
                    return false;
                }

                // Reset job status and increment retry count
                jobToRequeue.Status = QueuedJobStatus.Pending;
                jobToRequeue.RetryCount++;
                jobToRequeue.QueuedTime = DateTime.Now;

                // Use specified queue or find default
                var targetQueue = string.IsNullOrEmpty(queueId) 
                    ? GetDefaultQueue() 
                    : _queues.FirstOrDefault(q => q.Id == queueId);

                if (targetQueue == null)
                {
                    _logService?.LogError($"Target queue {queueId} not found for requeue", "XmlJobQueueService");
                    return false;
                }

                targetQueue.QueuedJobs.Add(jobToRequeue);
                targetQueue.ModifiedDate = DateTime.Now;
                SaveQueues();

                _logService?.LogInfo($"Requeued job {jobId} to queue '{targetQueue.Name}' (retry #{jobToRequeue.RetryCount})", "XmlJobQueueService");
                return true;
            }
        }

        #endregion

        #region Queue Status and Information

        public List<QueuedJob> GetJobsInQueue(string queueId)
        {
            lock (_lockObject)
            {
                var queue = _queues.FirstOrDefault(q => q.Id == queueId);
                return queue?.QueuedJobs.ToList() ?? new List<QueuedJob>();
            }
        }

        public List<QueuedJob> GetPendingJobs(string queueId = null)
        {
            lock (_lockObject)
            {
                if (!string.IsNullOrEmpty(queueId))
                {
                    var queue = _queues.FirstOrDefault(q => q.Id == queueId);
                    return queue?.QueuedJobs.Where(j => j.Status == QueuedJobStatus.Pending).ToList() ?? new List<QueuedJob>();
                }

                return _queues.SelectMany(q => q.QueuedJobs.Where(j => j.Status == QueuedJobStatus.Pending)).ToList();
            }
        }

        public List<QueuedJob> GetRunningJobs(string queueId = null)
        {
            lock (_lockObject)
            {
                if (!string.IsNullOrEmpty(queueId))
                {
                    var queue = _queues.FirstOrDefault(q => q.Id == queueId);
                    return queue?.QueuedJobs.Where(j => j.Status == QueuedJobStatus.Running).ToList() ?? new List<QueuedJob>();
                }

                return _queues.SelectMany(q => q.QueuedJobs.Where(j => j.Status == QueuedJobStatus.Running)).ToList();
            }
        }

        public QueuedJob GetQueuedJob(string jobId)
        {
            lock (_lockObject)
            {
                foreach (var queue in _queues)
                {
                    var job = queue.QueuedJobs.FirstOrDefault(j => j.JobId == jobId);
                    if (job != null) return job;
                }
                return null;
            }
        }

        #endregion

        #region Queue Operations

        public bool StartQueue(string queueId)
        {
            lock (_lockObject)
            {
                var queue = _queues.FirstOrDefault(q => q.Id == queueId);
                if (queue == null) return false;

                queue.IsActive = true;
                queue.ModifiedDate = DateTime.Now;
                SaveQueues();

                _logService?.LogInfo($"Started queue '{queue.Name}'", "XmlJobQueueService");
                QueueStatusChanged?.Invoke(this, new QueueStatusChangedEventArgs 
                { 
                    QueueId = queueId, 
                    IsActive = true, 
                    StatusMessage = "Queue started" 
                });

                return true;
            }
        }

        public bool StopQueue(string queueId)
        {
            lock (_lockObject)
            {
                var queue = _queues.FirstOrDefault(q => q.Id == queueId);
                if (queue == null) return false;

                queue.IsActive = false;
                queue.ModifiedDate = DateTime.Now;
                SaveQueues();

                _logService?.LogInfo($"Stopped queue '{queue.Name}'", "XmlJobQueueService");
                QueueStatusChanged?.Invoke(this, new QueueStatusChangedEventArgs 
                { 
                    QueueId = queueId, 
                    IsActive = false, 
                    StatusMessage = "Queue stopped" 
                });

                return true;
            }
        }

        public bool PauseQueue(string queueId)
        {
            return StopQueue(queueId);
        }

        public bool ClearQueue(string queueId)
        {
            lock (_lockObject)
            {
                var queue = _queues.FirstOrDefault(q => q.Id == queueId);
                if (queue == null) return false;

                var clearedCount = queue.QueuedJobs.Count;
                queue.QueuedJobs.Clear();
                queue.ModifiedDate = DateTime.Now;
                SaveQueues();

                _logService?.LogInfo($"Cleared {clearedCount} jobs from queue '{queue.Name}'", "XmlJobQueueService");
                return true;
            }
        }

        #endregion

        #region Statistics and Monitoring

        public QueueStatistics GetQueueStatistics(string queueId)
        {
            lock (_lockObject)
            {
                var queue = _queues.FirstOrDefault(q => q.Id == queueId);
                if (queue == null) return null;

                var stats = new QueueStatistics
                {
                    QueueId = queue.Id,
                    QueueName = queue.Name,
                    TotalJobs = queue.QueuedJobs.Count,
                    PendingJobs = queue.QueuedJobs.Count(j => j.Status == QueuedJobStatus.Pending),
                    RunningJobs = queue.QueuedJobs.Count(j => j.Status == QueuedJobStatus.Running),
                    CompletedJobs = queue.QueuedJobs.Count(j => j.Status == QueuedJobStatus.Completed),
                    FailedJobs = queue.QueuedJobs.Count(j => j.Status == QueuedJobStatus.Failed),
                    CancelledJobs = queue.QueuedJobs.Count(j => j.Status == QueuedJobStatus.Cancelled),
                    LastActivity = queue.ModifiedDate,
                    MaxConcurrentJobs = queue.MaxConcurrentJobs,
                    IsActive = queue.IsActive
                };

                if (stats.TotalJobs > 0)
                {
                    stats.SuccessRate = (double)stats.CompletedJobs / (stats.CompletedJobs + stats.FailedJobs + stats.CancelledJobs) * 100.0;
                }

                return stats;
            }
        }

        public List<QueueStatistics> GetAllQueueStatistics()
        {
            lock (_lockObject)
            {
                return _queues.Select(q => GetQueueStatistics(q.Id)).ToList();
            }
        }

        #endregion

        #region Private Methods

        private void EnsureDefaultQueue()
        {
            lock (_lockObject)
            {
                if (!_queues.Any(q => q.Name == "Default"))
                {
                    var defaultQueue = new JobQueue
                    {
                        Name = "Default",
                        MaxConcurrentJobs = 3,
                        Priority = 0,
                        IsActive = true
                    };
                    _queues.Add(defaultQueue);
                    SaveQueues();
                    _logService?.LogInfo("Created default job queue", "XmlJobQueueService");
                }
            }
        }

        private JobQueue GetDefaultQueue()
        {
            return _queues.First(q => q.Name == "Default");
        }

        private List<JobQueue> LoadQueues()
        {
            try
            {
                if (!File.Exists(_queuesFilePath))
                {
                    _logService?.LogInfo("Job queues file not found, creating new queue list", "XmlJobQueueService");
                    return new List<JobQueue>();
                }

                using (var fs = new FileStream(_queuesFilePath, FileMode.Open, FileAccess.Read))
                {
                    var serializer = new XmlSerializer(typeof(List<JobQueue>));
                    var queues = (List<JobQueue>)serializer.Deserialize(fs);
                    _logService?.LogInfo($"Loaded {queues.Count} job queues from XML", "XmlJobQueueService");
                    return queues ?? new List<JobQueue>();
                }
            }
            catch (Exception ex)
            {
                _logService?.LogError($"Failed to load job queues: {ex.Message}", "XmlJobQueueService");
                return new List<JobQueue>();
            }
        }

        private void SaveQueues()
        {
            try
            {
                var directory = Path.GetDirectoryName(_queuesFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var fs = new FileStream(_queuesFilePath, FileMode.Create, FileAccess.Write))
                {
                    var serializer = new XmlSerializer(typeof(List<JobQueue>));
                    serializer.Serialize(fs, _queues);
                }
            }
            catch (Exception ex)
            {
                _logService?.LogError($"Failed to save job queues: {ex.Message}", "XmlJobQueueService");
                throw;
            }
        }

        #endregion
    }
}





