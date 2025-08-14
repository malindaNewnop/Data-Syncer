using syncer.ui.Services;
using System;
using System.IO;

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

        public static void Initialize()
        {
            try
            {
                // Make sure all required paths exist
                EnsureDirectoriesExist();
                
                // Create the log service first since other services might need it
                _logService = new Services.CoreLogServiceAdapter();
                
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
                
                // Can't log properly yet, so show in console
                Console.WriteLine("Error initializing services: " + ex.Message);
                throw; // Re-throw so we can show the detailed error
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

        public static ISyncJobService SyncJobService
        {
            get { return _syncJobService ?? (_syncJobService = new SyncJobService()); }
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
