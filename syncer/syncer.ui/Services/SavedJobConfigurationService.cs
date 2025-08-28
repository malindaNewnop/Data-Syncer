using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace syncer.ui.Services
{
    /// <summary>
    /// .NET 3.5 compatibility helper
    /// </summary>
    internal static class StringExtensionsLocal
    {
        public static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrEmpty(value) || value.Trim().Length == 0;
        }
    }
}

namespace syncer.ui.Services
{
    /// <summary>
    /// Service for managing saved job configurations
    /// Provides functionality to save, load, and manage job configurations with connections
    /// </summary>
    public class SavedJobConfigurationService : ISavedJobConfigurationService
    {
        private readonly string _configDirectory;
        private readonly string _configurationsFile;
        private readonly string _quickLaunchFile;
        private readonly Dictionary<string, SavedJobConfiguration> _configurations;
        private readonly List<QuickLaunchItem> _quickLaunchItems;
        private readonly object _lockObject = new object();

        public event EventHandler<SavedJobConfigurationEventArgs> ConfigurationSaved;
        public event EventHandler<SavedJobConfigurationEventArgs> ConfigurationDeleted;
        public event EventHandler<SavedJobConfigurationEventArgs> ConfigurationLoaded;

        public SavedJobConfigurationService(string configDirectory = null)
        {
            // Set up configuration directory
            _configDirectory = configDirectory ?? Path.Combine(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "DataSyncer"), "SavedConfigurations");
            
            _configurationsFile = Path.Combine(_configDirectory, "configurations.json");
            _quickLaunchFile = Path.Combine(_configDirectory, "quicklaunch.json");
            
            _configurations = new Dictionary<string, SavedJobConfiguration>();
            _quickLaunchItems = new List<QuickLaunchItem>();
            
            EnsureConfigDirectory();
            LoadConfigurations();
        }

        #region Basic CRUD Operations

        public bool SaveConfiguration(SavedJobConfiguration config)
        {
            if (config == null)
                return false;

            try
            {
                lock (_lockObject)
                {
                    // Validate configuration
                    var validationErrors = GetValidationErrors(config);
                    if (validationErrors.Count > 0)
                    {
                        OnConfigurationSaved(config, "Save", false, string.Join("; ", validationErrors.ToArray()));
                        return false;
                    }

                    // Generate new ID if not exists
                    if (string.IsNullOrEmpty(config.Id))
                        config.Id = Guid.NewGuid().ToString();

                    // Update or add configuration
                    bool isUpdate = _configurations.ContainsKey(config.Id);
                    _configurations[config.Id] = config;

                    // Save to file
                    SaveConfigurationsToFile();

                    OnConfigurationSaved(config, isUpdate ? "Update" : "Create", true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnConfigurationSaved(config, "Save", false, ex.Message);
                return false;
            }
        }

        public SavedJobConfiguration GetConfiguration(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            lock (_lockObject)
            {
                SavedJobConfiguration config;
                if (_configurations.TryGetValue(id, out config))
                {
                    OnConfigurationLoaded(config, "Load", true);
                    return config;
                }
                return null;
            }
        }

        public SavedJobConfiguration GetConfigurationByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            lock (_lockObject)
            {
                var config = _configurations.Values.FirstOrDefault(c => 
                    string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
                
                if (config != null)
                    OnConfigurationLoaded(config, "LoadByName", true);
                
                return config;
            }
        }

        public List<SavedJobConfiguration> GetAllConfigurations()
        {
            lock (_lockObject)
            {
                return _configurations.Values.OrderBy(c => c.Name).ToList();
            }
        }

        public List<SavedJobConfiguration> GetConfigurationsByCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return new List<SavedJobConfiguration>();

            lock (_lockObject)
            {
                return _configurations.Values
                    .Where(c => string.Equals(c.Category, category, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(c => c.Name)
                    .ToList();
            }
        }

        public bool DeleteConfiguration(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            try
            {
                lock (_lockObject)
                {
                    SavedJobConfiguration config;
                    if (_configurations.TryGetValue(id, out config))
                    {
                        _configurations.Remove(id);
                        
                        // Remove from quick launch if exists
                        _quickLaunchItems.RemoveAll(q => q.ConfigurationId == id);
                        
                        SaveConfigurationsToFile();
                        SaveQuickLaunchToFile();
                        
                        OnConfigurationDeleted(config, "Delete", true);
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnConfigurationDeleted(null, "Delete", false, ex.Message);
                return false;
            }
        }

        public bool ConfigurationExists(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            lock (_lockObject)
            {
                return _configurations.ContainsKey(id);
            }
        }

        public bool ConfigurationNameExists(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            lock (_lockObject)
            {
                return _configurations.Values.Any(c => 
                    string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
            }
        }

        #endregion

        #region Import/Export

        public bool ExportConfiguration(string id, string filePath)
        {
            var config = GetConfiguration(id);
            if (config == null)
                return false;

            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ExportAllConfigurations(string filePath)
        {
            try
            {
                var allConfigs = GetAllConfigurations();
                var json = JsonConvert.SerializeObject(allConfigs, Formatting.Indented);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public SavedJobConfiguration ImportConfiguration(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                var json = File.ReadAllText(filePath);
                var config = JsonConvert.DeserializeObject<SavedJobConfiguration>(json);
                
                if (config != null)
                {
                    // Generate new ID to avoid conflicts
                    config.Id = Guid.NewGuid().ToString();
                    config.CreatedDate = DateTime.Now;
                    config.LastUsed = null;
                    config.TimesUsed = 0;
                    
                    if (SaveConfiguration(config))
                        return config;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public List<SavedJobConfiguration> ImportMultipleConfigurations(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return new List<SavedJobConfiguration>();

                var json = File.ReadAllText(filePath);
                var configs = JsonConvert.DeserializeObject<List<SavedJobConfiguration>>(json);
                var importedConfigs = new List<SavedJobConfiguration>();
                
                if (configs != null)
                {
                    foreach (var config in configs)
                    {
                        // Generate new ID to avoid conflicts
                        config.Id = Guid.NewGuid().ToString();
                        config.CreatedDate = DateTime.Now;
                        config.LastUsed = null;
                        config.TimesUsed = 0;
                        
                        if (SaveConfiguration(config))
                            importedConfigs.Add(config);
                    }
                }
                
                return importedConfigs;
            }
            catch
            {
                return new List<SavedJobConfiguration>();
            }
        }

        #endregion

        #region Quick Launch

        public List<QuickLaunchItem> GetQuickLaunchItems()
        {
            lock (_lockObject)
            {
                return _quickLaunchItems.OrderBy(q => q.SortOrder).ThenBy(q => q.DisplayName).ToList();
            }
        }

        public bool AddToQuickLaunch(string configurationId, QuickLaunchItem item)
        {
            if (string.IsNullOrEmpty(configurationId) || item == null)
                return false;

            var config = GetConfiguration(configurationId);
            if (config == null)
                return false;

            try
            {
                lock (_lockObject)
                {
                    // Remove existing item for this configuration
                    _quickLaunchItems.RemoveAll(q => q.ConfigurationId == configurationId);
                    
                    // Set configuration ID and defaults
                    item.ConfigurationId = configurationId;
                    if (string.IsNullOrEmpty(item.DisplayName))
                        item.DisplayName = config.DisplayName;
                    if (string.IsNullOrEmpty(item.Description))
                        item.Description = config.FormattedDescription;
                    
                    _quickLaunchItems.Add(item);
                    SaveQuickLaunchToFile();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveFromQuickLaunch(string configurationId)
        {
            if (string.IsNullOrEmpty(configurationId))
                return false;

            try
            {
                lock (_lockObject)
                {
                    int removedCount = _quickLaunchItems.RemoveAll(q => q.ConfigurationId == configurationId);
                    if (removedCount > 0)
                    {
                        SaveQuickLaunchToFile();
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateQuickLaunchItem(QuickLaunchItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.ConfigurationId))
                return false;

            try
            {
                lock (_lockObject)
                {
                    var existingItem = _quickLaunchItems.FirstOrDefault(q => q.ConfigurationId == item.ConfigurationId);
                    if (existingItem != null)
                    {
                        _quickLaunchItems.Remove(existingItem);
                        _quickLaunchItems.Add(item);
                        SaveQuickLaunchToFile();
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool SetQuickLaunchFavorite(string configurationId, bool isFavorite)
        {
            if (string.IsNullOrEmpty(configurationId))
                return false;

            try
            {
                lock (_lockObject)
                {
                    var item = _quickLaunchItems.FirstOrDefault(q => q.ConfigurationId == configurationId);
                    if (item != null)
                    {
                        item.IsFavorite = isFavorite;
                        SaveQuickLaunchToFile();
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Default Configuration

        public SavedJobConfiguration GetDefaultConfiguration()
        {
            lock (_lockObject)
            {
                return _configurations.Values.FirstOrDefault(c => c.IsDefault);
            }
        }

        public bool SetDefaultConfiguration(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            try
            {
                lock (_lockObject)
                {
                    var config = GetConfiguration(id);
                    if (config == null)
                        return false;

                    // Clear existing default
                    foreach (var c in _configurations.Values)
                        c.IsDefault = false;

                    // Set new default
                    config.IsDefault = true;
                    SaveConfigurationsToFile();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool ClearDefaultConfiguration()
        {
            try
            {
                lock (_lockObject)
                {
                    bool hasDefault = false;
                    foreach (var config in _configurations.Values)
                    {
                        if (config.IsDefault)
                        {
                            config.IsDefault = false;
                            hasDefault = true;
                        }
                    }

                    if (hasDefault)
                    {
                        SaveConfigurationsToFile();
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Statistics and Usage

        public bool UpdateUsageStatistics(string id)
        {
            var config = GetConfiguration(id);
            if (config == null)
                return false;

            try
            {
                lock (_lockObject)
                {
                    config.UpdateUsageStatistics();
                    SaveConfigurationsToFile();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public List<SavedJobConfiguration> GetMostUsedConfigurations(int count = 5)
        {
            lock (_lockObject)
            {
                return _configurations.Values
                    .OrderByDescending(c => c.TimesUsed)
                    .ThenByDescending(c => c.LastUsed)
                    .Take(count)
                    .ToList();
            }
        }

        public List<SavedJobConfiguration> GetRecentlyUsedConfigurations(int count = 5)
        {
            lock (_lockObject)
            {
                return _configurations.Values
                    .Where(c => c.LastUsed.HasValue)
                    .OrderByDescending(c => c.LastUsed.Value)
                    .Take(count)
                    .ToList();
            }
        }

        #endregion

        #region Search and Filtering

        public List<SavedJobConfiguration> SearchConfigurations(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return GetAllConfigurations();

            lock (_lockObject)
            {
                var lowerSearchTerm = searchTerm.ToLower();
                return _configurations.Values
                    .Where(c => 
                        (c.Name != null && c.Name.ToLower().Contains(lowerSearchTerm)) ||
                        (c.Description != null && c.Description.ToLower().Contains(lowerSearchTerm)) ||
                        (c.Category != null && c.Category.ToLower().Contains(lowerSearchTerm)) ||
                        (c.JobSettings != null && c.JobSettings.Name != null && c.JobSettings.Name.ToLower().Contains(lowerSearchTerm)))
                    .OrderBy(c => c.Name)
                    .ToList();
            }
        }

        public List<SavedJobConfiguration> FilterByTags(List<string> tags)
        {
            if (tags == null || tags.Count == 0)
                return GetAllConfigurations();

            lock (_lockObject)
            {
                return _configurations.Values
                    .Where(c => c.Tags != null && tags.Any(tag => 
                        c.Tags.Any(ctag => string.Equals(ctag, tag, StringComparison.OrdinalIgnoreCase))))
                    .OrderBy(c => c.Name)
                    .ToList();
            }
        }

        public List<string> GetAllCategories()
        {
            lock (_lockObject)
            {
                return _configurations.Values
                    .Where(c => !string.IsNullOrEmpty(c.Category))
                    .Select(c => c.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
            }
        }

        public List<string> GetAllTags()
        {
            lock (_lockObject)
            {
                var allTags = new List<string>();
                foreach (var config in _configurations.Values)
                {
                    if (config.Tags != null)
                        allTags.AddRange(config.Tags);
                }
                return allTags.Distinct().OrderBy(t => t).ToList();
            }
        }

        #endregion

        #region Validation and Cleanup

        public bool ValidateConfiguration(SavedJobConfiguration config)
        {
            return GetValidationErrors(config).Count == 0;
        }

        public List<string> GetValidationErrors(SavedJobConfiguration config)
        {
            var errors = new List<string>();
            
            if (config == null)
            {
                errors.Add("Configuration is null");
                return errors;
            }

            if (string.IsNullOrEmpty(config.Name))
                errors.Add("Configuration name is required");

            if (config.JobSettings == null)
                errors.Add("Job settings are required");
            else
                errors.AddRange(config.JobSettings.ValidateConfiguration());

            if (config.SourceConnection?.Settings == null)
                errors.Add("Source connection settings are required");

            if (config.DestinationConnection?.Settings == null)
                errors.Add("Destination connection settings are required");

            return errors;
        }

        public int CleanupUnusedConfigurations(int daysOld = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-daysOld);
            var configsToDelete = new List<string>();

            lock (_lockObject)
            {
                foreach (var config in _configurations.Values)
                {
                    // Don't delete default configurations
                    if (config.IsDefault)
                        continue;

                    // Don't delete configurations that have been used recently
                    if (config.LastUsed.HasValue && config.LastUsed.Value > cutoffDate)
                        continue;

                    // Don't delete frequently used configurations
                    if (config.TimesUsed > 5)
                        continue;

                    // Delete if created long ago and never used or used infrequently
                    if (config.CreatedDate < cutoffDate && 
                        (!config.LastUsed.HasValue || config.TimesUsed <= 1))
                    {
                        configsToDelete.Add(config.Id);
                    }
                }

                foreach (var id in configsToDelete)
                {
                    DeleteConfiguration(id);
                }
            }

            return configsToDelete.Count;
        }

        #endregion

        #region Private Methods

        private void EnsureConfigDirectory()
        {
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
            }
        }

        private void LoadConfigurations()
        {
            try
            {
                if (File.Exists(_configurationsFile))
                {
                    var json = File.ReadAllText(_configurationsFile);
                    var configList = JsonConvert.DeserializeObject<List<SavedJobConfiguration>>(json);
                    
                    lock (_lockObject)
                    {
                        _configurations.Clear();
                        if (configList != null)
                        {
                            foreach (var config in configList)
                            {
                                if (!string.IsNullOrEmpty(config.Id))
                                    _configurations[config.Id] = config;
                            }
                        }
                    }
                }

                if (File.Exists(_quickLaunchFile))
                {
                    var json = File.ReadAllText(_quickLaunchFile);
                    var quickLaunchList = JsonConvert.DeserializeObject<List<QuickLaunchItem>>(json);
                    
                    lock (_lockObject)
                    {
                        _quickLaunchItems.Clear();
                        if (quickLaunchList != null)
                            _quickLaunchItems.AddRange(quickLaunchList);
                    }
                }
            }
            catch
            {
                // If loading fails, start with empty collections
                lock (_lockObject)
                {
                    _configurations.Clear();
                    _quickLaunchItems.Clear();
                }
            }
        }

        private void SaveConfigurationsToFile()
        {
            try
            {
                var configList = _configurations.Values.ToList();
                var json = JsonConvert.SerializeObject(configList, Formatting.Indented);
                File.WriteAllText(_configurationsFile, json);
            }
            catch
            {
                // Ignore save errors for now
            }
        }

        private void SaveQuickLaunchToFile()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_quickLaunchItems, Formatting.Indented);
                File.WriteAllText(_quickLaunchFile, json);
            }
            catch
            {
                // Ignore save errors for now
            }
        }

        #endregion

        #region Event Helpers

        private void OnConfigurationSaved(SavedJobConfiguration config, string operation, bool success, string errorMessage = null)
        {
            ConfigurationSaved?.Invoke(this, new SavedJobConfigurationEventArgs(config, operation, success, errorMessage));
        }

        private void OnConfigurationDeleted(SavedJobConfiguration config, string operation, bool success, string errorMessage = null)
        {
            ConfigurationDeleted?.Invoke(this, new SavedJobConfigurationEventArgs(config, operation, success, errorMessage));
        }

        private void OnConfigurationLoaded(SavedJobConfiguration config, string operation, bool success, string errorMessage = null)
        {
            ConfigurationLoaded?.Invoke(this, new SavedJobConfigurationEventArgs(config, operation, success, errorMessage));
        }

        #endregion
    }
}
