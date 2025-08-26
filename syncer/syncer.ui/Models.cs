
using System;
using System.Collections.Generic;

namespace syncer.ui
{
    /// <summary>
    /// Represents a sync job configuration with enhanced features
    /// </summary>
    public class SyncJob
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public bool IncludeSubFolders { get; set; }
        public bool OverwriteExisting { get; set; }
        public DateTime StartTime { get; set; }
        public int IntervalValue { get; set; }
        public string IntervalType { get; set; } 
        public string TransferMode { get; set; } 
        public DateTime? LastRun { get; set; }
        public string LastStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Connection settings for source and destination
        public ConnectionSettings SourceConnection { get; set; }
        public ConnectionSettings DestinationConnection { get; set; }

        // Enhanced properties for better functionality
        public int RetryCount { get; set; }
        public int MaxRetries { get; set; }
        public int RetryDelaySeconds { get; set; }
        public bool EnableBackup { get; set; }
        public string BackupPath { get; set; }
        public bool ValidateTransfer { get; set; }
        public bool ShowTransferProgress { get; set; }
        
        // Statistics
        public long LastTransferSize { get; set; }
        public int LastFileCount { get; set; }
        public TimeSpan LastDuration { get; set; }
        public double LastAverageSpeed { get; set; } // Bytes per second

        // Filter settings
        public FilterSettings FilterSettings { get; set; }

        public SyncJob()
        {
            CreatedDate = DateTime.Now;
            IsEnabled = true;
            IntervalValue = 60;
            IntervalType = "Minutes";
            TransferMode = "Copy (Keep both files)";
            LastStatus = "Never Run";
            IncludeSubFolders = true;
            OverwriteExisting = true;
            
            // Initialize with local connections by default
            SourceConnection = new ConnectionSettings();
            DestinationConnection = new ConnectionSettings();
            
            // Enhanced defaults
            MaxRetries = 3;
            RetryDelaySeconds = 5;
            EnableBackup = false;
            ValidateTransfer = true;
            ShowTransferProgress = true;
            RetryCount = 0;
            LastTransferSize = 0;
            LastFileCount = 0;
            LastDuration = TimeSpan.Zero;
            LastAverageSpeed = 0;
            
            // Initialize filter settings
            FilterSettings = new FilterSettings();
        }

        public string GetNextRunTime()
        {
            if (!IsEnabled)
                return "Not Scheduled";
                
            // Use StartTime as base if job never ran before, otherwise use LastRun
            // If LastRun is MinValue (0001-01-01), consider it as never run
            DateTime baseTime;
            
            if (LastRun == null || !LastRun.HasValue || LastRun.Value == DateTime.MinValue)
            {
                // For new jobs, use StartTime or current time if StartTime is too old
                if (StartTime > DateTime.Now.AddDays(-1)) // If StartTime is recent
                    baseTime = StartTime;
                else
                    baseTime = DateTime.Now; // Use current time for jobs with old StartTime
            }
            else
            {
                baseTime = LastRun.Value;
            }
            
            DateTime nextRun = baseTime;
            switch (IntervalType)
            {
                case "Minutes":
                    nextRun = baseTime.AddMinutes(IntervalValue);
                    break;
                case "Hours":
                    nextRun = baseTime.AddHours(IntervalValue);
                    break;
                case "Days":
                    nextRun = baseTime.AddDays(IntervalValue);
                    break;
            }
            
            // If next run is in the past, increment until it's in the future
            while (nextRun < DateTime.Now)
            {
                switch (IntervalType)
                {
                    case "Minutes":
                        nextRun = nextRun.AddMinutes(IntervalValue);
                        break;
                    case "Hours":
                        nextRun = nextRun.AddHours(IntervalValue);
                        break;
                    case "Days":
                        nextRun = nextRun.AddDays(IntervalValue);
                        break;
                }
            }

            return nextRun.ToString("yyyy-MM-dd HH:mm");
        }

        public override string ToString()
        {
            return Name + " - " + (IsEnabled ? "Enabled" : "Disabled");
        }

        /// <summary>
        /// Get human-readable transfer speed
        /// </summary>
        public string GetFormattedSpeed()
        {
            if (LastAverageSpeed < 1024)
                return string.Format("{0:F1} B/s", LastAverageSpeed);
            else if (LastAverageSpeed < 1024 * 1024)
                return string.Format("{0:F1} KB/s", LastAverageSpeed / 1024);
            else if (LastAverageSpeed < 1024 * 1024 * 1024)
                return string.Format("{0:F1} MB/s", LastAverageSpeed / (1024 * 1024));
            else
                return string.Format("{0:F1} GB/s", LastAverageSpeed / (1024 * 1024 * 1024));
        }

        /// <summary>
        /// Get human-readable data size
        /// </summary>
        public string GetFormattedDataSize()
        {
            if (LastTransferSize < 1024)
                return string.Format("{0} B", LastTransferSize);
            else if (LastTransferSize < 1024 * 1024)
                return string.Format("{0:F1} KB", LastTransferSize / 1024.0);
            else if (LastTransferSize < 1024 * 1024 * 1024)
                return string.Format("{0:F1} MB", LastTransferSize / (1024.0 * 1024));
            else
                return string.Format("{0:F1} GB", LastTransferSize / (1024.0 * 1024 * 1024));
        }

        /// <summary>
        /// Reset statistics
        /// </summary>
        public void ResetStatistics()
        {
            RetryCount = 0;
            LastTransferSize = 0;
            LastFileCount = 0;
            LastDuration = TimeSpan.Zero;
            LastAverageSpeed = 0;
        }

        /// <summary>
        /// Validate job configuration
        /// </summary>
        public List<string> ValidateConfiguration()
        {
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(Name))
                errors.Add("Job name is required");
                
            if (string.IsNullOrEmpty(SourcePath))
                errors.Add("Source path is required");
                
            if (string.IsNullOrEmpty(DestinationPath))
                errors.Add("Destination path is required");
                
            if (IntervalValue <= 0)
                errors.Add("Interval value must be greater than 0");
                
            if (MaxRetries < 0)
                errors.Add("Max retries cannot be negative");
                
            if (RetryDelaySeconds < 0)
                errors.Add("Retry delay cannot be negative");
            
            // Validate connections
            if (SourceConnection != null && SourceConnection.IsRemoteConnection)
            {
                if (string.IsNullOrEmpty(SourceConnection.Host))
                    errors.Add("Source connection host is required");
                if (string.IsNullOrEmpty(SourceConnection.Username))
                    errors.Add("Source connection username is required");
            }
            
            if (DestinationConnection != null && DestinationConnection.IsRemoteConnection)
            {
                if (string.IsNullOrEmpty(DestinationConnection.Host))
                    errors.Add("Destination connection host is required");
                if (string.IsNullOrEmpty(DestinationConnection.Username))
                    errors.Add("Destination connection username is required");
            }
            
            return errors;
        }
    }

    /// <summary>
    /// Represents connection settings
    /// </summary>
    public class ConnectionSettings
    {
        public int ProtocolType { get; set; } = 0; // 0 = Local, 1 = FTP, 2 = SFTP
        public string Protocol { get; set; } = "LOCAL";
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UsePassiveMode { get; set; }
        public bool UseSftp { get; set; }
        public bool IsConnected { get; set; }
        public DateTime? LastConnectionTest { get; set; }
        
        // Enhanced SFTP Properties
        public string SshKeyPath { get; set; }
        public int Timeout { get; set; } = 30;
        public bool UseKeyAuthentication { get; set; }
        public string SshKeyPassphrase { get; set; }

        // Enhanced FTP Properties
        public bool EnableSsl { get; set; }
        public bool IgnoreCertificateErrors { get; set; }
        public string CertificatePath { get; set; }

        // Connection Management
        public int MaxConnections { get; set; } = 1;
        public bool KeepAlive { get; set; } = false;
        public int OperationTimeout { get; set; } = 60;

        public ConnectionSettings()
        {
            Protocol = "LOCAL";
            ProtocolType = 0;
            Port = 21;
            UsePassiveMode = true;
            Timeout = 30;
            OperationTimeout = 60;
            MaxConnections = 1;
            KeepAlive = false;
            EnableSsl = false;
            IgnoreCertificateErrors = false;
        }

        public bool IsLocalConnection
        {
            get { return ProtocolType == 0 || (Protocol != null && Protocol.Equals("LOCAL", StringComparison.OrdinalIgnoreCase)); }
        }

        public bool IsRemoteConnection
        {
            get { return !IsLocalConnection; }
        }

        public string ConnectionTypeDisplay
        {
            get
            {
                switch (ProtocolType)
                {
                    case 0: return "Local File System";
                    case 1: return "FTP Server";
                    case 2: return "SFTP Server";
                    default: return "Unknown";
                }
            }
        }

        /// <summary>
        /// Test connection settings
        /// </summary>
        public ConnectionTestResult TestConnection()
        {
            var result = new ConnectionTestResult
            {
                Protocol = (ProtocolType)this.ProtocolType,
                Host = this.Host,
                Port = this.Port,
                TestTime = DateTime.Now
            };

            try
            {
                var startTime = DateTime.Now;
                
                if (IsLocalConnection)
                {
                    result.Success = true;
                    result.ServerInfo = "Local File System";
                }
                else
                {
                    // This would be implemented with actual connection testing
                    // For now, just basic validation
                    result.Success = !string.IsNullOrEmpty(Host) && !string.IsNullOrEmpty(Username);
                    if (!result.Success)
                    {
                        result.ErrorMessage = "Host and Username are required for remote connections";
                    }
                }
                
                result.ConnectionTime = DateTime.Now - startTime;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            LastConnectionTest = result.TestTime;
            IsConnected = result.Success;
            
            return result;
        }

        /// <summary>
        /// Get connection string for display
        /// </summary>
        public string GetConnectionString()
        {
            if (IsLocalConnection)
                return "Local File System";
                
            var protocolName = Protocol.ToUpper();
            if (ProtocolType == 2) // SFTP
                protocolName = "SFTP";
                
            return string.Format("{0}://{1}@{2}:{3}", protocolName, Username, Host, Port);
        }

        /// <summary>
        /// Clone connection settings
        /// </summary>
        public ConnectionSettings Clone()
        {
            return new ConnectionSettings
            {
                ProtocolType = this.ProtocolType,
                Protocol = this.Protocol,
                Host = this.Host,
                Port = this.Port,
                Username = this.Username,
                Password = this.Password,
                UsePassiveMode = this.UsePassiveMode,
                UseSftp = this.UseSftp,
                SshKeyPath = this.SshKeyPath,
                Timeout = this.Timeout,
                UseKeyAuthentication = this.UseKeyAuthentication,
                SshKeyPassphrase = this.SshKeyPassphrase,
                EnableSsl = this.EnableSsl,
                IgnoreCertificateErrors = this.IgnoreCertificateErrors,
                CertificatePath = this.CertificatePath,
                MaxConnections = this.MaxConnections,
                KeepAlive = this.KeepAlive,
                OperationTimeout = this.OperationTimeout
            };
        }
    }

    /// <summary>
    /// Represents filter settings
    /// </summary>
    public class FilterSettings
    {
        public bool FiltersEnabled { get; set; }
        public string[] AllowedFileTypes { get; set; }
        public decimal MinFileSize { get; set; }
        public decimal MaxFileSize { get; set; }
        public bool IncludeHiddenFiles { get; set; }
        public bool IncludeSystemFiles { get; set; }
        public bool IncludeReadOnlyFiles { get; set; }
        public string ExcludePatterns { get; set; }

        public FilterSettings()
        {
            FiltersEnabled = true;
            MinFileSize = 0;
            MaxFileSize = 100;
            IncludeReadOnlyFiles = true;
        }
    }

    /// <summary>
    /// Protocol types for connections
    /// </summary>
    public enum ProtocolType 
    { 
        Local = 0, 
        Ftp = 1, 
        Sftp = 2 
    }

    /// <summary>
    /// Connection test result
    /// </summary>
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
    /// Transfer progress information for UI
    /// </summary>
    public class TransferProgress
    {
        public string JobName { get; set; }
        public string CurrentFile { get; set; }
        public int CurrentFileProgress { get; set; }
        public int OverallProgress { get; set; }
        public int FilesCompleted { get; set; }
        public int TotalFiles { get; set; }
        public long BytesTransferred { get; set; }
        public long TotalBytes { get; set; }
        public string Speed { get; set; }
        public string TimeRemaining { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Represents a saved connection with metadata
    /// </summary>
    public class SavedConnection
    {
        public string Name { get; set; }
        public ConnectionSettings Settings { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUsed { get; set; }
        public bool IsDefault { get; set; }
        public string Description { get; set; }

        public string DisplayName
        {
            get
            {
                if (IsDefault)
                    return $"{Name} (Default)";
                return Name;
            }
        }

        public string ConnectionSummary
        {
            get
            {
                if (Settings == null) return "Invalid Connection";

                if (Settings.IsLocalConnection)
                    return "Local File System";

                return $"{Settings.Protocol}://{Settings.Username}@{Settings.Host}:{Settings.Port}";
            }
        }
    }
}
