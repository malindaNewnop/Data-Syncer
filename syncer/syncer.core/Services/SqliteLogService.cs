using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace syncer.core
{
    /// <summary>
    /// SQLite-based implementation of the ILogService interface
    /// Stores log entries in a SQLite database
    /// </summary>
    public class SqliteLogService : ILogService
    {
        private readonly object _lockObject = new object();
        
        public SqliteLogService()
        {
            DatabaseHelper.InitializeDatabase();
            CleanupOldLogs();
        }
        
        public void LogInfo(string message, string source = null)
        {
            LogMessage("INFO", message, source);
        }
        
        public void LogWarning(string message, string source = null)
        {
            LogMessage("WARNING", message, source);
        }
        
        public void LogError(string message, string source = null)
        {
            LogMessage("ERROR", message, source);
        }
        
        public void LogJobStart(SyncJob job)
        {
            LogMessage("INFO", $"Job started: {job.Name}", "JobRunner", job.Id);
        }
        
        public void LogJobProgress(SyncJob job, string message)
        {
            LogMessage("INFO", message, "JobRunner", job.Id);
        }
        
        public void LogJobSuccess(SyncJob job, string message)
        {
            LogMessage("INFO", $"Job completed successfully: {job.Name} - {message}", "JobRunner", job.Id);
        }
        
        public void LogJobError(SyncJob job, string message, Exception ex = null)
        {
            LogMessage("ERROR", $"Job error: {job.Name} - {message}", "JobRunner", job.Id, ex);
        }
        
        private void LogMessage(string level, string message, string source = null, string jobId = null, Exception ex = null, object extraData = null)
        {
            lock (_lockObject)
            {
                try
                {
                    using (var connection = DatabaseHelper.CreateConnection())
                    {
                        using (var command = new SQLiteCommand(@"
                            INSERT INTO Logs (Id, Timestamp, Level, Source, Message, JobId, Exception, ExtraData)
                            VALUES (@Id, @Timestamp, @Level, @Source, @Message, @JobId, @Exception, @ExtraData)", connection))
                        {
                            string id = Guid.NewGuid().ToString();
                            command.Parameters.AddWithValue("@Id", id);
                            command.Parameters.AddWithValue("@Timestamp", DateTime.Now.ToString("o"));
                            command.Parameters.AddWithValue("@Level", level);
                            command.Parameters.AddWithValue("@Source", source ?? "");
                            command.Parameters.AddWithValue("@Message", message ?? "");
                            command.Parameters.AddWithValue("@JobId", jobId ?? "");
                            command.Parameters.AddWithValue("@Exception", ex != null ? ex.ToString() : "");
                            command.Parameters.AddWithValue("@ExtraData", extraData != null ? JsonConvert.SerializeObject(extraData) : "");
                            
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception logEx)
                {
                    // If we can't log to the database, log to console at least
                    Console.WriteLine($"Error logging to database: {logEx.Message}");
                    Console.WriteLine($"Original log: [{level}] {source}: {message}");
                }
            }
        }
        
        public List<LogEntry> GetLogs(int maxCount = 1000, string level = null, string source = null, string jobId = null)
        {
            lock (_lockObject)
            {
                try
                {
                    var logs = new List<LogEntry>();
                    
                    using (var connection = DatabaseHelper.CreateConnection())
                    {
                        var sql = new StringBuilder("SELECT * FROM Logs WHERE 1=1");
                        
                        if (!string.IsNullOrEmpty(level))
                        {
                            sql.Append(" AND Level = @Level");
                        }
                        
                        if (!string.IsNullOrEmpty(source))
                        {
                            sql.Append(" AND Source = @Source");
                        }
                        
                        if (!string.IsNullOrEmpty(jobId))
                        {
                            sql.Append(" AND JobId = @JobId");
                        }
                        
                        sql.Append(" ORDER BY Timestamp DESC LIMIT @MaxCount");
                        
                        using (var command = new SQLiteCommand(sql.ToString(), connection))
                        {
                            if (!string.IsNullOrEmpty(level))
                            {
                                command.Parameters.AddWithValue("@Level", level);
                            }
                            
                            if (!string.IsNullOrEmpty(source))
                            {
                                command.Parameters.AddWithValue("@Source", source);
                            }
                            
                            if (!string.IsNullOrEmpty(jobId))
                            {
                                command.Parameters.AddWithValue("@JobId", jobId);
                            }
                            
                            command.Parameters.AddWithValue("@MaxCount", maxCount);
                            
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var log = new LogEntry
                                    {
                                        Id = reader["Id"].ToString(),
                                        Timestamp = DateTime.Parse(reader["Timestamp"].ToString()),
                                        Level = reader["Level"].ToString(),
                                        Source = reader["Source"].ToString(),
                                        Message = reader["Message"].ToString(),
                                        JobId = reader["JobId"].ToString(),
                                        Exception = reader["Exception"].ToString()
                                    };
                                    
                                    string extraData = reader["ExtraData"].ToString();
                                    if (!string.IsNullOrEmpty(extraData))
                                    {
                                        try
                                        {
                                            log.ExtraData = JsonConvert.DeserializeObject(extraData);
                                        }
                                        catch
                                        {
                                            // Ignore deserialization errors
                                        }
                                    }
                                    
                                    logs.Add(log);
                                }
                            }
                        }
                    }
                    
                    return logs;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting logs: {ex.Message}");
                    return new List<LogEntry>();
                }
            }
        }
        
        public List<LogEntry> GetJobLogs(string jobId, int maxCount = 1000)
        {
            return GetLogs(maxCount, null, null, jobId);
        }
        
        public void ClearLogs()
        {
            lock (_lockObject)
            {
                try
                {
                    using (var connection = DatabaseHelper.CreateConnection())
                    {
                        using (var command = new SQLiteCommand("DELETE FROM Logs", connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error clearing logs: {ex.Message}");
                }
            }
        }
        
        private void CleanupOldLogs()
        {
            lock (_lockObject)
            {
                try
                {
                    // Get retention days from settings
                    int retentionDays = 30;
                    string retentionSetting = DatabaseHelper.GetSetting("LogRetentionDays");
                    if (!string.IsNullOrEmpty(retentionSetting) && int.TryParse(retentionSetting, out int days))
                    {
                        retentionDays = days;
                    }
                    
                    // Calculate cutoff date
                    var cutoffDate = DateTime.Now.AddDays(-retentionDays);
                    
                    using (var connection = DatabaseHelper.CreateConnection())
                    {
                        using (var command = new SQLiteCommand(
                            "DELETE FROM Logs WHERE Timestamp < @CutoffDate", connection))
                        {
                            command.Parameters.AddWithValue("@CutoffDate", cutoffDate.ToString("o"));
                            int rowsDeleted = command.ExecuteNonQuery();
                            
                            if (rowsDeleted > 0)
                            {
                                using (var logCommand = new SQLiteCommand(@"
                                    INSERT INTO Logs (Id, Timestamp, Level, Source, Message, JobId, Exception, ExtraData)
                                    VALUES (@Id, @Timestamp, @Level, @Source, @Message, @JobId, @Exception, @ExtraData)", connection))
                                {
                                    string id = Guid.NewGuid().ToString();
                                    logCommand.Parameters.AddWithValue("@Id", id);
                                    logCommand.Parameters.AddWithValue("@Timestamp", DateTime.Now.ToString("o"));
                                    logCommand.Parameters.AddWithValue("@Level", "INFO");
                                    logCommand.Parameters.AddWithValue("@Source", "LogService");
                                    logCommand.Parameters.AddWithValue("@Message", $"Cleaned up {rowsDeleted} log entries older than {retentionDays} days");
                                    logCommand.Parameters.AddWithValue("@JobId", "");
                                    logCommand.Parameters.AddWithValue("@Exception", "");
                                    logCommand.Parameters.AddWithValue("@ExtraData", "");
                                    
                                    logCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cleaning up old logs: {ex.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// Represents a log entry retrieved from the database
    /// </summary>
    public class LogEntry
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public string JobId { get; set; }
        public string Exception { get; set; }
        public object ExtraData { get; set; }
    }
}
