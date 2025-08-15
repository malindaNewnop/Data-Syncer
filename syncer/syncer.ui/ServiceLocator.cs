using syncer.ui.Services;
using System;
using System.IO;
using System.Reflection;

namespace syncer.ui
{
    /// Simple service locator for dependency injection
    /// This now uses real implementations from the backend
    public static class ServiceLocator
    {
        private static ISyncJobService _syncJobService;
        private static IConnectionService _connectionService;
        private static IFilterService _filterService;
        private static ILogService _logService;
        private static IServiceManager _serviceManager;
        private static IConfigurationService _configurationService;

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
                _connectionService = new Services.CoreConnectionServiceAdapter();
                
                // Keep using UI implementations for these services for now
                _filterService = new FilterService();
                _serviceManager = new ServiceManager();
                _configurationService = new ConfigurationService();
                
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
        
        public static void InitializeStubs()
        {
            // Initialize with stub implementations
            _syncJobService = new SyncJobService();
            _connectionService = new ConnectionService();
            _filterService = new FilterService();
            _logService = new LogService();
            _serviceManager = new ServiceManager();
            _configurationService = new ConfigurationService();
            
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

        public static IFilterService FilterService
        {
            get { return _filterService ?? (_filterService = new FilterService()); }
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

        public static void SetSyncJobService(ISyncJobService service) { _syncJobService = service; }
        public static void SetConnectionService(IConnectionService service) { _connectionService = service; }
        public static void SetFilterService(IFilterService service) { _filterService = service; }
        public static void SetLogService(ILogService service) { _logService = service; }
        public static void SetServiceManager(IServiceManager service) { _serviceManager = service; }
        public static void SetConfigurationService(IConfigurationService service) { _configurationService = service; }
    }
}
