using System;
using System.IO;
using syncer.core;
using syncer.core.Utilities;

namespace syncer.core.Services
{
    /// <summary>
    /// Enhanced Settings Service with improved JSON serialization and error handling
    /// Compatible with .NET 3.5
    /// </summary>
    public class EnhancedSettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private AppSettingsEnhanced _currentSettings;
        private readonly object _lockObject = new object();

        public EnhancedSettingsService()
        {
            _settingsFilePath = Paths.SettingsFile;
            _currentSettings = LoadSettings();
        }

        public EnhancedSettingsService(string settingsPath)
        {
            _settingsFilePath = settingsPath ?? throw new ArgumentNullException("settingsPath");
            _currentSettings = LoadSettings();
        }

        public AppSettings GetSettings()
        {
            lock (_lockObject)
            {
                // Convert enhanced settings to base settings for compatibility
                var enhanced = GetEnhancedSettings();
                return new AppSettings
                {
                    LogLevel = enhanced.LogLevel,
                    MaxLogFileSize = enhanced.MaxLogFileSize,
                    KeepLogDays = enhanced.KeepLogDays,
                    AutoStartService = enhanced.AutoStartService,
                    EnableLogging = enhanced.EnableLogging,
                    PipeName = enhanced.PipeName,
                    ServiceName = enhanced.ServiceName,
                    ServiceDisplayName = enhanced.ServiceDisplayName,
                    TempFolder = enhanced.TempFolder,
                    LogFolder = enhanced.LogFolder
                };
            }
        }

        public AppSettingsEnhanced GetEnhancedSettings()
        {
            lock (_lockObject)
            {
                return _currentSettings ?? new AppSettingsEnhanced();
            }
        }

        public bool SaveSettings(AppSettings settings)
        {
            if (settings == null) return false;

            // Convert to enhanced settings
            var enhanced = new AppSettingsEnhanced
            {
                LogLevel = settings.LogLevel,
                MaxLogFileSize = settings.MaxLogFileSize,
                KeepLogDays = settings.KeepLogDays,
                AutoStartService = settings.AutoStartService,
                EnableLogging = settings.EnableLogging,
                PipeName = settings.PipeName,
                ServiceName = settings.ServiceName,
                ServiceDisplayName = settings.ServiceDisplayName,
                TempFolder = settings.TempFolder,
                LogFolder = settings.LogFolder
            };

            return SaveEnhancedSettings(enhanced);
        }

        public bool SaveEnhancedSettings(AppSettingsEnhanced settings)
        {
            if (settings == null) return false;

            lock (_lockObject)
            {
                try
                {
                    // Ensure directory exists
                    var directory = Path.GetDirectoryName(_settingsFilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Create backup
                    var backupFile = _settingsFilePath + ".backup";
                    if (File.Exists(_settingsFilePath))
                    {
                        File.Copy(_settingsFilePath, backupFile, true);
                    }

                    // Serialize to JSON using enhanced serializer
                    var json = SimpleJsonSerializer.Serialize(settings);
                    
                    // Format JSON for readability
                    var formattedJson = FormatJson(json);
                    
                    // Write to file
                    File.WriteAllText(_settingsFilePath, formattedJson);
                    
                    // Clean up backup on success
                    if (File.Exists(backupFile))
                    {
                        File.Delete(backupFile);
                    }

                    _currentSettings = settings;
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
                    
                    // Restore from backup if available
                    var backupFile = _settingsFilePath + ".backup";
                    if (File.Exists(backupFile))
                    {
                        try
                        {
                            File.Copy(backupFile, _settingsFilePath, true);
                            File.Delete(backupFile);
                        }
                        catch { }
                    }
                    
                    return false;
                }
            }
        }

        public bool ResetToDefaults()
        {
            var defaultSettings = new AppSettingsEnhanced();
            return SaveEnhancedSettings(defaultSettings);
        }

        public T GetSetting<T>(string key, T defaultValue = default(T))
        {
            try
            {
                var settings = GetEnhancedSettings();
                
                switch (key.ToLower())
                {
                    case "loglevel":
                        if (typeof(T) == typeof(string))
                            return (T)(object)(settings.LogLevel ?? defaultValue.ToString());
                        break;
                    case "maxlogfilesize":
                        if (typeof(T) == typeof(long))
                            return (T)(object)settings.MaxLogFileSize;
                        break;
                    case "keeplogdays":
                        if (typeof(T) == typeof(int))
                            return (T)(object)settings.KeepLogDays;
                        break;
                    case "autostartservice":
                        if (typeof(T) == typeof(bool))
                            return (T)(object)settings.AutoStartService;
                        break;
                    case "enablelogging":
                        if (typeof(T) == typeof(bool))
                            return (T)(object)settings.EnableLogging;
                        break;
                    case "defaultretrycount":
                        if (typeof(T) == typeof(int))
                            return (T)(object)settings.DefaultRetryCount;
                        break;
                    case "connectiontimeout":
                        if (typeof(T) == typeof(int))
                            return (T)(object)settings.ConnectionTimeoutSeconds;
                        break;
                    case "maxconcurrentjobs":
                        if (typeof(T) == typeof(int))
                            return (T)(object)settings.MaxConcurrentJobs;
                        break;
                    case "minimizetotray":
                        if (typeof(T) == typeof(bool))
                            return (T)(object)settings.MinimizeToTray;
                        break;
                    case "enabletransferresume":
                        if (typeof(T) == typeof(bool))
                            return (T)(object)settings.EnableTransferResume;
                        break;
                }
                
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public bool SetSetting<T>(string key, T value)
        {
            try
            {
                var settings = GetEnhancedSettings();
                
                switch (key.ToLower())
                {
                    case "loglevel":
                        settings.LogLevel = value?.ToString() ?? "Info";
                        break;
                    case "maxlogfilesize":
                        if (value is long)
                            settings.MaxLogFileSize = (long)(object)value;
                        break;
                    case "keeplogdays":
                        if (value is int)
                            settings.KeepLogDays = (int)(object)value;
                        break;
                    case "autostartservice":
                        if (value is bool)
                            settings.AutoStartService = (bool)(object)value;
                        break;
                    case "enablelogging":
                        if (value is bool)
                            settings.EnableLogging = (bool)(object)value;
                        break;
                    case "defaultretrycount":
                        if (value is int)
                            settings.DefaultRetryCount = (int)(object)value;
                        break;
                    case "connectiontimeout":
                        if (value is int)
                            settings.ConnectionTimeoutSeconds = (int)(object)value;
                        break;
                    case "maxconcurrentjobs":
                        if (value is int)
                            settings.MaxConcurrentJobs = (int)(object)value;
                        break;
                    case "minimizetotray":
                        if (value is bool)
                            settings.MinimizeToTray = (bool)(object)value;
                        break;
                    case "enabletransferresume":
                        if (value is bool)
                            settings.EnableTransferResume = (bool)(object)value;
                        break;
                    default:
                        return false; // Unknown setting
                }
                
                return SaveEnhancedSettings(settings);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Export settings to file
        /// </summary>
        public bool ExportSettings(string filePath)
        {
            try
            {
                var settings = GetEnhancedSettings();
                var json = SimpleJsonSerializer.Serialize(settings);
                var formattedJson = FormatJson(json);
                
                File.WriteAllText(filePath, formattedJson);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Import settings from file
        /// </summary>
        public bool ImportSettings(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return false;
                
                var json = File.ReadAllText(filePath);
                var settings = SimpleJsonSerializer.Deserialize<AppSettingsEnhanced>(json);
                
                if (settings != null)
                {
                    return SaveEnhancedSettings(settings);
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate settings integrity
        /// </summary>
        public bool ValidateSettings()
        {
            try
            {
                var settings = GetEnhancedSettings();
                
                // Basic validation rules
                if (settings.MaxLogFileSize <= 0) return false;
                if (settings.KeepLogDays < 0) return false;
                if (settings.DefaultRetryCount < 0) return false;
                if (settings.ConnectionTimeoutSeconds <= 0) return false;
                if (settings.MaxConcurrentJobs <= 0) return false;
                if (string.IsNullOrEmpty(settings.LogLevel)) return false;
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        private AppSettingsEnhanced LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    return new AppSettingsEnhanced(); // Return defaults
                }

                var json = File.ReadAllText(_settingsFilePath);
                
                // Try enhanced deserializer first
                if (SimpleJsonSerializer.TryDeserialize<AppSettingsEnhanced>(json, out AppSettingsEnhanced enhancedSettings))
                {
                    return enhancedSettings;
                }
                
                // Fallback to basic AppSettings for backward compatibility
                if (SimpleJsonSerializer.TryDeserialize<AppSettings>(json, out AppSettings basicSettings))
                {
                    return ConvertToEnhanced(basicSettings);
                }
                
                // If all else fails, return defaults
                return new AppSettingsEnhanced();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
                return new AppSettingsEnhanced(); // Return defaults on error
            }
        }

        private AppSettingsEnhanced ConvertToEnhanced(AppSettings basic)
        {
            var enhanced = new AppSettingsEnhanced();
            
            // Copy basic properties
            enhanced.LogLevel = basic.LogLevel;
            enhanced.MaxLogFileSize = basic.MaxLogFileSize;
            enhanced.KeepLogDays = basic.KeepLogDays;
            enhanced.AutoStartService = basic.AutoStartService;
            enhanced.EnableLogging = basic.EnableLogging;
            enhanced.PipeName = basic.PipeName;
            enhanced.ServiceName = basic.ServiceName;
            enhanced.ServiceDisplayName = basic.ServiceDisplayName;
            enhanced.TempFolder = basic.TempFolder;
            enhanced.LogFolder = basic.LogFolder;
            
            // Enhanced properties will use defaults from constructor
            return enhanced;
        }

        private string FormatJson(string json)
        {
            // Simple JSON formatting for readability
            var formatted = json.Replace(",", ",\n  ")
                               .Replace("{", "{\n  ")
                               .Replace("}", "\n}")
                               .Replace("[", "[\n    ")
                               .Replace("]", "\n  ]");
                               
            return formatted;
        }
    }
}
