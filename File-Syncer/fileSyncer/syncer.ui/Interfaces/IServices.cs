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
    }

    /// <summary>
    /// Interface for managing filter settings
    /// </summary>
    public interface IFilterService
    {
        FilterSettings GetFilterSettings();
        bool SaveFilterSettings(FilterSettings settings);
        string[] GetDefaultFileTypes();
    }

    /// <summary>
    /// Interface for log management
    /// </summary>
    public interface ILogService
    {
        DataTable GetLogs(DateTime? fromDate = null, DateTime? toDate = null, string logLevel = null);
        bool ClearLogs();
        bool ExportLogs(string filePath, DateTime? fromDate = null, DateTime? toDate = null);
        void LogInfo(string message, string jobName = "");
        void LogWarning(string message, string jobName = "");
        void LogError(string message, string jobName = "");
    }

    /// <summary>
    /// Interface for service management
    /// </summary>
    public interface IServiceManager
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
        T GetSetting<T>(string key, T defaultValue = default(T));
        bool SaveSetting<T>(string key, T value);
        bool DeleteSetting(string key);
        void SaveAllSettings();
        void LoadAllSettings();
    }
}