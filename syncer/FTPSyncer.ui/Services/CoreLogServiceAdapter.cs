using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using FTPSyncer.core;

namespace FTPSyncer.ui.Services
{
    /// <summary>
    /// Adapter to connect UI LogService interface to Core LogService implementation
    /// </summary>
    public class CoreLogServiceAdapter : ILogService
    {
        private readonly FTPSyncer.core.FileLogService _coreLogService;
        
        // Real-time logging support
        private readonly object _realTimeLock = new object();
        private bool _realTimeLoggingEnabled = false;
        private string _realTimeLogPath = null;
        private StreamWriter _realTimeCsvWriter = null;
        
        public event EventHandler<LogEntryEventArgs> RealTimeLogEntry;

        public CoreLogServiceAdapter()
        {
            try
            {
                _coreLogService = new FTPSyncer.core.FileLogService();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetLogs()
        {
            // Get all logs from the last 30 days
            return _coreLogService.GetLogs(DateTime.Now.AddDays(-30), DateTime.Now, null);
        }

        public DataTable GetLogs(DateTime? fromDate, DateTime? toDate, string logLevel)
        {
            FTPSyncer.core.LogLevel? level = null;

            if (!string.IsNullOrEmpty(logLevel) && logLevel != "All")
            {
                if (logLevel == "INFO")
                    level = FTPSyncer.core.LogLevel.Info;
                else if (logLevel == "WARNING")
                    level = FTPSyncer.core.LogLevel.Warning;
                else if (logLevel == "ERROR")
                    level = FTPSyncer.core.LogLevel.Error;
            }

            return _coreLogService.GetLogs(
                fromDate ?? DateTime.Now.AddDays(-30),
                toDate ?? DateTime.Now,
                level);
        }

        public bool ClearLogs()
        {
            _coreLogService.Clear();
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
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath))
                {
                    writer.WriteLine("DateTime,Level,JobName,File,Status,Message");
                    foreach (DataRow row in logs.Rows)
                    {
                        string line = row["Timestamp"].ToString() + "," +
                                      EscapeCsvField(row["Level"].ToString()) + "," +
                                      EscapeCsvField(row["JobName"].ToString()) + "," +
                                      EscapeCsvField(row["FileName"].ToString()) + "," +
                                      EscapeCsvField(row["Source"].ToString()) + "," +
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
            if (string.IsNullOrEmpty(field)) return "";

            field = field.Replace("\"", "\"\"");
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field + "\"";
            }
            return field;
        }

        public void LogInfo(string message)
        {
            _coreLogService.Info(message, "UI");
            WriteToRealTimeLog("INFO", message);
        }

        public void LogInfo(string message, string jobName)
        {
            _coreLogService.Info(message, jobName);
            WriteToRealTimeLog("INFO", message, jobName);
        }

        public void LogInfo(string message, string source, string jobId)
        {
            _coreLogService.Info(message, source, jobId);
            WriteToRealTimeLog("INFO", message, source);
        }

        public void LogWarning(string message)
        {
            _coreLogService.Warning(message, "UI");
            WriteToRealTimeLog("WARNING", message);
        }

        public void LogWarning(string message, string jobName)
        {
            _coreLogService.Warning(message, jobName);
            WriteToRealTimeLog("WARNING", message, jobName);
        }

        public void LogWarning(string message, string source, string jobId)
        {
            _coreLogService.Warning(message, source, jobId);
            WriteToRealTimeLog("WARNING", message, source);
        }

        public void LogError(string message)
        {
            _coreLogService.Error(message, "UI", null);
            WriteToRealTimeLog("ERROR", message);
        }

        public void LogError(string message, string jobName)
        {
            _coreLogService.Error(message, jobName, null);
            WriteToRealTimeLog("ERROR", message, jobName);
        }

        public void LogError(string message, string source, string jobId)
        {
            _coreLogService.Error(message, source, null, jobId);
            WriteToRealTimeLog("ERROR", message, source);
        }

        // Real-time logging methods
        public void EnableRealTimeLogging(string csvFilePath)
        {
            lock (_realTimeLock)
            {
                try
                {
                    DisableRealTimeLogging();
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(csvFilePath));
                    
                    _realTimeCsvWriter = new StreamWriter(csvFilePath, true);
                    _realTimeLogPath = csvFilePath;
                    _realTimeLoggingEnabled = true;
                    
                    // Write CSV header if file is empty
                    FileInfo fileInfo = new FileInfo(csvFilePath);
                    if (fileInfo.Length == 0)
                    {
                        _realTimeCsvWriter.WriteLine("Timestamp,Level,Job,Source,Message,JobId,Exception,FileName,FileSize,Duration");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to enable real-time logging: " + ex.Message, ex);
                }
            }
        }

        public void DisableRealTimeLogging()
        {
            lock (_realTimeLock)
            {
                if (_realTimeCsvWriter != null)
                {
                    try
                    {
                        _realTimeCsvWriter.Close();
                        _realTimeCsvWriter.Dispose();
                    }
                    catch (Exception)
                    {
                        // Error closing file - continue
                    }
                    _realTimeCsvWriter = null;
                }
                
                _realTimeLoggingEnabled = false;
                _realTimeLogPath = null;
            }
        }

        public bool IsRealTimeLoggingEnabled()
        {
            return _realTimeLoggingEnabled;
        }

        public string GetRealTimeLogPath()
        {
            return _realTimeLogPath;
        }

        private void WriteToRealTimeLog(string level, string message, string jobName = "UI", string source = "core", Exception ex = null, string fileName = "", long fileSize = 0, double duration = 0)
        {
            if (!_realTimeLoggingEnabled || _realTimeCsvWriter == null) return;

            lock (_realTimeLock)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string csvLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                        EscapeCsvField(timestamp),
                        EscapeCsvField(level),
                        EscapeCsvField(jobName),
                        EscapeCsvField(source),
                        EscapeCsvField(message),
                        EscapeCsvField(""), // JobId - will be enhanced later
                        EscapeCsvField(ex != null ? ex.Message : ""),
                        EscapeCsvField(fileName),
                        fileSize.ToString(),
                        duration.ToString("F2"));
                    
                    _realTimeCsvWriter.WriteLine(csvLine);
                    _realTimeCsvWriter.Flush();
                    
                    // Fire event for UI updates
                    OnRealTimeLogEntry(new LogEntryEventArgs
                    {
                        Timestamp = DateTime.Now,
                        Level = level,
                        JobName = jobName,
                        Message = message,
                        Source = source,
                        Exception = ex
                    });
                }
                catch (Exception)
                {
                    // Disable real-time logging if we can't write
                    DisableRealTimeLogging();
                }
            }
        }

        private void OnRealTimeLogEntry(LogEntryEventArgs e)
        {
            EventHandler<LogEntryEventArgs> handler = RealTimeLogEntry;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #region Enhanced ILogService Interface Methods

        /// <summary>
        /// Enhanced job start logging with detailed information
        /// </summary>
        public void LogJobStart(SyncJob job)
        {
            if (_coreLogService != null && typeof(ILogService).IsAssignableFrom(_coreLogService.GetType()))
            {
                MethodInfo method = _coreLogService.GetType().GetMethod("LogJobStart");
                if (method != null)
                    method.Invoke(_coreLogService, new object[] { job });
            }
            
            string message = string.Format("Job started: {0} -> {1}", job.SourcePath, job.DestinationPath);
            WriteToRealTimeLog("INFO", message, job.Name, "JobManager");
        }

        /// <summary>
        /// Enhanced job progress logging
        /// </summary>
        public void LogJobProgress(SyncJob job, string message)
        {
            if (_coreLogService != null && typeof(ILogService).IsAssignableFrom(_coreLogService.GetType()))
            {
                MethodInfo method = _coreLogService.GetType().GetMethod("LogJobProgress");
                if (method != null)
                    method.Invoke(_coreLogService, new object[] { job, message });
            }
            WriteToRealTimeLog("INFO", message, job.Name, "JobManager");
        }

        /// <summary>
        /// Enhanced job success logging
        /// </summary>
        public void LogJobSuccess(SyncJob job, string message)
        {
            if (_coreLogService != null && typeof(ILogService).IsAssignableFrom(_coreLogService.GetType()))
            {
                MethodInfo method = _coreLogService.GetType().GetMethod("LogJobSuccess");
                if (method != null)
                    method.Invoke(_coreLogService, new object[] { job, message });
            }
            WriteToRealTimeLog("INFO", message, job.Name, "JobManager");
        }

        /// <summary>
        /// Enhanced job error logging with exception details
        /// </summary>
        public void LogJobError(SyncJob job, string message, Exception ex = null)
        {
            if (_coreLogService != null && typeof(ILogService).IsAssignableFrom(_coreLogService.GetType()))
            {
                MethodInfo method = _coreLogService.GetType().GetMethod("LogJobError");
                if (method != null)
                    method.Invoke(_coreLogService, new object[] { job, message, ex });
            }
            WriteToRealTimeLog("ERROR", message, job.Name, "JobManager", ex);
        }

        /// <summary>
        /// Enhanced file transfer logging
        /// </summary>
        public void LogTransfer(string jobName, string fileName, long fileSize, bool success, string error)
        {
            if (_coreLogService != null && typeof(ILogService).IsAssignableFrom(_coreLogService.GetType()))
            {
                MethodInfo method = _coreLogService.GetType().GetMethod("LogTransfer");
                if (method != null)
                    method.Invoke(_coreLogService, new object[] { jobName, fileName, fileSize, success, error });
            }
            
            // Enhanced real-time logging with file transfer details
            string level = success ? "INFO" : "ERROR";
            string message = success 
                ? string.Format("File transfer successful: {0} ({1} bytes)", fileName, fileSize)
                : string.Format("File transfer failed: {0} - {1}", fileName, error);
                
            WriteToRealTimeLog(level, message, jobName, "Transfer", success ? null : new Exception(error), fileName, fileSize);
        }

        /// <summary>
        /// Enhanced file transfer logging with detailed metrics
        /// </summary>
        public void LogTransferDetailed(string jobName, string fileName, long fileSize, TimeSpan duration, 
            bool success, string error)
        {
            string level = success ? "INFO" : "ERROR";
            string message = success 
                ? string.Format("Transfer completed: {0} ({1} bytes) in {2:F2}s", fileName, fileSize, duration.TotalSeconds)
                : string.Format("Transfer failed: {0} - {1}", fileName, error);
                
            WriteToRealTimeLog(level, message, jobName, "Transfer", success ? null : new Exception(error), 
                fileName, fileSize, duration.TotalSeconds);
        }

        #endregion
    }
}




