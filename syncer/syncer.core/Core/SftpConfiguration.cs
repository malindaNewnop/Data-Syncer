using System;

namespace syncer.core
{
    /// <summary>
    /// Advanced configuration settings for SFTP transfers
    /// </summary>
    [Serializable]
    public class SftpConfiguration
    {
        /// <summary>
        /// Connection timeout in milliseconds (default: 30000)
        /// </summary>
        public int ConnectionTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Operation timeout in milliseconds (default: 60000)
        /// </summary>
        public int OperationTimeoutMs { get; set; } = 60000;

        /// <summary>
        /// Maximum number of retry attempts (default: 3)
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Initial retry delay in milliseconds (default: 1000)
        /// </summary>
        public int RetryDelayMs { get; set; } = 1000;

        /// <summary>
        /// Whether to use exponential backoff for retries (default: true)
        /// </summary>
        public bool UseExponentialBackoff { get; set; } = true;

        /// <summary>
        /// Bandwidth limit in bytes per second (0 = unlimited)
        /// </summary>
        public long BandwidthLimitBytesPerSecond { get; set; } = 0;

        /// <summary>
        /// Buffer size for file transfers in bytes (default: 32768)
        /// </summary>
        public int TransferBufferSize { get; set; } = 32768;

        /// <summary>
        /// Whether to enable transfer resumption (default: true)
        /// </summary>
        public bool EnableTransferResumption { get; set; } = true;

        /// <summary>
        /// Minimum file size for resumption in bytes (default: 1MB)
        /// </summary>
        public long MinFileSizeForResumption { get; set; } = 1024 * 1024;

        /// <summary>
        /// Whether to verify file integrity after transfer (default: false)
        /// </summary>
        public bool VerifyTransferIntegrity { get; set; } = false;

        /// <summary>
        /// Whether to preserve file timestamps (default: true)
        /// </summary>
        public bool PreserveTimestamps { get; set; } = true;

        /// <summary>
        /// Whether to preserve file permissions (default: true)
        /// </summary>
        public bool PreservePermissions { get; set; } = true;

        /// <summary>
        /// SSH key exchange algorithms to prefer (comma-separated)
        /// </summary>
        public string PreferredKeyExchangeAlgorithms { get; set; } = "";

        /// <summary>
        /// SSH cipher algorithms to prefer (comma-separated)
        /// </summary>
        public string PreferredCipherAlgorithms { get; set; } = "";

        /// <summary>
        /// SSH MAC algorithms to prefer (comma-separated)
        /// </summary>
        public string PreferredMacAlgorithms { get; set; } = "";

        /// <summary>
        /// Whether to enable compression (default: false)
        /// </summary>
        public bool EnableCompression { get; set; } = false;

        /// <summary>
        /// Keep-alive interval in milliseconds (0 = disabled)
        /// </summary>
        public int KeepAliveIntervalMs { get; set; } = 0;

        /// <summary>
        /// Maximum number of concurrent transfers (default: 1)
        /// </summary>
        public int MaxConcurrentTransfers { get; set; } = 1;

        /// <summary>
        /// Whether to enable debug logging (default: false)
        /// </summary>
        public bool EnableDebugLogging { get; set; } = false;
    }

    /// <summary>
    /// Transfer statistics and metrics
    /// </summary>
    public class TransferStatistics
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public long TotalBytes { get; set; }
        public long TransferredBytes { get; set; }
        public double AverageSpeedBytesPerSecond { get; set; }
        public double ProgressPercentage { get; set; }
        public int RetryCount { get; set; }
        public bool WasResumed { get; set; }
        public long ResumedFromByte { get; set; }
        public string ErrorMessage { get; set; }
        public bool Success { get; set; }
    }

    /// <summary>
    /// Event arguments for transfer progress events
    /// </summary>
    public class TransferProgressEventArgs : EventArgs
    {
        public string FileName { get; set; }
        public long TotalBytes { get; set; }
        public long TransferredBytes { get; set; }
        public double ProgressPercentage { get; set; }
        public double SpeedBytesPerSecond { get; set; }
        public TimeSpan Elapsed { get; set; }
        public TimeSpan EstimatedTimeRemaining { get; set; }
        public bool IsUpload { get; set; }
    }

    /// <summary>
    /// Event arguments for transfer completion events
    /// </summary>
    public class TransferCompletedEventArgs : EventArgs
    {
        public string FileName { get; set; }
        public TransferStatistics Statistics { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
