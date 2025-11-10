using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using FTPSyncer.core;

namespace FTPSyncer.core.Services
{
    /// <summary>
    /// Enhanced logging service with log rotation, filtering, and multiple output capabilities
    /// </summary>
    public class EnhancedLogService : ILogService
    {
        private readonly string _logFolder;
        private string _logFile;
        private string _currentLogDate; // Track current date for daily rotation
        private readonly int _maxLogSizeKB;
        private readonly int _keepLogDays;
        private readonly LogLevel _minLevel;
        private readonly object _logLock = new object();
        private readonly List<LogEntry> _recentLogs;
        private readonly int _maxRecentLogs;
        private readonly bool _enableConsoleOutput;
        
        public event EventHandler<LogEntryEventArgs> LogEntryAdded;

        public EnhancedLogService(string logFolder, int maxLogSizeKB = 5120, int keepLogDays = 30, 
                                LogLevel minLevel = LogLevel.Info, bool enableConsoleOutput = false)
        {
            _logFolder = logFolder ?? throw new ArgumentNullException("logFolder");
            _maxLogSizeKB = maxLogSizeKB;
            _keepLogDays = keepLogDays;
            _minLevel = minLevel;
            _enableConsoleOutput = enableConsoleOutput;
            _recentLogs = new List<LogEntry>();
            _maxRecentLogs = 1000; // Keep last 1000 log entries in memory
            
            // Create log file name with date
            _currentLogDate = DateTime.Now.ToString("yyyyMMdd");
            _logFile = Path.Combine(_logFolder, string.Format("syncer_{0}.log", _currentLogDate));
            
            EnsureLogFolder();
            PerformLogMaintenance();
            
            LogInfo("Logging service started", "LogService");
        }

        /// <summary>
        /// Log informational message
        /// </summary>
        public void LogInfo(string message, string source = null)
        {
            if (_minLevel <= LogLevel.Info)
                LogMessage(LogLevel.Info, message, source);
        }

        /// <summary>
        /// Log informational message with job ID
        /// </summary>
        public void LogInfo(string message, string source, string jobId)
        {
            if (_minLevel <= LogLevel.Info)
                LogMessage(LogLevel.Info, message, source, null, jobId);
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public void LogWarning(string message, string source = null)
        {
            if (_minLevel <= LogLevel.Warning)
                LogMessage(LogLevel.Warning, message, source);
        }

        /// <summary>
        /// Log warning message with job ID
        /// </summary>
        public void LogWarning(string message, string source, string jobId)
        {
            if (_minLevel <= LogLevel.Warning)
                LogMessage(LogLevel.Warning, message, source, null, jobId);
        }

        /// <summary>
        /// Log error message
        /// </summary>
        public void LogError(string message, string source = null)
        {
            if (_minLevel <= LogLevel.Error)
                LogMessage(LogLevel.Error, message, source, null);
        }

        /// <summary>
        /// Log error message with job ID
        /// </summary>
        public void LogError(string message, string source, string jobId)
        {
            if (_minLevel <= LogLevel.Error)
                LogMessage(LogLevel.Error, message, source, null, jobId);
        }

        /// <summary>
        /// Log error message with exception details
        /// </summary>
        public void LogError(string message, Exception ex, string source = null)
        {
            if (_minLevel <= LogLevel.Error)
            {
                string fullMessage = message;
                if (ex != null)
                {
                    fullMessage += $" Exception: {ex.Message}";
                    if (ex.StackTrace != null)
                        fullMessage += $" Stack trace: {ex.StackTrace}";
                }
                LogMessage(LogLevel.Error, fullMessage, source, ex);
            }
        }

        /// <summary>
        /// Log job start event
        /// </summary>
        public void LogJobStart(SyncJob job)
        {
            if (job == null) return;
            string message = string.Format("Job '{0}' started. Source: {1}, Destination: {2}", 
                job.Name, job.SourcePath, job.DestinationPath);
            LogMessage(LogLevel.Info, message, "JobManager", null, job.Id);
        }

        /// <summary>
        /// Log job progress event
        /// </summary>
        public void LogJobProgress(SyncJob job, string message)
        {
            if (job == null) return;
            string fullMessage = string.Format("Job '{0}' progress: {1}", job.Name, message);
            LogMessage(LogLevel.Info, fullMessage, "JobManager", null, job.Id);
        }

        /// <summary>
        /// Log job success event
        /// </summary>
        public void LogJobSuccess(SyncJob job, string message)
        {
            if (job == null) return;
            string fullMessage = string.Format("Job '{0}' completed successfully: {1}", job.Name, message);
            LogMessage(LogLevel.Info, fullMessage, "JobManager", null, job.Id);
        }

        /// <summary>
        /// Log job error event
        /// </summary>
        public void LogJobError(SyncJob job, string message, Exception ex = null)
        {
            if (job == null) return;
            
            string fullMessage = string.Format("Job '{0}' error: {1}", job.Name, message);
            LogMessage(LogLevel.Error, fullMessage, "JobManager", ex, job.Id);
        }

        /// <summary>
        /// Get recent logs filtered by criteria
        /// </summary>
        public List<LogEntry> GetRecentLogs(
            LogLevel minLevel = LogLevel.Info, 
            string sourceFilter = null, 
            DateTime? fromTime = null, 
            DateTime? toTime = null,
            int maxResults = 100)
        {
            lock (_logLock)
            {
                var query = _recentLogs.Where(l => l.Level >= minLevel);
                
                if (!string.IsNullOrEmpty(sourceFilter))
                    query = query.Where(l => l.Source != null && l.Source.Contains(sourceFilter));
                
                if (fromTime.HasValue)
                    query = query.Where(l => l.Timestamp >= fromTime.Value);
                    
                if (toTime.HasValue)
                    query = query.Where(l => l.Timestamp <= toTime.Value);
                
                return query.OrderByDescending(l => l.Timestamp)
                           .Take(maxResults)
                           .ToList();
            }
        }

        /// <summary>
        /// Core logging method
        /// </summary>
        private void LogMessage(LogLevel level, string message, string source, Exception ex = null, string jobId = null)
        {
            if (string.IsNullOrEmpty(message))
                return;
                
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logSource = string.IsNullOrEmpty(source) ? "System" : source;
            string threadId = Thread.CurrentThread.ManagedThreadId.ToString();
            
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Source = logSource,
                ThreadId = threadId,
                Exception = ex,
                JobId = jobId
            };
            
            string formattedMessage = string.Format("{0} [{1}] [{2}] [{3}] {4}", 
                timestamp, level, threadId, logSource, message);
            
            lock (_logLock)
            {
                try
                {
                    // Write to log file
                    WriteToLogFile(formattedMessage);
                    
                    // Add to recent logs with limit
                    _recentLogs.Add(logEntry);
                    if (_recentLogs.Count > _maxRecentLogs)
                        _recentLogs.RemoveAt(0);
                        
                    // Console output if enabled
                    if (_enableConsoleOutput)
                    {
                        ConsoleColor originalColor = Console.ForegroundColor;
                        
                        switch (level)
                        {
                            case LogLevel.Error:
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case LogLevel.Warning:
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                break;
                            case LogLevel.Info:
                                Console.ForegroundColor = ConsoleColor.White;
                                break;
                            case LogLevel.Debug:
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                        }
                        
                        Console.WriteLine(formattedMessage);
                        Console.ForegroundColor = originalColor;
                    }
                    
                    // Trigger event
                    OnLogEntryAdded(new LogEntryEventArgs 
                    { 
                        Timestamp = logEntry.Timestamp,
                        Level = logEntry.Level.ToString(),
                        Source = logEntry.Source,
                        Message = logEntry.Message,
                        JobName = logEntry.JobId,
                        Exception = logEntry.Exception
                    });
                }
                catch (Exception e)
                {
                    // Last resort - can't log an error about logging
                    if (_enableConsoleOutput)
                        Console.WriteLine($"Error in logging system: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Write a message to the log file, handling rotation if needed
        /// </summary>
        private void WriteToLogFile(string message)
        {
            try
            {
                // Check if date has changed - create new file for new day
                string currentDate = DateTime.Now.ToString("yyyyMMdd");
                if (_currentLogDate != currentDate)
                {
                    _currentLogDate = currentDate;
                    _logFile = Path.Combine(_logFolder, string.Format("syncer_{0}.log", _currentLogDate));
                    
                    // Log the rotation
                    string rotationMessage = string.Format("{0} [INFO] [LogService] New day - switched to new log file: {1}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), _logFile);
                    
                    // Ensure directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(_logFile));
                    
                    // Create new file with header
                    using (StreamWriter writer = new StreamWriter(_logFile, true, Encoding.UTF8))
                    {
                        writer.WriteLine(rotationMessage);
                        writer.WriteLine(message);
                    }
                    return;
                }
                
                // Check if we need to rotate log file based on size
                if (File.Exists(_logFile))
                {
                    var fileInfo = new FileInfo(_logFile);
                    if (fileInfo.Length / 1024 > _maxLogSizeKB)
                    {
                        RotateLogFile();
                    }
                }

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(_logFile));
                
                // Append to log file
                using (StreamWriter writer = new StreamWriter(_logFile, true, Encoding.UTF8))
                {
                    writer.WriteLine(message);
                }
            }
            catch
            {
                // Silently fail - can't log an error about logging
            }
        }

        /// <summary>
        /// Rotate the log file by renaming it with a timestamp
        /// </summary>
        private void RotateLogFile()
        {
            try
            {
                if (!File.Exists(_logFile))
                    return;
                    
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string directory = Path.GetDirectoryName(_logFile);
                string filename = Path.GetFileNameWithoutExtension(_logFile);
                string extension = Path.GetExtension(_logFile);
                string newPath = Path.Combine(directory, $"{filename}_{timestamp}{extension}");
                
                File.Move(_logFile, newPath);
            }
            catch
            {
                // Silently fail - can't log an error about logging
            }
        }

        /// <summary>
        /// Ensure the log folder exists
        /// </summary>
        private void EnsureLogFolder()
        {
            try
            {
                if (!Directory.Exists(_logFolder))
                    Directory.CreateDirectory(_logFolder);
            }
            catch
            {
                // Silently fail - can't log an error about logging
            }
        }

        /// <summary>
        /// Delete old log files based on retention policy
        /// </summary>
        private void PerformLogMaintenance()
        {
            try
            {
                if (!Directory.Exists(_logFolder))
                    return;
                    
                var cutoffDate = DateTime.Now.AddDays(-_keepLogDays);
                var files = Directory.GetFiles(_logFolder, "syncer_*.log");
                
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                            // Silently continue with other files
                        }
                    }
                }
            }
            catch
            {
                // Silently fail - can't log an error about logging
            }
        }
        
        /// <summary>
        /// Log transfer event
        /// </summary>
        public void LogTransfer(string jobName, string fileName, long fileSize, bool success, string error)
        {
            string level = success ? "INFO" : "ERROR";
            string message = success 
                ? string.Format("Transfer completed: {0} ({1} bytes)", fileName, fileSize)
                : string.Format("Transfer failed: {0} - {1}", fileName, error);
            
            if (success)
                LogInfo(message, jobName);
            else
                LogError(message, jobName);
        }

        #region Real-Time Logging Support
        
        private bool _realTimeLoggingEnabled = false;
        private string _realTimeLogPath = null;
        private string _realTimeLogBasePath = null; // Base path without date for daily rotation
        private string _realTimeLogDate = null; // Current date for real-time log
        private StreamWriter _realTimeCsvWriter = null;
        private readonly object _realTimeLock = new object();
        
        public event EventHandler<LogEntryEventArgs> RealTimeLogEntry;

        /// <summary>
        /// Enable real-time logging to custom directory/file
        /// </summary>
        public void EnableRealTimeLogging(string customFilePath)
        {
            lock (_realTimeLock)
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

                    // Store base path and add date to filename
                    _realTimeLogBasePath = customFilePath;
                    _realTimeLogDate = DateTime.Now.ToString("yyyyMMdd");
                    
                    // Insert date before file extension
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(customFilePath);
                    string extension = Path.GetExtension(customFilePath);
                    string directoryPath = Path.GetDirectoryName(customFilePath);
                    _realTimeLogPath = Path.Combine(directoryPath, 
                        string.Format("{0}_{1}{2}", fileNameWithoutExt, _realTimeLogDate, extension));
                    
                    // Initialize log writer (plain text format)
                    _realTimeCsvWriter = new StreamWriter(_realTimeLogPath, true, Encoding.UTF8);
                    
                    // Write header if file is new/empty
                    if (new FileInfo(_realTimeLogPath).Length == 0)
                    {
                        _realTimeCsvWriter.WriteLine("=== Log File Started: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ===");
                        _realTimeCsvWriter.Flush();
                    }
                    
                    _realTimeLoggingEnabled = true;
                    
                    LogInfo("Real-time logging enabled to: " + _realTimeLogPath, "EnhancedLogService");
                }
                catch (Exception ex)
                {
                    _realTimeLoggingEnabled = false;
                    if (_realTimeCsvWriter != null)
                    {
                        try { _realTimeCsvWriter.Close(); } catch { }
                        _realTimeCsvWriter = null;
                    }
                    throw new InvalidOperationException("Failed to enable real-time logging: " + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Disable real-time logging
        /// </summary>
        public void DisableRealTimeLogging()
        {
            lock (_realTimeLock)
            {
                if (_realTimeLoggingEnabled)
                {
                    LogInfo("Real-time logging disabled", "EnhancedLogService");
                    _realTimeLoggingEnabled = false;
                }

                if (_realTimeCsvWriter != null)
                {
                    try
                    {
                        _realTimeCsvWriter.Flush();
                        _realTimeCsvWriter.Close();
                    }
                    catch
                    {
                        // Ignore errors during cleanup
                    }
                    finally
                    {
                        _realTimeCsvWriter = null;
                    }
                }

                _realTimeLogPath = null;
            }
        }

        /// <summary>
        /// Check if real-time logging is enabled
        /// </summary>
        public bool IsRealTimeLoggingEnabled()
        {
            lock (_realTimeLock)
            {
                return _realTimeLoggingEnabled;
            }
        }

        /// <summary>
        /// Get current real-time log path
        /// </summary>
        public string GetRealTimeLogPath()
        {
            lock (_realTimeLock)
            {
                return _realTimeLogPath;
            }
        }

        #endregion
        
        protected virtual void OnLogEntryAdded(LogEntryEventArgs e)
        {
            LogEntryAdded?.Invoke(this, e);
            
            // Also trigger real-time logging if enabled
            if (_realTimeLoggingEnabled)
            {
                // Create LogEntry from EventArgs for real-time logging
                var logEntry = new LogEntry
                {
                    Timestamp = e.Timestamp,
                    Level = (LogLevel)Enum.Parse(typeof(LogLevel), e.Level),
                    Source = e.Source,
                    Message = e.Message,
                    Exception = e.Exception,
                    JobId = e.JobName
                };
                WriteToRealTimeLog(logEntry);
                OnRealTimeLogEntry(e);
            }
        }
        
        private void WriteToRealTimeLog(LogEntry logEntry)
        {
            if (!_realTimeLoggingEnabled || _realTimeCsvWriter == null)
                return;

            lock (_realTimeLock)
            {
                try
                {
                    // Check if date has changed - create new file for new day
                    string currentDate = DateTime.Now.ToString("yyyyMMdd");
                    if (_realTimeLogDate != currentDate && !string.IsNullOrEmpty(_realTimeLogBasePath))
                    {
                        // Close current writer
                        if (_realTimeCsvWriter != null)
                        {
                            _realTimeCsvWriter.WriteLine("=== Log File Ended: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " (New day started) ===");
                            _realTimeCsvWriter.Flush();
                            _realTimeCsvWriter.Close();
                            _realTimeCsvWriter.Dispose();
                        }
                        
                        // Update date and create new file path
                        _realTimeLogDate = currentDate;
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(_realTimeLogBasePath);
                        string extension = Path.GetExtension(_realTimeLogBasePath);
                        string directoryPath = Path.GetDirectoryName(_realTimeLogBasePath);
                        _realTimeLogPath = Path.Combine(directoryPath, 
                            string.Format("{0}_{1}{2}", fileNameWithoutExt, _realTimeLogDate, extension));
                        
                        // Create new writer
                        _realTimeCsvWriter = new StreamWriter(_realTimeLogPath, true, Encoding.UTF8);
                        _realTimeCsvWriter.WriteLine("=== Log File Started: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ===");
                        _realTimeCsvWriter.Flush();
                    }
                    
                    // Write in plain text format instead of CSV
                    string logLine = string.Format("[{0}] [{1}] {2}: {3}",
                        logEntry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        logEntry.Level.ToString().ToUpper(),
                        string.IsNullOrEmpty(logEntry.Source) ? "General" : logEntry.Source,
                        logEntry.Message ?? ""
                    );
                    
                    _realTimeCsvWriter.WriteLine(logLine);
                    
                    // If there's an exception, write it on the next line with indentation
                    if (logEntry.Exception != null)
                    {
                        _realTimeCsvWriter.WriteLine("    Exception: " + logEntry.Exception.ToString());
                    }
                    
                    _realTimeCsvWriter.Flush(); // Ensure immediate write for real-time
                }
                catch
                {
                    // Silently fail - don't break main logging
                }
            }
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
    }

    /// <summary>
    /// Represents a log entry
    /// </summary>
    /// <summary>
    /// Represents a log entry
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string ThreadId { get; set; }
        public Exception Exception { get; set; }
        public string JobId { get; set; }
    }

    /// <summary>
    /// Log level enum
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }
}





