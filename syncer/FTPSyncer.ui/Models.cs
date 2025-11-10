
using System;
using System.Collections.Generic;

namespace FTPSyncer.ui
{
    /// <summary>
    /// Result of a save configuration operation
    /// </summary>
    public enum SaveConfigurationResult
    {
        Success,
        ValidationError,
        NameConflict,
        Error
    }

    /// <summary>
    /// Detailed result of a save configuration operation
    /// </summary>
    public class SaveConfigurationOperationResult
    {
        public SaveConfigurationResult Result { get; set; }
        public string Message { get; set; }
        public SavedJobConfiguration ConflictingConfiguration { get; set; }
        public bool Success => Result == SaveConfigurationResult.Success;

        public SaveConfigurationOperationResult()
        {
        }

        public SaveConfigurationOperationResult(SaveConfigurationResult result, string message = null)
        {
            Result = result;
            Message = message;
        }
    }
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

        // Delete source after transfer setting
        public bool DeleteSourceAfterTransfer { get; set; }

        // File filtering properties
        public bool EnableFilters { get; set; }
        public string IncludeFileTypes { get; set; } // Comma-separated file extensions like "pdf,jpg,png"
        public string ExcludeFileTypes { get; set; } // Comma-separated file extensions like "tmp,bak"

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
            
            // Initialize delete source setting
            DeleteSourceAfterTransfer = false;
            
            // Initialize filter settings
            EnableFilters = false;
            IncludeFileTypes = "";
            ExcludeFileTypes = "";
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

    /// <summary>
    /// Represents a saved job configuration with connection settings
    /// </summary>
    [Serializable]
    public class SavedJobConfiguration
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUsed { get; set; }
        public string CreatedBy { get; set; }
        public bool IsDefault { get; set; }
        public string Category { get; set; }
        public List<string> Tags { get; set; }

        // Job Settings - Support for multiple jobs
        public SyncJob JobSettings { get; set; } // Keep for backward compatibility
        public List<SyncJob> Jobs { get; set; } // New multi-job support
        
        // Connection Settings
        public SavedConnection SourceConnection { get; set; }
        public SavedConnection DestinationConnection { get; set; }
        
        // Quick Launch Settings
        public bool EnableQuickLaunch { get; set; }
        public bool AutoStartOnLoad { get; set; }
        public bool ShowNotificationOnStart { get; set; }
        
        // Statistics
        public int TimesUsed { get; set; }
        public DateTime? LastSuccessfulRun { get; set; }
        public TimeSpan? LastExecutionTime { get; set; }
        
        // File Information
        /// <summary>
        /// The file path from which this configuration was imported
        /// </summary>
        public string SourceFilePath { get; set; }
        
        public SavedJobConfiguration()
        {
            Id = Guid.NewGuid().ToString();
            CreatedDate = DateTime.Now;
            CreatedBy = Environment.UserName;
            IsDefault = false;
            Tags = new List<string>();
            Category = "General";
            EnableQuickLaunch = true;
            AutoStartOnLoad = false;
            ShowNotificationOnStart = true;
            TimesUsed = 0;
            
            JobSettings = new SyncJob();
            Jobs = new List<SyncJob>();
            SourceConnection = new SavedConnection();
            DestinationConnection = new SavedConnection();
        }
        
        /// <summary>
        /// Updates usage statistics
        /// </summary>
        public void UpdateUsageStatistics()
        {
            TimesUsed++;
            LastUsed = DateTime.Now;
        }
        
        /// <summary>
        /// Ensures backward compatibility by migrating old JobSettings to new Jobs collection
        /// This should be called whenever a configuration is loaded
        /// </summary>
        public void EnsureJobsCompatibility()
        {
            // Debug logging to understand what's happening
            var logService = FTPSyncer.ui.ServiceLocator.LogService;
            if (logService != null)
            {
                logService.LogInfo($"EnsureJobsCompatibility called for config '{Name}': Jobs={Jobs?.Count ?? -1}, JobSettings={JobSettings != null}", "SavedJobConfiguration");
            }
            
            // If we have Jobs collection, we're good
            if (Jobs != null && Jobs.Count > 0)
            {
                if (logService != null)
                {
                    logService.LogInfo($"Configuration '{Name}' already has {Jobs.Count} jobs in Jobs collection", "SavedJobConfiguration");
                }
                return;
            }
                
            // If we don't have Jobs collection, initialize it
            if (Jobs == null)
            {
                Jobs = new List<SyncJob>();
                if (logService != null)
                {
                    logService.LogInfo($"Initialized empty Jobs collection for config '{Name}'", "SavedJobConfiguration");
                }
            }
            
            // If we have old JobSettings and no jobs in the new collection, migrate it
            if (JobSettings != null && Jobs.Count == 0)
            {
                Jobs.Add(JobSettings);
                if (logService != null)
                {
                    logService.LogInfo($"Migrated single JobSettings to Jobs collection for config '{Name}'", "SavedJobConfiguration");
                }
                // Keep JobSettings for backward compatibility, but Jobs is the primary collection now
            }
            
            if (logService != null)
            {
                logService.LogInfo($"EnsureJobsCompatibility completed for config '{Name}': Final Jobs count = {Jobs.Count}", "SavedJobConfiguration");
            }
        }
        
        /// <summary>
        /// Gets whether this configuration has multiple jobs
        /// </summary>
        public bool IsMultiJob
        {
            get 
            { 
                EnsureJobsCompatibility();
                return Jobs != null && Jobs.Count > 1; 
            }
        }
        
        /// <summary>
        /// Gets the total number of jobs in this configuration
        /// </summary>
        public int JobCount
        {
            get
            {
                EnsureJobsCompatibility();
                return Jobs?.Count ?? 0;
            }
        }
        
        /// <summary>
        /// Gets all jobs (including legacy single job) as a unified collection
        /// </summary>
        public List<SyncJob> GetAllJobs()
        {
            var allJobs = new List<SyncJob>();
            
            if (IsMultiJob)
            {
                allJobs.AddRange(Jobs);
            }
            else if (JobSettings != null)
            {
                allJobs.Add(JobSettings);
            }
            
            return allJobs;
        }
        
        /// <summary>
        /// Adds a new job to the configuration
        /// </summary>
        public void AddJob(SyncJob job)
        {
            if (job == null) return;
            
            if (Jobs == null)
                Jobs = new List<SyncJob>();
                
            Jobs.Add(job);
        }
        
        /// <summary>
        /// Removes a job from the configuration
        /// </summary>
        public bool RemoveJob(SyncJob job)
        {
            if (Jobs == null || job == null) return false;
            return Jobs.Remove(job);
        }
        
        /// <summary>
        /// Removes a job at the specified index
        /// </summary>
        public bool RemoveJobAt(int index)
        {
            if (Jobs == null || index < 0 || index >= Jobs.Count) return false;
            Jobs.RemoveAt(index);
            return true;
        }
        
        /// <summary>
        /// Migrates legacy single job to multi-job format
        /// </summary>
        public void MigrateToMultiJob()
        {
            if (!IsMultiJob && JobSettings != null)
            {
                if (Jobs == null)
                    Jobs = new List<SyncJob>();
                    
                Jobs.Add(JobSettings);
                JobSettings = null; // Clear the legacy job
            }
        }
        
        /// <summary>
        /// Gets a display name for the configuration
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    if (IsMultiJob)
                        return string.Format("{0} ({1} jobs)", Name, JobCount);
                    return Name;
                }
                if (JobSettings != null && !string.IsNullOrEmpty(JobSettings.Name))
                    return JobSettings.Name;
                if (IsMultiJob && Jobs.Count > 0 && !string.IsNullOrEmpty(Jobs[0].Name))
                    return string.Format("{0} (+{1} more)", Jobs[0].Name, Jobs.Count - 1);
                return "Unnamed Configuration";
            }
        }
        
        /// <summary>
        /// Gets a formatted description for display
        /// </summary>
        public string FormattedDescription
        {
            get
            {
                if (!string.IsNullOrEmpty(Description))
                    return Description;
                    
                if (JobSettings != null)
                {
                    var source = SourceConnection?.Settings?.IsLocalConnection == true ? "Local" : SourceConnection?.Settings?.Host ?? "Unknown";
                    var dest = DestinationConnection?.Settings?.IsLocalConnection == true ? "Local" : DestinationConnection?.Settings?.Host ?? "Unknown";
                    return string.Format("Transfer from {0} to {1}", source, dest);
                }
                
                return "No description available";
            }
        }
        
        /// <summary>
        /// Validates the configuration
        /// </summary>
        public List<string> ValidateConfiguration()
        {
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(Name))
                errors.Add("Configuration name is required");
            
            // Validate job configuration - either single job or multi-job
            if (JobSettings == null && (Jobs == null || Jobs.Count == 0))
            {
                errors.Add("Either job settings or jobs collection is required");
            }
            else if (JobSettings != null)
            {
                // Single job configuration
                errors.AddRange(JobSettings.ValidateConfiguration());
                
                // For single job, connection settings are required
                if (SourceConnection?.Settings == null)
                    errors.Add("Source connection settings are required");
                    
                if (DestinationConnection?.Settings == null)
                    errors.Add("Destination connection settings are required");
            }
            else if (Jobs != null && Jobs.Count > 0)
            {
                // Multi-job configuration - each job has its own connections
                for (int i = 0; i < Jobs.Count; i++)
                {
                    var job = Jobs[i];
                    if (job == null)
                    {
                        errors.Add($"Job {i + 1} is null");
                        continue;
                    }
                    
                    var jobErrors = job.ValidateConfiguration();
                    foreach (var jobError in jobErrors)
                    {
                        errors.Add($"Job '{job.Name ?? (i + 1).ToString()}': {jobError}");
                    }
                }
            }
                
            return errors;
        }
    }
    
    /// <summary>
    /// Represents a quick launch item for saved configurations
    /// </summary>
    [Serializable]
    public class QuickLaunchItem
    {
        public string ConfigurationId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUsed { get; set; }
        public bool IsFavorite { get; set; }
        public int SortOrder { get; set; }
        public string IconPath { get; set; }
        public string Hotkey { get; set; }
        
        // Reference to the full configuration
        public SavedJobConfiguration Configuration { get; set; }
        
        public QuickLaunchItem()
        {
            CreatedDate = DateTime.Now;
            IsFavorite = false;
            SortOrder = 0;
        }
        
        public override string ToString()
        {
            return DisplayName ?? "Unnamed Configuration";
        }
    }
}





