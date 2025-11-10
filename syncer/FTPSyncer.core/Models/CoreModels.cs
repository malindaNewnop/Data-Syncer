using System;
using System.Collections.Generic;

namespace FTPSyncer.core
{
    /// <summary>
    /// Enhanced Settings model for JSON serialization with .NET 3.5 compatibility
    /// </summary>
    [Serializable]
    public class AppSettingsEnhanced
    {
        // Application Settings
        public string LogLevel { get; set; }
        public long MaxLogFileSize { get; set; }
        public int KeepLogDays { get; set; }
        public bool AutoStartService { get; set; }
        public bool EnableLogging { get; set; }
        public string PipeName { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDisplayName { get; set; }
        public string TempFolder { get; set; }
        public string LogFolder { get; set; }

        // Transfer Settings
        public int DefaultRetryCount { get; set; }
        public int RetryDelaySeconds { get; set; }
        public int ConnectionTimeoutSeconds { get; set; }
        public int OperationTimeoutSeconds { get; set; }
        public long MaxFileSize { get; set; }
        public bool EnableTransferResume { get; set; }
        public bool ValidateChecksums { get; set; }

        // FTP Settings
        public bool DefaultFtpPassiveMode { get; set; }
        public int DefaultFtpPort { get; set; }
        public bool EnableFtpSsl { get; set; }

        // SFTP Settings  
        public int DefaultSftpPort { get; set; }
        public bool EnableSftpCompression { get; set; }
        public string DefaultSftpKeyPath { get; set; }
        public int SftpKeepaliveInterval { get; set; }

        // UI Settings
        public bool MinimizeToTray { get; set; }
        public bool StartMinimized { get; set; }
        public bool ShowTransferProgress { get; set; }
        public bool ConfirmJobDeletion { get; set; }
        public string DefaultTheme { get; set; }

        // Advanced Settings
        public int MaxConcurrentJobs { get; set; }
        public bool EnableJobQueue { get; set; }
        public bool EnableScheduledJobs { get; set; }
        public bool EnableJobHistory { get; set; }
        public int JobHistoryRetentionDays { get; set; }

        public AppSettingsEnhanced()
        {
            // Set comprehensive defaults
            LogLevel = "Info";
            MaxLogFileSize = 10 * 1024 * 1024; // 10 MB
            KeepLogDays = 30;
            AutoStartService = true;
            EnableLogging = true;
            PipeName = "DataSyncerPipe";
            ServiceName = "DataSyncerService";
            ServiceDisplayName = "FTPSyncer Service";
            TempFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\DataSyncer\\Temp";
            LogFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\DataSyncer\\Logs";

            // Transfer defaults
            DefaultRetryCount = 3;
            RetryDelaySeconds = 5;
            ConnectionTimeoutSeconds = 30;
            OperationTimeoutSeconds = 300;
            MaxFileSize = 2L * 1024 * 1024 * 1024; // 2GB
            EnableTransferResume = true;
            ValidateChecksums = false;

            // Protocol defaults
            DefaultFtpPassiveMode = true;
            DefaultFtpPort = 21;
            EnableFtpSsl = false;
            DefaultSftpPort = 22;
            EnableSftpCompression = true;
            DefaultSftpKeyPath = "";
            SftpKeepaliveInterval = 30;

            // UI defaults
            MinimizeToTray = true;
            StartMinimized = false;
            ShowTransferProgress = true;
            ConfirmJobDeletion = true;
            DefaultTheme = "Default";

            // Advanced defaults
            MaxConcurrentJobs = 3;
            EnableJobQueue = true;
            EnableScheduledJobs = true;
            EnableJobHistory = true;
            JobHistoryRetentionDays = 90;
        }
    }

    /// <summary>
    /// Enhanced Transfer Result with comprehensive logging
    /// </summary>
    [Serializable]
    public class TransferResultEnhanced
    {
        public string JobId { get; set; }
        public string JobName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public TransferStatus Status { get; set; }
        
        // File Statistics
        public int TotalFiles { get; set; }
        public int ProcessedFiles { get; set; }
        public int SuccessfulFiles { get; set; }
        public int FailedFiles { get; set; }
        public int SkippedFiles { get; set; }
        
        // Byte Statistics
        public long TotalBytes { get; set; }
        public long TransferredBytes { get; set; }
        public long SkippedBytes { get; set; }
        
        // Speed Statistics
        public double AverageSpeedBytesPerSecond { get; set; }
        public double PeakSpeedBytesPerSecond { get; set; }
        
        // Error Handling
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
        public string LastError { get; set; }
        public bool Success { get; set; }
        
        // File Lists
        public List<string> TransferredFiles { get; set; }
        public List<string> FailedFilesList { get; set; }
        public List<string> SkippedFilesList { get; set; }
        
        // Additional Info
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public string TransferMode { get; set; }
        public ProtocolType SourceProtocol { get; set; }
        public ProtocolType DestinationProtocol { get; set; }
        
        public TransferResultEnhanced()
        {
            Errors = new List<string>();
            Warnings = new List<string>();
            TransferredFiles = new List<string>();
            FailedFilesList = new List<string>();
            SkippedFilesList = new List<string>();
            Status = TransferStatus.Pending;
            Success = false;
        }

        /// <summary>
        /// Get transfer success rate as percentage
        /// </summary>
        public double GetSuccessRate()
        {
            if (TotalFiles == 0) return 0;
            return (double)SuccessfulFiles / TotalFiles * 100;
        }

        /// <summary>
        /// Get human readable transfer speed
        /// </summary>
        public string GetFormattedSpeed()
        {
            if (AverageSpeedBytesPerSecond < 1024)
                return string.Format("{0:F1} B/s", AverageSpeedBytesPerSecond);
            else if (AverageSpeedBytesPerSecond < 1024 * 1024)
                return string.Format("{0:F1} KB/s", AverageSpeedBytesPerSecond / 1024);
            else if (AverageSpeedBytesPerSecond < 1024 * 1024 * 1024)
                return string.Format("{0:F1} MB/s", AverageSpeedBytesPerSecond / (1024 * 1024));
            else
                return string.Format("{0:F1} GB/s", AverageSpeedBytesPerSecond / (1024 * 1024 * 1024));
        }

        /// <summary>
        /// Get human readable data size
        /// </summary>
        public string GetFormattedDataSize()
        {
            if (TransferredBytes < 1024)
                return string.Format("{0} B", TransferredBytes);
            else if (TransferredBytes < 1024 * 1024)
                return string.Format("{0:F1} KB", TransferredBytes / 1024.0);
            else if (TransferredBytes < 1024 * 1024 * 1024)
                return string.Format("{0:F1} MB", TransferredBytes / (1024.0 * 1024));
            else
                return string.Format("{0:F1} GB", TransferredBytes / (1024.0 * 1024 * 1024));
        }
    }

    /// <summary>
    /// Enhanced connection test result
    /// </summary>
    [Serializable]
    public class ConnectionTestResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public TimeSpan ConnectionTime { get; set; }
        public DateTime TestTime { get; set; }
        public string ServerInfo { get; set; }
        public ProtocolType Protocol { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        
        public ConnectionTestResult()
        {
            TestTime = DateTime.Now;
            Success = false;
        }
    }

    /// <summary>
    /// File transfer progress information
    /// </summary>
    [Serializable]
    public class TransferProgress
    {
        public string JobId { get; set; }
        public string CurrentFile { get; set; }
        public long CurrentFileSize { get; set; }
        public long CurrentFileBytesTransferred { get; set; }
        public int CurrentFileProgress { get; set; }
        public int OverallProgress { get; set; }
        public int FilesCompleted { get; set; }
        public int TotalFiles { get; set; }
        public long TotalBytesTransferred { get; set; }
        public long TotalBytes { get; set; }
        public DateTime StartTime { get; set; }
        public double SpeedBytesPerSecond { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan EstimatedTimeRemaining { get; set; }
        public TransferStatus Status { get; set; }
        
        public TransferProgress()
        {
            StartTime = DateTime.Now;
            Status = TransferStatus.Pending;
        }

        /// <summary>
        /// Update progress calculations
        /// </summary>
        public void UpdateProgress()
        {
            ElapsedTime = DateTime.Now - StartTime;
            
            if (ElapsedTime.TotalSeconds > 0)
            {
                SpeedBytesPerSecond = TotalBytesTransferred / ElapsedTime.TotalSeconds;
                
                if (SpeedBytesPerSecond > 0 && TotalBytes > 0)
                {
                    var remainingBytes = TotalBytes - TotalBytesTransferred;
                    var estimatedSeconds = remainingBytes / SpeedBytesPerSecond;
                    EstimatedTimeRemaining = TimeSpan.FromSeconds(estimatedSeconds);
                }
            }

            if (TotalBytes > 0)
                OverallProgress = (int)((TotalBytesTransferred * 100) / TotalBytes);

            if (CurrentFileSize > 0)
                CurrentFileProgress = (int)((CurrentFileBytesTransferred * 100) / CurrentFileSize);
        }
    }
}





