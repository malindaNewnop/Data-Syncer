using System;
using System.Collections.Generic;
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

        #region Enhanced Connection Management (Stub Implementation)

        public bool SaveConnection(string connectionName, ConnectionSettings settings, bool setAsDefault = false)
        {
            // Stub implementation - actual implementation should use persistent storage
            return SaveConnectionSettings(settings);
        }

        public ConnectionSettings GetConnection(string connectionName)
        {
            // Stub implementation - return current settings
            return GetConnectionSettings();
        }

        public List<SavedConnection> GetAllConnections()
        {
            // Stub implementation - return empty list
            return new List<SavedConnection>();
        }

        public ConnectionSettings GetDefaultConnection()
        {
            // Stub implementation - return current settings
            return GetConnectionSettings();
        }

        public bool SetDefaultConnection(string connectionName)
        {
            // Stub implementation
            return true;
        }

        public bool DeleteConnection(string connectionName)
        {
            // Stub implementation
            return true;
        }

        public bool ConnectionExists(string connectionName)
        {
            // Stub implementation
            return false;
        }

        public List<string> GetConnectionNames()
        {
            // Stub implementation - return empty list
            return new List<string>();
        }

        public ConnectionSettings LoadConnectionForStartup()
        {
            // Stub implementation - return current settings
            return GetConnectionSettings();
        }

        #endregion
    }
}
