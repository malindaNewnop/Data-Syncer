using System;
using syncer.core;

namespace syncer.ui.Services
{
    /// <summary>
    /// Adapter class to connect UI connection service to core transfer client
    /// </summary>
    public class CoreConnectionServiceAdapter : IConnectionService
    {
        private readonly syncer.core.ITransferClientFactory _transferClientFactory;
        private ConnectionSettings _settings;

        public CoreConnectionServiceAdapter()
        {
            try
            {
                _transferClientFactory = syncer.core.ServiceFactory.CreateTransferClientFactory();
                _settings = new ConnectionSettings();
                
                DebugLogger.LogServiceActivity("CoreConnectionServiceAdapter", "Initialized successfully");
            }
            catch (Exception ex)
            {
                DebugLogger.LogError(ex, "CoreConnectionServiceAdapter initialization");
                throw;
            }
        }

        public ConnectionSettings GetConnectionSettings()
        {
            return _settings;
        }

        public bool SaveConnectionSettings(ConnectionSettings settings)
        {
            _settings = settings;
            return true;
        }

        public bool TestConnection(ConnectionSettings settings)
        {
            try
            {
                // Convert UI connection settings to core connection settings
                var coreSettings = new syncer.core.ConnectionSettings
                {
                    Protocol = (syncer.core.ProtocolType)settings.ProtocolType,
                    Host = settings.Host,
                    Port = settings.Port,
                    Username = settings.Username,
                    Password = settings.Password,
                    UsePassiveMode = settings.UsePassiveMode,
                    UseSftp = settings.UseSftp
                };

                // Create the appropriate transfer client
                var client = _transferClientFactory.Create((syncer.core.ProtocolType)settings.ProtocolType);
                
                // Test the connection
                string error;
                bool result = client.TestConnection(coreSettings, out error);
                
                return result;
            }
            catch
            {
                return false;
            }
        }

        public bool IsConnected()
        {
            // For local connections, always return true
            if (_settings.ProtocolType == 0) // Local
                return true;
                
            // For remote connections, actually test the connection
            return TestConnection(_settings);
        }

        // Enhanced connection management methods
        public bool SaveConnection(string connectionName, ConnectionSettings settings, bool setAsDefault = false)
        {
            if (StringExtensions.IsNullOrWhiteSpace(connectionName) || settings == null)
                return false;

            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\DataSyncer\\Connections"))
                {
                    using (Microsoft.Win32.RegistryKey connectionKey = key.CreateSubKey(connectionName))
                    {
                        connectionKey.SetValue("Protocol", settings.Protocol ?? "LOCAL");
                        connectionKey.SetValue("ProtocolType", settings.ProtocolType);
                        connectionKey.SetValue("Host", settings.Host ?? "");
                        connectionKey.SetValue("Port", settings.Port);
                        connectionKey.SetValue("Username", settings.Username ?? "");
                        connectionKey.SetValue("SshKeyPath", settings.SshKeyPath ?? "");
                        connectionKey.SetValue("Timeout", settings.Timeout);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ConnectionSettings GetConnection(string connectionName)
        {
            if (StringExtensions.IsNullOrWhiteSpace(connectionName))
                return null;

            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\DataSyncer\\Connections"))
                {
                    if (key != null)
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
                                    Username = connectionKey.GetValue("Username", "").ToString(),
                                    SshKeyPath = connectionKey.GetValue("SshKeyPath", "").ToString(),
                                    Timeout = Convert.ToInt32(connectionKey.GetValue("Timeout", 30))
                                };
                                return settings;
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        public System.Collections.Generic.List<SavedConnection> GetAllConnections()
        {
            var connections = new System.Collections.Generic.List<SavedConnection>();
            
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\DataSyncer\\Connections"))
                {
                    if (key != null)
                    {
                        foreach (string connectionName in key.GetSubKeyNames())
                        {
                            var settings = GetConnection(connectionName);
                            if (settings != null)
                            {
                                connections.Add(new SavedConnection
                                {
                                    Name = connectionName,
                                    Settings = settings,
                                    CreatedDate = DateTime.Now,
                                    LastUsed = DateTime.Now,
                                    IsDefault = false
                                });
                            }
                        }
                    }
                }
            }
            catch { }

            return connections;
        }

        public ConnectionSettings GetDefaultConnection()
        {
            var connections = GetAllConnections();
            if (connections.Count > 0)
            {
                return connections[0].Settings;
            }
            return null;
        }

        public bool SetDefaultConnection(string connectionName)
        {
            var connection = GetConnection(connectionName);
            if (connection != null)
            {
                _settings = connection;
                return true;
            }
            return false;
        }

        public bool DeleteConnection(string connectionName)
        {
            if (StringExtensions.IsNullOrWhiteSpace(connectionName))
                return false;

            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\DataSyncer\\Connections", true))
                {
                    if (key != null)
                    {
                        key.DeleteSubKey(connectionName, false);
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        public bool ConnectionExists(string connectionName)
        {
            return GetConnection(connectionName) != null;
        }

        public System.Collections.Generic.List<string> GetConnectionNames()
        {
            var names = new System.Collections.Generic.List<string>();
            
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\DataSyncer\\Connections"))
                {
                    if (key != null)
                    {
                        foreach (string connectionName in key.GetSubKeyNames())
                        {
                            names.Add(connectionName);
                        }
                    }
                }
            }
            catch { }

            return names;
        }

        public ConnectionSettings LoadConnectionForStartup()
        {
            var defaultConnection = GetDefaultConnection();
            if (defaultConnection != null)
            {
                _settings = defaultConnection;
                return defaultConnection;
            }
            return _settings;
        }
    }
}
