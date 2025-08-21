using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Timers;
using System.Threading;
using System.Linq;

namespace syncer.ui.Services
{
    // Full implementation of sync job service with actual scheduling functionality
    public class SyncJobService : ISyncJobService
    {
        private List<SyncJob> _jobs;
        private Dictionary<int, System.Timers.Timer> _jobTimers;

        public SyncJobService()
        {
            _jobs = new List<SyncJob>();
            _jobTimers = new Dictionary<int, System.Timers.Timer>();
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
            
            if (job.IsEnabled)
            {
                ScheduleJob(job);
            }
            
            return job.Id;
        }

        public bool UpdateJob(SyncJob job)
        {
            SyncJob existing = GetJobById(job.Id);
            if (existing == null) return false;
            
            StopJobTimer(job.Id);
            
            int idx = _jobs.IndexOf(existing);
            _jobs[idx] = job;
            
            if (job.IsEnabled)
            {
                ScheduleJob(job);
            }
            
            return true;
        }

        public bool DeleteJob(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return false;
            
            StopJobTimer(id);
            _jobs.Remove(job);
            return true;
        }

        public bool StartJob(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return false;
            
            job.IsEnabled = true;
            ScheduleJob(job);
            return true;
        }

        public bool StopJob(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return false;
            
            job.IsEnabled = false;
            StopJobTimer(id);
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
            
            StopJobTimer(job.Id);
            
            DateTime nextRun = CalculateNextRunTime(job);
            double millisecondsToNextRun = (nextRun - DateTime.Now).TotalMilliseconds;
            
            if (millisecondsToNextRun <= 0)
            {
                millisecondsToNextRun = GetIntervalInMilliseconds(job);
            }
            
            System.Timers.Timer timer = new System.Timers.Timer(millisecondsToNextRun);
            timer.Elapsed += (sender, e) => OnTimerElapsed(job.Id);
            timer.AutoReset = false;
            timer.Start();
            
            _jobTimers[job.Id] = timer;
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
                ExecuteJob(job);
                job.LastRun = DateTime.Now;
                job.LastStatus = "Completed Successfully";
                
                if (job.IsEnabled)
                {
                    ScheduleJob(job);
                }
            }
            catch (Exception ex)
            {
                job.LastRun = DateTime.Now;
                job.LastStatus = "Failed: " + ex.Message;
                
                if (job.IsEnabled)
                {
                    ScheduleJob(job);
                }
            }
        }

        private void ExecuteJob(SyncJob job)
        {
            // Basic local file copy implementation
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
                    return job.IntervalValue * 60 * 1000;
            }
        }
        
        public void StartScheduler()
        {
            try
            {
                foreach (var job in _jobs)
                {
                    if (job.IsEnabled && job.IntervalValue > 0 && !string.IsNullOrEmpty(job.IntervalType))
                    {
                        ScheduleJob(job);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Failed to start scheduler: " + ex.Message);
            }
        }
        
        public void StopScheduler()
        {
            try
            {
                foreach (var kvp in _jobTimers)
                {
                    try
                    {
                        kvp.Value.Stop();
                        kvp.Value.Dispose();
                    }
                    catch { }
                }
                _jobTimers.Clear();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Failed to stop scheduler: " + ex.Message);
            }
        }
    }

    // Enhanced connection service with persistent storage
    public class ConnectionService : IConnectionService
    {
        private ConnectionSettings _settings;

        public ConnectionService()
        {
            // Try to load default connection on startup
            _settings = LoadDefaultConnectionFromRegistry() ?? new ConnectionSettings();
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

        // Enhanced connection management methods
        public bool SaveConnection(string connectionName, ConnectionSettings settings, bool setAsDefault = false)
        {
            if (StringExtensions.IsNullOrWhiteSpace(connectionName) || settings == null)
                return false;

            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\DataSyncer\\Connections"))
                {
                    using (Microsoft.Win32.RegistryKey connectionKey = key.CreateSubKey(connectionName))
                    {
                        connectionKey.SetValue("Protocol", settings.Protocol ?? "LOCAL");
                        connectionKey.SetValue("ProtocolType", settings.ProtocolType);
                        connectionKey.SetValue("Host", settings.Host ?? "");
                        connectionKey.SetValue("Port", settings.Port);
                        connectionKey.SetValue("Username", settings.Username ?? "");
                        connectionKey.SetValue("SshKeyPath", settings.SshKeyPath ?? "");
                        connectionKey.SetValue("Timeout", settings.Timeout);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ConnectionSettings GetConnection(string connectionName)
        {
            if (StringExtensions.IsNullOrWhiteSpace(connectionName))
                return null;

            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\DataSyncer\\Connections"))
                {
                    if (key != null)
                    {
                        using (Microsoft.Win32.RegistryKey connectionKey = key.OpenSubKey(connectionName))
                        {
                            if (connectionKey != null)
                            {
                                var settings = new ConnectionSettings
                                {
                                    Protocol = connectionKey.GetValue("Protocol", "LOCAL").ToString(),
                                    ProtocolType = Convert.ToInt32(connectionKey.GetValue("ProtocolType", 0)),
                                    Host = connectionKey.GetValue("Host", "").ToString(),
                                    Port = Convert.ToInt32(connectionKey.GetValue("Port", 21)),
                                    Username = connectionKey.GetValue("Username", "").ToString(),
                                    SshKeyPath = connectionKey.GetValue("SshKeyPath", "").ToString(),
                                    Timeout = Convert.ToInt32(connectionKey.GetValue("Timeout", 30))
                                };
                                return settings;
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        public List<SavedConnection> GetAllConnections()
        {
            var connections = new List<SavedConnection>();
            
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\DataSyncer\\Connections"))
                {
                    if (key != null)
                    {
                        foreach (string connectionName in key.GetSubKeyNames())
                        {
                            var settings = GetConnection(connectionName);
                            if (settings != null)
                            {
                                connections.Add(new SavedConnection
                                {
                                    Name = connectionName,
                                    Settings = settings,
                                    CreatedDate = DateTime.Now,
                                    LastUsed = DateTime.Now,
                                    IsDefault = false
                                });
                            }
                        }
                    }
                }
            }
            catch { }

            return connections;
        }

        public ConnectionSettings GetDefaultConnection()
        {
            var connections = GetAllConnections();
            if (connections.Count > 0)
            {
                return connections[0].Settings;
            }
            return LoadDefaultConnectionFromRegistry();
        }

        public bool SetDefaultConnection(string connectionName)
        {
            var connection = GetConnection(connectionName);
            if (connection != null)
            {
                _settings = connection;
                return true;
            }
            return false;
        }

        public bool DeleteConnection(string connectionName)
        {
            if (StringExtensions.IsNullOrWhiteSpace(connectionName))
                return false;

            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\DataSyncer\\Connections", true))
                {
                    if (key != null)
                    {
                        key.DeleteSubKey(connectionName, false);
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        public bool ConnectionExists(string connectionName)
        {
            return GetConnection(connectionName) != null;
        }

        public List<string> GetConnectionNames()
        {
            var names = new List<string>();
            
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\DataSyncer\\Connections"))
                {
                    if (key != null)
                    {
                        foreach (string connectionName in key.GetSubKeyNames())
                        {
                            names.Add(connectionName);
                        }
                    }
                }
            }
            catch { }

            return names;
        }

        public ConnectionSettings LoadConnectionForStartup()
        {
            var defaultConnection = GetDefaultConnection();
            if (defaultConnection != null)
            {
                _settings = defaultConnection;
                return defaultConnection;
            }
            return _settings;
        }

        private ConnectionSettings LoadDefaultConnectionFromRegistry()
        {
            var connections = GetAllConnections();
            if (connections.Count > 0)
            {
                return connections[0].Settings;
            }
            return null;
        }
    }

    // Stub implementation of filter service
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
            return new string[] { "*.txt", "*.doc", "*.docx", "*.pdf", "*.xls", "*.xlsx" };
        }
    }

    // Simple log entry class for the UI project
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string JobName { get; set; }
    }

    // Stub implementation of log service
    public class LogService : ILogService
    {
        private List<LogEntry> _logs;

        public LogService()
        {
            _logs = new List<LogEntry>();
        }

        public DataTable GetLogs()
        {
            var dt = new DataTable();
            dt.Columns.Add("Timestamp", typeof(DateTime));
            dt.Columns.Add("Level", typeof(string));
            dt.Columns.Add("Message", typeof(string));
            
            foreach (var log in _logs)
            {
                dt.Rows.Add(log.Timestamp, log.Level, log.Message);
            }
            
            return dt;
        }

        public DataTable GetLogs(DateTime? fromDate, DateTime? toDate, string logLevel)
        {
            var filteredLogs = _logs.AsEnumerable();
            
            if (fromDate.HasValue)
                filteredLogs = filteredLogs.Where(l => l.Timestamp >= fromDate.Value);
                
            if (toDate.HasValue)
                filteredLogs = filteredLogs.Where(l => l.Timestamp <= toDate.Value);
                
            if (!string.IsNullOrEmpty(logLevel))
                filteredLogs = filteredLogs.Where(l => l.Level == logLevel);
            
            var dt = new DataTable();
            dt.Columns.Add("Timestamp", typeof(DateTime));
            dt.Columns.Add("Level", typeof(string));
            dt.Columns.Add("Message", typeof(string));
            
            foreach (var log in filteredLogs)
            {
                dt.Rows.Add(log.Timestamp, log.Level, log.Message);
            }
            
            return dt;
        }

        public bool ClearLogs()
        {
            _logs.Clear();
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
                var filteredLogs = _logs.AsEnumerable();
                
                if (fromDate.HasValue)
                    filteredLogs = filteredLogs.Where(l => l.Timestamp >= fromDate.Value);
                    
                if (toDate.HasValue)
                    filteredLogs = filteredLogs.Where(l => l.Timestamp <= toDate.Value);
                
                using (var writer = new System.IO.StreamWriter(filePath))
                {
                    writer.WriteLine("Timestamp,Level,Message,JobName");
                    foreach (var log in filteredLogs)
                    {
                        writer.WriteLine("{0},{1},{2},{3}", 
                            log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                            log.Level,
                            log.Message?.Replace(",", ";"),
                            log.JobName ?? "");
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void LogInfo(string message)
        {
            LogInfo(message, null);
        }

        public void LogInfo(string message, string jobName)
        {
            _logs.Add(new LogEntry { Timestamp = DateTime.Now, Level = "INFO", Message = message, JobName = jobName });
            System.Console.WriteLine("INFO: " + message);
        }

        public void LogWarning(string message)
        {
            LogWarning(message, null);
        }

        public void LogWarning(string message, string jobName)
        {
            _logs.Add(new LogEntry { Timestamp = DateTime.Now, Level = "WARNING", Message = message, JobName = jobName });
            System.Console.WriteLine("WARNING: " + message);
        }

        public void LogError(string message)
        {
            LogError(message, null);
        }

        public void LogError(string message, string jobName)
        {
            _logs.Add(new LogEntry { Timestamp = DateTime.Now, Level = "ERROR", Message = message, JobName = jobName });
            System.Console.WriteLine("ERROR: " + message);
        }
    }

    // Stub implementation of service manager
    public class ServiceManager : IServiceManager
    {
        private SyncJobService _jobService;

        public ServiceManager()
        {
            _jobService = new SyncJobService();
        }

        public bool StartService()
        {
            try
            {
                _jobService.StartScheduler();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool StopService()
        {
            try
            {
                _jobService.StopScheduler();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RestartService()
        {
            return StopService() && StartService();
        }

        public string GetServiceStatus()
        {
            return "Running";
        }

        public bool IsServiceRunning()
        {
            return true;
        }

        public void Dispose()
        {
            try
            {
                StopService();
                _jobService = null;
            }
            catch { }
        }
    }

    // Stub implementation of configuration service
    public class ConfigurationService : IConfigurationService
    {
        private Dictionary<string, object> _settings;

        public ConfigurationService()
        {
            _settings = new Dictionary<string, object>();
        }

        public T GetSetting<T>(string key, T defaultValue)
        {
            if (_settings.ContainsKey(key))
            {
                try
                {
                    return (T)_settings[key];
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
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteSetting(string key)
        {
            return _settings.Remove(key);
        }

        public void SaveAllSettings()
        {
            // Implementation would save to registry or file
        }

        public void LoadAllSettings()
        {
            // Implementation would load from registry or file
        }

        public string GetConfigurationValue(string key)
        {
            return GetSetting<string>(key, "");
        }

        public bool SetConfigurationValue(string key, string value)
        {
            return SaveSetting<string>(key, value);
        }

        public bool SaveConfiguration()
        {
            SaveAllSettings();
            return true;
        }

        public bool LoadConfiguration()
        {
            LoadAllSettings();
            return true;
        }
    }
}
