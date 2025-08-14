using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Text;

namespace syncer.core
{
    /// <summary>
    /// Full-featured SFTP Transfer Client with secure key handling, bandwidth throttling,
    /// transfer resumption, and advanced error handling with retry logic
    /// </summary>
    public class SftpTransferClient : ITransferClient
    {
        private Action<int> _progressCallback;
        private readonly int _defaultRetryCount = 3;
        private readonly int _defaultRetryDelayMs = 1000;
        private readonly int _connectionTimeoutMs = 30000;
        private readonly int _operationTimeoutMs = 60000;
        private long _bandwidthLimitBytesPerSecond = 0; // 0 = unlimited
        private readonly object _transferLock = new object();
        private volatile bool _cancelRequested = false;

        // Progress tracking for file transfers
        private long _currentFileSize = 0;
        private long _currentBytesTransferred = 0;
        private DateTime _transferStartTime;

        public ProtocolType Protocol { get { return ProtocolType.Sftp; } }

        /// <summary>
        /// Sets bandwidth limit in bytes per second (0 = unlimited)
        /// </summary>
        public void SetBandwidthLimit(long bytesPerSecond)
        {
            _bandwidthLimitBytesPerSecond = bytesPerSecond;
        }

        /// <summary>
        /// Cancels current transfer operation
        /// </summary>
        public void CancelTransfer()
        {
            _cancelRequested = true;
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

            // Keyboard-interactive authentication (fallback)
            authMethods.Add(new KeyboardInteractiveAuthenticationMethod(settings.Username));

            if (authMethods.Count == 0)
            {
                throw new InvalidOperationException("No authentication method available. Provide either SSH key path or password.");
            }

            return new ConnectionInfo(settings.Host, settings.Port, settings.Username, authMethods.ToArray());
        }

        public bool TestConnection(ConnectionSettings settings, out string error)
        {
            return ExecuteWithRetry(() =>
            {
                using (var sftp = CreateSftpClient(settings))
                {
                    sftp.Connect();
                    var connected = sftp.IsConnected;
                    if (connected)
                    {
                        // Test basic operations
                        sftp.ListDirectory(".");
                    }
                    sftp.Disconnect();
                    return connected;
                }
            }, "SFTP connection test", out error);
        }

        public bool EnsureDirectory(ConnectionSettings settings, string remoteDir, out string error)
        {
            return ExecuteWithRetry(() =>
            {
                using (var sftp = CreateSftpClient(settings))
                {
                    sftp.Connect();
                    
                    // Normalize path
                    remoteDir = NormalizePath(remoteDir);
                    
                    if (!sftp.Exists(remoteDir))
                    {
                        CreateDirectoryRecursive(sftp, remoteDir);
                    }
                    
                    sftp.Disconnect();
                    return true;
                }
            }, $"ensure directory {remoteDir}", out error);
        }

        /// <summary>
        /// Creates directory recursively, handling parent directories
        /// </summary>
        private void CreateDirectoryRecursive(SftpClient sftp, string remotePath)
        {
            var parts = remotePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var currentPath = remotePath.StartsWith("/") ? "/" : "";
            
            foreach (var part in parts)
            {
                currentPath += part + "/";
                if (!sftp.Exists(currentPath))
                {
                    sftp.CreateDirectory(currentPath);
                }
            }
        }

        public bool UploadFile(ConnectionSettings settings, string localPath, string remotePath, bool overwrite, out string error)
        {
            return ExecuteFileTransfer(() =>
            {
                if (!File.Exists(localPath))
                {
                    throw new FileNotFoundException($"Source file does not exist: {localPath}");
                }

                var fileInfo = new FileInfo(localPath);
                _currentFileSize = fileInfo.Length;
                _currentBytesTransferred = 0;
                _transferStartTime = DateTime.Now;

                using (var sftp = CreateSftpClient(settings))
                {
                    sftp.Connect();
                    
                    remotePath = NormalizePath(remotePath);
                    
                    // Check if file exists and handle overwrite logic
                    if (!overwrite && sftp.Exists(remotePath))
                    {
                        throw new InvalidOperationException($"Destination file already exists: {remotePath}");
                    }
                    
                    // Ensure directory exists
                    var remoteDir = GetDirectoryPath(remotePath);
                    if (!string.IsNullOrEmpty(remoteDir) && !sftp.Exists(remoteDir))
                    {
                        CreateDirectoryRecursive(sftp, remoteDir);
                    }
                    
                    // Check for resumable transfer
                    long resumeOffset = 0;
                    if (sftp.Exists(remotePath))
                    {
                        var remoteFileInfo = sftp.GetAttributes(remotePath);
                        if (remoteFileInfo.Size < _currentFileSize)
                        {
                            resumeOffset = remoteFileInfo.Size;
                            _currentBytesTransferred = resumeOffset;
                        }
                    }
                    
                    using (var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read))
                    {
                        if (resumeOffset > 0)
                        {
                            fileStream.Seek(resumeOffset, SeekOrigin.Begin);
                        }
                        
                        // Use custom stream wrapper for bandwidth throttling and progress
                        using (var throttledStream = new ThrottledStream(fileStream, _bandwidthLimitBytesPerSecond))
                        {
                            if (resumeOffset > 0)
                            {
                                sftp.UploadFile(throttledStream, remotePath, true, ProgressCallback);
                            }
                            else
                            {
                                sftp.UploadFile(throttledStream, remotePath, overwrite, ProgressCallback);
                            }
                        }
                    }
                    
                    sftp.Disconnect();
                    return true;
                }
            }, $"upload file {localPath} to {remotePath}", out error);
        }

        public bool DownloadFile(ConnectionSettings settings, string remotePath, string localPath, bool overwrite, out string error)
        {
            return ExecuteFileTransfer(() =>
            {
                using (var sftp = CreateSftpClient(settings))
                {
                    sftp.Connect();
                    
                    remotePath = NormalizePath(remotePath);
                    
                    if (!sftp.Exists(remotePath))
                    {
                        throw new FileNotFoundException($"Source file does not exist: {remotePath}");
                    }
                    
                    var remoteFileInfo = sftp.GetAttributes(remotePath);
                    _currentFileSize = remoteFileInfo.Size;
                    _currentBytesTransferred = 0;
                    _transferStartTime = DateTime.Now;
                    
                    if (!overwrite && File.Exists(localPath))
                    {
                        throw new InvalidOperationException($"Destination file already exists: {localPath}");
                    }
                    
                    // Ensure local directory exists
                    var localDir = Path.GetDirectoryName(localPath);
                    if (!string.IsNullOrEmpty(localDir) && !Directory.Exists(localDir))
                    {
                        Directory.CreateDirectory(localDir);
                    }
                    
                    // Check for resumable transfer
                    long resumeOffset = 0;
                    if (File.Exists(localPath))
                    {
                        var localFileInfo = new FileInfo(localPath);
                        if (localFileInfo.Length < _currentFileSize)
                        {
                            resumeOffset = localFileInfo.Length;
                            _currentBytesTransferred = resumeOffset;
                        }
                    }
                    
                    using (var fileStream = new FileStream(localPath, 
                        resumeOffset > 0 ? FileMode.Append : FileMode.Create, FileAccess.Write))
                    {
                        // Use custom stream wrapper for bandwidth throttling and progress
                        using (var throttledStream = new ThrottledStream(fileStream, _bandwidthLimitBytesPerSecond))
                        {
                            sftp.DownloadFile(remotePath, throttledStream, ProgressCallback);
                        }
                    }
                    
                    sftp.Disconnect();
                    return true;
                }
            }, $"download file {remotePath} to {localPath}", out error);
        }

        public bool FileExists(ConnectionSettings settings, string remotePath, out bool exists, out string error)
        {
            exists = false;
            bool tempExists = false;
            
            var result = ExecuteWithRetry(() =>
            {
                using (var sftp = CreateSftpClient(settings))
                {
                    sftp.Connect();
                    
                    remotePath = NormalizePath(remotePath);
                    tempExists = sftp.Exists(remotePath);
                    
                    sftp.Disconnect();
                    return true;
                }
            }, $"check if file exists {remotePath}", out error);
            
            exists = tempExists;
            return result;
        }

        public bool DeleteFile(ConnectionSettings settings, string remotePath, out string error)
        {
            return ExecuteWithRetry(() =>
            {
                using (var sftp = CreateSftpClient(settings))
                {
                    sftp.Connect();
                    
                    remotePath = NormalizePath(remotePath);
                    
                    if (sftp.Exists(remotePath))
                    {
                        sftp.DeleteFile(remotePath);
                    }
                    
                    sftp.Disconnect();
                    return true;
                }
            }, $"delete file {remotePath}", out error);
        }

        public bool ListFiles(ConnectionSettings settings, string remoteDir, out List<string> files, out string error)
        {
            files = new List<string>();
            var tempFiles = new List<string>();
            
            var result = ExecuteWithRetry(() =>
            {
                using (var sftp = CreateSftpClient(settings))
                {
                    sftp.Connect();
                    
                    remoteDir = NormalizePath(remoteDir);
                    
                    var sftpFiles = sftp.ListDirectory(remoteDir);
                    foreach (var file in sftpFiles)
                    {
                        if (!file.IsDirectory && file.Name != "." && file.Name != "..")
                        {
                            tempFiles.Add(file.FullName);
                        }
                    }
                    
                    sftp.Disconnect();
                    return true;
                }
            }, $"list files in {remoteDir}", out error);
            
            files = tempFiles;
            return result;
        }

        public void SetProgressCallback(Action<int> progressCallback)
        {
            _progressCallback = progressCallback;
        }

        #region Helper Methods

        /// <summary>
        /// Normalizes path to Unix format for SFTP
        /// </summary>
        private string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "/";
                
            path = path.Replace('\\', '/');
            if (!path.StartsWith("/"))
                path = "/" + path;
            return path;
        }

        /// <summary>
        /// Gets directory path from file path
        /// </summary>
        private string GetDirectoryPath(string filePath)
        {
            var lastSlash = filePath.LastIndexOf('/');
            if (lastSlash > 0)
            {
                return filePath.Substring(0, lastSlash);
            }
            return "/";
        }

        /// <summary>
        /// Executes an operation with retry logic and advanced error handling
        /// </summary>
        private bool ExecuteWithRetry(Func<bool> operation, string operationName, out string error)
        {
            error = null;
            var retryCount = 0;
            var lastException = (Exception)null;

            while (retryCount <= _defaultRetryCount)
            {
                try
                {
                    _cancelRequested = false;
                    return operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    retryCount++;

                    // Check if this is a retryable error
                    if (!IsRetryableError(ex) || retryCount > _defaultRetryCount)
                    {
                        error = $"{operationName} failed: {ex.Message}";
                        if (ex.InnerException != null)
                        {
                            error += $" Inner: {ex.InnerException.Message}";
                        }
                        return false;
                    }

                    // Wait before retry with exponential backoff
                    var delayMs = _defaultRetryDelayMs * (int)Math.Pow(2, retryCount - 1);
                    Thread.Sleep(delayMs);
                }
            }

            error = $"{operationName} failed after {_defaultRetryCount} retries: {lastException?.Message}";
            return false;
        }

        /// <summary>
        /// Executes file transfer operations with retry logic
        /// </summary>
        private bool ExecuteFileTransfer(Func<bool> operation, string operationName, out string error)
        {
            error = null;
            var retryCount = 0;
            var lastException = (Exception)null;

            while (retryCount <= _defaultRetryCount)
            {
                try
                {
                    _cancelRequested = false;
                    lock (_transferLock)
                    {
                        return operation();
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    retryCount++;

                    // Check for cancellation
                    if (_cancelRequested)
                    {
                        error = $"{operationName} was cancelled";
                        return false;
                    }

                    // Check if this is a retryable error
                    if (!IsRetryableError(ex) || retryCount > _defaultRetryCount)
                    {
                        error = $"{operationName} failed: {ex.Message}";
                        if (ex.InnerException != null)
                        {
                            error += $" Inner: {ex.InnerException.Message}";
                        }
                        return false;
                    }

                    // Wait before retry with exponential backoff
                    var delayMs = _defaultRetryDelayMs * (int)Math.Pow(2, retryCount - 1);
                    Thread.Sleep(delayMs);

                    // Reset progress for retry
                    _currentBytesTransferred = 0;
                }
            }

            error = $"{operationName} failed after {_defaultRetryCount} retries: {lastException?.Message}";
            return false;
        }

        /// <summary>
        /// Determines if an error is retryable
        /// </summary>
        private bool IsRetryableError(Exception ex)
        {
            // Network-related errors that can be retried
            if (ex is SshConnectionException ||
                ex is SshOperationTimeoutException ||
                ex is SshException)
            {
                return true;
            }

            // Check for specific error messages that indicate retryable conditions
            var message = ex.Message.ToLower();
            return message.Contains("timeout") ||
                   message.Contains("connection") ||
                   message.Contains("network") ||
                   message.Contains("socket") ||
                   message.Contains("host") ||
                   message.Contains("unreachable");
        }

        /// <summary>
        /// Progress callback for SSH.NET file transfers
        /// </summary>
        private void ProgressCallback(ulong uploadedBytes)
        {
            if (_cancelRequested)
            {
                throw new OperationCanceledException("Transfer was cancelled");
            }

            _currentBytesTransferred = (long)uploadedBytes;
            
            if (_progressCallback != null && _currentFileSize > 0)
            {
                var percentage = (int)((_currentBytesTransferred * 100) / _currentFileSize);
                _progressCallback(Math.Min(percentage, 100));
            }

            // Apply bandwidth throttling if configured
            if (_bandwidthLimitBytesPerSecond > 0)
            {
                var elapsed = DateTime.Now - _transferStartTime;
                var expectedDurationMs = (_currentBytesTransferred * 1000) / _bandwidthLimitBytesPerSecond;
                var actualDurationMs = elapsed.TotalMilliseconds;
                
                if (actualDurationMs < expectedDurationMs)
                {
                    var delayMs = (int)(expectedDurationMs - actualDurationMs);
                    if (delayMs > 0)
                    {
                        Thread.Sleep(delayMs);
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Stream wrapper that provides bandwidth throttling
    /// </summary>
    internal class ThrottledStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly long _maxBytesPerSecond;
        private readonly DateTime _startTime;
        private long _bytesRead;
        private long _bytesWritten;

        public ThrottledStream(Stream baseStream, long maxBytesPerSecond)
        {
            _baseStream = baseStream;
            _maxBytesPerSecond = maxBytesPerSecond;
            _startTime = DateTime.Now;
        }

        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => _baseStream.CanWrite;
        public override long Length => _baseStream.Length;

        public override long Position
        {
            get => _baseStream.Position;
            set => _baseStream.Position = value;
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = _baseStream.Read(buffer, offset, count);
            _bytesRead += bytesRead;
            
            if (_maxBytesPerSecond > 0)
            {
                ThrottleIfNeeded(_bytesRead);
            }
            
            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Write(buffer, offset, count);
            _bytesWritten += count;
            
            if (_maxBytesPerSecond > 0)
            {
                ThrottleIfNeeded(_bytesWritten);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        private void ThrottleIfNeeded(long totalBytes)
        {
            var elapsed = DateTime.Now - _startTime;
            var expectedDurationMs = (totalBytes * 1000) / _maxBytesPerSecond;
            var actualDurationMs = elapsed.TotalMilliseconds;
            
            if (actualDurationMs < expectedDurationMs)
            {
                var delayMs = (int)(expectedDurationMs - actualDurationMs);
                if (delayMs > 0)
                {
                    Thread.Sleep(delayMs);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _baseStream?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
