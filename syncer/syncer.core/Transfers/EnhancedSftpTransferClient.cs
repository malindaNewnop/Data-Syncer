using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using Renci.SshNet;
using Renci.SshNet.Common;
using syncer.core.Configuration;

namespace syncer.core
{
    /// <summary>
    /// Enhanced SFTP Transfer Client with advanced features including
    /// secure key handling, bandwidth throttling, transfer resumption,
    /// integrity verification, and comprehensive error handling
    /// </summary>
    public class EnhancedSftpTransferClient : SftpTransferClient
    {
        private SftpConfiguration _config;
        private readonly object _statsLock = new object();
        private TransferStatistics _currentStats;

        // Events for progress and completion notifications
        public event EventHandler<TransferProgressEventArgs> TransferProgress;
        public event EventHandler<TransferCompletedEventArgs> TransferCompleted;

        public EnhancedSftpTransferClient() : this(new SftpConfiguration())
        {
        }

        public EnhancedSftpTransferClient(SftpConfiguration config)
        {
            _config = config ?? new SftpConfiguration();
            SetBandwidthLimit(_config.BandwidthLimitBytesPerSecond);
        }

        /// <summary>
        /// Updates the configuration
        /// </summary>
        public void UpdateConfiguration(SftpConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            SetBandwidthLimit(_config.BandwidthLimitBytesPerSecond);
        }

        /// <summary>
        /// Gets the current configuration
        /// </summary>
        public SftpConfiguration GetConfiguration()
        {
            return _config;
        }

        /// <summary>
        /// Uploads a file with enhanced features and progress tracking
        /// </summary>
        public bool UploadFileEnhanced(ConnectionSettings settings, string localPath, string remotePath, 
            bool overwrite, out string error, out TransferStatistics statistics)
        {
            statistics = new TransferStatistics { StartTime = DateTime.Now };
            _currentStats = statistics;

            try
            {
                var result = UploadFileWithIntegrity(settings, localPath, remotePath, overwrite, out error);
                
                statistics.EndTime = DateTime.Now;
                statistics.Success = result;
                statistics.ErrorMessage = error;

                if (result && statistics.TotalBytes > 0)
                {
                    // AverageSpeedBytesPerSecond is calculated automatically
                }

                TransferCompleted?.Invoke(this, new TransferCompletedEventArgs
                {
                    Statistics = statistics,
                    Success = result,
                    ErrorMessage = error
                });

                return result;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                statistics.Success = false;
                statistics.ErrorMessage = error;
                statistics.EndTime = DateTime.Now;
                return false;
            }
        }

        /// <summary>
        /// Downloads a file with enhanced features and progress tracking
        /// </summary>
        public bool DownloadFileEnhanced(ConnectionSettings settings, string remotePath, string localPath, 
            bool overwrite, out string error, out TransferStatistics statistics)
        {
            statistics = new TransferStatistics { StartTime = DateTime.Now };
            _currentStats = statistics;

            try
            {
                var result = DownloadFileWithIntegrity(settings, remotePath, localPath, overwrite, out error);
                
                statistics.EndTime = DateTime.Now;
                statistics.Success = result;
                statistics.ErrorMessage = error;

                if (result && statistics.TotalBytes > 0)
                {
                    // AverageSpeedBytesPerSecond is calculated automatically
                }

                TransferCompleted?.Invoke(this, new TransferCompletedEventArgs
                {
                    Statistics = statistics,
                    Success = result,
                    ErrorMessage = error
                });

                return result;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                statistics.Success = false;
                statistics.ErrorMessage = error;
                statistics.EndTime = DateTime.Now;
                return false;
            }
        }

        /// <summary>
        /// Uploads file with integrity verification
        /// </summary>
        private bool UploadFileWithIntegrity(ConnectionSettings settings, string localPath, string remotePath, bool overwrite, out string error)
        {
            var result = UploadFile(settings, localPath, remotePath, overwrite, out error);
            
            if (result && _config.VerifyTransferIntegrity)
            {
                if (!VerifyFileIntegrity(settings, localPath, remotePath, out var verifyError))
                {
                    error = $"Upload succeeded but integrity verification failed: {verifyError}";
                    return false;
                }
            }

            return result;
        }

        /// <summary>
        /// Downloads file with integrity verification
        /// </summary>
        private bool DownloadFileWithIntegrity(ConnectionSettings settings, string remotePath, string localPath, bool overwrite, out string error)
        {
            var result = DownloadFile(settings, remotePath, localPath, overwrite, out error);
            
            if (result && _config.VerifyTransferIntegrity)
            {
                if (!VerifyFileIntegrity(settings, localPath, remotePath, out var verifyError))
                {
                    error = $"Download succeeded but integrity verification failed: {verifyError}";
                    return false;
                }
            }

            return result;
        }

        /// <summary>
        /// Verifies file integrity by comparing checksums
        /// </summary>
        private bool VerifyFileIntegrity(ConnectionSettings settings, string localPath, string remotePath, out string error)
        {
            error = null;
            
            try
            {
                // Calculate local file checksum
                string localChecksum = CalculateFileChecksum(localPath);
                
                // Calculate remote file checksum
                string remoteChecksum = CalculateRemoteFileChecksum(settings, remotePath);
                
                if (localChecksum != remoteChecksum)
                {
                    error = $"File integrity check failed. Local: {localChecksum}, Remote: {remoteChecksum}";
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                error = $"Integrity verification failed: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Calculates SHA-256 checksum of a local file
        /// </summary>
        private string CalculateFileChecksum(string filePath)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// Calculates SHA-256 checksum of a remote file
        /// </summary>
        private string CalculateRemoteFileChecksum(ConnectionSettings settings, string remotePath)
        {
            using (var sftp = CreateSftpClient(settings))
            {
                sftp.Connect();
                
                using (var sha256 = SHA256.Create())
                using (var stream = sftp.OpenRead(remotePath))
                {
                    var hash = sha256.ComputeHash(stream);
                    sftp.Disconnect();
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        /// <summary>
        /// Gets comprehensive directory listing with file attributes
        /// </summary>
        public bool GetDirectoryListingDetailed(ConnectionSettings settings, string remoteDir, 
            out List<SftpFileInfo> files, out string error)
        {
            files = new List<SftpFileInfo>();
            error = null;

            try
            {
                using (var sftp = CreateSftpClient(settings))
                {
                    sftp.Connect();
                    
                    remoteDir = NormalizePath(remoteDir);
                    
                    var sftpFiles = sftp.ListDirectory(remoteDir);
                    foreach (var file in sftpFiles)
                    {
                        if (file.Name != "." && file.Name != "..")
                        {
                            files.Add(new SftpFileInfo
                            {
                                Name = file.Name,
                                FullPath = file.FullName,
                                IsDirectory = file.IsDirectory,
                                Size = file.Length,
                                LastModified = file.LastWriteTime,
                                Permissions = "N/A", // Simplified for .NET 3.5 compatibility
                                Owner = file.Attributes.UserId.ToString(),
                                Group = file.Attributes.GroupId.ToString()
                            });
                        }
                    }
                    
                    sftp.Disconnect();
                    return true;
                }
            }
            catch (Exception ex)
            {
                error = $"Failed to get directory listing: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Synchronizes a local directory with a remote directory
        /// </summary>
        public bool SynchronizeDirectory(ConnectionSettings settings, string localDir, string remoteDir, 
            SyncOptions options, out string error, out List<TransferStatistics> transferStats)
        {
            error = null;
            transferStats = new List<TransferStatistics>();

            try
            {
                // Get local files
                var localFiles = Directory.GetFiles(localDir, "*", 
                    options.IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                // Get remote files
                if (!GetDirectoryListingDetailed(settings, remoteDir, out var remoteFiles, out error))
                {
                    return false;
                }

                var remoteFileDict = new Dictionary<string, SftpFileInfo>();
                foreach (var file in remoteFiles)
                {
                    if (!file.IsDirectory)
                    {
                        var relativePath = file.FullPath.Substring(remoteDir.Length).TrimStart('/');
                        remoteFileDict[relativePath] = file;
                    }
                }

                // Process each local file
                foreach (var localFile in localFiles)
                {
                    var relativePath = GetRelativePath(localDir, localFile).Replace('\\', '/');
                    var remotePath = remoteDir.TrimEnd('/') + "/" + relativePath;

                    bool shouldTransfer = true;
                    
                    if (remoteFileDict.ContainsKey(relativePath))
                    {
                        var remoteFile = remoteFileDict[relativePath];
                        var localFileInfo = new FileInfo(localFile);

                        // Check if transfer is needed based on sync options
                        if (options.SyncMode == SyncMode.TimestampAndSize)
                        {
                            shouldTransfer = localFileInfo.LastWriteTime > remoteFile.LastModified ||
                                           localFileInfo.Length != remoteFile.Size;
                        }
                        else if (options.SyncMode == SyncMode.SizeOnly)
                        {
                            shouldTransfer = localFileInfo.Length != remoteFile.Size;
                        }
                        else if (options.SyncMode == SyncMode.TimestampOnly)
                        {
                            shouldTransfer = localFileInfo.LastWriteTime > remoteFile.LastModified;
                        }
                    }

                    if (shouldTransfer)
                    {
                        if (UploadFileEnhanced(settings, localFile, remotePath, true, out var uploadError, out var stats))
                        {
                            transferStats.Add(stats);
                        }
                        else
                        {
                            error = uploadError;
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                error = $"Directory synchronization failed: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Enhanced progress callback with detailed statistics
        /// </summary>
        protected virtual void OnProgressCallback(ulong uploadedBytes)
        {
            if (_currentStats != null)
            {
                lock (_statsLock)
                {
                    _currentStats.TransferredBytes = (long)uploadedBytes;
                    
                    if (_currentStats.TotalBytes > 0)
                    {
                        // ProgressPercentage is calculated automatically
                    }

                    var elapsed = DateTime.Now - _currentStats.StartTime;
                    var speed = elapsed.TotalSeconds > 0 ? _currentStats.TransferredBytes / elapsed.TotalSeconds : 0;

                    TransferProgress?.Invoke(this, new TransferProgressEventArgs
                    {
                        TotalBytes = _currentStats.TotalBytes,
                        TransferredBytes = _currentStats.TransferredBytes,
                        ProgressPercentage = _currentStats.ProgressPercentage,
                        SpeedBytesPerSecond = speed,
                        Elapsed = elapsed,
                        EstimatedTimeRemaining = speed > 0 ? 
                            TimeSpan.FromSeconds((_currentStats.TotalBytes - _currentStats.TransferredBytes) / speed) : 
                            TimeSpan.Zero
                    });
                }
            }
        }
        
        // Helper method to normalize paths
        private string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "/";
                
            path = path.Replace('\\', '/');
            if (!path.StartsWith("/"))
                path = "/" + path;
            return path;
        }

        // .NET 3.5 compatible GetRelativePath implementation
        private string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath) || string.IsNullOrEmpty(toPath))
                return toPath;

            Uri fromUri = new Uri(fromPath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? fromPath : fromPath + Path.DirectorySeparatorChar);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme)
                return toPath;

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
                relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);

            return relativePath;
        }

        // Creates SFTP client - should be accessible from base class
        private SftpClient CreateSftpClient(ConnectionSettings settings)
        {
            // This should call the base class method
            // For now, implementing basic version
            var connectionInfo = new ConnectionInfo(settings.Host, settings.Port, settings.Username,
                new PasswordAuthenticationMethod(settings.Username, settings.Password));
            
            var client = new SftpClient(connectionInfo);
            client.ConnectionInfo.Timeout = TimeSpan.FromMilliseconds(_config.ConnectionTimeoutMs);
            client.OperationTimeout = TimeSpan.FromMilliseconds(_config.OperationTimeoutMs);
            return client;
        }
    }

    /// <summary>
    /// Detailed file information for SFTP files
    /// </summary>
    public class SftpFileInfo
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string Permissions { get; set; }
        public string Owner { get; set; }
        public string Group { get; set; }
    }

    /// <summary>
    /// Synchronization options
    /// </summary>
    public class SyncOptions
    {
        public SyncMode SyncMode { get; set; } = SyncMode.TimestampAndSize;
        public bool IncludeSubdirectories { get; set; } = true;
        public bool DeleteExtraFiles { get; set; } = false;
        public bool PreserveTimestamps { get; set; } = true;
        public List<string> ExcludePatterns { get; set; } = new List<string>();
    }

    /// <summary>
    /// Synchronization modes
    /// </summary>
    public enum SyncMode
    {
        Always,
        TimestampOnly,
        SizeOnly,
        TimestampAndSize,
        Checksum
    }
}
