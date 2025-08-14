using System;
using System.IO;
using System.Diagnostics;

namespace syncer.ui
{
    /// <summary>
    /// Simple debugging utility to log service activity
    /// </summary>
    public static class DebugLogger
    {
        private static string _logFile;
        private static bool _enabled = true;

        static DebugLogger()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dataSyncerPath = Path.Combine(appDataPath, "DataSyncer");
            string logPath = Path.Combine(dataSyncerPath, "debug");
            
            if (!Directory.Exists(logPath))
            {
                try 
                { 
                    Directory.CreateDirectory(logPath); 
                }
                catch
                {
                    _enabled = false;
                    return;
                }
            }

            _logFile = Path.Combine(logPath, "debug_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
            
            Log("Debug Logger Initialized");
        }

        public static void Log(string message)
        {
            if (!_enabled) return;

            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = timestamp + " - " + message;
                
                // Write to file
                File.AppendAllText(_logFile, logMessage + Environment.NewLine);
                
                // Also write to debug output
                Debug.WriteLine(logMessage);
            }
            catch
            {
                _enabled = false;
            }
        }

        public static void LogFormat(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }

        public static void LogError(Exception ex, string context = "")
        {
            if (!_enabled) return;

            string message = "ERROR: ";
            if (!string.IsNullOrEmpty(context))
            {
                message += "[" + context + "] ";
            }
            message += ex.Message;
            
            if (ex.StackTrace != null)
            {
                message += Environment.NewLine + "Stack Trace:" + Environment.NewLine + ex.StackTrace;
            }

            Log(message);
        }

        public static void LogServiceActivity(string serviceName, string activity)
        {
            Log("Service: " + serviceName + " - " + activity);
        }
    }
}
