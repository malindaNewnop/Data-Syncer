using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace syncer.core
{
    public class FileLogService : ILogService
    {
        private readonly object _syncLock = new object();
        private const int MaxLogSizeBytes = 10 * 1024 * 1024; // 10MB

        // Implement the missing interface methods
        public void LogInfo(string jobId, string message)
        {
            Write(LogLevel.Info, "", jobId, "core", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void LogWarning(string jobId, string message)
        {
            Write(LogLevel.Warning, "", jobId, "core", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void LogError(string jobId, string message)
        {
            Write(LogLevel.Error, "", jobId, "core", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void LogJobStart(string jobId, string jobName)
        {
            Write(LogLevel.Info, jobName, jobId, "job", "Job started", null, "", 0, TimeSpan.Zero, "", "");
        }

        public void LogJobEnd(string jobId, string status, int processedFiles, int failedFiles)
        {
            string message = string.Format("Job ended: {0} - Processed: {1}, Failed: {2}", status, processedFiles, failedFiles);
            Write(LogLevel.Info, "", jobId, "job", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void LogTransfer(string jobId, string sourcePath, string destPath, bool success, string error)
        {
            var level = success ? LogLevel.Info : LogLevel.Error;
            var message = success ? "Transfer completed" : "Transfer failed: " + error;
            Write(level, "", jobId, "transfer", message, null, Path.GetFileName(sourcePath), 0, TimeSpan.Zero, sourcePath, destPath);
        }

        public void Info(string message, string jobName)
        {
            Write(LogLevel.Info, jobName, "", "core", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void Warning(string message, string jobName)
        {
            Write(LogLevel.Warning, jobName, "", "core", message, null, "", 0, TimeSpan.Zero, "", "");
        }

        public void Error(string message, string jobName, Exception ex)
        {
            Write(LogLevel.Error, jobName, "", "core", message, ex, "", 0, TimeSpan.Zero, "", "");
        }

        public void LogTransfer(string jobName, string fileName, long fileSize, bool success, string error)
        {
            var level = success ? LogLevel.Info : LogLevel.Error;
            var message = success ? "Transfer completed: " + fileName : "Transfer failed: " + fileName + " - " + error;
            Write(level, jobName, "", "transfer", message, null, fileName, fileSize, TimeSpan.Zero, "", "");
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
                            EscapeCsv(ex?.ToString() ?? ""),
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
                try {
                    entry.Level = (LogLevel)Enum.Parse(typeof(LogLevel), parts[1]);
                }
                catch {
                    entry.Level = LogLevel.Info; // Default value if parsing fails
                }
                
                entry.JobName = parts[2];
                entry.JobId = parts[3];
                entry.Source = parts[4];
                entry.Message = parts[5];
                entry.Exception = parts[6];
                entry.FileName = parts[7];
                long.TryParse(parts[8], out entry.FileSize);
                if (double.TryParse(parts[9], out double durationMs))
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
                        Archive(DateTime.Now.AddDays(-30)); // Archive logs older than 30 days
                    }
                }
            }
            catch
            {
                // Ignore rotation errors
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
    }
}
