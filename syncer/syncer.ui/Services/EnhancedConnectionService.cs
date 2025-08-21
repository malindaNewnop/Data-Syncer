using System;
using System.Collections.Generic;

namespace syncer.ui.Services
{
    /// <summary>
    /// Enhanced connection service with persistent storage and multiple connection support
    /// </summary>
    public class EnhancedConnectionService : IConnectionService
    {
        private readonly ConnectionStorageService _storageService;
        private ConnectionSettings _currentSettings;
        private readonly object _lockObject = new object();

        public EnhancedConnectionService()
        {
            _storageService = new ConnectionStorageService();
            
            // Try to load default connection on startup
            var defaultConnection = _storageService.LoadConnectionForStartup();
            if (defaultConnection != null)
            {
                _currentSettings = defaultConnection;
                ServiceLocator.LogService?.LogInfo("Default connection loaded automatically on service initialization");
            }
            else
            {
                _currentSettings = new ConnectionSettings();
            }
        }

        #region Basic Connection Service Methods (Backward Compatibility)

        public ConnectionSettings GetConnectionSettings()
        {
            lock (_lockObject)
            {
                return _currentSettings?.Clone() ?? new ConnectionSettings();
            }
        }

        public bool SaveConnectionSettings(ConnectionSettings settings)
        {
            if (settings == null) return false;

            lock (_lockObject)
            {
                _currentSettings = settings.Clone();
                
                // Also save as default for backward compatibility
                return _storageService.SaveConnection("Default", _currentSettings, true);
            }
        }

        public bool TestConnection(ConnectionSettings settings)
        {
            if (settings == null) return false;

            try
            {
                // Use the core connection service adapter for testing
                var adapter = new CoreConnectionServiceAdapter();
                return adapter.TestConnection(settings);
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService?.LogError($"Connection test failed: {ex.Message}");
                return false;
            }
        }

        public bool IsConnected()
        {
            lock (_lockObject)
            {
                return _currentSettings?.IsConnected ?? false;
            }
        }

        #endregion

        #region Enhanced Connection Management

        public bool SaveConnection(string connectionName, ConnectionSettings settings, bool setAsDefault = false)
        {
            if (string.IsNullOrWhiteSpace(connectionName) || settings == null)
                return false;

            bool result = _storageService.SaveConnection(connectionName, settings, setAsDefault);

            if (result && setAsDefault)
            {
                lock (_lockObject)
                {
                    _currentSettings = settings.Clone();
                }
            }

            return result;
        }

        public ConnectionSettings GetConnection(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
                return null;

            var connection = _storageService.GetConnection(connectionName);
            
            if (connection != null)
            {
                lock (_lockObject)
                {
                    _currentSettings = connection.Clone();
                }
            }

            return connection;
        }

        public List<SavedConnection> GetAllConnections()
        {
            return _storageService.GetAllConnections();
        }

        public ConnectionSettings GetDefaultConnection()
        {
            var defaultConnection = _storageService.GetDefaultConnection();
            
            if (defaultConnection != null)
            {
                lock (_lockObject)
                {
                    _currentSettings = defaultConnection.Clone();
                }
            }

            return defaultConnection;
        }

        public bool SetDefaultConnection(string connectionName)
        {
            bool result = _storageService.SetDefaultConnection(connectionName);
            
            if (result)
            {
                // Update current settings to the new default
                var defaultConnection = _storageService.GetDefaultConnection();
                if (defaultConnection != null)
                {
                    lock (_lockObject)
                    {
                        _currentSettings = defaultConnection.Clone();
                    }
                }
            }

            return result;
        }

        public bool DeleteConnection(string connectionName)
        {
            return _storageService.DeleteConnection(connectionName);
        }

        public bool ConnectionExists(string connectionName)
        {
            return _storageService.ConnectionExists(connectionName);
        }

        public List<string> GetConnectionNames()
        {
            return _storageService.GetConnectionNames();
        }

        public ConnectionSettings LoadConnectionForStartup()
        {
            var connection = _storageService.LoadConnectionForStartup();
            
            if (connection != null)
            {
                lock (_lockObject)
                {
                    _currentSettings = connection.Clone();
                }
            }

            return connection;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generate a connection name based on connection settings
        /// </summary>
        public string GenerateConnectionName(ConnectionSettings settings)
        {
            if (settings == null) return "New Connection";

            if (settings.IsLocalConnection)
                return "Local File System";

            if (!string.IsNullOrWhiteSpace(settings.Host))
            {
                string name = $"{settings.Protocol} - {settings.Host}";
                if (!string.IsNullOrWhiteSpace(settings.Username))
                    name = $"{settings.Protocol} - {settings.Username}@{settings.Host}";
                
                if (settings.Port != 21 && settings.Port != 22 && settings.Port > 0)
                    name += $":{settings.Port}";

                return name;
            }

            return "New Connection";
        }

        /// <summary>
        /// Import connections from old registry-based storage (migration helper)
        /// </summary>
        public int ImportConnectionsFromRegistry()
        {
            int imported = 0;
            
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\DataSyncer\\Connections"))
                {
                    if (key != null)
                    {
                        foreach (string connectionName in key.GetSubKeyNames())
                        {
                            try
                            {
                                using (Microsoft.Win32.RegistryKey connectionKey = key.OpenSubKey(connectionName))
                                {
                                    if (connectionKey != null)
                                    {
                                        var settings = new ConnectionSettings
                                        {
                                            Protocol = connectionKey.GetValue("Protocol", "LOCAL").ToString(),
                                            ProtocolType = Convert.ToInt32(connectionKey.GetValue("ProtocolType", 0)),
                                            Host = connectionKey.GetValue("Host", "").ToString(),
                                            Port = Convert.ToInt32(connectionKey.GetValue("Port", 21)),
                                            Username = connectionKey.GetValue("Username", "").ToString()
                                            // Note: Don't import passwords for security reasons
                                        };

                                        if (SaveConnection(connectionName, settings))
                                        {
                                            imported++;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ServiceLocator.LogService?.LogWarning($"Failed to import connection '{connectionName}': {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService?.LogWarning($"Failed to import connections from registry: {ex.Message}");
            }

            if (imported > 0)
            {
                ServiceLocator.LogService?.LogInfo($"Successfully imported {imported} connections from registry");
            }

            return imported;
        }

        #endregion
    }
}
