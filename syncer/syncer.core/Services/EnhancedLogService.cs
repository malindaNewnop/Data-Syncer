using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace syncer.core.Services
{
    /// <summary>
    /// Enhanced logging service with log rotation, filtering, and multiple output capabilities
    /// </summary>
    public class EnhancedLogService : ILogService
    {
        private readonly string _logFolder;
        private readonly string _logFile;
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
            string dateStr = DateTime.Now.ToString("yyyyMMdd");
            _logFile = Path.Combine(_logFolder, $"syncer_{dateStr}.log");
            
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
        /// Log warning message
        /// </summary>
        public void LogWarning(string message, string source = null)
        {
            if (_minLevel <= LogLevel.Warning)
                LogMessage(LogLevel.Warning, message, source);
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
            LogInfo($"Job '{job.Name}' started. Source: {job.SourcePath}, Destination: {job.DestinationPath}", "JobManager");
        }

        /// <summary>
        /// Log job progress event
        /// </summary>
        public void LogJobProgress(SyncJob job, string message)
        {
            if (job == null) return;
            LogInfo($"Job '{job.Name}' progress: {message}", "JobManager");
        }

        /// <summary>
        /// Log job success event
        /// </summary>
        public void LogJobSuccess(SyncJob job, string message)
        {
            if (job == null) return;
            LogInfo($"Job '{job.Name}' completed successfully: {message}", "JobManager");
        }

        /// <summary>
        /// Log job error event
        /// </summary>
        public void LogJobError(SyncJob job, string message, Exception ex = null)
        {
            if (job == null) return;
            
            if (ex != null)
                LogError($"Job '{job.Name}' error: {message}", ex, "JobManager");
            else
                LogError($"Job '{job.Name}' error: {message}", "JobManager");
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
        private void LogMessage(LogLevel level, string message, string source, Exception ex = null)
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
                Exception = ex
            };
            
            string formattedMessage = $"{timestamp} [{level}] [{threadId}] [{logSource}] {message}";
            
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
                    OnLogEntryAdded(new LogEntryEventArgs { LogEntry = logEntry });
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
                // Check if we need to rotate log file
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
        
        protected virtual void OnLogEntryAdded(LogEntryEventArgs e)
        {
            LogEntryAdded?.Invoke(this, e);
        }
    }

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
    }

    /// <summary>
    /// Event arguments for log entry events
    /// </summary>
    public class LogEntryEventArgs : EventArgs
    {
        public LogEntry LogEntry { get; set; }
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
