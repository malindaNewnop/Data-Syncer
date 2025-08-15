using System;
using System.IO;
using System.Xml.Serialization;

namespace syncer.core
{
    /// <summary>
    /// Configuration settings for multi-job execution
    /// </summary>
    [Serializable]
    [XmlRoot("MultiJobConfiguration")]
    public class MultiJobConfiguration
    {
        /// <summary>
        /// Maximum number of jobs that can run concurrently across all queues
        /// </summary>
        [XmlAttribute]
        public int GlobalMaxConcurrentJobs { get; set; } = 5;

        /// <summary>
        /// Default maximum concurrent jobs per queue
        /// </summary>
        [XmlAttribute]
        public int DefaultQueueMaxConcurrentJobs { get; set; } = 3;

        /// <summary>
        /// Interval in seconds for queue processing checks
        /// </summary>
        [XmlAttribute]
        public int QueueProcessingIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// Maximum retry attempts for failed jobs
        /// </summary>
        [XmlAttribute]
        public int DefaultMaxRetries { get; set; } = 3;

        /// <summary>
        /// Delay in minutes between retry attempts
        /// </summary>
        [XmlAttribute]
        public int RetryDelayMinutes { get; set; } = 5;

        /// <summary>
        /// Whether to automatically retry failed jobs
        /// </summary>
        [XmlAttribute]
        public bool EnableAutoRetry { get; set; } = true;

        /// <summary>
        /// Whether to enable job dependency checking
        /// </summary>
        [XmlAttribute]
        public bool EnableDependencyChecking { get; set; } = true;

        /// <summary>
        /// Whether to enable job prioritization
        /// </summary>
        [XmlAttribute]
        public bool EnableJobPrioritization { get; set; } = true;

        /// <summary>
        /// Maximum job execution timeout in minutes (0 = no timeout)
        /// </summary>
        [XmlAttribute]
        public int JobTimeoutMinutes { get; set; } = 60;

        /// <summary>
        /// Whether to enable detailed job execution logging
        /// </summary>
        [XmlAttribute]
        public bool EnableDetailedLogging { get; set; } = true;

        /// <summary>
        /// Whether to preserve completed jobs in queue for history
        /// </summary>
        [XmlAttribute]
        public bool PreserveCompletedJobs { get; set; } = true;

        /// <summary>
        /// Number of days to keep completed job history
        /// </summary>
        [XmlAttribute]
        public int CompletedJobHistoryDays { get; set; } = 30;

        /// <summary>
        /// Whether to enable queue statistics collection
        /// </summary>
        [XmlAttribute]
        public bool EnableQueueStatistics { get; set; } = true;

        public MultiJobConfiguration()
        {
            // Default values are set in property initializers
        }
    }

    /// <summary>
    /// Service for managing multi-job configuration
    /// </summary>
    public class MultiJobConfigurationService
    {
        private readonly string _configFilePath;
        private readonly ILogService _logService;
        private MultiJobConfiguration _configuration;
        private readonly object _lockObject = new object();

        public MultiJobConfigurationService(ILogService logService = null)
        {
            _logService = logService;
            _configFilePath = Path.Combine(Paths.AppDataFolder, "MultiJobConfiguration.xml");
            LoadConfiguration();
        }

        public MultiJobConfiguration GetConfiguration()
        {
            lock (_lockObject)
            {
                return _configuration;
            }
        }

        public bool UpdateConfiguration(MultiJobConfiguration configuration)
        {
            if (configuration == null) return false;

            lock (_lockObject)
            {
                try
                {
                    // Validate configuration values
                    ValidateConfiguration(configuration);

                    _configuration = configuration;
                    SaveConfiguration();

                    _logService?.LogInfo("Multi-job configuration updated successfully", "MultiJobConfigurationService");
                    return true;
                }
                catch (Exception ex)
                {
                    _logService?.LogError($"Failed to update multi-job configuration: {ex.Message}", "MultiJobConfigurationService");
                    return false;
                }
            }
        }

        public bool ResetToDefaults()
        {
            lock (_lockObject)
            {
                try
                {
                    _configuration = new MultiJobConfiguration();
                    SaveConfiguration();

                    _logService?.LogInfo("Multi-job configuration reset to defaults", "MultiJobConfigurationService");
                    return true;
                }
                catch (Exception ex)
                {
                    _logService?.LogError($"Failed to reset multi-job configuration: {ex.Message}", "MultiJobConfigurationService");
                    return false;
                }
            }
        }

        private void LoadConfiguration()
        {
            lock (_lockObject)
            {
                try
                {
                    if (File.Exists(_configFilePath))
                    {
                        using (var fs = new FileStream(_configFilePath, FileMode.Open, FileAccess.Read))
                        {
                            var serializer = new XmlSerializer(typeof(MultiJobConfiguration));
                            _configuration = (MultiJobConfiguration)serializer.Deserialize(fs);
                        }

                        _logService?.LogInfo("Multi-job configuration loaded from XML", "MultiJobConfigurationService");
                    }
                    else
                    {
                        _configuration = new MultiJobConfiguration();
                        SaveConfiguration(); // Create default configuration file

                        _logService?.LogInfo("Created default multi-job configuration", "MultiJobConfigurationService");
                    }

                    ValidateConfiguration(_configuration);
                }
                catch (Exception ex)
                {
                    _logService?.LogError($"Failed to load multi-job configuration, using defaults: {ex.Message}", "MultiJobConfigurationService");
                    _configuration = new MultiJobConfiguration();
                }
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                var directory = Path.GetDirectoryName(_configFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var fs = new FileStream(_configFilePath, FileMode.Create, FileAccess.Write))
                {
                    var serializer = new XmlSerializer(typeof(MultiJobConfiguration));
                    serializer.Serialize(fs, _configuration);
                }
            }
            catch (Exception ex)
            {
                _logService?.LogError($"Failed to save multi-job configuration: {ex.Message}", "MultiJobConfigurationService");
                throw;
            }
        }

        private void ValidateConfiguration(MultiJobConfiguration config)
        {
            if (config.GlobalMaxConcurrentJobs < 1)
                config.GlobalMaxConcurrentJobs = 1;

            if (config.DefaultQueueMaxConcurrentJobs < 1)
                config.DefaultQueueMaxConcurrentJobs = 1;

            if (config.QueueProcessingIntervalSeconds < 1)
                config.QueueProcessingIntervalSeconds = 1;

            if (config.DefaultMaxRetries < 0)
                config.DefaultMaxRetries = 0;

            if (config.RetryDelayMinutes < 0)
                config.RetryDelayMinutes = 1;

            if (config.JobTimeoutMinutes < 0)
                config.JobTimeoutMinutes = 0;

            if (config.CompletedJobHistoryDays < 1)
                config.CompletedJobHistoryDays = 1;
        }
    }
}
