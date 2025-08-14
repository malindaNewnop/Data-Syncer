using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace syncer.ui.Services
{
    // Stub implementation of sync job service - will be replaced with actual backend implementation
    public class SyncJobService : ISyncJobService
    {
        private List<SyncJob> _jobs;

        public SyncJobService()
        {
            _jobs = new List<SyncJob>();
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
            return job.Id;
        }

        public bool UpdateJob(SyncJob job)
        {
            SyncJob existing = GetJobById(job.Id);
            if (existing == null) return false;
            int idx = _jobs.IndexOf(existing);
            _jobs[idx] = job;
            return true;
        }

        public bool DeleteJob(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return false;
            _jobs.Remove(job);
            return true;
        }

        public bool StartJob(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return false;
            job.IsEnabled = true;
            return true;
        }

        public bool StopJob(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return false;
            job.IsEnabled = false;
            return true;
        }

        public string GetJobStatus(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return "Not Found";
            return job.IsEnabled ? "Enabled" : "Disabled";
        }
    }

    // Stub implementation of connection service - will be replaced with actual backend implementation
    public class ConnectionService : IConnectionService
    {
        private ConnectionSettings _settings;

        public ConnectionService()
        {
            _settings = new ConnectionSettings();
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
    }

    // Stub implementation of configuration service - will be replaced with actual backend implementation
    public class ConfigurationService : IConfigurationService
    {
        private Dictionary<string, object> _settings = new Dictionary<string, object>();
        public ConfigurationService() { LoadAllSettings(); }
        public T GetSetting<T>(string key, T defaultValue)
        {
            if (_settings.ContainsKey(key))
            {
                try { return (T)_settings[key]; } catch { return defaultValue; }
            }
            return defaultValue;
        }
        public bool SaveSetting<T>(string key, T value) { _settings[key] = value; return true; }
        public bool DeleteSetting(string key) { return _settings.Remove(key); }
        public void SaveAllSettings() { }
        public void LoadAllSettings() { }
    }
}
