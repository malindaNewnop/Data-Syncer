using System;
using System.Collections.Generic;

namespace syncer.core
{
    public enum ProtocolType { Local = 0, Ftp = 1, Sftp = 2 }
    public enum TimeUnit { Minutes = 0, Hours = 1, Days = 2 }
    public enum TransferMode { Mirror = 0, OverwriteAll = 1, SkipExisting = 2 }
    public enum LogLevel { Info = 0, Warning = 1, Error = 2 }
    public enum TransferDirection { SourceToDestination = 0, DestinationToSource = 1, Bidirectional = 2 }
    public enum TransferStatus { Pending = 0, Running = 1, Completed = 2, CompletedWithErrors = 3, Failed = 4, Cancelled = 5 }
    public enum FileAction { Copy = 0, Overwrite = 1, Skip = 2 }
    public enum PreviewStatus { Pending = 0, Skipped = 1, Error = 2 }

    [Serializable]
    public class ConnectionSettings
    {
        public ProtocolType Protocol = ProtocolType.Local;
        public string Host = "";
        public int Port = 21;
        public string Username = "";
        public string Password = "";
        public bool UsePassiveMode = true;
        public bool UseSftp = false;
        public string SshKeyPath = "";
        public int Timeout = 30;
    }

    [Serializable]
    public class FilterSettings
    {
        public string IncludePattern = "";
        public string ExcludePattern = "";
        public List<string> FileExtensions = new List<string>();
        public long MinSizeBytes = -1;
        public long MaxSizeBytes = -1;
        public bool IncludeHidden = false;
        public bool IncludeSystem = false;
        public bool IncludeReadOnly = true;
        public bool IncludeSubdirectories = false;
        public List<string> ExcludePatterns = new List<string>();
        public DateTime? ModifiedAfter;
        public DateTime? ModifiedBefore;
    }

    [Serializable]
    public class ScheduleSettings
    {
        public DateTime StartTime = DateTime.Now;
        public int RepeatEvery = 60;
        public TimeUnit Unit = TimeUnit.Minutes;
        public bool IsEnabled = true;
        public string CronExpression = "";
        public bool UseCronExpression = false;
        public List<DayOfWeek> DaysOfWeek = new List<DayOfWeek>();
        public bool RunOnWeekdays = false;
        public bool RunOnWeekends = false;
    }

    [Serializable]
    public class PostProcessSettings
    {
        public bool Enabled = false;
        public bool DeleteSourceFiles = false;
        public bool ArchiveFiles = false;
        public bool DeleteSourceAfterTransfer = false;
        public bool MoveSourceAfterTransfer = false;
        public string MoveDestinationPath = "";
        public bool ArchiveTransferredFiles = false;
        public string ArchivePath = "";
        public bool NotifyOnSuccess = false;
        public bool NotifyOnError = true;
        public string NotificationEmail = "";
    }

    [Serializable]
    public class SyncJob
    {
        public string Id = Guid.NewGuid().ToString();
        public string Name = "New Sync Job";
        public string Description = "";
        public bool IsEnabled = true;
        public bool IsScheduled = false;
        public string SourcePath = "";
        public string DestinationPath = "";
        public bool IncludeSubfolders = true;
        public bool OverwriteExisting = true;
        public TransferDirection Direction = TransferDirection.SourceToDestination;
        public TransferMode TransferMode = TransferMode.Mirror;
        public ConnectionSettings Connection = new ConnectionSettings();
        public ConnectionSettings DestinationConnection = new ConnectionSettings();
        public FilterSettings Filters = new FilterSettings();
        public ScheduleSettings Schedule = new ScheduleSettings();
        public PostProcessSettings PostProcess = new PostProcessSettings();
        public DateTime CreatedDate = DateTime.Now;
        public DateTime LastRun = DateTime.MinValue;
        public DateTime? NextRun;
        public string LastStatus = "Never Run";
        public int LastTransferCount = 0;
        public long LastTransferBytes = 0;
        public TimeSpan LastDuration = TimeSpan.Zero;
        public string LastError = "";
        public bool IsRunning = false;
        public int RetryCount = 0;
        public int MaxRetries = 3;
        public int RetryIntervalMinutes = 5;
    }

    public class LogEntry
    {
        public DateTime Timestamp = DateTime.Now;
        public LogLevel Level;
        public string JobName = "";
        public string JobId = "";
        public string Source = "";
        public string Message = "";
        public string Exception = "";
        public string FileName = "";
        public long FileSize = 0;
        public TimeSpan Duration = TimeSpan.Zero;
        public string RemotePath = "";
        public string LocalPath = "";
    }

    public class TransferResult
    {
        public string JobId;
        public string JobName;
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan Duration;
        public TransferStatus Status;
        public int TotalFiles;
        public int ProcessedFiles;
        public int SuccessfulFiles;
        public int FailedFiles;
        public long TotalBytes;
        public long TransferredBytes;
        public List<string> Errors = new List<string>();
        public bool Success;
        public string Error;
        public int FilesTransferred;
        public long BytesTransferred;
        public List<string> TransferredFiles = new List<string>();
        public List<string> FailedFilesList = new List<string>();
        public List<string> SkippedFiles = new List<string>();
    }

    public class JobStatus
    {
        public string JobId;
        public string JobName;
        public bool IsRunning;
        public DateTime? LastRun;
        public DateTime? NextRun;
        public string Status;
        public int Progress; // 0-100
        public string CurrentFile;
        public TransferResult LastResult;
    }

    // Event Args classes for .NET 3.5 compatibility
    public class JobStartedEventArgs : EventArgs
    {
        public SyncJob Job;
        public TransferResult Result;
    }

    public class JobProgressEventArgs : EventArgs
    {
        public SyncJob Job;
        public TransferResult Result;
        public int ProgressPercent;
        public string CurrentFile;
        public int FileProgress;
    }

    public class JobCompletedEventArgs : EventArgs
    {
        public SyncJob Job;
        public TransferResult Result;
    }

    public class FileTransferEventArgs : EventArgs
    {
        public string SourcePath;
        public string DestinationPath;
        public SyncJob Job;
        public bool Success;
        public string Error;
    }

    public class JobScheduledEventArgs : EventArgs
    {
        public SyncJob Job;
    }

    public class ScheduledJobTriggeredEventArgs : EventArgs
    {
        public SyncJob Job;
    }

    public class PipeMessageEventArgs : EventArgs
    {
        public string Message;
        public string ClientId;
    }

    public class PipeClientEventArgs : EventArgs
    {
        public string ClientId;
    }

    // Preview classes
    public class PreviewResult
    {
        public string JobId;
        public string JobName;
        public List<FilePreviewItem> FilesToProcess = new List<FilePreviewItem>();
        public int TotalFiles;
        public long TotalSize;
        public TimeSpan EstimatedDuration;
        public bool HasErrors;
        public List<string> Errors = new List<string>();
    }

    public class FilePreviewItem
    {
        public string SourcePath;
        public string DestinationPath;
        public FileAction Action;
        public long Size;
        public DateTime LastModified;
        public PreviewStatus Status;
        public string Message;
    }
}
