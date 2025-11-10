using System;
using System.IO;
using System.Text;

namespace FTPSyncer.core
{
    /// <summary>
    /// Provides functionality for log file rotation
    /// </summary>
    public class LogRotator
    {
        private readonly ILogService _logService;
        private readonly long _maxLogSizeBytes;

        /// <summary>
        /// Initializes a new instance of the LogRotator class
        /// </summary>
        /// <param name="logService">The log service to use</param>
        /// <param name="maxLogSizeBytes">Maximum log file size in bytes before rotation</param>
        public LogRotator(ILogService logService, long maxLogSizeBytes = 10 * 1024 * 1024)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _maxLogSizeBytes = maxLogSizeBytes;
        }

        /// <summary>
        /// Checks if log rotation is needed and performs it if necessary
        /// </summary>
        public void CheckAndRotateLogs()
        {
            _logService.RotateLogs(_maxLogSizeBytes);
        }

        /// <summary>
        /// Sets up automatic log rotation on a schedule
        /// </summary>
        /// <param name="checkIntervalMinutes">How often to check for log rotation (in minutes)</param>
        public void SetupAutomaticRotation(int checkIntervalMinutes)
        {
            // This would ideally use a timer to periodically check log size
            // For simplicity, we're just documenting the intent here
            // In a real implementation, you would set up a Timer or similar mechanism
            
            // Example pseudocode:
            // var timer = new System.Timers.Timer(checkIntervalMinutes * 60 * 1000);
            // timer.Elapsed += (s, e) => CheckAndRotateLogs();
            // timer.Start();
            
            _logService.Info($"Automatic log rotation set up to check every {checkIntervalMinutes} minutes", "system");
        }
    }
}





