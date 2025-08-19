using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace syncer.core.Transfers
{
    /// <summary>
    /// Production-Ready SFTP Transfer Client with comprehensive error handling, retry logic, and progress tracking
    /// .NET 3.5 Compatible with SSH.NET library
    /// </summary>
    public class ProductionSftpTransferClient : ITransferClient
    {
        private Action<int> _progressCallback;
        private readonly int _defaultRetryCount = 3;
        private readonly int _defaultRetryDelayMs = 1000;
        private readonly int _connectionTimeoutMs = 30000;
        private readonly int _operationTimeoutMs = 60000;
        private readonly object _transferLock = new object();
        private volatile bool _cancelRequested = false;
        
        // Progress tracking for file transfers
        private long _currentFileSize = 0;
        private long _currentBytesTransferred = 0;
        private DateTime _transferStartTime;

        public ProtocolType Protocol { get { return ProtocolType.Sftp; } }

        /// <summary>
        /// Cancels current transfer operation
        /// </summary>
        public void CancelTransfer()
        {
            _cancelRequested = true;
        }

        /// <summary>
        /// Test SFTP connection with comprehensive error handling
        /// </summary>
        public bool TestConnection(ConnectionSettings settings, out string error)
        {
            error = null;
            SftpClient client = null;

            try
            {
                if (settings == null)
                {
                    error = "Connection settings are required for SFTP";
                    return false;
                }

                if (string.IsNullOrEmpty(settings.Host))
                {
                    error = "SFTP host is required";
                    return false;
                }

                if (string.IsNullOrEmpty(settings.Username))
                {
                    error = "SFTP username is required";
                    return false;
                }

                var testStartTime = DateTime.Now;
                client = CreateSftpClient(settings);
                
                client.Connect();
                
                if (client.IsConnected)
                {
                    // Test basic operations
                    var workingDir = client.WorkingDirectory;
                    var connectionTime = DateTime.Now - testStartTime;
                    
                    return true;
                }
                else
                {
                    error = "Failed to establish SFTP connection";
                    return false;
                }
            }
            catch (SshConnectionException ex)
            {
                error = $"SFTP connection failed: {ex.Message}";
                return false;
            }
            catch (SshAuthenticationException ex)
            {
                error = $"SFTP authentication failed: {ex.Message}";
                return false;
            }
            catch (SshOperationTimeoutException ex)
            {
                error = $"SFTP connection timed out: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                error = $"SFTP connection test failed: {ex.Message}";
                return false;
            }
            finally
            {
                if (client != null && client.IsConnected)
                {
                    try { client.Disconnect(); } catch { }
                    client.Dispose();
                }
            }
        }

        /// <summary>
        /// Ensure directory exists on SFTP server with recursive creation support
        /// </summary>
        public bool EnsureDirectory(ConnectionSettings settings, string remoteDir, out string error)
        {
            error = null;
            SftpClient client = null;

            try
            {
                if (string.IsNullOrEmpty(remoteDir))
                {
                    error = "Remote directory path cannot be empty";
                    return false;
                }

                client = CreateSftpClient(settings);
                client.Connect();

                // Normalize path separators for Unix/Linux systems
                remoteDir = remoteDir.Replace('\\', '/');
                
                // Remove trailing slash except for root
                if (remoteDir.Length > 1 && remoteDir.EndsWith("/"))
                    remoteDir = remoteDir.Substring(0, remoteDir.Length - 1);

                // Check if directory exists
                if (client.Exists(remoteDir))
                {
                    var attrs = client.GetAttributes(remoteDir);
                    if (attrs.IsDirectory)
                    {
                        return true;
                    }
                    else
                    {
                        error = $"Path exists but is not a directory: {remoteDir}";
                        return false;
                    }
                }

                // Create directory recursively
                var pathParts = remoteDir.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                var currentPath = remoteDir.StartsWith("/") ? "" : ".";

                foreach (var part in pathParts)
                {
                    currentPath += "/" + part;
                    
                    if (!client.Exists(currentPath))
                    {
                        client.CreateDirectory(currentPath);
                    }
                    else
                    {
                        var attrs = client.GetAttributes(currentPath);
                        if (!attrs.IsDirectory)
                        {
                            error = $"Path exists but is not a directory: {currentPath}";
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (SshConnectionException ex)
            {
                error = $"SFTP connection failed while creating directory: {ex.Message}";
                return false;
            }
            catch (SftpPathNotFoundException ex)
            {
                error = $"Parent directory not found: {ex.Message}";
                return false;
            }
            catch (SftpPermissionDeniedException ex)
            {
                error = $"Permission denied creating directory: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                error = $"Failed to ensure directory '{remoteDir}': {ex.Message}";
                return false;
            }
            finally
            {
                if (client != null && client.IsConnected)
                {
                    try { client.Disconnect(); } catch { }
                    client.Dispose();
                }
            }
        }

        /// <summary>
        /// Upload file with retry logic and progress tracking
        /// </summary>
        public bool UploadFile(ConnectionSettings settings, string localPath, string remotePath, bool overwrite, out string error)
        {
            error = null;
            
            for (int attempt = 0; attempt <= _defaultRetryCount; attempt++)
            {
                if (_cancelRequested)
                {
                    error = "Transfer was cancelled";
                    return false;
                }

                SftpClient client = null;
                
                try
                {
                    if (!File.Exists(localPath))
                    {
                        error = $"Source file does not exist: {localPath}";
                        return false;
                    }

                    // Normalize remote path for Unix/Linux systems
                    remotePath = remotePath.Replace('\\', '/');

                    client = CreateSftpClient(settings);
                    client.Connect();

                    // Check if file exists and overwrite is false
                    if (!overwrite && client.Exists(remotePath))
                    {
                        var attrs = client.GetAttributes(remotePath);
                        if (attrs.IsRegularFile)
                        {
                            error = $"Destination file already exists: {remotePath}";
                            return false;
                        }
                    }

                    // Ensure remote directory exists
                    var remoteDir = GetDirectoryPath(remotePath);
                    if (!string.IsNullOrEmpty(remoteDir))
                    {
                        string dirError;
                        client.Disconnect();
                        client.Dispose();
                        
                        if (!EnsureDirectory(settings, remoteDir, out dirError))
                        {
                            error = $"Failed to create remote directory: {dirError}";
                            return false;
                        }
                        
                        // Reconnect after directory creation
                        client = CreateSftpClient(settings);
                        client.Connect();
                    }

                    // Get file size for progress tracking
                    var fileInfo = new FileInfo(localPath);
                    _currentFileSize = fileInfo.Length;
                    _currentBytesTransferred = 0;
                    _transferStartTime = DateTime.Now;

                    // Upload with progress callback
                    using (var fileStream = File.OpenRead(localPath))
                    {
                        client.UploadFile(fileStream, remotePath, overwrite, (bytesUploaded) =>
                        {
                            if (_cancelRequested) return; // Note: SSH.NET doesn't support cancellation in this callback

                            _currentBytesTransferred = (long)bytesUploaded;
                            
                            if (_progressCallback != null && _currentFileSize > 0)
                            {
                                var progress = (int)((_currentBytesTransferred * 100) / _currentFileSize);
                                _progressCallback(Math.Min(progress, 100));
                            }
                        });
                    }

                    // Verify upload
                    if (client.Exists(remotePath))
                    {
                        var remoteAttrs = client.GetAttributes(remotePath);
                        if (remoteAttrs.Size == _currentFileSize)
                        {
                            return true;
                        }
                        else
                        {
                            error = $"Upload verification failed: file sizes do not match (local: {_currentFileSize}, remote: {remoteAttrs.Size})";
                            
                            if (attempt < _defaultRetryCount)
                            {
                                Thread.Sleep(_defaultRetryDelayMs * (attempt + 1));
                                continue;
                            }
                            return false;
                        }
                    }
                    else
                    {
                        error = "Upload failed: remote file does not exist after upload";
                        
                        if (attempt < _defaultRetryCount)
                        {
                            Thread.Sleep(_defaultRetryDelayMs * (attempt + 1));
                            continue;
                        }
                        return false;
                    }
                }
                catch (SshConnectionException ex)
                {
                    error = $"SFTP connection failed during upload: {ex.Message}";
                    
                    if (attempt < _defaultRetryCount)
                    {
                        Thread.Sleep(_defaultRetryDelayMs * (attempt + 1));
                        continue;
                    }
                    return false;
                }
                catch (SftpPermissionDeniedException ex)
                {
                    error = $"Permission denied during upload: {ex.Message}";
                    return false; // Don't retry permission errors
                }
                catch (SftpPathNotFoundException ex)
                {
                    error = $"Remote path not found during upload: {ex.Message}";
                    return false; // Don't retry path errors
                }
                catch (Exception ex)
                {
                    error = $"Upload failed: {ex.Message}";
                    
                    if (attempt < _defaultRetryCount)
                    {
                        Thread.Sleep(_defaultRetryDelayMs * (attempt + 1));
                        continue;
                    }
                    return false;
                }
                finally
                {
                    if (client != null && client.IsConnected)
                    {
                        try { client.Disconnect(); } catch { }
                        client.Dispose();
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Download file with retry logic and progress tracking
        /// </summary>
        public bool DownloadFile(ConnectionSettings settings, string remotePath, string localPath, bool overwrite, out string error)
        {
            error = null;
            
            for (int attempt = 0; attempt <= _defaultRetryCount; attempt++)
            {
                if (_cancelRequested)
                {
                    error = "Transfer was cancelled";
                    return false;
                }

                SftpClient client = null;
                
                try
                {
                    // Normalize remote path for Unix/Linux systems
                    remotePath = remotePath.Replace('\\', '/');

                    client = CreateSftpClient(settings);
                    client.Connect();

                    // Check if remote file exists
                    if (!client.Exists(remotePath))
                    {
                        error = $"Source file does not exist: {remotePath}";
                        return false;
                    }

                    var remoteAttrs = client.GetAttributes(remotePath);
                    if (!remoteAttrs.IsRegularFile)
                    {
                        error = $"Source is not a regular file: {remotePath}";
                        return false;
                    }

                    if (!overwrite && File.Exists(localPath))
                    {
                        error = $"Destination file already exists: {localPath}";
                        return false;
                    }

                    // Ensure local directory exists
                    var localDir = Path.GetDirectoryName(localPath);
                    if (!Directory.Exists(localDir))
                        Directory.CreateDirectory(localDir);

                    // Get file size for progress tracking
                    _currentFileSize = remoteAttrs.Size;
                    _currentBytesTransferred = 0;
                    _transferStartTime = DateTime.Now;

                    // Download with progress callback
                    using (var fileStream = File.Create(localPath))
                    {
                        client.DownloadFile(remotePath, fileStream, (bytesDownloaded) =>
                        {
                            if (_cancelRequested) return; // Note: SSH.NET doesn't support cancellation in this callback

                            _currentBytesTransferred = (long)bytesDownloaded;
                            
                            if (_progressCallback != null && _currentFileSize > 0)
                            {
                                var progress = (int)((_currentBytesTransferred * 100) / _currentFileSize);
                                _progressCallback(Math.Min(progress, 100));
                            }
                        });
                    }

                    // Verify download
                    if (File.Exists(localPath))
                    {
                        var localFileInfo = new FileInfo(localPath);
                        if (localFileInfo.Length == _currentFileSize)
                        {
                            return true;
                        }
                        else
                        {
                            error = $"Download verification failed: file sizes do not match (remote: {_currentFileSize}, local: {localFileInfo.Length})";
                            
                            // Clean up partial file
                            try { File.Delete(localPath); } catch { }
                            
                            if (attempt < _defaultRetryCount)
                            {
                                Thread.Sleep(_defaultRetryDelayMs * (attempt + 1));
                                continue;
                            }
                            return false;
                        }
                    }
                    else
                    {
                        error = "Download failed: local file does not exist after download";
                        
                        if (attempt < _defaultRetryCount)
                        {
                            Thread.Sleep(_defaultRetryDelayMs * (attempt + 1));
                            continue;
                        }
                        return false;
                    }
                }
                catch (SshConnectionException ex)
                {
                    error = $"SFTP connection failed during download: {ex.Message}";
                    
                    // Clean up partial file
                    if (File.Exists(localPath))
                    {
                        try { File.Delete(localPath); } catch { }
                    }
                    
                    if (attempt < _defaultRetryCount)
                    {
                        Thread.Sleep(_defaultRetryDelayMs * (attempt + 1));
                        continue;
                    }
                    return false;
                }
                catch (SftpPermissionDeniedException ex)
                {
                    error = $"Permission denied during download: {ex.Message}";
                    
                    // Clean up partial file
                    if (File.Exists(localPath))
                    {
                        try { File.Delete(localPath); } catch { }
                    }
                    
                    return false; // Don't retry permission errors
                }
                catch (SftpPathNotFoundException ex)
                {
                    error = $"Remote path not found during download: {ex.Message}";
                    return false; // Don't retry path errors
                }
                catch (Exception ex)
                {
                    error = $"Download failed: {ex.Message}";
                    
                    // Clean up partial file
                    if (File.Exists(localPath))
                    {
                        try { File.Delete(localPath); } catch { }
                    }
                    
                    if (attempt < _defaultRetryCount)
                    {
                        Thread.Sleep(_defaultRetryDelayMs * (attempt + 1));
                        continue;
                    }
                    return false;
                }
                finally
                {
                    if (client != null && client.IsConnected)
                    {
                        try { client.Disconnect(); } catch { }
                        client.Dispose();
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check if file exists on SFTP server
        /// </summary>
        public bool FileExists(ConnectionSettings settings, string remotePath, out bool exists, out string error)
        {
            exists = false;
            error = null;
            SftpClient client = null;

            try
            {
                remotePath = remotePath.Replace('\\', '/');

                client = CreateSftpClient(settings);
                client.Connect();

                exists = client.Exists(remotePath);
                
                if (exists)
                {
                    var attrs = client.GetAttributes(remotePath);
                    exists = attrs.IsRegularFile; // Only return true for regular files
                }

                return true;
            }
            catch (SshConnectionException ex)
            {
                error = $"SFTP connection failed while checking file existence: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                error = $"Failed to check file existence: {ex.Message}";
                return false;
            }
            finally
            {
                if (client != null && client.IsConnected)
                {
                    try { client.Disconnect(); } catch { }
                    client.Dispose();
                }
            }
        }

        /// <summary>
        /// Delete file from SFTP server
        /// </summary>
        public bool DeleteFile(ConnectionSettings settings, string remotePath, out string error)
        {
            error = null;
            SftpClient client = null;

            try
            {
                remotePath = remotePath.Replace('\\', '/');

                client = CreateSftpClient(settings);
                client.Connect();

                if (!client.Exists(remotePath))
                {
                    error = $"File does not exist: {remotePath}";
                    return false;
                }

                client.DeleteFile(remotePath);
                return true;
            }
            catch (SshConnectionException ex)
            {
                error = $"SFTP connection failed while deleting file: {ex.Message}";
                return false;
            }
            catch (SftpPermissionDeniedException ex)
            {
                error = $"Permission denied deleting file: {ex.Message}";
                return false;
            }
            catch (SftpPathNotFoundException ex)
            {
                error = $"File not found: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                error = $"Delete failed: {ex.Message}";
                return false;
            }
            finally
            {
                if (client != null && client.IsConnected)
                {
                    try { client.Disconnect(); } catch { }
                    client.Dispose();
                }
            }
        }

        /// <summary>
        /// List files in SFTP directory
        /// </summary>
        public bool ListFiles(ConnectionSettings settings, string remoteDir, out List<string> files, out string error)
        {
            files = new List<string>();
            error = null;
            SftpClient client = null;

            try
            {
                remoteDir = remoteDir.Replace('\\', '/');

                client = CreateSftpClient(settings);
                client.Connect();

                if (!client.Exists(remoteDir))
                {
                    error = $"Directory does not exist: {remoteDir}";
                    return false;
                }

                var dirAttrs = client.GetAttributes(remoteDir);
                if (!dirAttrs.IsDirectory)
                {
                    error = $"Path is not a directory: {remoteDir}";
                    return false;
                }

                var entries = client.ListDirectory(remoteDir);
                foreach (var entry in entries)
                {
                    if (entry.IsRegularFile)
                    {
                        files.Add(entry.Name);
                    }
                }

                return true;
            }
            catch (SshConnectionException ex)
            {
                error = $"SFTP connection failed while listing files: {ex.Message}";
                return false;
            }
            catch (SftpPermissionDeniedException ex)
            {
                error = $"Permission denied listing directory: {ex.Message}";
                return false;
            }
            catch (SftpPathNotFoundException ex)
            {
                error = $"Directory not found: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                error = $"List files failed: {ex.Message}";
                return false;
            }
            finally
            {
                if (client != null && client.IsConnected)
                {
                    try { client.Disconnect(); } catch { }
                    client.Dispose();
                }
            }
        }

        public void SetProgressCallback(Action<int> progressCallback)
        {
            _progressCallback = progressCallback;
        }

        /// <summary>
        /// Creates an SFTP client with proper authentication and connection settings
        /// </summary>
        private SftpClient CreateSftpClient(ConnectionSettings settings)
        {
            var connectionInfo = CreateConnectionInfo(settings);
            var client = new SftpClient(connectionInfo);
            client.ConnectionInfo.Timeout = TimeSpan.FromMilliseconds(_connectionTimeoutMs);
            client.OperationTimeout = TimeSpan.FromMilliseconds(_operationTimeoutMs);
            return client;
        }

        /// <summary>
        /// Creates connection info with secure authentication methods
        /// </summary>
        private ConnectionInfo CreateConnectionInfo(ConnectionSettings settings)
        {
            var authMethods = new List<AuthenticationMethod>();

            // Key-based authentication (preferred)
            if (!string.IsNullOrEmpty(settings.SshKeyPath) && File.Exists(settings.SshKeyPath))
            {
                try
                {
                    PrivateKeyFile keyFile;
                    if (!string.IsNullOrEmpty(settings.Password))
                    {
                        // Key with passphrase
                        keyFile = new PrivateKeyFile(settings.SshKeyPath, settings.Password);
                    }
                    else
                    {
                        // Key without passphrase
                        keyFile = new PrivateKeyFile(settings.SshKeyPath);
                    }
                    authMethods.Add(new PrivateKeyAuthenticationMethod(settings.Username, keyFile));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to load SSH key from {settings.SshKeyPath}: {ex.Message}");
                }
            }

            // Password authentication (fallback)
            if (!string.IsNullOrEmpty(settings.Password))
            {
                authMethods.Add(new PasswordAuthenticationMethod(settings.Username, settings.Password));
            }

            if (authMethods.Count == 0)
            {
                throw new InvalidOperationException("No authentication methods available. Provide either a password or SSH key.");
            }

            return new ConnectionInfo(settings.Host, settings.Port, settings.Username, authMethods.ToArray());
        }

        /// <summary>
        /// Gets directory path from file path (Unix-style)
        /// </summary>
        private string GetDirectoryPath(string filePath)
        {
            var lastSlash = filePath.LastIndexOf('/');
            if (lastSlash > 0)
            {
                return filePath.Substring(0, lastSlash);
            }
            return string.Empty;
        }
    }
}
