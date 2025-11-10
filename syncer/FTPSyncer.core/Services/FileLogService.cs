using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace FTPSyncer.core
{
    public class FileLogService : ILogService
    {
        private readonly object _syncLock = new object();
        private const int MaxLogSizeBytes = 10 * 1024 * 1024; // 10MB
        private const int MaxBackupCount = 10; // Maximum number of backup files to keep

        // Implement the interface methods with correct signatures
        public void LogJobStart(SyncJob job)
        {
            Write(LogLevel.Info, job.Name, job.Id, "job", "Job started", null, "", 0, TimeSpan.Zero, "", "");
        }

        public void LogJobProgress(SyncJob job, string message)
        {
            Write(LogLevel.Info, job.Name, job.Id, "job", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void LogJobSuccess(SyncJob job, string message)
        {
            Write(LogLevel.Info, job.Name, job.Id, "job", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void LogJobError(SyncJob job, string message, Exception ex)
        {
            Write(LogLevel.Error, job.Name, job.Id, "job", message, ex, "", 0, TimeSpan.Zero, "", "");
        }

        // ILogService interface implementation
        public void LogInfo(string message, string source = null)
        {
            Write(LogLevel.Info, "", "", source ?? "core", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void LogInfo(string message, string source, string jobId)
        {
            Write(LogLevel.Info, "", jobId ?? "", source ?? "core", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void LogWarning(string message, string source = null)
        {
            Write(LogLevel.Warning, "", "", source ?? "core", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void LogWarning(string message, string source, string jobId)
        {
            Write(LogLevel.Warning, "", jobId ?? "", source ?? "core", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void LogError(string message, string source = null)
        {
            Write(LogLevel.Error, "", "", source ?? "core", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void LogError(string message, string source, string jobId)
        {
            Write(LogLevel.Error, "", jobId ?? "", source ?? "core", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void Info(string message, string jobName)
        {
            Write(LogLevel.Info, jobName, "", "core", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void Info(string message, string jobName, string jobId)
        {
            Write(LogLevel.Info, jobName, jobId, "core", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void Warning(string message, string jobName)
        {
            Write(LogLevel.Warning, jobName, "", "core", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void Warning(string message, string jobName, string jobId)
        {
            Write(LogLevel.Warning, jobName, jobId, "core", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void Error(string message, string jobName, Exception ex)
        {
            Write(LogLevel.Error, jobName, "", "core", message, ex, "", 0, TimeSpan.Zero, "", "");
        }

        public void Error(string message, string jobName, Exception ex, string jobId)
        {
            Write(LogLevel.Error, jobName, jobId, "core", message, ex, "", 0, TimeSpan.Zero, "", "");
        }

        public void LogTransfer(string jobName, string fileName, long fileSize, bool success, string error)
        {
            var level = success ? LogLevel.Info : LogLevel.Error;
            var message = success ? "Transfer completed: " + fileName : "Transfer failed: " + fileName + " - " + error;
            Write(level, jobName, "", "transfer", message, null, fileName, fileSize, TimeSpan.Zero, "", "");
        }

        /// <summary>
        /// Enhanced transfer logging with duration and path information
        /// </summary>
        public void LogTransferDetailed(string jobName, string fileName, long fileSize, TimeSpan duration, 
            string remotePath, string localPath, bool success, string error)
        {
            var level = success ? LogLevel.Info : LogLevel.Error;
            var message = success ? 
                string.Format("Transfer completed: {0} ({1} bytes in {2:F1}s)", fileName, fileSize, duration.TotalSeconds) :
                string.Format("Transfer failed: {0} - {1}", fileName, error ?? "Unknown error");
            
            Write(level, jobName, "", "transfer", message, null, fileName, fileSize, duration, remotePath, localPath);
        }

        private void Write(LogLevel level, string jobName, string jobId, string source, string message,
                          Exception ex, string fileName, long fileSize, TimeSpan duration,
                          string remotePath, string localPath)
        {
            lock (_syncLock)
            {
                try
                {
                    CheckLogRotation();

                    bool exists = File.Exists(Paths.LogsFile);
                    using (var sw = new StreamWriter(Paths.LogsFile, true, Encoding.UTF8))
                    {
                        if (!exists)
                        {
                            sw.WriteLine("Timestamp,Level,JobName,JobId,Source,Message,Exception,FileName,FileSize,Duration,RemotePath,LocalPath");
                        }

                        string line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                            DateTime.Now.ToString("o", CultureInfo.InvariantCulture),
                            level,
                            EscapeCsv(jobName),
                            EscapeCsv(jobId),
                            EscapeCsv(source),
                            EscapeCsv(message),
                            EscapeCsv(ex != null ? ex.ToString() : ""),
                            EscapeCsv(fileName),
                            fileSize,
                            duration.TotalMilliseconds,
                            EscapeCsv(remotePath),
                            EscapeCsv(localPath));

                        sw.WriteLine(line);
                    }
                }
                catch
                {
                    // Ignore logging errors to prevent infinite loops
                }
            }
        }

        public DataTable GetLogs(DateTime from, DateTime to, LogLevel? level)
        {
            var dt = CreateLogTable();

            if (!File.Exists(Paths.LogsFile)) return dt;

            lock (_syncLock)
            {
                try
                {
                    using (var sr = new StreamReader(Paths.LogsFile, Encoding.UTF8))
                    {
                        string header = sr.ReadLine(); // Skip header
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            var entry = ParseLogLine(line);
                            if (entry != null &&
                                entry.Timestamp >= from &&
                                entry.Timestamp <= to &&
                                (!level.HasValue || entry.Level == level.Value))
                            {
                                dt.Rows.Add(entry.Timestamp, entry.Level.ToString(), entry.JobName,
                                           entry.JobId, entry.Source, entry.Message, entry.Exception,
                                           entry.FileName, entry.FileSize, entry.Duration.TotalMilliseconds,
                                           entry.RemotePath, entry.LocalPath);
                            }
                        }
                    }
                }
                catch
                {
                    // Return empty table on error
                }
            }

            return dt;
        }

        public DataTable GetJobLogs(string jobId, DateTime from, DateTime to)
        {
            var dt = CreateLogTable();

            if (!File.Exists(Paths.LogsFile)) return dt;

            lock (_syncLock)
            {
                try
                {
                    using (var sr = new StreamReader(Paths.LogsFile, Encoding.UTF8))
                    {
                        string header = sr.ReadLine(); // Skip header
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            var entry = ParseLogLine(line);
                            if (entry != null &&
                                entry.JobId == jobId &&
                                entry.Timestamp >= from &&
                                entry.Timestamp <= to)
                            {
                                dt.Rows.Add(entry.Timestamp, entry.Level.ToString(), entry.JobName,
                                           entry.JobId, entry.Source, entry.Message, entry.Exception,
                                           entry.FileName, entry.FileSize, entry.Duration.TotalMilliseconds,
                                           entry.RemotePath, entry.LocalPath);
                            }
                        }
                    }
                }
                catch
                {
                    // Return empty table on error
                }
            }

            return dt;
        }

        public void Clear()
        {
            lock (_syncLock)
            {
                try
                {
                    if (File.Exists(Paths.LogsFile))
                    {
                        File.Delete(Paths.LogsFile);
                    }
                }
                catch
                {
                    // Ignore clear errors
                }
            }
        }

        public void Archive(DateTime cutoffDate)
        {
            lock (_syncLock)
            {
                try
                {
                    if (!File.Exists(Paths.LogsFile)) return;

                    var archivePath = Paths.GetLogArchivePath(cutoffDate);
                    var tempLogs = new List<string>();
                    var archiveLogs = new List<string>();

                    using (var sr = new StreamReader(Paths.LogsFile, Encoding.UTF8))
                    {
                        string header = sr.ReadLine();
                        archiveLogs.Add(header);
                        tempLogs.Add(header);

                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            var entry = ParseLogLine(line);
                            if (entry != null)
                            {
                                if (entry.Timestamp < cutoffDate)
                                {
                                    archiveLogs.Add(line);
                                }
                                else
                                {
                                    tempLogs.Add(line);
                                }
                            }
                        }
                    }

                    // Write archive file
                    if (archiveLogs.Count > 1) // More than just header
                    {
                        File.WriteAllLines(archivePath, archiveLogs.ToArray(), Encoding.UTF8);
                    }

                    // Write current file with remaining logs
                    File.WriteAllLines(Paths.LogsFile, tempLogs.ToArray(), Encoding.UTF8);
                }
                catch
                {
                    // Ignore archive errors
                }
            }
        }

        private DataTable CreateLogTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("Timestamp", typeof(DateTime));
            dt.Columns.Add("Level", typeof(string));
            dt.Columns.Add("JobName", typeof(string));
            dt.Columns.Add("JobId", typeof(string));
            dt.Columns.Add("Source", typeof(string));
            dt.Columns.Add("Message", typeof(string));
            dt.Columns.Add("Exception", typeof(string));
            dt.Columns.Add("FileName", typeof(string));
            dt.Columns.Add("FileSize", typeof(long));
            dt.Columns.Add("Duration", typeof(double));
            dt.Columns.Add("RemotePath", typeof(string));
            dt.Columns.Add("LocalPath", typeof(string));
            return dt;
        }

        private LogEntry ParseLogLine(string line)
        {
            try
            {
                var parts = SplitCsvLine(line);
                if (parts.Length < 12) return null;

                var entry = new LogEntry();
                DateTime.TryParse(parts[0], null, DateTimeStyles.RoundtripKind, out entry.Timestamp);

                // Replace Enum.TryParse with manual parsing for .NET 3.5 compatibility
                try
                {
                    entry.Level = (LogLevel)Enum.Parse(typeof(LogLevel), parts[1], true);
                }
                catch
                {
                    entry.Level = LogLevel.Info; // Default value if parsing fails
                }

                entry.JobName = parts[2];
                entry.JobId = parts[3];
                entry.Source = parts[4];
                entry.Message = parts[5];
                entry.Exception = parts[6];
                entry.FileName = parts[7];
                long.TryParse(parts[8], out entry.FileSize);

                double durationMs;
                if (double.TryParse(parts[9], out durationMs))
                {
                    entry.Duration = TimeSpan.FromMilliseconds(durationMs);
                }

                entry.RemotePath = parts[10];
                entry.LocalPath = parts[11];

                return entry;
            }
            catch
            {
                return null;
            }
        }

        private void CheckLogRotation()
        {
            try
            {
                if (File.Exists(Paths.LogsFile))
                {
                    var fileInfo = new FileInfo(Paths.LogsFile);
                    if (fileInfo.Length > MaxLogSizeBytes)
                    {
                        RotateLogs(MaxLogSizeBytes);
                    }
                }
            }
            catch
            {
                // Ignore rotation errors
            }
        }

        /// <summary>
        /// Rotates log files when they exceed the specified size limit.
        /// Creates a backup of the current log file and starts a new one.
        /// </summary>
        /// <param name="maxSizeBytes">Maximum size in bytes before rotation</param>
        public void RotateLogs(long maxSizeBytes)
        {
            lock (_syncLock)
            {
                try
                {
                    if (!File.Exists(Paths.LogsFile))
                        return;

                    var fileInfo = new FileInfo(Paths.LogsFile);
                    if (fileInfo.Length > maxSizeBytes)
                    {
                        string backupPath = Paths.LogsFile + "." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bak";
                        File.Move(Paths.LogsFile, backupPath);

                        // Create a new log file with header
                        using (var sw = new StreamWriter(Paths.LogsFile, false, Encoding.UTF8))
                        {
                            sw.WriteLine("Timestamp,Level,JobName,JobId,Source,Message,Exception,FileName,FileSize,Duration,RemotePath,LocalPath");
                        }

                        LogInfo("Log file rotated due to size limit. Previous log archived to: " + backupPath, "system");

                        // Clean up old backup files to prevent disk space issues
                        CleanupOldBackups();
                    }
                }
                catch (Exception)
                {
                    // Can't log here as it would be recursive
                }
            }
        }

        /// <summary>
        /// Cleans up old log backup files, keeping only the most recent ones
        /// </summary>
        private void CleanupOldBackups()
        {
            try
            {
                string logsDir = Path.GetDirectoryName(Paths.LogsFile);
                if (string.IsNullOrEmpty(logsDir) || !Directory.Exists(logsDir))
                    return;

                string logFileName = Path.GetFileName(Paths.LogsFile);
                string[] backupFiles = Directory.GetFiles(logsDir, logFileName + ".*.bak");

                if (backupFiles.Length <= MaxBackupCount)
                    return;

                // Sort backup files by creation time (oldest first) - .NET 3.5 compatible
                Array.Sort(backupFiles, new Comparison<string>((a, b) => {
                    try
                    {
                        return File.GetCreationTime(a).CompareTo(File.GetCreationTime(b));
                    }
                    catch
                    {
                        return 0; // If we can't compare, treat as equal
                    }
                }));

                // Delete oldest backups to keep only MaxBackupCount files
                int filesToDelete = backupFiles.Length - MaxBackupCount;
                for (int i = 0; i < filesToDelete; i++)
                {
                    try
                    {
                        File.Delete(backupFiles[i]);
                    }
                    catch
                    {
                        // Ignore errors deleting individual backup files
                    }
                }
            }
            catch
            {
                // Ignore any errors in the cleanup process
            }
        }

        private static string EscapeCsv(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";

            field = field.Replace("\"", "\"\"");
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field + "\"";
            }
            return field;
        }

        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++; // Skip next quote
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        result.Add(current.ToString());
                        // Use StringBuilder's constructor instead of Clear() for .NET 3.5 compatibility
                        current = new StringBuilder();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }

        #region Real-time Logging Interface Implementation (No-op for FileLogService)
        
#pragma warning disable 0067 // Event is never used - required for interface implementation
        public event EventHandler<LogEntryEventArgs> RealTimeLogEntry;
#pragma warning restore 0067

        /// <summary>
        /// Enable real-time logging (no-op implementation for FileLogService)
        /// </summary>
        public void EnableRealTimeLogging(string customFilePath)
        {
            // FileLogService doesn't support real-time logging to custom directories
            // This is a no-op implementation
        }

        /// <summary>
        /// Disable real-time logging (no-op implementation for FileLogService)
        /// </summary>
        public void DisableRealTimeLogging()
        {
            // FileLogService doesn't support real-time logging to custom directories
            // This is a no-op implementation
        }

        /// <summary>
        /// Check if real-time logging is enabled (always false for FileLogService)
        /// </summary>
        public bool IsRealTimeLoggingEnabled()
        {
            return false; // FileLogService doesn't support real-time logging
        }

        /// <summary>
        /// Get real-time log path (always null for FileLogService)
        /// </summary>
        public string GetRealTimeLogPath()
        {
            return null; // FileLogService doesn't support real-time logging
        }

        #endregion
    }
}




