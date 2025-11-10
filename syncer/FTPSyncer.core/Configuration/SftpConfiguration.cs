using System;

namespace FTPSyncer.core.Configuration
{
    /// <summary>
    /// Configuration settings for SFTP transfers with advanced features
    /// </summary>
    public class SftpConfiguration
    {
        // Bandwidth Control
        public long BandwidthLimitBytesPerSecond { get; set; } = 0; // 0 = unlimited
        public bool EnableBandwidthThrottling => BandwidthLimitBytesPerSecond > 0;

        // Transfer Resumption
        public bool EnableTransferResumption { get; set; } = true;
        public string ResumeDataDirectory { get; set; } = null; // null = use temp directory

        // Integrity Verification
        public bool VerifyTransferIntegrity { get; set; } = true;
        public string HashAlgorithm { get; set; } = "SHA256"; // SHA256, SHA1, MD5

        // Retry Configuration
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelayMs { get; set; } = 1000;
        public bool UseExponentialBackoff { get; set; } = true;
        public int MaxRetryDelayMs { get; set; } = 30000;

        // Connection Timeouts
        public int ConnectionTimeoutMs { get; set; } = 30000;
        public int OperationTimeoutMs { get; set; } = 60000;
        public int KeepAliveIntervalMs { get; set; } = 30000;

        // Transfer Options
        public bool PreserveTimestamps { get; set; } = true;
        public bool EnableCompression { get; set; } = false;
        public int BufferSize { get; set; } = 32768; // 32KB default
        public bool OverwriteExisting { get; set; } = true;

        // Progress Reporting
        public bool EnableProgressReporting { get; set; } = true;
        public int ProgressReportIntervalMs { get; set; } = 1000;

        // Logging
        public bool EnableDetailedLogging { get; set; } = false;
        public string LogDirectory { get; set; } = null; // null = use default

        public SftpConfiguration()
        {
            // Set reasonable defaults
        }

        /// <summary>
        /// Creates a configuration optimized for fast local network transfers
        /// </summary>
        public static SftpConfiguration CreateFastLocalConfig()
        {
            return new SftpConfiguration
            {
                BandwidthLimitBytesPerSecond = 0, // Unlimited
                EnableTransferResumption = true,
                VerifyTransferIntegrity = false, // Skip for speed on local network
                MaxRetryAttempts = 1,
                RetryDelayMs = 500,
                UseExponentialBackoff = false,
                ConnectionTimeoutMs = 10000,
                OperationTimeoutMs = 30000,
                EnableCompression = false,
                BufferSize = 65536, // 64KB for faster local transfers
                EnableProgressReporting = true,
                ProgressReportIntervalMs = 500
            };
        }

        /// <summary>
        /// Creates a configuration optimized for slow/unreliable connections
        /// </summary>
        public static SftpConfiguration CreateReliableConfig()
        {
            return new SftpConfiguration
            {
                BandwidthLimitBytesPerSecond = 1048576, // 1MB/s limit
                EnableTransferResumption = true,
                VerifyTransferIntegrity = true,
                MaxRetryAttempts = 5,
                RetryDelayMs = 2000,
                UseExponentialBackoff = true,
                MaxRetryDelayMs = 60000,
                ConnectionTimeoutMs = 60000,
                OperationTimeoutMs = 120000,
                KeepAliveIntervalMs = 15000,
                EnableCompression = true,
                BufferSize = 16384, // 16KB for unreliable connections
                EnableProgressReporting = true,
                ProgressReportIntervalMs = 2000,
                EnableDetailedLogging = true
            };
        }

        /// <summary>
        /// Validates the configuration settings
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = null;

            if (BandwidthLimitBytesPerSecond < 0)
            {
                errorMessage = "Bandwidth limit cannot be negative";
                return false;
            }

            if (MaxRetryAttempts < 0)
            {
                errorMessage = "Max retry attempts cannot be negative";
                return false;
            }

            if (RetryDelayMs < 0)
            {
                errorMessage = "Retry delay cannot be negative";
                return false;
            }

            if (ConnectionTimeoutMs <= 0)
            {
                errorMessage = "Connection timeout must be positive";
                return false;
            }

            if (OperationTimeoutMs <= 0)
            {
                errorMessage = "Operation timeout must be positive";
                return false;
            }

            if (BufferSize <= 0)
            {
                errorMessage = "Buffer size must be positive";
                return false;
            }

            if (ProgressReportIntervalMs <= 0)
            {
                errorMessage = "Progress report interval must be positive";
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Profile containing both connection settings and transfer configuration
    /// </summary>
    public class SftpProfile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ConnectionSettings ConnectionSettings { get; set; }
        public SftpConfiguration TransferConfiguration { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUsedDate { get; set; }
        public int UsageCount { get; set; }

        public SftpProfile()
        {
            CreatedDate = DateTime.Now;
            LastUsedDate = DateTime.Now;
            UsageCount = 0;
            ConnectionSettings = new ConnectionSettings();
            TransferConfiguration = new SftpConfiguration();
        }

        public SftpProfile(string name) : this()
        {
            Name = name;
        }
    }

    /// <summary>
    /// Statistics for transfer operations
    /// </summary>
    public class TransferStatistics
    {
        public long TotalBytesTransferred { get; set; }
        public long TotalBytes { get; set; }
        public long TransferredBytes { get; set; }
        public TimeSpan TotalTransferTime { get; set; }
        public int FilesTransferred { get; set; }
        public int FilesProcessed { get; set; }
        public int FilesSucceeded { get; set; }
        public int FilesSkipped { get; set; }
        public int FilesFailed { get; set; }
        public int RetryAttempts { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public TimeSpan Duration => EndTime?.Subtract(StartTime) ?? DateTime.Now.Subtract(StartTime);

        public double AverageSpeedBytesPerSecond
        {
            get
            {
                if (TotalTransferTime.TotalSeconds > 0)
                    return TotalBytesTransferred / TotalTransferTime.TotalSeconds;
                if (Duration.TotalSeconds > 0)
                    return TransferredBytes / Duration.TotalSeconds;
                return 0;
            }
        }

        public double ProgressPercentage
        {
            get
            {
                return TotalBytes > 0 ? (double)TransferredBytes / TotalBytes * 100 : 0;
            }
        }

        public string FormattedAverageSpeed
        {
            get
            {
                double speed = AverageSpeedBytesPerSecond;
                if (speed >= 1073741824) // GB/s
                    return string.Format("{0:F2} GB/s", speed / 1073741824);
                if (speed >= 1048576) // MB/s
                    return string.Format("{0:F2} MB/s", speed / 1048576);
                if (speed >= 1024) // KB/s
                    return string.Format("{0:F2} KB/s", speed / 1024);
                return string.Format("{0:F0} B/s", speed);
            }
        }

        public TransferStatistics()
        {
            StartTime = DateTime.Now;
        }
    }

    /// <summary>
    /// Event arguments for transfer progress events
    /// </summary>
    public class TransferProgressEventArgs : EventArgs
    {
        public string FileName { get; set; }
        public long BytesTransferred { get; set; }
        public long TransferredBytes { get; set; }
        public long TotalBytes { get; set; }
        public double ProgressPercentage { get; set; }
        public double CurrentSpeedBytesPerSecond { get; set; }
        public double SpeedBytesPerSecond { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan Elapsed { get; set; }
        public TimeSpan EstimatedTimeRemaining { get; set; }

        public string FormattedProgress
        {
            get { return string.Format("{0:F1}%", ProgressPercentage); }
        }

        public string FormattedSpeed
        {
            get
            {
                if (CurrentSpeedBytesPerSecond >= 1048576) // MB/s
                    return string.Format("{0:F2} MB/s", CurrentSpeedBytesPerSecond / 1048576);
                if (CurrentSpeedBytesPerSecond >= 1024) // KB/s
                    return string.Format("{0:F2} KB/s", CurrentSpeedBytesPerSecond / 1024);
                return string.Format("{0:F0} B/s", CurrentSpeedBytesPerSecond);
            }
        }
    }

    /// <summary>
    /// Event arguments for transfer completion
    /// </summary>
    public class TransferCompletedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public TransferStatistics Statistics { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.Now;
    }
}





