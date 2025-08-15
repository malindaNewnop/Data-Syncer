using System;
using System.Collections.Generic;
using System.Data;

namespace syncer.core
{
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

    // Enhanced Logging Interface
    public interface ILogService
    {
        void LogInfo(string message, string source = null);
        void LogWarning(string message, string source = null);
        void LogError(string message, string source = null);
        void LogJobStart(SyncJob job);
        void LogJobProgress(SyncJob job, string message);
        void LogJobSuccess(SyncJob job, string message);
        void LogJobError(SyncJob job, string message, Exception ex = null);
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
        List<string> EnumerateFiles(string rootPath, FilterSettings filters, bool includeSubfolders);
        List<string> EnumerateDirectories(string rootPath, FilterSettings filters, bool includeSubfolders);
    }

    // Preview Service Interface
    public interface IPreviewService
    {
        PreviewResult GeneratePreview(SyncJob job);
        bool ValidateJob(SyncJob job, out List<string> validationErrors);
    }

    // Job Runner Interface
    public interface IJobRunner
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
}
