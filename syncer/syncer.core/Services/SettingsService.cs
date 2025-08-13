using System;
using System.IO;

namespace syncer.core
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private AppSettings _currentSettings;
        private readonly object _lockObject = new object();

        public SettingsService()
        {
            _settingsFilePath = Paths.SettingsFile;
            _currentSettings = LoadSettings();
        }

        public AppSettings GetSettings()
        {
            lock (_lockObject)
            {
                return _currentSettings ?? new AppSettings();
            }
        }

        public bool SaveSettings(AppSettings settings)
        {
            if (settings == null)
                return false;

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

                    // Serialize to JSON (manual implementation for .NET 3.5 compatibility)
                    var json = SerializeToJson(settings);
                    
                    // Write to file with backup
                    var backupFile = _settingsFilePath + ".backup";
                    if (File.Exists(_settingsFilePath))
                    {
                        File.Copy(_settingsFilePath, backupFile, true);
                    }

                    File.WriteAllText(_settingsFilePath, json);
                    
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
            var defaultSettings = new AppSettings();
            return SaveSettings(defaultSettings);
        }

        public T GetSetting<T>(string key, T defaultValue = default(T))
        {
            try
            {
                var settings = GetSettings();
                
                // Simple key-value lookup (you might want to implement a more sophisticated approach)
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
                var settings = GetSettings();
                
                // Simple key-value setting (you might want to implement a more sophisticated approach)
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
                    default:
                        return false; // Unknown setting
                }
                
                return SaveSettings(settings);
            }
            catch
            {
                return false;
            }
        }

        private AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    return new AppSettings(); // Return defaults
                }

                var json = File.ReadAllText(_settingsFilePath);
                return DeserializeFromJson(json) ?? new AppSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
                return new AppSettings(); // Return defaults on error
            }
        }

        // Manual JSON serialization for .NET 3.5 compatibility
        private string SerializeToJson(AppSettings settings)
        {
            var json = new System.Text.StringBuilder();
            json.AppendLine("{");
            
            json.AppendLine($"  \"LogLevel\": \"{EscapeJsonString(settings.LogLevel)}\",");
            json.AppendLine($"  \"MaxLogFileSize\": {settings.MaxLogFileSize},");
            json.AppendLine($"  \"KeepLogDays\": {settings.KeepLogDays},");
            json.AppendLine($"  \"AutoStartService\": {settings.AutoStartService.ToString().ToLower()},");
            json.AppendLine($"  \"EnableLogging\": {settings.EnableLogging.ToString().ToLower()},");
            json.AppendLine($"  \"PipeName\": \"{EscapeJsonString(settings.PipeName)}\",");
            json.AppendLine($"  \"ServiceName\": \"{EscapeJsonString(settings.ServiceName)}\",");
            json.AppendLine($"  \"ServiceDisplayName\": \"{EscapeJsonString(settings.ServiceDisplayName)}\",");
            json.AppendLine($"  \"TempFolder\": \"{EscapeJsonString(settings.TempFolder)}\",");
            json.AppendLine($"  \"LogFolder\": \"{EscapeJsonString(settings.LogFolder)}\"");
            
            json.AppendLine("}");
            return json.ToString();
        }

        private AppSettings DeserializeFromJson(string json)
        {
            try
            {
                var settings = new AppSettings();
                
                // Simple JSON parsing for .NET 3.5 compatibility
                var lines = json.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.Contains(":"))
                    {
                        var parts = trimmed.Split(new char[] { ':' }, 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim().Trim('"');
                            var value = parts[1].Trim().TrimEnd(',').Trim();
                            
                            switch (key)
                            {
                                case "LogLevel":
                                    settings.LogLevel = UnescapeJsonString(value.Trim('"'));
                                    break;
                                case "MaxLogFileSize":
                                    if (long.TryParse(value, out long maxLogSize))
                                        settings.MaxLogFileSize = maxLogSize;
                                    break;
                                case "KeepLogDays":
                                    if (int.TryParse(value, out int keepDays))
                                        settings.KeepLogDays = keepDays;
                                    break;
                                case "AutoStartService":
                                    if (bool.TryParse(value, out bool autoStart))
                                        settings.AutoStartService = autoStart;
                                    break;
                                case "EnableLogging":
                                    if (bool.TryParse(value, out bool enableLogging))
                                        settings.EnableLogging = enableLogging;
                                    break;
                                case "PipeName":
                                    settings.PipeName = UnescapeJsonString(value.Trim('"'));
                                    break;
                                case "ServiceName":
                                    settings.ServiceName = UnescapeJsonString(value.Trim('"'));
                                    break;
                                case "ServiceDisplayName":
                                    settings.ServiceDisplayName = UnescapeJsonString(value.Trim('"'));
                                    break;
                                case "TempFolder":
                                    settings.TempFolder = UnescapeJsonString(value.Trim('"'));
                                    break;
                                case "LogFolder":
                                    settings.LogFolder = UnescapeJsonString(value.Trim('"'));
                                    break;
                            }
                        }
                    }
                }
                
                return settings;
            }
            catch
            {
                return new AppSettings(); // Return defaults on parse error
            }
        }

        private string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
                
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        private string UnescapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
                
            return str.Replace("\\\"", "\"").Replace("\\\\", "\\").Replace("\\r", "\r").Replace("\\n", "\n");
        }
    }

    // AppSettings class for configuration
    public class AppSettings
    {
        public string LogLevel { get; set; }
        public long MaxLogFileSize { get; set; }
        public int KeepLogDays { get; set; }
        public bool AutoStartService { get; set; }
        public bool EnableLogging { get; set; }
        public string PipeName { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDisplayName { get; set; }
        public string TempFolder { get; set; }
        public string LogFolder { get; set; }

        public AppSettings()
        {
            // Set defaults
            LogLevel = "Info";
            MaxLogFileSize = 10 * 1024 * 1024; // 10 MB
            KeepLogDays = 30;
            AutoStartService = true;
            EnableLogging = true;
            PipeName = Paths.PipeName;
            ServiceName = "DataSyncerService";
            ServiceDisplayName = "Data Syncer Service";
            TempFolder = Paths.TempFolder;
            
        }
    }
}
