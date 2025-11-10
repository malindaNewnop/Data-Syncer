using System;
using System.Collections.Generic;
using System.Data;

namespace syncer.core
{
    [Flags]
    public enum ValidationOptions
    {
        None = 0,
        Existence = 1,
        FileSize = 2,
        Timestamp = 4,
        PreserveTimestamp = 8,
        All = Existence | FileSize | Timestamp
    }

    public enum RelocationOptions
    {
        None,
        Delete,
        Archive,
        CustomFolder
    }
    // Repository Interface
    public interface IJobRepository
    {
        List<SyncJob> LoadAll();
        List<SyncJob> GetAll();
        void SaveAll(List<SyncJob> jobs);
        SyncJob GetById(string id);
        void Save(SyncJob job);
        void Delete(string id);
    }

    // Scheduler Interface
    public interface ISchedulerService
    {
        void Start();
        void Stop();
        void RefreshScheduledJobs();
        void ScheduleJob(SyncJob job);
        void UnscheduleJob(string jobId);
        
        event EventHandler JobScheduled;
        event EventHandler JobTriggered;
    }

    // Event arguments defined in SchedulerService.cs
    
    // Enhanced Logging Interface
    public interface ILogService
    {
        void LogInfo(string message, string source = null);
        void LogWarning(string message, string source = null);
        void LogError(string message, string source = null);
        
        // Overloads with jobId parameter
        void LogInfo(string message, string source, string jobId);
        void LogWarning(string message, string source, string jobId);
        void LogError(string message, string source, string jobId);
        
        void LogJobStart(SyncJob job);
        void LogJobProgress(SyncJob job, string message);
        void LogJobSuccess(SyncJob job, string message);
        void LogJobError(SyncJob job, string message, Exception ex = null);
        void LogTransfer(string jobName, string fileName, long fileSize, bool success, string error);
        
        // Real-time Custom Directory Logging
        void EnableRealTimeLogging(string customFilePath);
        void DisableRealTimeLogging();
        bool IsRealTimeLoggingEnabled();
        string GetRealTimeLogPath();
        event EventHandler<LogEntryEventArgs> RealTimeLogEntry;
    }
    
    // Event arguments for real-time logging
    public class LogEntryEventArgs : EventArgs
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public string JobName { get; set; }
        public Exception Exception { get; set; }
    }

    // Transfer Client Interface
    public interface ITransferClient
    {
        ProtocolType Protocol { get; }
        bool TestConnection(ConnectionSettings settings, out string error);
        bool EnsureDirectory(ConnectionSettings settings, string remoteDir, out string error);
        bool UploadFile(ConnectionSettings settings, string localPath, string remotePath, bool overwrite, out string error);
        bool DownloadFile(ConnectionSettings settings, string remotePath, string localPath, bool overwrite, out string error);
        bool FileExists(ConnectionSettings settings, string remotePath, out bool exists, out string error);
        bool DeleteFile(ConnectionSettings settings, string remotePath, out string error);
        bool ListFiles(ConnectionSettings settings, string remoteDir, out List<string> files, out string error);
        bool GetFileModifiedTime(ConnectionSettings settings, string remotePath, out DateTime modifiedTime, out string error);
        bool GetFileSize(ConnectionSettings settings, string remotePath, out long fileSize, out string error);
        void SetProgressCallback(Action<int> progressCallback);
    }

    // Transfer Client Factory
    public interface ITransferClientFactory
    {
        ITransferClient Create(ProtocolType protocol);
        ITransferClient CreateSecure(ProtocolType protocol, string keyPath);
    }

    // File Operations Interface
    public interface IFileEnumerator
    {
        List<string> EnumerateFiles(string rootPath, bool includeSubfolders);
        List<string> EnumerateFiles(string rootPath, FilterSettings filters, bool includeSubfolders);
        List<string> EnumerateDirectories(string rootPath, bool includeSubfolders);
    }

    // Preview Service Interface
    public interface IPreviewService
    {
        PreviewResult GeneratePreview(SyncJob job);
        bool ValidateJob(SyncJob job, out List<string> validationErrors);
    }

    // File Filter Service Interface
    public interface IFileFilterService
    {
        List<string> ApplyFilters(List<string> files, FilterSettings filters);
    }

    // Job Runner Interface
    public interface IJobRunner : IDisposable
    {
        bool IsRunning(string jobId);
        List<string> GetRunningJobIds();
        bool StartJob(string jobId);
        bool CancelJob(string jobId);
        bool WaitForJob(string jobId, int timeoutMilliseconds = -1);
        event EventHandler<JobStatusEventArgs> JobStatusChanged;
    }

    // Scheduler Interface
    public interface IJobScheduler
    {
        void Start();
        void Stop();
        bool ScheduleJob(SyncJob job);
        bool UnscheduleJob(string jobId);
        bool IsJobScheduled(string jobId);
        List<string> GetScheduledJobs();
        DateTime? GetNextRunTime(string jobId);
        event EventHandler<JobScheduledEventArgs> JobScheduled;
        event EventHandler<JobScheduledEventArgs> JobUnscheduled;
        event EventHandler<ScheduledJobTriggeredEventArgs> ScheduledJobTriggered;
    }

    // Settings Management Interface
    public interface ISettingsService
    {
        AppSettings GetSettings();
        bool SaveSettings(AppSettings settings);
        bool ResetToDefaults();
        T GetSetting<T>(string key, T defaultValue);
        bool SetSetting<T>(string key, T value);
    }

    // Named Pipe Communication Interface
    public interface IPipeServer
    {
        void Start();
        void Stop();
        bool IsRunning { get; }
        bool SendMessage(string message);
        event EventHandler<PipeMessageEventArgs> MessageReceived;
        event EventHandler<PipeClientEventArgs> ClientConnected;
        event EventHandler<PipeClientEventArgs> ClientDisconnected;
    }

    public interface IPipeClient
    {
        bool Connect(int timeoutMs = 5000);
        void Disconnect();
        bool IsConnected { get; }
        bool SendMessage(string message);
        event EventHandler<PipeMessageEventArgs> MessageReceived;
        event EventHandler Connected;
        event EventHandler Disconnected;
    }

    // Event Args
    public class JobStatusEventArgs : EventArgs
    {
        public string JobId { get; set; }
        public string Status { get; set; }
    }

    public class JobExecutionEventArgs : EventArgs
    {
        public SyncJob Job { get; set; }
        public TransferResult Result { get; set; }
    }

    /// <summary>
    /// Interface for job queue management operations
    /// </summary>
    public interface IJobQueueService
    {
        // Queue Management
        List<JobQueue> GetAllQueues();
        JobQueue GetQueueById(string queueId);
        JobQueue CreateQueue(string name, int maxConcurrentJobs = 3, int priority = 0);
        bool UpdateQueue(JobQueue queue);
        bool DeleteQueue(string queueId);
        
        // Job Queueing Operations
        bool QueueJob(string jobId, string queueId = null, int priority = 0, DateTime? scheduledTime = null);
        bool QueueJobWithDependencies(string jobId, List<string> dependencyJobIds, string queueId = null, int priority = 0);
        bool DequeueJob(string jobId);
        bool RequeueJob(string jobId, string queueId = null);
        
        // Queue Status and Information
        List<QueuedJob> GetJobsInQueue(string queueId);
        List<QueuedJob> GetPendingJobs(string queueId = null);
        List<QueuedJob> GetRunningJobs(string queueId = null);
        QueuedJob GetQueuedJob(string jobId);
        
        // Queue Operations
        bool StartQueue(string queueId);
        bool StopQueue(string queueId);
        bool PauseQueue(string queueId);
        bool ClearQueue(string queueId);
        
        // Statistics and Monitoring
        QueueStatistics GetQueueStatistics(string queueId);
        List<QueueStatistics> GetAllQueueStatistics();
        
        // Events
        event EventHandler<JobQueuedEventArgs> JobQueued;
        event EventHandler<JobDequeuedEventArgs> JobDequeued;
        event EventHandler<QueueStatusChangedEventArgs> QueueStatusChanged;
    }

    /// <summary>
    /// Enhanced job runner interface for multiple job execution
    /// </summary>
    public interface IMultiJobRunner : IJobRunner
    {
        // Multi-job capabilities
        bool StartMultipleJobs(List<string> jobIds);
        bool StartJobsInQueue(string queueId);
        Dictionary<string, string> GetAllJobStatuses();
        
        // Queue integration
        bool ProcessJobQueue(string queueId);
        void SetMaxConcurrentJobs(int maxJobs);
        int GetMaxConcurrentJobs();
        
        // Advanced operations
        bool StartJobWithDependencies(string jobId, List<string> dependencyJobIds);
        List<string> GetJobDependencies(string jobId);
        bool IsJobEligibleToRun(string jobId);
        
        // Events for multi-job operations
        event EventHandler<MultiJobStatusEventArgs> MultiJobStatusChanged;
        event EventHandler<JobBatchCompletedEventArgs> JobBatchCompleted;
    }
}
