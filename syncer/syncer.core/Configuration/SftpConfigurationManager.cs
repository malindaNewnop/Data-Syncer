using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using syncer.core.Configuration;

namespace syncer.core
{
    /// <summary>
    /// .NET 3.5 compatibility helper
    /// </summary>
    internal static class StringHelper
    {
        public static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrEmpty(value) || value.Trim().Length == 0;
        }
    }

    /// <summary>
    /// Manages SFTP configuration profiles and settings
    /// </summary>
    public class SftpConfigurationManager
    {
        private readonly string _configDirectory;
        private readonly string _profilesFile;
        private readonly string _globalConfigFile;
        private Dictionary<string, SftpProfile> _profiles;
        private SftpGlobalConfiguration _globalConfig;

        public event EventHandler<ProfileEventArgs> ProfileAdded;
        public event EventHandler<ProfileEventArgs> ProfileRemoved;
        public event EventHandler<ProfileEventArgs> ProfileUpdated;

        public SftpConfigurationManager(string configDirectory = null)
        {
            _configDirectory = configDirectory ?? Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DataSyncer"), "SFTP");
            _profilesFile = Path.Combine(_configDirectory, "profiles.json");
            _globalConfigFile = Path.Combine(_configDirectory, "global-config.json");
            
            EnsureConfigDirectory();
            LoadConfiguration();
        }

        /// <summary>
        /// Gets all available profiles
        /// </summary>
        public IEnumerable<SftpProfile> GetAllProfiles()
        {
            return _profiles.Values.ToList();
        }

        /// <summary>
        /// Gets a profile by name
        /// </summary>
        public SftpProfile GetProfile(string name)
        {
            return _profiles.TryGetValue(name, out var profile) ? profile : null;
        }

        /// <summary>
        /// Adds or updates a profile
        /// </summary>
        public void SaveProfile(SftpProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            if (StringHelper.IsNullOrWhiteSpace(profile.Name))
                throw new ArgumentException("Profile name cannot be empty");

            var isNew = !_profiles.ContainsKey(profile.Name);
            _profiles[profile.Name] = profile;
            
            SaveProfiles();

            if (isNew)
            {
                ProfileAdded?.Invoke(this, new ProfileEventArgs(profile));
            }
            else
            {
                ProfileUpdated?.Invoke(this, new ProfileEventArgs(profile));
            }
        }

        /// <summary>
        /// Removes a profile
        /// </summary>
        public bool RemoveProfile(string name)
        {
            if (_profiles.TryGetValue(name, out var profile))
            {
                _profiles.Remove(name);
                SaveProfiles();
                ProfileRemoved?.Invoke(this, new ProfileEventArgs(profile));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the global configuration
        /// </summary>
        public SftpGlobalConfiguration GetGlobalConfiguration()
        {
            return _globalConfig;
        }

        /// <summary>
        /// Updates the global configuration
        /// </summary>
        public void UpdateGlobalConfiguration(SftpGlobalConfiguration config)
        {
            _globalConfig = config ?? throw new ArgumentNullException(nameof(config));
            SaveGlobalConfiguration();
        }

        /// <summary>
        /// Exports profiles to a file
        /// </summary>
        public void ExportProfiles(string filePath, IEnumerable<string> profileNames = null)
        {
            var profilesToExport = profileNames?.Select(GetProfile).Where(p => p != null).ToList() 
                                 ?? _profiles.Values.ToList();

            var exportData = new
            {
                ExportDate = DateTime.Now,
                Profiles = profilesToExport
            };

            var json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Imports profiles from a file
        /// </summary>
        public void ImportProfiles(string filePath, bool overwriteExisting = false)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Import file not found: {filePath}");

            var json = File.ReadAllText(filePath);
            var importData = JsonConvert.DeserializeAnonymousType(json, new
            {
                ExportDate = DateTime.MinValue,
                Profiles = new List<SftpProfile>()
            });

            foreach (var profile in importData.Profiles)
            {
                if (overwriteExisting || !_profiles.ContainsKey(profile.Name))
                {
                    SaveProfile(profile);
                }
            }
        }

        /// <summary>
        /// Validates a profile configuration
        /// </summary>
        public List<string> ValidateProfile(SftpProfile profile)
        {
            var errors = new List<string>();

            if (StringHelper.IsNullOrWhiteSpace(profile.Name))
                errors.Add("Profile name is required");

            if (StringHelper.IsNullOrWhiteSpace(profile.ConnectionSettings.Host))
                errors.Add("Host is required");

            if (profile.ConnectionSettings.Port <= 0 || profile.ConnectionSettings.Port > 65535)
                errors.Add("Port must be between 1 and 65535");

            if (StringHelper.IsNullOrWhiteSpace(profile.ConnectionSettings.Username))
                errors.Add("Username is required");

            if (StringHelper.IsNullOrWhiteSpace(profile.ConnectionSettings.Password) && 
                StringHelper.IsNullOrWhiteSpace(profile.ConnectionSettings.SshKeyPath))
                errors.Add("Either password or SSH key path is required");

            if (!StringHelper.IsNullOrWhiteSpace(profile.ConnectionSettings.SshKeyPath) && 
                !File.Exists(profile.ConnectionSettings.SshKeyPath))
                errors.Add("SSH key file does not exist");

            return errors;
        }

        /// <summary>
        /// Tests connectivity for a profile
        /// </summary>
        public bool TestProfile(string profileName, out string error)
        {
            error = null;
            var profile = GetProfile(profileName);
            
            if (profile == null)
            {
                error = "Profile not found";
                return false;
            }

            var client = new SftpTransferClient();
            return client.TestConnection(profile.ConnectionSettings, out error);
        }

        /// <summary>
        /// Gets profile usage statistics
        /// </summary>
        public ProfileStatistics GetProfileStatistics(string profileName)
        {
            var profile = GetProfile(profileName);
            if (profile == null)
                return null;

            return new ProfileStatistics
            {
                ProfileName = profileName,
                CreatedDate = profile.CreatedDate,
                LastUsed = profile.LastUsed,
                UsageCount = profile.UsageCount,
                TotalTransfers = profile.TotalTransfers,
                TotalBytesTransferred = profile.TotalBytesTransferred,
                AverageTransferSpeed = profile.AverageTransferSpeed
            };
        }

        /// <summary>
        /// Updates profile usage statistics
        /// </summary>
        public void UpdateProfileUsage(string profileName, long bytesTransferred, double transferSpeed)
        {
            var profile = GetProfile(profileName);
            if (profile != null)
            {
                profile.LastUsed = DateTime.Now;
                profile.UsageCount++;
                profile.TotalTransfers++;
                profile.TotalBytesTransferred += bytesTransferred;
                
                // Calculate moving average of transfer speed
                if (profile.AverageTransferSpeed == 0)
                {
                    profile.AverageTransferSpeed = transferSpeed;
                }
                else
                {
                    profile.AverageTransferSpeed = (profile.AverageTransferSpeed + transferSpeed) / 2;
                }

                SaveProfile(profile);
            }
        }

        /// <summary>
        /// Creates a backup of all configurations
        /// </summary>
        public void CreateBackup(string backupPath)
        {
            var backupData = new
            {
                BackupDate = DateTime.Now,
                GlobalConfiguration = _globalConfig,
                Profiles = _profiles.Values.ToList()
            };

            var json = JsonConvert.SerializeObject(backupData, Formatting.Indented);
            File.WriteAllText(backupPath, json);
        }

        /// <summary>
        /// Restores configuration from backup
        /// </summary>
        public void RestoreFromBackup(string backupPath)
        {
            if (!File.Exists(backupPath))
                throw new FileNotFoundException($"Backup file not found: {backupPath}");

            var json = File.ReadAllText(backupPath);
            var backupData = JsonConvert.DeserializeAnonymousType(json, new
            {
                BackupDate = DateTime.MinValue,
                GlobalConfiguration = new SftpGlobalConfiguration(),
                Profiles = new List<SftpProfile>()
            });

            _globalConfig = backupData.GlobalConfiguration;
            _profiles.Clear();

            foreach (var profile in backupData.Profiles)
            {
                _profiles[profile.Name] = profile;
            }

            SaveConfiguration();
        }

        #region Private Methods

        private void EnsureConfigDirectory()
        {
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
            }
        }

        private void LoadConfiguration()
        {
            LoadProfiles();
            LoadGlobalConfiguration();
        }

        private void LoadProfiles()
        {
            _profiles = new Dictionary<string, SftpProfile>();

            if (File.Exists(_profilesFile))
            {
                try
                {
                    var json = File.ReadAllText(_profilesFile);
                    var profiles = JsonConvert.DeserializeObject<List<SftpProfile>>(json);
                    
                    foreach (var profile in profiles)
                    {
                        _profiles[profile.Name] = profile;
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue with empty profiles
                    System.Diagnostics.Debug.WriteLine($"Failed to load profiles: {ex.Message}");
                }
            }
        }

        private void LoadGlobalConfiguration()
        {
            _globalConfig = new SftpGlobalConfiguration();

            if (File.Exists(_globalConfigFile))
            {
                try
                {
                    var json = File.ReadAllText(_globalConfigFile);
                    _globalConfig = JsonConvert.DeserializeObject<SftpGlobalConfiguration>(json);
                }
                catch (Exception ex)
                {
                    // Log error but continue with default config
                    System.Diagnostics.Debug.WriteLine($"Failed to load global configuration: {ex.Message}");
                }
            }
        }

        private void SaveConfiguration()
        {
            SaveProfiles();
            SaveGlobalConfiguration();
        }

        private void SaveProfiles()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_profiles.Values.ToList(), Formatting.Indented);
                File.WriteAllText(_profilesFile, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save profiles: {ex.Message}", ex);
            }
        }

        private void SaveGlobalConfiguration()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_globalConfig, Formatting.Indented);
                File.WriteAllText(_globalConfigFile, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save global configuration: {ex.Message}", ex);
            }
        }

        #endregion
    }

    /// <summary>
    /// SFTP connection profile
    /// </summary>
    [Serializable]
    public class SftpProfile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ConnectionSettings ConnectionSettings { get; set; } = new ConnectionSettings();
        public SftpConfiguration TransferConfiguration { get; set; } = new SftpConfiguration();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastUsed { get; set; }
        public int UsageCount { get; set; }
        public int TotalTransfers { get; set; }
        public long TotalBytesTransferred { get; set; }
        public double AverageTransferSpeed { get; set; }
        public Dictionary<string, object> CustomProperties { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Global SFTP configuration
    /// </summary>
    [Serializable]
    public class SftpGlobalConfiguration
    {
        public string DefaultKeyDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".ssh");
        public int DefaultConnectionTimeout { get; set; } = 30000;
        public int DefaultOperationTimeout { get; set; } = 60000;
        public int DefaultRetryCount { get; set; } = 3;
        public bool EnableLogging { get; set; } = true;
        public string LogLevel { get; set; } = "Info";
        public string LogDirectory { get; set; } = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DataSyncer"), "Logs");
        public bool AutoCleanupOldLogs { get; set; } = true;
        public int LogRetentionDays { get; set; } = 30;
        public Dictionary<string, object> AdvancedSettings { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Profile usage statistics
    /// </summary>
    public class ProfileStatistics
    {
        public string ProfileName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUsed { get; set; }
        public int UsageCount { get; set; }
        public int TotalTransfers { get; set; }
        public long TotalBytesTransferred { get; set; }
        public double AverageTransferSpeed { get; set; }
    }

    /// <summary>
    /// Event arguments for profile events
    /// </summary>
    public class ProfileEventArgs : EventArgs
    {
        public SftpProfile Profile { get; }

        public ProfileEventArgs(SftpProfile profile)
        {
            Profile = profile;
        }
    }
}
