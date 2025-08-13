using syncer.ui.Services;

namespace syncer.ui
{
    /// Simple service locator for dependency injection
    /// This will be replaced with proper DI container when backend is implemented
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
            _syncJobService = new SyncJobService();
            _connectionService = new ConnectionService();
            _filterService = new FilterService();
            _logService = new LogService();
            _serviceManager = new ServiceManager();
            _configurationService = new ConfigurationService();
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
