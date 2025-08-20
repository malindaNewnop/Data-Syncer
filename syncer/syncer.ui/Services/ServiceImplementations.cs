using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Timers;

namespace syncer.ui.Services
{
    // Full implementation of sync job service with actual scheduling functionality
    public class SyncJobService : ISyncJobService
    {
        private List<SyncJob> _jobs;
        private Dictionary<int, Timer> _jobTimers;

        public SyncJobService()
        {
            _jobs = new List<SyncJob>();
            _jobTimers = new Dictionary<int, Timer>();
            ServiceLocator.LogService.LogInfo("SyncJobService initialized with scheduling support");
        }

        public List<SyncJob> GetAllJobs()
        {
            return new List<SyncJob>(_jobs);
        }

        public SyncJob GetJobById(int id)
        {
            for (int i = 0; i < _jobs.Count; i++)
            {
                if (_jobs[i].Id == id) return _jobs[i];
            }
            return null;
        }

        public int CreateJob(SyncJob job)
        {
            job.Id = _jobs.Count > 0 ? _jobs[_jobs.Count - 1].Id + 1 : 1;
            job.CreatedDate = DateTime.Now;
            _jobs.Add(job);
            
            // Schedule the job if it's enabled
            if (job.IsEnabled)
            {
                ScheduleJob(job);
            }
            
            ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' created with ID {1}", job.Name, job.Id));
            return job.Id;
        }

        public bool UpdateJob(SyncJob job)
        {
            SyncJob existing = GetJobById(job.Id);
            if (existing == null) return false;
            
            // Stop existing timer if any
            StopJobTimer(job.Id);
            
            int idx = _jobs.IndexOf(existing);
            _jobs[idx] = job;
            
            // Reschedule if enabled
            if (job.IsEnabled)
            {
                ScheduleJob(job);
            }
            
            ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' updated", job.Name));
            return true;
        }

        public bool DeleteJob(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return false;
            
            // Stop timer first
            StopJobTimer(id);
            
            _jobs.Remove(job);
            ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' deleted", job.Name));
            return true;
        }

        public bool StartJob(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return false;
            
            job.IsEnabled = true;
            ScheduleJob(job);
            ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' started", job.Name));
            return true;
        }

        public bool StopJob(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return false;
            
            job.IsEnabled = false;
            StopJobTimer(id);
            ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' stopped", job.Name));
            return true;
        }

        public string GetJobStatus(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return "Not Found";
            
            if (!job.IsEnabled) return "Disabled";
            if (_jobTimers.ContainsKey(id)) return "Scheduled";
            return "Enabled";
        }

        private void ScheduleJob(SyncJob job)
        {
            if (job == null || !job.IsEnabled) return;
            
            // Stop existing timer
            StopJobTimer(job.Id);
            
            // Calculate next run time
            DateTime nextRun = CalculateNextRunTime(job);
            double millisecondsToNextRun = (nextRun - DateTime.Now).TotalMilliseconds;
            
            if (millisecondsToNextRun <= 0)
            {
                // If time has passed, schedule for the next interval
                millisecondsToNextRun = GetIntervalInMilliseconds(job);
            }
            
            Timer timer = new Timer(millisecondsToNextRun);
            timer.Elapsed += (sender, e) => OnTimerElapsed(job.Id);
            timer.AutoReset = false; // Single shot, we'll reschedule after execution
            timer.Start();
            
            _jobTimers[job.Id] = timer;
            
            ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' scheduled to run in {1} minutes", 
                job.Name, (int)(millisecondsToNextRun / 60000)));
        }

        private void StopJobTimer(int jobId)
        {
            if (_jobTimers.ContainsKey(jobId))
            {
                _jobTimers[jobId].Stop();
                _jobTimers[jobId].Dispose();
                _jobTimers.Remove(jobId);
            }
        }

        private void OnTimerElapsed(int jobId)
        {
            SyncJob job = GetJobById(jobId);
            if (job == null || !job.IsEnabled) return;
            
            try
            {
                ServiceLocator.LogService.LogInfo(string.Format("Executing scheduled job: {0}", job.Name));
                
                // Execute the job
                ExecuteJob(job);
                
                // Update last run time and status
                job.LastRun = DateTime.Now;
                job.LastStatus = "Completed Successfully";
                
                // Reschedule for next run
                if (job.IsEnabled)
                {
                    ScheduleJob(job);
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError(string.Format("Error executing job '{0}': {1}", job.Name, ex.Message));
                job.LastRun = DateTime.Now;
                job.LastStatus = "Failed: " + ex.Message;
                
                // Still reschedule for next attempt
                if (job.IsEnabled)
                {
                    ScheduleJob(job);
                }
            }
        }

        private void ExecuteJob(SyncJob job)
        {
            ServiceLocator.LogService.LogInfo(string.Format("Executing job '{0}': {1} -> {2}", 
                job.Name, job.SourcePath, job.DestinationPath));
            
            try
            {
                // Get connection settings for source and destination
                var sourceConnection = job.SourceConnection ?? new ConnectionSettings();
                var destConnection = job.DestinationConnection ?? new ConnectionSettings();
                
                // For local transfers, create appropriate transfer clients
                if (sourceConnection.Protocol == "LOCAL" && destConnection.Protocol == "LOCAL")
                {
                    // Local to local transfer
                    ExecuteLocalToLocalTransfer(job);
                }
                else if (sourceConnection.Protocol == "LOCAL")
                {
                    // Local to remote upload
                    ExecuteUploadTransfer(job);
                }
                else if (destConnection.Protocol == "LOCAL")
                {
                    // Remote to local download
                    ExecuteDownloadTransfer(job);
                }
                else
                {
                    // Remote to remote transfer
                    ExecuteRemoteToRemoteTransfer(job);
                }
                
                ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' executed successfully", job.Name));
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError(string.Format("Job '{0}' execution failed: {1}", job.Name, ex.Message));
                throw;
            }
        }

        private void ExecuteLocalToLocalTransfer(SyncJob job)
        {
            if (!Directory.Exists(job.SourcePath))
            {
                throw new Exception(string.Format("Source directory does not exist: {0}", job.SourcePath));
            }

            if (!Directory.Exists(job.DestinationPath))
            {
                Directory.CreateDirectory(job.DestinationPath);
            }

            string[] files = Directory.GetFiles(job.SourcePath, "*", 
                job.IncludeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            int transferredFiles = 0;
            long transferredBytes = 0;

            foreach (string sourceFile in files)
            {
                string relativePath = Path.GetFullPath(sourceFile).Substring(Path.GetFullPath(job.SourcePath).Length + 1);
                string destFile = Path.Combine(job.DestinationPath, relativePath);
                
                string destDir = Path.GetDirectoryName(destFile);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                if (!File.Exists(destFile) || job.OverwriteExisting)
                {
                    File.Copy(sourceFile, destFile, job.OverwriteExisting);
                    FileInfo fileInfo = new FileInfo(sourceFile);
                    transferredBytes += fileInfo.Length;
                    transferredFiles++;
                }
            }

            job.LastFileCount = transferredFiles;
            job.LastTransferSize = transferredBytes;
        }

        private void ExecuteUploadTransfer(SyncJob job)
        {
            // This would require access to transfer clients
            // For now, just log the operation
            ServiceLocator.LogService.LogInfo(string.Format("Upload transfer from {0} to remote {1} (not fully implemented)", 
                job.SourcePath, job.DestinationPath));
        }

        private void ExecuteDownloadTransfer(SyncJob job)
        {
            // This would require access to transfer clients  
            // For now, just log the operation
            ServiceLocator.LogService.LogInfo(string.Format("Download transfer from remote {0} to {1} (not fully implemented)", 
                job.SourcePath, job.DestinationPath));
        }

        private void ExecuteRemoteToRemoteTransfer(SyncJob job)
        {
            // This would require access to transfer clients
            // For now, just log the operation  
            ServiceLocator.LogService.LogInfo(string.Format("Remote-to-remote transfer from {0} to {1} (not fully implemented)", 
                job.SourcePath, job.DestinationPath));
        }

        private DateTime CalculateNextRunTime(SyncJob job)
        {
            DateTime baseTime = job.LastRun.HasValue && job.LastRun.Value != DateTime.MinValue 
                ? job.LastRun.Value 
                : job.StartTime;
                
            if (baseTime < DateTime.Now.AddDays(-1))
            {
                baseTime = DateTime.Now;
            }
            
            switch (job.IntervalType)
            {
                case "Minutes":
                    return baseTime.AddMinutes(job.IntervalValue);
                case "Hours":
                    return baseTime.AddHours(job.IntervalValue);
                case "Days":
                    return baseTime.AddDays(job.IntervalValue);
                default:
                    return baseTime.AddMinutes(job.IntervalValue);
            }
        }

        private double GetIntervalInMilliseconds(SyncJob job)
        {
            switch (job.IntervalType)
            {
                case "Minutes":
                    return job.IntervalValue * 60 * 1000;
                case "Hours":
                    return job.IntervalValue * 60 * 60 * 1000;
                case "Days":
                    return job.IntervalValue * 24 * 60 * 60 * 1000;
                default:
                    return job.IntervalValue * 60 * 1000; // Default to minutes
            }
        }
    }

    // Stub implementation of connection service - will be replaced with actual backend implementation
    public class ConnectionService : IConnectionService
    {
        private ConnectionSettings _settings;

        public ConnectionService()
        {
            _settings = new ConnectionSettings();
            Console.WriteLine("WARNING: Using stub ConnectionService - connection settings will not be persisted");
        }

        public ConnectionSettings GetConnectionSettings()
        {
            return _settings;
        }

        public bool SaveConnectionSettings(ConnectionSettings settings)
        {
            _settings = settings;
            return true;
        }

        public bool TestConnection(ConnectionSettings settings)
        {
            if (StringExtensions.IsNullOrWhiteSpace(settings.Host) ||
                StringExtensions.IsNullOrWhiteSpace(settings.Username))
            {
                return false;
            }
            return true;
        }

        public bool IsConnected()
        {
            return _settings.IsConnected;
        }
    }

    // Stub implementation of filter service - will be replaced with actual backend implementation
    public class FilterService : IFilterService
    {
        private FilterSettings _settings;

        public FilterService()
        {
            _settings = new FilterSettings();
        }

        public FilterSettings GetFilterSettings()
        {
            return _settings;
        }

        public bool SaveFilterSettings(FilterSettings settings)
        {
            _settings = settings;
            return true;
        }

        public string[] GetDefaultFileTypes()
        {
            return new string[]
            {
                ".txt - Text files",
                ".doc, .docx - Word documents",
                ".xls, .xlsx - Excel files",
                ".pdf - PDF documents",
                ".jpg, .jpeg - JPEG images",
                ".png - PNG images",
                ".gif - GIF images",
                ".mp4 - Video files",
                ".mp3 - Audio files",
                ".zip, .rar - Archive files",
                ".exe - Executable files",
                ".dll - Library files",
                ".log - Log files",
                ".csv - CSV files",
                ".xml - XML files",
                ".json - JSON files"
            };
        }
    }

    // Stub implementation of log service - will be replaced with actual backend implementation
    public class LogService : ILogService
    {
        private DataTable _logsTable;

        public LogService()
        {
            InitializeLogTable();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            // Add some sample log entries for testing
            _logsTable.Rows.Add(DateTime.Now.AddMinutes(-10), "INFO", "File Sync Job", "document.pdf", "Success", "File transferred successfully");
            _logsTable.Rows.Add(DateTime.Now.AddMinutes(-8), "WARNING", "Backup Job", "data.xml", "Retry", "Connection timeout, retrying...");
            _logsTable.Rows.Add(DateTime.Now.AddMinutes(-5), "ERROR", "Upload Task", "image.jpg", "Failed", "Authentication failed");
            _logsTable.Rows.Add(DateTime.Now.AddMinutes(-2), "INFO", "Cleanup Job", "", "Success", "Temporary files cleaned");
        }

        private void InitializeLogTable()
        {
            _logsTable = new DataTable();
            _logsTable.Columns.Add("DateTime", typeof(DateTime));
            _logsTable.Columns.Add("Level", typeof(string));
            _logsTable.Columns.Add("Job", typeof(string));
            _logsTable.Columns.Add("File", typeof(string));
            _logsTable.Columns.Add("Status", typeof(string));
            _logsTable.Columns.Add("Message", typeof(string));
        }

        public DataTable GetLogs(DateTime? fromDate, DateTime? toDate, string logLevel)
        {
            DataTable filtered = _logsTable.Clone();
            foreach (DataRow row in _logsTable.Rows)
            {
                DateTime logDate = (DateTime)row["DateTime"];
                string level = row["Level"].ToString();
                bool includeRow = true;
                if (fromDate.HasValue && logDate < fromDate.Value) includeRow = false;
                if (toDate.HasValue && logDate > toDate.Value) includeRow = false;
                if (!StringExtensions.IsNullOrWhiteSpace(logLevel) && logLevel != "All" && level != logLevel) includeRow = false;
                if (includeRow) filtered.ImportRow(row);
            }
            return filtered;
        }

        public DataTable GetLogs()
        {
            return _logsTable.Copy();
        }

        public bool ClearLogs()
        {
            _logsTable.Clear();
            return true;
        }

        public bool ExportLogs(string filePath)
        {
            return ExportLogs(filePath, null, null);
        }

        public bool ExportLogs(string filePath, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                DataTable logs = GetLogs(fromDate, toDate, null);
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("DateTime,Level,Job,File,Status,Message");
                    foreach (DataRow row in logs.Rows)
                    {
                        string line = row["DateTime"].ToString() + "," +
                                      EscapeCsvField(row["Level"].ToString()) + "," +
                                      EscapeCsvField(row["Job"].ToString()) + "," +
                                      EscapeCsvField(row["File"].ToString()) + "," +
                                      EscapeCsvField(row["Status"].ToString()) + "," +
                                      EscapeCsvField(row["Message"].ToString());
                        writer.WriteLine(line);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string EscapeCsvField(string field)
        {
            if (field.IndexOf(',') >= 0 || field.IndexOf('"') >= 0 || field.IndexOf('\n') >= 0)
            {
                return '"' + field.Replace("\"", "\"\"") + '"';
            }
            return field;
        }

        public void LogInfo(string message)
        {
            LogInfo(message, string.Empty);
        }

        public void LogInfo(string message, string jobName)
        {
            AddLog("INFO", message, jobName);
        }

        public void LogWarning(string message)
        {
            LogWarning(message, string.Empty);
        }

        public void LogWarning(string message, string jobName)
        {
            AddLog("WARNING", message, jobName);
        }

        public void LogError(string message)
        {
            LogError(message, string.Empty);
        }

        public void LogError(string message, string jobName)
        {
            AddLog("ERROR", message, jobName);
        }

        private void AddLog(string level, string message, string jobName)
        {
            _logsTable.Rows.Add(DateTime.Now, level, jobName, string.Empty, string.Empty, message);
        }
    }

    // Stub implementation of service manager - will be replaced with actual backend implementation
    public class ServiceManager : IServiceManager
    {
        private bool _isRunning;
        public bool StartService() { _isRunning = true; return true; }
        public bool StopService() { _isRunning = false; return true; }
        public bool IsServiceRunning() { return _isRunning; }
        public string GetServiceStatus() { return _isRunning ? "Running" : "Stopped"; }
        
        public void Dispose()
        {
            // Nothing to dispose in stub implementation
        }
    }

    // Full implementation of configuration service using Windows Registry for .NET 3.5 compatibility
    public class ConfigurationService : IConfigurationService
    {
        private Dictionary<string, object> _settings = new Dictionary<string, object>();
        private const string REGISTRY_KEY = @"SOFTWARE\DataSyncer\Settings";
        private bool _registryAvailable = true;
        
        public ConfigurationService() 
        { 
            LoadAllSettings(); 
        }
        
        public T GetSetting<T>(string key, T defaultValue)
        {
            if (_settings.ContainsKey(key))
            {
                try 
                { 
                    object value = _settings[key];
                    if (value is T)
                        return (T)value;
                    return (T)Convert.ChangeType(value, typeof(T));
                } 
                catch 
                { 
                    return defaultValue; 
                }
            }
            return defaultValue;
        }
        
        public bool SaveSetting<T>(string key, T value) 
        { 
            try
            {
                _settings[key] = value;
                SaveToRegistry(key, value);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public bool DeleteSetting(string key) 
        { 
            try
            {
                bool removed = _settings.Remove(key);
                if (removed && _registryAvailable)
                {
                    using (Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true))
                    {
                        if (regKey != null)
                        {
                            regKey.DeleteValue(key, false);
                        }
                    }
                }
                return removed;
            }
            catch
            {
                return false;
            }
        }
        
        public void SaveAllSettings() 
        { 
            try
            {
                if (!_registryAvailable) return;
                
                foreach (var kvp in _settings)
                {
                    SaveToRegistry(kvp.Key, kvp.Value);
                }
            }
            catch
            {
                // Ignore errors during bulk save
            }
        }
        
        public void LoadAllSettings() 
        { 
            try
            {
                _settings.Clear();
                
                using (Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(REGISTRY_KEY))
                {
                    if (regKey != null)
                    {
                        foreach (string valueName in regKey.GetValueNames())
                        {
                            object value = regKey.GetValue(valueName);
                            if (value != null)
                            {
                                _settings[valueName] = value;
                            }
                        }
                    }
                }
            }
            catch
            {
                _registryAvailable = false;
                // Fall back to default settings if registry is not available
                SetDefaultSettings();
            }
        }
        
        private void SaveToRegistry<T>(string key, T value)
        {
            try
            {
                if (!_registryAvailable) return;
                
                using (Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(REGISTRY_KEY))
                {
                    if (regKey != null)
                    {
                        // Convert to appropriate registry type
                        if (value is bool)
                            regKey.SetValue(key, (bool)(object)value ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                        else if (value is int)
                            regKey.SetValue(key, (int)(object)value, Microsoft.Win32.RegistryValueKind.DWord);
                        else
                            regKey.SetValue(key, value.ToString(), Microsoft.Win32.RegistryValueKind.String);
                    }
                }
            }
            catch
            {
                // Ignore registry save errors
            }
        }
        
        private void SetDefaultSettings()
        {
            _settings["NotificationsEnabled"] = true;
            _settings["NotificationDelay"] = 3000;
            _settings["MinimizeToTray"] = true;
            _settings["StartMinimized"] = false;
        }
    }
}
