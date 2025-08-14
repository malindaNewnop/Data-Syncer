using System;
using System.Data;
using System.Collections.Generic;
using syncer.core;

namespace syncer.ui.Services
{
    /// <summary>
    /// Adapter to connect UI LogService interface to Core LogService implementation
    /// </summary>
    public class CoreLogServiceAdapter : ILogService
    {
        private readonly syncer.core.ILogService _coreLogService;

        public CoreLogServiceAdapter()
        {
            try
            {
                _coreLogService = new syncer.core.FileLogService();
                DebugLogger.LogServiceActivity("CoreLogServiceAdapter", "Initialized successfully");
            }
            catch (Exception ex)
            {
                DebugLogger.LogError(ex, "CoreLogServiceAdapter initialization");
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
            syncer.core.LogLevel? level = null;
            
            if (!string.IsNullOrEmpty(logLevel) && logLevel != "All")
            {
                if (logLevel == "INFO")
                    level = syncer.core.LogLevel.Info;
                else if (logLevel == "WARNING")
                    level = syncer.core.LogLevel.Warning;
                else if (logLevel == "ERROR")
                    level = syncer.core.LogLevel.Error;
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
        }

        public void LogInfo(string message, string jobName)
        {
            _coreLogService.Info(message, jobName);
        }

        public void LogWarning(string message)
        {
            _coreLogService.Warning(message, "UI");
        }

        public void LogWarning(string message, string jobName)
        {
            _coreLogService.Warning(message, jobName);
        }

        public void LogError(string message)
        {
            _coreLogService.Error(message, "UI", null);
        }

        public void LogError(string message, string jobName)
        {
            _coreLogService.Error(message, jobName, null);
        }
    }
}
