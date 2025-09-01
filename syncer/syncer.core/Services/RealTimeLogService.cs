using System;
using System.IO;
using System.Data;
using System.Threading;
using System.Globalization;

namespace syncer.core.Services
{
    /// <summary>
    /// Real-time logging service with CSV output capability for custom directories
    /// Compatible with .NET Framework 3.5
    /// </summary>
    public class RealTimeLogService : ILogService
    {
        private readonly object _syncLock = new object();
        private ILogService _baseLogService;
        private string _customLogFilePath;
        private bool _realTimeLoggingEnabled;
        private StreamWriter _csvWriter;
        private readonly string _dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
        
        public event EventHandler<LogEntryEventArgs> RealTimeLogEntry;

        public RealTimeLogService(ILogService baseLogService)
        {
            _baseLogService = baseLogService ?? throw new ArgumentNullException("baseLogService");
            _realTimeLoggingEnabled = false;
        }

        #region ILogService Implementation

        public void LogInfo(string message, string source = null)
        {
            _baseLogService.LogInfo(message, source);
            WriteRealTimeLog("INFO", message, source ?? "System", null, null);
        }

        public void LogWarning(string message, string source = null)
        {
            _baseLogService.LogWarning(message, source);
            WriteRealTimeLog("WARNING", message, source ?? "System", null, null);
        }

        public void LogError(string message, string source = null)
        {
            _baseLogService.LogError(message, source);
            WriteRealTimeLog("ERROR", message, source ?? "System", null, null);
        }

        public void LogJobStart(SyncJob job)
        {
            _baseLogService.LogJobStart(job);
            WriteRealTimeLog("INFO", "Job started", "JobManager", job != null ? job.Name : null, null);
        }

        public void LogJobProgress(SyncJob job, string message)
        {
            _baseLogService.LogJobProgress(job, message);
            WriteRealTimeLog("INFO", message, "JobManager", job != null ? job.Name : null, null);
        }

        public void LogJobSuccess(SyncJob job, string message)
        {
            _baseLogService.LogJobSuccess(job, message);
            WriteRealTimeLog("INFO", message, "JobManager", job != null ? job.Name : null, null);
        }

        public void LogJobError(SyncJob job, string message, Exception ex = null)
        {
            _baseLogService.LogJobError(job, message, ex);
            WriteRealTimeLog("ERROR", message, "JobManager", job != null ? job.Name : null, ex);
        }

        public void LogTransfer(string jobName, string fileName, long fileSize, bool success, string error)
        {
            _baseLogService.LogTransfer(jobName, fileName, fileSize, success, error);
            string level = success ? "INFO" : "ERROR";
            string message = success 
                ? string.Format("File '{0}' transferred successfully ({1} bytes)", fileName, fileSize)
                : string.Format("File '{0}' transfer failed: {1}", fileName, error);
            WriteRealTimeLog(level, message, "Transfer", jobName, null);
        }

        #endregion

        #region Real-Time Logging Implementation

        public void EnableRealTimeLogging(string customFilePath)
        {
            lock (_syncLock)
            {
                try
                {
                    // Disable current logging if active
                    DisableRealTimeLogging();

                    if (string.IsNullOrEmpty(customFilePath))
                        throw new ArgumentException("Custom file path cannot be null or empty");

                    // Ensure directory exists
                    string directory = Path.GetDirectoryName(customFilePath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    _customLogFilePath = customFilePath;
                    
                    // Initialize CSV writer
                    _csvWriter = new StreamWriter(_customLogFilePath, true); // Append mode
                    
                    // Write CSV header if file is new/empty
                    if (new FileInfo(_customLogFilePath).Length == 0)
                    {
                        WriteCSVHeader();
                    }
                    
                    _realTimeLoggingEnabled = true;
                    
                    // Log the enabling of real-time logging
                    WriteRealTimeLog("INFO", "Real-time logging enabled to: " + _customLogFilePath, "RealTimeLogService", null, null);
                }
                catch (Exception ex)
                {
                    _realTimeLoggingEnabled = false;
                    if (_csvWriter != null)
                    {
                        try { _csvWriter.Close(); } catch { }
                        _csvWriter = null;
                    }
                    throw new InvalidOperationException("Failed to enable real-time logging: " + ex.Message, ex);
                }
            }
        }

        public void DisableRealTimeLogging()
        {
            lock (_syncLock)
            {
                if (_realTimeLoggingEnabled)
                {
                    WriteRealTimeLog("INFO", "Real-time logging disabled", "RealTimeLogService", null, null);
                    _realTimeLoggingEnabled = false;
                }

                if (_csvWriter != null)
                {
                    try
                    {
                        _csvWriter.Flush();
                        _csvWriter.Close();
                    }
                    catch
                    {
                        // Ignore errors during cleanup
                    }
                    finally
                    {
                        _csvWriter = null;
                    }
                }

                _customLogFilePath = null;
            }
        }

        public bool IsRealTimeLoggingEnabled()
        {
            lock (_syncLock)
            {
                return _realTimeLoggingEnabled;
            }
        }

        public string GetRealTimeLogPath()
        {
            lock (_syncLock)
            {
                return _customLogFilePath;
            }
        }

        #endregion

        #region Private Helper Methods

        private void WriteCSVHeader()
        {
            try
            {
                _csvWriter.WriteLine("Timestamp,Level,Source,JobName,Message,Exception");
                _csvWriter.Flush();
            }
            catch
            {
                // Ignore header write errors
            }
        }

        private void WriteRealTimeLog(string level, string message, string source, string jobName, Exception ex)
        {
            if (!_realTimeLoggingEnabled || _csvWriter == null)
                return;

            lock (_syncLock)
            {
                try
                {
                    DateTime timestamp = DateTime.Now;
                    
                    // Format CSV line
                    string csvLine = string.Format("{0},{1},{2},{3},{4},{5}",
                        EscapeCSVField(timestamp.ToString(_dateFormat)),
                        EscapeCSVField(level ?? ""),
                        EscapeCSVField(source ?? ""),
                        EscapeCSVField(jobName ?? ""),
                        EscapeCSVField(message ?? ""),
                        EscapeCSVField(ex != null ? ex.ToString() : "")
                    );

                    _csvWriter.WriteLine(csvLine);
                    _csvWriter.Flush(); // Ensure immediate write for real-time

                    // Raise event for UI updates
                    OnRealTimeLogEntry(new LogEntryEventArgs
                    {
                        Timestamp = timestamp,
                        Level = level,
                        Source = source,
                        Message = message,
                        JobName = jobName,
                        Exception = ex
                    });
                }
                catch
                {
                    // Silently fail - don't break main logging
                }
            }
        }

        private string EscapeCSVField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // Escape quotes and handle special characters
            if (field.Contains("\"") || field.Contains(",") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }

        protected virtual void OnRealTimeLogEntry(LogEntryEventArgs e)
        {
            EventHandler<LogEntryEventArgs> handler = RealTimeLogEntry;
            if (handler != null)
            {
                try
                {
                    handler(this, e);
                }
                catch
                {
                    // Don't let event handler exceptions break logging
                }
            }
        }

        #endregion

        #region IDisposable Pattern

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisableRealTimeLogging();
                }
                _disposed = true;
            }
        }

        ~RealTimeLogService()
        {
            Dispose(false);
        }

        #endregion
    }
}
