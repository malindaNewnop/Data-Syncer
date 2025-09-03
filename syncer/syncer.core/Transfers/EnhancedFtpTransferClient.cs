using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace syncer.core.Transfers
{
    /// <summary>
    /// Enhanced FTP Transfer Client with comprehensive error handling, retry logic, and progress tracking
    /// .NET 3.5 Compatible
    /// </summary>
    public class EnhancedFtpTransferClient : ITransferClient
    {
        private Action<int> _progressCallback;
        private readonly int _defaultRetryCount = 3;
        private readonly int _defaultRetryDelayMs = 1000;
        private readonly int _connectionTimeoutMs = 30000;
        private readonly int _readWriteTimeoutMs = 60000;
        private readonly object _transferLock = new object();
        private volatile bool _cancelRequested = false;

        // Progress tracking for file transfers
        private long _currentFileSize = 0;
        private long _currentBytesTransferred = 0;
        private DateTime _transferStartTime;

        public ProtocolType Protocol { get { return ProtocolType.Ftp; } }

        /// <summary>
        /// Cancels current transfer operation
        /// </summary>
        public void CancelTransfer()
        {
            _cancelRequested = true;
        }

        /// <summary>
        /// Test FTP connection with comprehensive error handling
        /// </summary>
        public bool TestConnection(ConnectionSettings settings, out string error)
        {
            error = null;
            try
            {
                if (settings == null)
                {
                    error = "Connection settings are required for FTP";
                    return false;
                }

                if (string.IsNullOrEmpty(settings.Host))
                {
                    error = "FTP host is required";
                    return false;
                }

                var testStartTime = DateTime.Now;
                var request = CreateFtpRequest(settings, "/", WebRequestMethods.Ftp.ListDirectory);
                request.Timeout = _connectionTimeoutMs;
                
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    var connectionTime = DateTime.Now - testStartTime;
                    
                    if (response.StatusCode == FtpStatusCode.OpeningData ||
                        response.StatusCode == FtpStatusCode.DataAlreadyOpen)
                    {
                        return true;
                    }
                    else
                    {
                        error = $"FTP server returned status: {response.StatusCode} - {response.StatusDescription}";
                        return false;
                    }
                }
            }
            catch (WebException ex)
            {
                error = $"FTP connection failed: {GetWebExceptionMessage(ex)}";
                return false;
            }
            catch (Exception ex)
            {
                error = $"FTP connection test failed: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Ensure directory exists on FTP server with recursive creation support
        /// </summary>
        public bool EnsureDirectory(ConnectionSettings settings, string remoteDir, out string error)
        {
            error = null;
            try
            {
                if (string.IsNullOrEmpty(remoteDir))
                {
                    error = "Remote directory path cannot be empty";
                    return false;
                }

                // Normalize path separators
                remoteDir = remoteDir.Replace('\\', '/');
                if (!remoteDir.StartsWith("/"))
                    remoteDir = "/" + remoteDir;

                // Remove trailing slash except for root
                if (remoteDir.Length > 1 && remoteDir.EndsWith("/"))
                    remoteDir = remoteDir.Substring(0, remoteDir.Length - 1);

                // Check if directory exists first
                try
                {
                    var request = CreateFtpRequest(settings, remoteDir, WebRequestMethods.Ftp.ListDirectory);
                    using (var response = (FtpWebResponse)request.GetResponse())
                    {
                        return true; // Directory exists
                    }
                }
                catch (WebException)
                {
                    // Directory doesn't exist, need to create it
                }

                // Create directory recursively
                var pathParts = remoteDir.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                var currentPath = "";

                foreach (var part in pathParts)
                {
                    currentPath += "/" + part;
                    
                    try
                    {
                        // Try to list the directory to see if it exists
                        var listRequest = CreateFtpRequest(settings, currentPath, WebRequestMethods.Ftp.ListDirectory);
                        using (var listResponse = (FtpWebResponse)listRequest.GetResponse())
                        {
                            // Directory exists, continue
                        }
                    }
                    catch (WebException)
                    {
                        // Directory doesn't exist, create it
                        try
                        {
                            var createRequest = CreateFtpRequest(settings, currentPath, WebRequestMethods.Ftp.MakeDirectory);
                            using (var createResponse = (FtpWebResponse)createRequest.GetResponse())
                            {
                                if (createResponse.StatusCode != FtpStatusCode.PathnameCreated &&
                                    createResponse.StatusCode != FtpStatusCode.FileActionOK)
                                {
                                    error = $"Failed to create directory '{currentPath}': {createResponse.StatusDescription}";
                                    return false;
                                }
                            }
                        }
                        catch (WebException createEx)
                        {
                            // Check if error is "directory already exists"
                            if (createEx.Response is FtpWebResponse ftpResponse)
                            {
                                if (ftpResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                                {
                                    // Directory might already exist, continue
                                    continue;
                                }
                            }
                            
                            error = $"Failed to create directory '{currentPath}': {GetWebExceptionMessage(createEx)}";
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to ensure directory '{remoteDir}': {ex.Message}";
                return false;
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

                try
                {
                    if (!File.Exists(localPath))
                    {
                        error = $"Source file does not exist: {localPath}";
                        return false;
                    }

                    // Normalize remote path
                    remotePath = remotePath.Replace('\\', '/');
                    if (!remotePath.StartsWith("/"))
                        remotePath = "/" + remotePath;

                    // Check if file exists and overwrite is false
                    if (!overwrite)
                    {
                        bool exists;
                        string checkError;
                        if (FileExists(settings, remotePath, out exists, out checkError))
                        {
                            if (exists)
                            {
                                error = $"Destination file already exists: {remotePath}";
                                return false;
                            }
                        }
                    }

                    // Ensure remote directory exists
                    var remoteDir = Path.GetDirectoryName(remotePath).Replace('\\', '/');
                    if (!string.IsNullOrEmpty(remoteDir))
                    {
                        string dirError;
                        if (!EnsureDirectory(settings, remoteDir, out dirError))
                        {
                            error = $"Failed to create remote directory: {dirError}";
                            return false;
                        }
                    }

                    // Get file size for progress tracking
                    var fileInfo = new FileInfo(localPath);
                    _currentFileSize = fileInfo.Length;
                    _currentBytesTransferred = 0;
                    _transferStartTime = DateTime.Now;

                    // Upload the file
                    var request = CreateFtpRequest(settings, remotePath, WebRequestMethods.Ftp.UploadFile);
                    request.Timeout = _connectionTimeoutMs;
                    request.ReadWriteTimeout = _readWriteTimeoutMs;
                    request.ContentLength = _currentFileSize;

                    using (var requestStream = request.GetRequestStream())
                    using (var fileStream = File.OpenRead(localPath))
                    {
                        byte[] buffer = new byte[8192]; // 8KB buffer for better performance
                        int bytesRead;
                        
                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            if (_cancelRequested)
                            {
                                error = "Transfer was cancelled";
                                return false;
                            }

                            requestStream.Write(buffer, 0, bytesRead);
                            _currentBytesTransferred += bytesRead;

                            // Update progress
                            if (_progressCallback != null && _currentFileSize > 0)
                            {
                                var progress = (int)((_currentBytesTransferred * 100) / _currentFileSize);
                                _progressCallback(Math.Min(progress, 100));
                            }
                        }
                    }

                    using (var response = (FtpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == FtpStatusCode.ClosingData ||
                            response.StatusCode == FtpStatusCode.FileActionOK)
                        {
                            return true;
                        }
                        else
                        {
                            error = $"Upload failed with status: {response.StatusCode} - {response.StatusDescription}";
                            
                            if (attempt < _defaultRetryCount)
                            {
                                Thread.Sleep(_defaultRetryDelayMs * (attempt + 1));
                                continue;
                            }
                            return false;
                        }
                    }
                }
                catch (WebException ex)
                {
                    error = $"Upload failed: {GetWebExceptionMessage(ex)}";
                    
                    if (attempt < _defaultRetryCount && !IsConnectionError(ex))
                    {
                        Thread.Sleep(_defaultRetryDelayMs * (attempt + 1));
                        continue;
                    }
                    return false;
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
            }

            return false;
        }

        /// <summary>
        /// Download file from FTP server with retry logic and progress tracking
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

                try
                {
                    // Normalize remote path
                    remotePath = remotePath.Replace('\\', '/');
                    if (!remotePath.StartsWith("/"))
                        remotePath = "/" + remotePath;

                    // Check if remote file exists
                    bool exists;
                    string checkError;
                    if (!FileExists(settings, remotePath, out exists, out checkError))
                    {
                        error = $"Failed to check if remote file exists: {checkError}";
                        return false;
                    }

                    if (!exists)
                    {
                        error = $"Source file does not exist: {remotePath}";
                        return false;
                    }

                    // Check if local file exists and overwrite is false
                    if (!overwrite && File.Exists(localPath))
                    {
                        error = $"Destination file already exists: {localPath}";
                        return false;
                    }

                    // Ensure local directory exists
                    var localDir = Path.GetDirectoryName(localPath);
                    if (!string.IsNullOrEmpty(localDir) && !Directory.Exists(localDir))
                    {
                        Directory.CreateDirectory(localDir);
                    }

                    // Get file size for progress tracking
                    _currentFileSize = GetFileSize(settings, remotePath);
                    _currentBytesTransferred = 0;
                    _transferStartTime = DateTime.Now;

                    // Download the file
                    var request = CreateFtpRequest(settings, remotePath, WebRequestMethods.Ftp.DownloadFile);
                    request.Timeout = _connectionTimeoutMs;
                    request.ReadWriteTimeout = _readWriteTimeoutMs;

                    using (var response = (FtpWebResponse)request.GetResponse())
                    using (var responseStream = response.GetResponseStream())
                    using (var fileStream = File.Create(localPath))
                    {
                        byte[] buffer = new byte[8192]; // 8KB buffer for better performance
                        int bytesRead;
                        
                        while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            if (_cancelRequested)
                            {
                                error = "Transfer was cancelled";
                                // Clean up partial file
                                try
                                {
                                    fileStream.Close();
                                    File.Delete(localPath);
                                }
                                catch { } // Ignore cleanup errors
                                return false;
                            }

                            fileStream.Write(buffer, 0, bytesRead);
                            _currentBytesTransferred += bytesRead;

                            // Update progress
                            if (_progressCallback != null && _currentFileSize > 0)
                            {
                                var progress = (int)((_currentBytesTransferred * 100) / _currentFileSize);
                                _progressCallback(Math.Min(progress, 100));
                            }
                        }
                    }

                    // Verify download by checking file size
                    if (File.Exists(localPath))
                    {
                        var localFileInfo = new FileInfo(localPath);
                        if (_currentFileSize > 0 && localFileInfo.Length != _currentFileSize)
                        {
                            error = $"Download verification failed: file sizes do not match (local: {localFileInfo.Length}, remote: {_currentFileSize})";
                            
                            if (attempt < _defaultRetryCount)
                            {
                                Thread.Sleep(_defaultRetryDelayMs * (attempt + 1));
                                continue;
                            }
                            return false;
                        }
                        return true;
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
                catch (WebException ex)
                {
                    error = $"Download failed: {GetWebExceptionMessage(ex)}";
                    
                    if (attempt < _defaultRetryCount && !IsConnectionError(ex))
                    {
                        Thread.Sleep(_defaultRetryDelayMs * (attempt + 1));
                        continue;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    error = $"Download failed: {ex.Message}";
                    
                    if (attempt < _defaultRetryCount)
                    {
                        Thread.Sleep(_defaultRetryDelayMs * (attempt + 1));
                        continue;
                    }
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if file exists on FTP server
        /// </summary>
        public bool FileExists(ConnectionSettings settings, string remotePath, out bool exists, out string error)
        {
            exists = false;
            error = null;

            try
            {
                remotePath = remotePath.Replace('\\', '/');
                if (!remotePath.StartsWith("/"))
                    remotePath = "/" + remotePath;

                var request = CreateFtpRequest(settings, remotePath, WebRequestMethods.Ftp.GetFileSize);
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    exists = true;
                    return true;
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is FtpWebResponse ftpResponse)
                {
                    if (ftpResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        exists = false;
                        return true;
                    }
                }
                
                error = $"Failed to check file existence: {GetWebExceptionMessage(ex)}";
                return false;
            }
            catch (Exception ex)
            {
                error = $"Failed to check file existence: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Delete file from FTP server
        /// </summary>
        public bool DeleteFile(ConnectionSettings settings, string remotePath, out string error)
        {
            error = null;
            try
            {
                remotePath = remotePath.Replace('\\', '/');
                if (!remotePath.StartsWith("/"))
                    remotePath = "/" + remotePath;

                var request = CreateFtpRequest(settings, remotePath, WebRequestMethods.Ftp.DeleteFile);
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == FtpStatusCode.FileActionOK;
                }
            }
            catch (WebException ex)
            {
                error = $"Delete failed: {GetWebExceptionMessage(ex)}";
                return false;
            }
            catch (Exception ex)
            {
                error = $"Delete failed: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// List files in FTP directory
        /// </summary>
        public bool ListFiles(ConnectionSettings settings, string remoteDir, out List<string> files, out string error)
        {
            files = new List<string>();
            error = null;

            try
            {
                remoteDir = remoteDir.Replace('\\', '/');
                if (!remoteDir.StartsWith("/"))
                    remoteDir = "/" + remoteDir;
                if (!remoteDir.EndsWith("/"))
                    remoteDir = remoteDir + "/";

                // Recursively list all files including subfolders
                ListFilesRecursive(settings, remoteDir, files, out error);

                return error == null;
            }
            catch (WebException ex)
            {
                error = $"List files failed: {GetWebExceptionMessage(ex)}";
                return false;
            }
            catch (Exception ex)
            {
                error = $"List files failed: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Recursively list all files in directory and subdirectories
        /// </summary>
        private void ListFilesRecursive(ConnectionSettings settings, string directory, List<string> files, out string error)
        {
            error = null;
            try
            {
                var request = CreateFtpRequest(settings, directory, WebRequestMethods.Ftp.ListDirectoryDetails);
                using (var response = (FtpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var fileName = line.Trim();
                        if (!string.IsNullOrEmpty(fileName) && fileName != "." && fileName != "..")
                        {
                            var fullPath = directory + fileName;
                            
                            // Try to determine if this is a file or directory
                            // Simple approach: assume it's a directory if listing it doesn't fail
                            bool isDirectory = false;
                            try
                            {
                                var testRequest = CreateFtpRequest(settings, fullPath + "/", WebRequestMethods.Ftp.ListDirectory);
                                testRequest.Timeout = 5000; // Short timeout for directory test
                                using (var testResponse = (FtpWebResponse)testRequest.GetResponse())
                                {
                                    isDirectory = true;
                                }
                            }
                            catch
                            {
                                isDirectory = false;
                            }
                            
                            if (isDirectory)
                            {
                                // Recursively process subdirectory
                                string subError;
                                ListFilesRecursive(settings, fullPath + "/", files, out subError);
                                if (subError != null && error == null)
                                    error = subError;
                            }
                            else
                            {
                                files.Add(fullPath);
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                error = $"List files failed in directory {directory}: {GetWebExceptionMessage(ex)}";
            }
            catch (Exception ex)
            {
                error = $"List files failed in directory {directory}: {ex.Message}";
            }
        }

        public void SetProgressCallback(Action<int> progressCallback)
        {
            _progressCallback = progressCallback;
        }

        /// <summary>
        /// Get file size from FTP server
        /// </summary>
        private long GetFileSize(ConnectionSettings settings, string remotePath)
        {
            try
            {
                var request = CreateFtpRequest(settings, remotePath, WebRequestMethods.Ftp.GetFileSize);
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    return response.ContentLength;
                }
            }
            catch
            {
                return 0; // Unknown size
            }
        }

        /// <summary>
        /// Create FTP request with common settings
        /// </summary>
        private FtpWebRequest CreateFtpRequest(ConnectionSettings settings, string path, string method)
        {
            var uri = $"ftp://{settings.Host}:{settings.Port}{path}";
            var request = (FtpWebRequest)WebRequest.Create(uri);
            
            request.Method = method;
            request.UsePassive = settings.UsePassiveMode;
            request.UseBinary = true;
            request.KeepAlive = false;
            request.Timeout = _connectionTimeoutMs;
            
            if (!string.IsNullOrEmpty(settings.Username))
            {
                request.Credentials = new NetworkCredential(settings.Username, settings.Password ?? string.Empty);
            }

            return request;
        }

        /// <summary>
        /// Get user-friendly error message from WebException
        /// </summary>
        private string GetWebExceptionMessage(WebException ex)
        {
            if (ex.Response is FtpWebResponse ftpResponse)
            {
                return $"{ftpResponse.StatusDescription} ({ftpResponse.StatusCode})";
            }
            
            switch (ex.Status)
            {
                case WebExceptionStatus.ConnectFailure:
                    return "Could not connect to FTP server. Check host and port.";
                case WebExceptionStatus.Timeout:
                    return "Connection timed out. Server may be overloaded or unreachable.";
                case WebExceptionStatus.NameResolutionFailure:
                    return "Could not resolve FTP server hostname.";
                case WebExceptionStatus.ProtocolError:
                    return "FTP protocol error occurred.";
                default:
                    return ex.Message;
            }
        }

        /// <summary>
        /// Check if WebException indicates a connection error that shouldn't be retried
        /// </summary>
        private bool IsConnectionError(WebException ex)
        {
            return ex.Status == WebExceptionStatus.ConnectFailure ||
                   ex.Status == WebExceptionStatus.NameResolutionFailure ||
                   ex.Status == WebExceptionStatus.ProxyNameResolutionFailure;
        }
    }
}
