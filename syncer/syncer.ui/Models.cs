using System;

namespace syncer.ui
{
    /// <summary>
    /// Represents a sync job configuration
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
        }

        public string GetNextRunTime()
        {
            if (!IsEnabled || LastRun == null)
                return "Not Scheduled";

            DateTime nextRun = LastRun.Value;
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

            return nextRun.ToString("yyyy-MM-dd HH:mm");
        }

        public override string ToString()
        {
            return Name + " - " + (IsEnabled ? "Enabled" : "Disabled");
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

        public ConnectionSettings()
        {
            Protocol = "LOCAL";
            ProtocolType = 0;
            Port = 21;
            UsePassiveMode = true;
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
}
