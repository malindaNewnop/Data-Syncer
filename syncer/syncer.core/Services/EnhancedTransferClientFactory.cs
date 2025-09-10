using System;
using syncer.core.Transfers;

namespace syncer.core.Services
{
    /// <summary>
    /// Enhanced Transfer Client Factory with production-ready implementations
    /// .NET 3.5 Compatible
    /// </summary>
    public class EnhancedTransferClientFactory : ITransferClientFactory
    {
        private readonly bool _useProductionClients;

        /// <summary>
        /// Initialize factory with option to use production implementations
        /// </summary>
        /// <param name="useProductionClients">True to use enhanced/production clients, false to use basic clients</param>
        public EnhancedTransferClientFactory(bool useProductionClients = true)
        {
            _useProductionClients = useProductionClients;
        }

        /// <summary>
        /// Create transfer client for specified protocol
        /// </summary>
        public ITransferClient Create(ProtocolType protocol)
        {
            switch (protocol)
            {
                case ProtocolType.Local:
                    return new LocalTransferClient();

                case ProtocolType.Ftp:
                    return new EnhancedFtpTransferClient();

                case ProtocolType.Sftp:
                    return new ProductionSftpTransferClient();

                default:
                    throw new NotSupportedException($"Protocol {protocol} is not supported");
            }
        }

        /// <summary>
        /// Create secure transfer client with key-based authentication
        /// </summary>
        public ITransferClient CreateSecure(ProtocolType protocol, string keyPath)
        {
            var client = Create(protocol);
            
            // For SFTP clients, the key path will be used via ConnectionSettings
            // This method is for backward compatibility
            
            return client;
        }

        /// <summary>
        /// Create transfer client with specific settings
        /// </summary>
        public ITransferClient CreateWithSettings(ProtocolType protocol, ConnectionSettings settings)
        {
            var client = Create(protocol);
            
            // Pre-validate settings if needed
            if (protocol != ProtocolType.Local && settings != null)
            {
                ValidateConnectionSettings(protocol, settings);
            }
            
            return client;
        }

        /// <summary>
        /// Get recommended transfer client for protocol with error handling
        /// </summary>
        public ITransferClient GetRecommendedClient(ProtocolType protocol)
        {
            try
            {
                return Create(protocol);
            }
            catch (Exception)
            {
                // Fallback to same enhanced implementations (no basic fallback needed)
                switch (protocol)
                {
                    case ProtocolType.Local:
                        return new LocalTransferClient();
                    case ProtocolType.Ftp:
                        return new EnhancedFtpTransferClient();
                    case ProtocolType.Sftp:
                        return new ProductionSftpTransferClient();
                    default:
                        throw new NotSupportedException($"No fallback client available for protocol {protocol}");
                }
            }
        }

        /// <summary>
        /// Validate connection settings for protocol
        /// </summary>
        private void ValidateConnectionSettings(ProtocolType protocol, ConnectionSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings", "Connection settings are required for remote protocols");

            if (string.IsNullOrEmpty(settings.Host))
                throw new ArgumentException("Host is required for remote protocols");

            if (string.IsNullOrEmpty(settings.Username))
                throw new ArgumentException("Username is required for remote protocols");

            switch (protocol)
            {
                case ProtocolType.Ftp:
                    if (settings.Port <= 0) settings.Port = 21;
                    break;

                case ProtocolType.Sftp:
                    if (settings.Port <= 0) settings.Port = 22;
                    
                    // For SFTP, we need either password or key file
                    if (string.IsNullOrEmpty(settings.Password) && 
                        (string.IsNullOrEmpty(settings.SshKeyPath) || !System.IO.File.Exists(settings.SshKeyPath)))
                    {
                        throw new ArgumentException("SFTP requires either password or valid SSH key file");
                    }
                    break;
            }
        }

        /// <summary>
        /// Check if enhanced clients are available
        /// </summary>
        public bool AreEnhancedClientsAvailable()
        {
            return _useProductionClients;
        }

        /// <summary>
        /// Get client capabilities for protocol
        /// </summary>
        public TransferClientCapabilities GetClientCapabilities(ProtocolType protocol)
        {
            switch (protocol)
            {
                case ProtocolType.Local:
                    return new TransferClientCapabilities
                    {
                        SupportsProgressCallback = true,
                        SupportsResume = false,
                        SupportsDirectoryCreation = true,
                        SupportsFileList = true,
                        SupportsDelete = true,
                        MaxRetryAttempts = 1,
                        IsSecure = true // Local operations are inherently secure
                    };

                case ProtocolType.Ftp:
                    return new TransferClientCapabilities
                    {
                        SupportsProgressCallback = _useProductionClients,
                        SupportsResume = false, // FTP resume can be complex
                        SupportsDirectoryCreation = true,
                        SupportsFileList = true,
                        SupportsDelete = true,
                        MaxRetryAttempts = _useProductionClients ? 3 : 1,
                        IsSecure = false // Standard FTP is not secure
                    };

                case ProtocolType.Sftp:
                    return new TransferClientCapabilities
                    {
                        SupportsProgressCallback = true,
                        SupportsResume = false, // SSH.NET doesn't support resume out of the box
                        SupportsDirectoryCreation = true,
                        SupportsFileList = true,
                        SupportsDelete = true,
                        MaxRetryAttempts = _useProductionClients ? 3 : 1,
                        IsSecure = true // SFTP is secure
                    };

                default:
                    return new TransferClientCapabilities();
            }
        }
    }

    /// <summary>
    /// Represents the capabilities of a transfer client
    /// </summary>
    public class TransferClientCapabilities
    {
        public bool SupportsProgressCallback { get; set; }
        public bool SupportsResume { get; set; }
        public bool SupportsDirectoryCreation { get; set; }
        public bool SupportsFileList { get; set; }
        public bool SupportsDelete { get; set; }
        public int MaxRetryAttempts { get; set; }
        public bool IsSecure { get; set; }
        public bool SupportsCancellation { get; set; }
        public bool SupportsTimeout { get; set; } = true;

        public TransferClientCapabilities()
        {
            // Set defaults
            SupportsTimeout = true;
            MaxRetryAttempts = 1;
        }
    }
}
