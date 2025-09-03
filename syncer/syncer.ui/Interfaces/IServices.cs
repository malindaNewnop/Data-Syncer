using System;
using System.Collections.Generic;
using System.Data;

namespace syncer.ui
{
    /// <summary>
    /// Interface for managing sync job data
    /// </summary>
    public interface ISyncJobService
    {
        List<SyncJob> GetAllJobs();
        SyncJob GetJobById(int id);
        int CreateJob(SyncJob job);
        bool UpdateJob(SyncJob job);
        bool DeleteJob(int id);
        bool StartJob(int id);
        bool StopJob(int id);
        string GetJobStatus(int id);
    }

    /// <summary>
    /// Interface for managing connection settings
    /// </summary>
    public interface IConnectionService
    {
        ConnectionSettings GetConnectionSettings();
        bool SaveConnectionSettings(ConnectionSettings settings);
        bool TestConnection(ConnectionSettings settings);
        bool IsConnected();
        
        // Enhanced connection management methods
        bool SaveConnection(string connectionName, ConnectionSettings settings, bool setAsDefault = false);
        ConnectionSettings GetConnection(string connectionName);
        List<SavedConnection> GetAllConnections();
        ConnectionSettings GetDefaultConnection();
        bool SetDefaultConnection(string connectionName);
        bool DeleteConnection(string connectionName);
        bool ConnectionExists(string connectionName);
        List<string> GetConnectionNames();
        ConnectionSettings LoadConnectionForStartup();
    }

    /// <summary>
    /// Interface for log management
    /// </summary>
    public interface ILogService
    {
        DataTable GetLogs();
        DataTable GetLogs(DateTime? fromDate, DateTime? toDate, string logLevel);
        bool ClearLogs();
        bool ExportLogs(string filePath);
        bool ExportLogs(string filePath, DateTime? fromDate, DateTime? toDate);
        void LogInfo(string message);
        void LogInfo(string message, string jobName);
        void LogWarning(string message);
        void LogWarning(string message, string jobName);
        void LogError(string message);
        void LogError(string message, string jobName);
    }

    /// <summary>
    /// Interface for service management
    /// </summary>
    public interface IServiceManager : IDisposable
    {
        bool StartService();
        bool StopService();
        bool IsServiceRunning();
        string GetServiceStatus();
    }

    /// <summary>
    /// Interface for configuration management
    /// </summary>
    public interface IConfigurationService
    {
        T GetSetting<T>(string key, T defaultValue);
        bool SaveSetting<T>(string key, T value);
        bool DeleteSetting(string key);
        void SaveAllSettings();
        void LoadAllSettings();
    }

    /// <summary>
    /// Interface for managing saved job configurations
    /// </summary>
    public interface ISavedJobConfigurationService
    {
        // Basic CRUD operations
        bool SaveConfiguration(SavedJobConfiguration config);
        SavedJobConfiguration GetConfiguration(string id);
        SavedJobConfiguration GetConfigurationByName(string name);
        List<SavedJobConfiguration> GetAllConfigurations();
        List<SavedJobConfiguration> GetConfigurationsByCategory(string category);
        bool DeleteConfiguration(string id);
        bool ConfigurationExists(string id);
        bool ConfigurationNameExists(string name);
        
        // Import/Export
        bool ExportConfiguration(string id, string filePath);
        bool ExportAllConfigurations(string filePath);
        SavedJobConfiguration ImportConfiguration(string filePath);
        List<SavedJobConfiguration> ImportMultipleConfigurations(string filePath);
        
        // Quick Launch
        List<QuickLaunchItem> GetQuickLaunchItems();
        bool AddToQuickLaunch(string configurationId, QuickLaunchItem item);
        bool RemoveFromQuickLaunch(string configurationId);
        bool UpdateQuickLaunchItem(QuickLaunchItem item);
        bool SetQuickLaunchFavorite(string configurationId, bool isFavorite);
        
        // Default configuration
        SavedJobConfiguration GetDefaultConfiguration();
        bool SetDefaultConfiguration(string id);
        bool ClearDefaultConfiguration();
        
        // Statistics and usage
        bool UpdateUsageStatistics(string id);
        List<SavedJobConfiguration> GetMostUsedConfigurations(int count = 5);
        List<SavedJobConfiguration> GetRecentlyUsedConfigurations(int count = 5);
        
        // Search and filtering
        List<SavedJobConfiguration> SearchConfigurations(string searchTerm);
        List<SavedJobConfiguration> FilterByTags(List<string> tags);
        List<string> GetAllCategories();
        List<string> GetAllTags();
        
        // Validation and cleanup
        bool ValidateConfiguration(SavedJobConfiguration config);
        List<string> GetValidationErrors(SavedJobConfiguration config);
        int CleanupUnusedConfigurations(int daysOld = 30);
        
        // Events
        event EventHandler<SavedJobConfigurationEventArgs> ConfigurationSaved;
        event EventHandler<SavedJobConfigurationEventArgs> ConfigurationDeleted;
        event EventHandler<SavedJobConfigurationEventArgs> ConfigurationLoaded;
    }
    
    /// <summary>
    /// Event arguments for saved job configuration events
    /// </summary>
    public class SavedJobConfigurationEventArgs : EventArgs
    {
        public SavedJobConfiguration Configuration { get; set; }
        public string Operation { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        
        public SavedJobConfigurationEventArgs(SavedJobConfiguration configuration, string operation, bool success = true, string errorMessage = null)
        {
            Configuration = configuration;
            Operation = operation;
            Success = success;
            ErrorMessage = errorMessage;
        }
    }
}
