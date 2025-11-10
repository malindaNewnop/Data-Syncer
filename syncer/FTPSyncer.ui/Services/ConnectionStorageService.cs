using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace FTPSyncer.ui.Services
{
    /// <summary>
    /// Service for persisting connection settings to local storage
    /// Provides secure and reliable storage for multiple connection profiles
    /// </summary>
    public class ConnectionStorageService
    {
        private readonly string _connectionsFilePath;
        private readonly string _defaultConnectionFilePath;
        private List<SavedConnection> _savedConnections;
        private readonly object _lockObject = new object();

        public ConnectionStorageService()
        {
            // Store connections in user's AppData folder for security
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DataSyncer");
            Directory.CreateDirectory(appDataFolder);
            
            _connectionsFilePath = Path.Combine(appDataFolder, "connections.json");
            _defaultConnectionFilePath = Path.Combine(appDataFolder, "default_connection.json");
            
            LoadConnections();
        }

        /// <summary>
        /// Save a connection profile with a given name
        /// </summary>
        public bool SaveConnection(string connectionName, ConnectionSettings settings, bool setAsDefault = false)
        {
            if (string.IsNullOrWhiteSpace(connectionName) || settings == null)
                return false;

            lock (_lockObject)
            {
                try
                {
                    // Remove existing connection with same name
                    _savedConnections.RemoveAll(c => c.Name.Equals(connectionName, StringComparison.OrdinalIgnoreCase));

                    // Create new saved connection
                    var savedConnection = new SavedConnection
                    {
                        Name = connectionName,
                        Settings = settings.Clone(),
                        CreatedDate = DateTime.Now,
                        LastUsed = DateTime.Now,
                        IsDefault = setAsDefault
                    };

                    // If setting as default, remove default flag from others
                    if (setAsDefault)
                    {
                        foreach (var conn in _savedConnections)
                            conn.IsDefault = false;
                    }

                    _savedConnections.Add(savedConnection);

                    // Save to file
                    SaveConnectionsToFile();

                    // If this is the default connection, also save it separately for quick access
                    if (setAsDefault)
                    {
                        SaveDefaultConnection(settings);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    // Log error if logging service is available
                    try
                    {
                        ServiceLocator.LogService?.LogError($"Failed to save connection '{connectionName}': {ex.Message}");
                    }
                    catch { } // Ignore logging errors

                    return false;
                }
            }
        }

        /// <summary>
        /// Get all saved connections
        /// </summary>
        public List<SavedConnection> GetAllConnections()
        {
            lock (_lockObject)
            {
                return _savedConnections.OrderByDescending(c => c.LastUsed).ToList();
            }
        }

        /// <summary>
        /// Get a specific connection by name
        /// </summary>
        public ConnectionSettings GetConnection(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
                return null;

            lock (_lockObject)
            {
                var savedConnection = _savedConnections.FirstOrDefault(c => 
                    c.Name.Equals(connectionName, StringComparison.OrdinalIgnoreCase));

                if (savedConnection != null)
                {
                    // Update last used time
                    savedConnection.LastUsed = DateTime.Now;
                    SaveConnectionsToFile();
                    
                    return savedConnection.Settings.Clone();
                }

                return null;
            }
        }

        /// <summary>
        /// Get the default connection
        /// </summary>
        public ConnectionSettings GetDefaultConnection()
        {
            lock (_lockObject)
            {
                // First try to get from marked default connection
                var defaultConnection = _savedConnections.FirstOrDefault(c => c.IsDefault);
                if (defaultConnection != null)
                {
                    defaultConnection.LastUsed = DateTime.Now;
                    SaveConnectionsToFile();
                    return defaultConnection.Settings.Clone();
                }

                // If no default marked, try loading from default connection file
                if (File.Exists(_defaultConnectionFilePath))
                {
                    try
                    {
                        string json = File.ReadAllText(_defaultConnectionFilePath);
                        var settings = JsonConvert.DeserializeObject<ConnectionSettings>(json);
                        return settings;
                    }
                    catch
                    {
                        // File is corrupted or unreadable, ignore and return null
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Set a connection as default
        /// </summary>
        public bool SetDefaultConnection(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
                return false;

            lock (_lockObject)
            {
                var connection = _savedConnections.FirstOrDefault(c => 
                    c.Name.Equals(connectionName, StringComparison.OrdinalIgnoreCase));

                if (connection == null)
                    return false;

                // Remove default flag from all connections
                foreach (var conn in _savedConnections)
                    conn.IsDefault = false;

                // Set this connection as default
                connection.IsDefault = true;
                connection.LastUsed = DateTime.Now;

                SaveConnectionsToFile();
                SaveDefaultConnection(connection.Settings);

                return true;
            }
        }

        /// <summary>
        /// Delete a saved connection
        /// </summary>
        public bool DeleteConnection(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
                return false;

            lock (_lockObject)
            {
                int removedCount = _savedConnections.RemoveAll(c => 
                    c.Name.Equals(connectionName, StringComparison.OrdinalIgnoreCase));

                if (removedCount > 0)
                {
                    SaveConnectionsToFile();
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Check if a connection name already exists
        /// </summary>
        public bool ConnectionExists(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
                return false;

            lock (_lockObject)
            {
                return _savedConnections.Any(c => 
                    c.Name.Equals(connectionName, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Get connection names for dropdown/selection
        /// </summary>
        public List<string> GetConnectionNames()
        {
            lock (_lockObject)
            {
                return _savedConnections
                    .OrderByDescending(c => c.LastUsed)
                    .Select(c => c.Name)
                    .ToList();
            }
        }

        /// <summary>
        /// Auto-load default connection on application startup
        /// </summary>
        public ConnectionSettings LoadConnectionForStartup()
        {
            var defaultConnection = GetDefaultConnection();
            if (defaultConnection != null)
            {
                // Log successful auto-load
                try
                {
                    ServiceLocator.LogService?.LogInfo("Default connection loaded automatically on startup");
                }
                catch { } // Ignore logging errors

                return defaultConnection;
            }

            // If no default connection, try to get the most recently used one
            lock (_lockObject)
            {
                var mostRecent = _savedConnections
                    .Where(c => c.LastUsed.HasValue)
                    .OrderByDescending(c => c.LastUsed)
                    .FirstOrDefault();

                if (mostRecent != null)
                {
                    try
                    {
                        ServiceLocator.LogService?.LogInfo($"Most recent connection '{mostRecent.Name}' loaded automatically on startup");
                    }
                    catch { } // Ignore logging errors

                    mostRecent.LastUsed = DateTime.Now;
                    SaveConnectionsToFile();
                    return mostRecent.Settings.Clone();
                }
            }

            return null;
        }

        #region Private Methods

        private void LoadConnections()
        {
            lock (_lockObject)
            {
                _savedConnections = new List<SavedConnection>();

                if (File.Exists(_connectionsFilePath))
                {
                    try
                    {
                        string json = File.ReadAllText(_connectionsFilePath);
                        var connections = JsonConvert.DeserializeObject<List<SavedConnection>>(json);
                        if (connections != null)
                        {
                            _savedConnections = connections;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error if possible, but don't crash
                        try
                        {
                            ServiceLocator.LogService?.LogWarning($"Failed to load saved connections: {ex.Message}. Starting with empty connection list.");
                        }
                        catch { } // Ignore logging errors
                    }
                }
            }
        }

        private void SaveConnectionsToFile()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_savedConnections, Formatting.Indented);
                File.WriteAllText(_connectionsFilePath, json);
            }
            catch (Exception ex)
            {
                try
                {
                    ServiceLocator.LogService?.LogError($"Failed to save connections to file: {ex.Message}");
                }
                catch { } // Ignore logging errors
                
                throw; // Re-throw to let caller know save failed
            }
        }

        private void SaveDefaultConnection(ConnectionSettings settings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(_defaultConnectionFilePath, json);
            }
            catch (Exception ex)
            {
                try
                {
                    ServiceLocator.LogService?.LogWarning($"Failed to save default connection: {ex.Message}");
                }
                catch { } // Ignore logging errors
            }
        }

        #endregion
    }
}





