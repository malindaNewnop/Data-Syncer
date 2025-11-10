using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

namespace syncer.ui.Services
{
    /// <summary>
    /// Tracks files that failed to transfer due to locking and manages retry attempts
    /// </summary>
    [Serializable]
    [XmlRoot("LockedFileInfo")]
    public class LockedFileInfo
    {
        [XmlElement("FilePath")]
        public string FilePath { get; set; }
        
        [XmlElement("RetryCount")]
        public int RetryCount { get; set; }
        
        [XmlElement("LastRetryTime")]
        public DateTime LastRetryTime { get; set; }
        
        [XmlElement("FirstLockedTime")]
        public DateTime FirstLockedTime { get; set; }
        
        [XmlElement("LastError")]
        public string LastError { get; set; }

        public LockedFileInfo()
        {
        }

        public LockedFileInfo(string filePath)
        {
            FilePath = filePath;
            RetryCount = 0;
            FirstLockedTime = DateTime.Now;
            LastRetryTime = DateTime.Now;
        }
    }

    /// <summary>
    /// Manages file locking detection and retry logic for timer jobs
    /// </summary>
    [Serializable]
    public class FileRetryManager
    {
        private Dictionary<string, LockedFileInfo> _lockedFiles;
        private const int MaxRetryAttempts = 5; // Maximum retry attempts per iteration
        private const int RetryDelayMs = 2000; // 2 seconds between retries
        private const int MaxRetryDelayMs = 10000; // Maximum 10 seconds delay

        public FileRetryManager()
        {
            _lockedFiles = new Dictionary<string, LockedFileInfo>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if a file is locked/in-use by another process
        /// </summary>
        public static bool IsFileLocked(string filePath, out string error)
        {
            error = null;
            FileStream stream = null;

            try
            {
                // Try to open the file with exclusive access
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return false; // File is not locked
            }
            catch (IOException ioEx)
            {
                // Check if it's a sharing violation (file is locked)
                const int ERROR_SHARING_VIOLATION = 32;
                const int ERROR_LOCK_VIOLATION = 33;
                
                int errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(ioEx) & 0xFFFF;
                
                if (errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION)
                {
                    error = "File is being used by another process";
                    return true; // File is locked
                }
                
                error = $"IO error accessing file: {ioEx.Message}";
                return true; // Treat as locked to be safe
            }
            catch (UnauthorizedAccessException uaEx)
            {
                error = $"Access denied: {uaEx.Message}";
                return true; // Treat as locked
            }
            catch (Exception ex)
            {
                error = $"Error checking file lock: {ex.Message}";
                return true; // Treat as locked to be safe
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }

        /// <summary>
        /// Wait and retry accessing a locked file with exponential backoff
        /// </summary>
        public bool WaitForFileAccess(string filePath, int maxAttempts, ILogService logService)
        {
            string normalizedPath = NormalizePath(filePath);
            string fileName = Path.GetFileName(filePath);

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                string lockError;
                if (!IsFileLocked(filePath, out lockError))
                {
                    if (attempt > 0)
                    {
                        logService.LogInfo($"File '{fileName}' is now accessible after {attempt + 1} attempts");
                    }
                    return true; // File is accessible
                }

                if (attempt < maxAttempts - 1)
                {
                    // Calculate delay with exponential backoff
                    int delay = Math.Min(RetryDelayMs * (int)Math.Pow(2, attempt), MaxRetryDelayMs);
                    
                    logService.LogWarning($"File '{fileName}' is locked (attempt {attempt + 1}/{maxAttempts}). Retrying in {delay}ms... Error: {lockError}");
                    Thread.Sleep(delay);
                }
                else
                {
                    logService.LogError($"File '{fileName}' remains locked after {maxAttempts} attempts. Will retry in next iteration.");
                }
            }

            return false; // File is still locked after all attempts
        }

        /// <summary>
        /// Track a file that failed due to locking
        /// </summary>
        public void TrackLockedFile(string filePath, string error)
        {
            string normalizedPath = NormalizePath(filePath);

            if (_lockedFiles.ContainsKey(normalizedPath))
            {
                var info = _lockedFiles[normalizedPath];
                info.RetryCount++;
                info.LastRetryTime = DateTime.Now;
                info.LastError = error;
            }
            else
            {
                _lockedFiles[normalizedPath] = new LockedFileInfo(filePath)
                {
                    LastError = error
                };
            }
        }

        /// <summary>
        /// Mark a file as successfully transferred (remove from locked files)
        /// </summary>
        public void MarkFileTransferred(string filePath)
        {
            string normalizedPath = NormalizePath(filePath);
            if (_lockedFiles.ContainsKey(normalizedPath))
            {
                _lockedFiles.Remove(normalizedPath);
            }
        }

        /// <summary>
        /// Check if a file is currently being tracked as locked
        /// </summary>
        public bool IsFileTrackedAsLocked(string filePath)
        {
            string normalizedPath = NormalizePath(filePath);
            return _lockedFiles.ContainsKey(normalizedPath);
        }

        /// <summary>
        /// Get information about a locked file
        /// </summary>
        public LockedFileInfo GetLockedFileInfo(string filePath)
        {
            string normalizedPath = NormalizePath(filePath);
            if (_lockedFiles.ContainsKey(normalizedPath))
            {
                return _lockedFiles[normalizedPath];
            }
            return null;
        }

        /// <summary>
        /// Get all currently tracked locked files
        /// </summary>
        public List<LockedFileInfo> GetAllLockedFiles()
        {
            return _lockedFiles.Values.ToList();
        }

        /// <summary>
        /// Get count of locked files
        /// </summary>
        public int GetLockedFileCount()
        {
            return _lockedFiles.Count;
        }

        /// <summary>
        /// Clear all locked file tracking (e.g., after successful retry)
        /// </summary>
        public void Clear()
        {
            _lockedFiles.Clear();
        }

        /// <summary>
        /// Clean up locked files that were successfully transferred or no longer exist
        /// </summary>
        public void CleanupStaleEntries()
        {
            var staleKeys = _lockedFiles.Keys
                .Where(key => !File.Exists(key))
                .ToList();

            foreach (var key in staleKeys)
            {
                _lockedFiles.Remove(key);
            }
        }

        /// <summary>
        /// Get all file states for serialization
        /// </summary>
        public Dictionary<string, LockedFileInfo> GetAllLockedFileStates()
        {
            return new Dictionary<string, LockedFileInfo>(_lockedFiles);
        }

        /// <summary>
        /// Set all file states from deserialization
        /// </summary>
        public void SetAllLockedFileStates(Dictionary<string, LockedFileInfo> lockedFiles)
        {
            _lockedFiles = new Dictionary<string, LockedFileInfo>(lockedFiles ?? new Dictionary<string, LockedFileInfo>(), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Normalize file path for consistent comparison
        /// </summary>
        private string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            return path.Replace('\\', '/').ToLowerInvariant();
        }

        /// <summary>
        /// Reset retry count for files that should be retried again
        /// </summary>
        public void ResetRetryCountsForNextIteration()
        {
            foreach (var info in _lockedFiles.Values)
            {
                info.RetryCount = 0;
            }
        }

        /// <summary>
        /// Get files that should be prioritized in the next iteration (recently locked)
        /// </summary>
        public List<string> GetFilesToRetryInNextIteration()
        {
            return _lockedFiles.Keys.ToList();
        }
    }
}
