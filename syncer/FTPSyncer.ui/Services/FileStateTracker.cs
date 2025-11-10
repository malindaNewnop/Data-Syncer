using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace FTPSyncer.ui.Services
{
    /// <summary>
    /// Tracks file states (modification time and size) to detect changes between transfer iterations
    /// </summary>
    [Serializable]
    [XmlRoot("FileState")]
    public class FileState
    {
        [XmlElement("FilePath")]
        public string FilePath { get; set; }
        
        [XmlElement("LastModifiedTime")]
        public DateTime LastModifiedTime { get; set; }
        
        [XmlElement("FileSize")]
        public long FileSize { get; set; }
        
        [XmlElement("LastTransferTime")]
        public DateTime LastTransferTime { get; set; }

        public FileState()
        {
        }

        public FileState(string filePath, DateTime lastModifiedTime, long fileSize)
        {
            FilePath = filePath;
            LastModifiedTime = lastModifiedTime;
            FileSize = fileSize;
            LastTransferTime = DateTime.Now;
        }

        /// <summary>
        /// Check if the file has been modified since last transfer
        /// </summary>
        public bool HasChanged(DateTime currentModifiedTime, long currentSize)
        {
            // File has changed if either the modification time or size is different
            return currentModifiedTime != LastModifiedTime || currentSize != FileSize;
        }
    }

    /// <summary>
    /// Manages file state tracking for timer jobs to enable incremental transfers
    /// </summary>
    [Serializable]
    public class FileStateTracker
    {
        private Dictionary<string, FileState> _fileStates;

        public FileStateTracker()
        {
            _fileStates = new Dictionary<string, FileState>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if a file has been modified since last transfer or is new
        /// </summary>
        public bool ShouldTransferFile(string filePath, DateTime currentModifiedTime, long currentSize)
        {
            // Normalize the file path for consistent comparison
            string normalizedPath = NormalizePath(filePath);

            // If file is not tracked, it's new and should be transferred
            if (!_fileStates.ContainsKey(normalizedPath))
            {
                return true;
            }

            // Check if the file has changed
            FileState existingState = _fileStates[normalizedPath];
            return existingState.HasChanged(currentModifiedTime, currentSize);
        }

        /// <summary>
        /// Update file state after a successful transfer
        /// </summary>
        public void UpdateFileState(string filePath, DateTime modifiedTime, long fileSize)
        {
            string normalizedPath = NormalizePath(filePath);
            
            if (_fileStates.ContainsKey(normalizedPath))
            {
                // Update existing state
                _fileStates[normalizedPath].LastModifiedTime = modifiedTime;
                _fileStates[normalizedPath].FileSize = fileSize;
                _fileStates[normalizedPath].LastTransferTime = DateTime.Now;
            }
            else
            {
                // Add new state
                _fileStates[normalizedPath] = new FileState(normalizedPath, modifiedTime, fileSize);
            }
        }

        /// <summary>
        /// Remove file state (e.g., when file is deleted)
        /// </summary>
        public void RemoveFileState(string filePath)
        {
            string normalizedPath = NormalizePath(filePath);
            if (_fileStates.ContainsKey(normalizedPath))
            {
                _fileStates.Remove(normalizedPath);
            }
        }

        /// <summary>
        /// Clear all file states (reset tracking)
        /// </summary>
        public void Clear()
        {
            _fileStates.Clear();
        }

        /// <summary>
        /// Get count of tracked files
        /// </summary>
        public int GetTrackedFileCount()
        {
            return _fileStates.Count;
        }

        /// <summary>
        /// Get all tracked file paths
        /// </summary>
        public List<string> GetTrackedFiles()
        {
            return _fileStates.Keys.ToList();
        }

        /// <summary>
        /// Get file state for a specific file
        /// </summary>
        public FileState GetFileState(string filePath)
        {
            string normalizedPath = NormalizePath(filePath);
            if (_fileStates.ContainsKey(normalizedPath))
            {
                return _fileStates[normalizedPath];
            }
            return null;
        }

        /// <summary>
        /// Get all file states (for serialization)
        /// </summary>
        public Dictionary<string, FileState> GetAllFileStates()
        {
            return new Dictionary<string, FileState>(_fileStates);
        }

        /// <summary>
        /// Set all file states (for deserialization)
        /// </summary>
        public void SetAllFileStates(Dictionary<string, FileState> fileStates)
        {
            _fileStates = new Dictionary<string, FileState>(fileStates, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Normalize file path for consistent comparison (handle case sensitivity and path separators)
        /// </summary>
        private string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            // Replace all backslashes with forward slashes and convert to lowercase for consistent comparison
            return path.Replace('\\', '/').ToLowerInvariant();
        }

        /// <summary>
        /// Clean up stale entries for files that no longer exist
        /// </summary>
        public void CleanupStaleEntries(List<string> currentFiles)
        {
            var normalizedCurrentFiles = currentFiles.Select(f => NormalizePath(f)).ToList();
            var staleKeys = _fileStates.Keys.Where(key => !normalizedCurrentFiles.Contains(key)).ToList();
            
            foreach (var staleKey in staleKeys)
            {
                _fileStates.Remove(staleKey);
            }
        }
    }
}





