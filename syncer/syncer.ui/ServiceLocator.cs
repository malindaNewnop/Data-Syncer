using syncer.ui.Services;
using syncer.ui.Interfaces;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace syncer.ui
{
    /// Simple service locator for dependency injection
    /// This now uses real implementations from the backend
    public static class ServiceLocator
    {
        private static ISyncJobService _syncJobService;
        private static IConnectionService _connectionService;
        private static ILogService _logService;
        private static IServiceManager _serviceManager;
        private static IConfigurationService _configurationService;
        private static ITimerJobManager _timerJobManager; // New service for managing timer jobs
        private static ISavedJobConfigurationService _savedJobConfigService; // New service for saved configurations

        // New field for log rotation
        private static bool _logRotationEnabled = true;
        private static long _maxLogSizeBytes = 10 * 1024 * 1024; // 10MB default

        public static void Initialize()
        {
            try
            {
                // Make sure all required paths exist
                EnsureDirectoriesExist();
                
                // Create the log service first since other services might need it
                _logService = new Services.CoreLogServiceAdapter();
                
                // Set up log rotation
                SetupLogRotation();
                
                // Create real service implementations using adapters
                _syncJobService = new Services.CoreSyncJobServiceAdapter();
                
                // Try to create CoreConnectionServiceAdapter, fallback to stub if it fails
                try
                {
                    _connectionService = new Services.CoreConnectionServiceAdapter();
                    _logService.LogInfo("CoreConnectionServiceAdapter initialized successfully", "UI");
                }
                catch (Exception connEx)
                {
                    _logService.LogError("Failed to initialize CoreConnectionServiceAdapter: " + connEx.Message, "UI");
                    _connectionService = new ConnectionService();
                    _logService.LogInfo("Using stub ConnectionService instead", "UI");
                }
                
                // Keep using UI implementations for these services for now
                _serviceManager = new ServiceManager();
                _configurationService = new ConfigurationService();
                
                // Create the timer job manager
                _timerJobManager = new TimerJobManager();
                
                // Create the saved job configuration service
                _savedJobConfigService = new Services.SavedJobConfigurationService();
                
                // Initialize restart recovery functionality
                InitializeRestartRecovery();
                
                // Notify UI that initialization is complete
                NotifyInitializationComplete();
                
                // Log initialization
                _logService.LogInfo("Data Syncer UI started with core backend", "UI");
            }
            catch (Exception ex)
            {
                // Fall back to stub implementations if there's a problem
                InitializeStubs();
                
                // Log the error
                string errorMsg = "Error initializing services: " + ex.Message;
                if (ex.InnerException != null) {
                    errorMsg += "\r\nInner exception: " + ex.InnerException.Message;
                }
                Console.WriteLine(errorMsg);
                
                // Try to log to file if possible
                try {
                    string logDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "DataSyncer");
                    string logPath = Path.Combine(logDir, "initialization_error.log");
                    
                    if (!Directory.Exists(logDir)) {
                        Directory.CreateDirectory(logDir);
                    }
                    
                    File.AppendAllText(logPath, DateTime.Now.ToString() + ": " + errorMsg + "\r\n" + ex.StackTrace + "\r\n\r\n");
                } catch {}
                
                // Don't rethrow - let the application continue with stubs
                // throw;
            }
        }
        
        /// <summary>
        /// Set up automatic log rotation
        /// </summary>
        private static void SetupLogRotation()
        {
            if (!_logRotationEnabled || _logService == null) 
                return;
            
            try
            {
                // Use reflection to access the core service directly
                if (_logService is CoreLogServiceAdapter adapter)
                {
                    var coreServiceField = typeof(CoreLogServiceAdapter).GetField(
                        "_coreLogService", 
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    if (coreServiceField != null)
                    {
                        object coreService = coreServiceField.GetValue(adapter);
                        if (coreService != null)
                        {
                            // Immediately check if logs need rotation
                            var rotateLogsMethod = coreService.GetType().GetMethod(
                                "RotateLogs", 
                                BindingFlags.Public | BindingFlags.Instance);
                            
                            if (rotateLogsMethod != null)
                            {
                                rotateLogsMethod.Invoke(coreService, new object[] { _maxLogSizeBytes });
                                _logService.LogInfo("Log rotation service initialized with " + 
                                    (_maxLogSizeBytes / (1024 * 1024)) + "MB max size", "System");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error setting up log rotation: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Notify UI components that initialization and restart recovery is complete
        /// </summary>
        private static void NotifyInitializationComplete()
        {
            try
            {
                // Find the main form and update its connection status
                FormMain mainForm = null;
                foreach (Form form in System.Windows.Forms.Application.OpenForms)
                {
                    if (form is FormMain)
                    {
                        mainForm = (FormMain)form;
                        break;
                    }
                }
                
                if (mainForm != null)
                {
                    // Use Invoke to ensure we're on the UI thread
                    mainForm.Invoke((Action)(() =>
                    {
                        // Call a method to refresh the connection status after restart recovery
                        var updateMethod = mainForm.GetType().GetMethod("RefreshAfterStartup", 
                            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                        if (updateMethod != null)
                        {
                            updateMethod.Invoke(mainForm, null);
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                _logService?.LogWarning("Failed to notify UI of initialization completion: " + ex.Message);
            }
        }

        /// <summary>
        /// Initialize restart recovery functionality to ensure proper job and connection restoration
        /// </summary>
        private static void InitializeRestartRecovery()
        {
            try
            {
                // First, ensure connection settings are loaded from persistent storage
                if (_connectionService != null)
                {
                    try
                    {
                        // Load connection settings from persistent storage
                        var restoredSettings = _connectionService.LoadConnectionForStartup();
                        if (restoredSettings != null && !StringExtensions.IsNullOrWhiteSpace(restoredSettings.Host))
                        {
                            _logService.LogInfo(string.Format("Connection settings restored from persistent storage: {0}@{1}:{2} (Protocol: {3}, ProtocolType: {4}, IsRemote: {5})", 
                                restoredSettings.Username, restoredSettings.Host, restoredSettings.Port,
                                restoredSettings.Protocol, restoredSettings.ProtocolType, restoredSettings.IsRemoteConnection), "RestartRecovery");
                                
                            // Force reconnect to ensure IsConnected is properly set
                            bool reconnected = _connectionService.ForceReconnect();
                            if (reconnected)
                            {
                                _logService.LogInfo("Connection successfully restored and verified for restart", "RestartRecovery");
                            }
                            else
                            {
                                _logService.LogWarning("Connection restored but verification failed - jobs may not run properly", "RestartRecovery");
                            }
                        }
                        else
                        {
                            _logService.LogWarning("No connection settings found in persistent storage for restart recovery", "RestartRecovery");
                        }
                    }
                    catch (Exception connEx)
                    {
                        _logService.LogError("Error loading connection settings from persistent storage: " + connEx.Message, "RestartRecovery");
                    }
                }

                // Verify that sync jobs are properly loaded (this is now handled by SyncJobService constructor)
                if (_syncJobService != null)
                {
                    var jobs = _syncJobService.GetAllJobs();
                    _logService.LogInfo(string.Format("Sync jobs restoration verified: {0} jobs loaded", jobs.Count), "RestartRecovery");
                    
                    int enabledJobs = 0;
                    foreach (var job in jobs)
                    {
                        if (job.IsEnabled) enabledJobs++;
                    }
                    
                    if (enabledJobs > 0)
                    {
                        _logService.LogInfo(string.Format("{0} enabled sync jobs will resume scheduling after restart", enabledJobs), "RestartRecovery");
                    }
                }

                // Verify timer job manager restoration (this is handled by TimerJobManager constructor)
                if (_timerJobManager != null)
                {
                    var timerJobIds = _timerJobManager.GetRegisteredTimerJobs();
                    _logService.LogInfo(string.Format("Timer jobs restoration verified: {0} timer jobs loaded", timerJobIds.Count), "RestartRecovery");
                }

                _logService.LogInfo("Restart recovery initialization completed successfully", "RestartRecovery");
            }
            catch (Exception ex)
            {
                _logService.LogError("Error during restart recovery initialization: " + ex.Message, "RestartRecovery");
            }
        }
        
        /// <summary>
        /// Properly shutdown services and save state for restart
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                _logService.LogInfo("Shutting down services and saving state for restart...", "Shutdown");

                // Stop sync job scheduler and save jobs
                if (_syncJobService != null)
                {
                    try
                    {
                        // If the service has a StopScheduler method, call it
                        var stopMethod = _syncJobService.GetType().GetMethod("StopScheduler");
                        if (stopMethod != null)
                        {
                            stopMethod.Invoke(_syncJobService, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.LogError("Error stopping sync job scheduler: " + ex.Message, "Shutdown");
                    }
                }

                // Stop timer jobs and save their state
                if (_timerJobManager != null)
                {
                    try
                    {
                        var timerJobIds = _timerJobManager.GetRegisteredTimerJobs();
                        foreach (var jobId in timerJobIds)
                        {
                            _timerJobManager.StopTimerJob(jobId);
                        }
                        _logService.LogInfo(string.Format("Stopped {0} timer jobs for shutdown", timerJobIds.Count), "Shutdown");
                    }
                    catch (Exception ex)
                    {
                        _logService.LogError("Error stopping timer jobs: " + ex.Message, "Shutdown");
                    }
                }

                _logService.LogInfo("Application shutdown completed successfully", "Shutdown");
            }
            catch (Exception ex)
            {
                // Try to log the error even if the log service is failing
                try
                {
                    _logService.LogError("Error during application shutdown: " + ex.Message, "Shutdown");
                }
                catch
                {
                    // Last resort - write to console/debug
                    Console.WriteLine("Critical error during shutdown: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine("Critical error during shutdown: " + ex.Message);
                }
            }
        }
        
        /// <summary>
        /// Manually trigger restart recovery - useful for troubleshooting
        /// </summary>
        public static bool ManualRestartRecovery()
        {
            try
            {
                _logService.LogInfo("Manual restart recovery initiated", "ManualRecovery");
                
                // First, reload connection settings from persistent storage
                if (_connectionService != null)
                {
                    try
                    {
                        var restoredSettings = _connectionService.LoadConnectionForStartup();
                        if (restoredSettings != null && !StringExtensions.IsNullOrWhiteSpace(restoredSettings.Host))
                        {
                            _logService.LogInfo(string.Format("Manual recovery: Connection settings reloaded: {0}@{1}:{2}", 
                                restoredSettings.Username, restoredSettings.Host, restoredSettings.Port), "ManualRecovery");
                                
                            // Force reconnection
                            bool reconnected = _connectionService.ForceReconnect();
                            _logService.LogInfo("Manual force reconnect result: " + reconnected, "ManualRecovery");
                        }
                        else
                        {
                            _logService.LogWarning("Manual recovery: No connection settings found in persistent storage", "ManualRecovery");
                        }
                    }
                    catch (Exception reconnectEx)
                    {
                        _logService.LogError("Error during manual connection recovery: " + reconnectEx.Message, "ManualRecovery");
                    }
                }
                
                // Reload sync jobs from persistence
                if (_syncJobService != null)
                {
                    try
                    {
                        _syncJobService.ReloadJobsFromPersistence();
                        _logService.LogInfo("Manual sync jobs reload completed", "ManualRecovery");
                    }
                    catch (Exception reloadEx)
                    {
                        _logService.LogError("Error during manual sync jobs reload: " + reloadEx.Message, "ManualRecovery");
                    }
                }
                
                _logService.LogInfo("Manual restart recovery completed", "ManualRecovery");
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError("Error during manual restart recovery: " + ex.Message, "ManualRecovery");
                return false;
            }
        }
        
        public static void InitializeStubs()
        {
            // Initialize with stub implementations
            _syncJobService = new SyncJobService();
            _connectionService = new ConnectionService();
            _logService = new LogService();
            _serviceManager = new ServiceManager();
            _configurationService = new ConfigurationService();
            _timerJobManager = new Services.TimerJobManager();
            _savedJobConfigService = new Services.SavedJobConfigurationService();
            
            Console.WriteLine("Initialized with stub implementations");
        }
        
        // Helper method to ensure required directories exist
        private static void EnsureDirectoriesExist()
        {
            if (!FileSystemPermissionChecker.CheckAllRequiredFolders())
            {
                throw new Exception("Unable to create or access required folders. Please check that the application has permission to write to " + 
                                  Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\DataSyncer");
            }
        }

        /// <summary>
        /// Configure log rotation settings
        /// </summary>
        /// <param name="enabled">Whether log rotation is enabled</param>
        /// <param name="maxSizeBytes">Maximum log file size before rotation</param>
        public static void ConfigureLogRotation(bool enabled, long maxSizeBytes)
        {
            _logRotationEnabled = enabled;
            _maxLogSizeBytes = maxSizeBytes;
            
            if (_logService != null && enabled)
            {
                SetupLogRotation();
            }
        }
        
        /// <summary>
        /// Perform a manual log rotation right now
        /// </summary>
        public static void RotateLogsNow()
        {
            if (_logService == null)
                return;
                
            try
            {
                // Use reflection to access the core service directly
                if (_logService is CoreLogServiceAdapter adapter)
                {
                    var coreServiceField = typeof(CoreLogServiceAdapter).GetField(
                        "_coreLogService", 
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    if (coreServiceField != null)
                    {
                        object coreService = coreServiceField.GetValue(adapter);
                        if (coreService != null)
                        {
                            var rotateLogsMethod = coreService.GetType().GetMethod(
                                "RotateLogs", 
                                BindingFlags.Public | BindingFlags.Instance);
                            
                            if (rotateLogsMethod != null)
                            {
                                // Force rotation by using 0 as the max size
                                rotateLogsMethod.Invoke(coreService, new object[] { 0 });
                                _logService.LogInfo("Manual log rotation completed", "System");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_logService != null)
                {
                    _logService.LogError("Error during manual log rotation: " + ex.Message, "System");
                }
            }
        }

        public static ISyncJobService SyncJobService
        {
            get { return _syncJobService; }
        }

        public static IConnectionService ConnectionService
        {
            get { return _connectionService ?? (_connectionService = new ConnectionService()); }
        }

        public static ILogService LogService
        {
            get { return _logService ?? (_logService = new LogService()); }
        }

        public static IServiceManager ServiceManager
        {
            get { return _serviceManager ?? (_serviceManager = new ServiceManager()); }
        }

        public static IConfigurationService ConfigurationService
        {
            get { return _configurationService ?? (_configurationService = new ConfigurationService()); }
        }
        
        public static ITimerJobManager TimerJobManager
        {
            get { return _timerJobManager ?? (_timerJobManager = new Services.TimerJobManager()); }
        }
        
        public static ISavedJobConfigurationService SavedJobConfigurationService
        {
            get { return _savedJobConfigService ?? (_savedJobConfigService = new Services.SavedJobConfigurationService()); }
        }

        public static void SetSyncJobService(ISyncJobService service) { _syncJobService = service; }
        public static void SetConnectionService(IConnectionService service) { _connectionService = service; }
        public static void SetLogService(ILogService service) { _logService = service; }
        public static void SetServiceManager(IServiceManager service) { _serviceManager = service; }
        public static void SetConfigurationService(IConfigurationService service) { _configurationService = service; }
    }
}
