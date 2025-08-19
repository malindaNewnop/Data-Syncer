using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace syncer.core.Services
{
    /// <summary>
    /// Provides advanced file filtering capabilities including extension filtering,
    /// regex pattern matching, and recursive directory traversal with validation
    /// </summary>
    public class FileFilterService : IFileFilterService
    {
        private readonly ILogService _logService;

        public FileFilterService(ILogService logService)
        {
            _logService = logService ?? throw new ArgumentNullException("logService");
        }

        /// <summary>
        /// Apply filters to a collection of file paths
        /// </summary>
        public List<string> ApplyFilters(List<string> files, syncer.core.FilterSettings filters)
        {
            if (files == null || files.Count == 0 || filters == null)
                return files ?? new List<string>();

            var result = new List<string>(files);

            // Apply include/exclude extensions
            if (filters.IncludeExtensions != null && filters.IncludeExtensions.Count > 0)
            {
                result = result.Where(f => filters.IncludeExtensions.Contains(
                    Path.GetExtension(f).TrimStart('.').ToLowerInvariant())).ToList();
            }

            if (filters.ExcludeExtensions != null && filters.ExcludeExtensions.Count > 0)
            {
                result = result.Where(f => !filters.ExcludeExtensions.Contains(
                    Path.GetExtension(f).TrimStart('.').ToLowerInvariant())).ToList();
            }

            // Apply minimum/maximum size filters
            if (filters.MinSizeKB > 0 || filters.MaxSizeKB > 0)
            {
                result = result.Where(f => 
                {
                    try
                    {
                        var fileInfo = new FileInfo(f);
                        var sizeKB = fileInfo.Length / 1024;
                        
                        bool passesMin = filters.MinSizeKB <= 0 || sizeKB >= filters.MinSizeKB;
                        bool passesMax = filters.MaxSizeKB <= 0 || sizeKB <= filters.MaxSizeKB;
                        
                        return passesMin && passesMax;
                    }
                    catch
                    {
                        return true; // Include files we can't check (like remote files)
                    }
                }).ToList();
            }

            // Apply date filters
            if (filters.ModifiedAfter != DateTime.MinValue || filters.ModifiedBefore != DateTime.MinValue)
            {
                result = result.Where(f => 
                {
                    try
                    {
                        var fileInfo = new FileInfo(f);
                        
                        bool passesAfter = filters.ModifiedAfter == DateTime.MinValue || 
                                          fileInfo.LastWriteTime >= filters.ModifiedAfter;
                        
                        bool passesBefore = filters.ModifiedBefore == DateTime.MinValue || 
                                           fileInfo.LastWriteTime <= filters.ModifiedBefore;
                        
                        return passesAfter && passesBefore;
                    }
                    catch
                    {
                        return true; // Include files we can't check
                    }
                }).ToList();
            }

            // Apply regex patterns
            if (!string.IsNullOrEmpty(filters.IncludePattern))
            {
                try
                {
                    Regex includeRegex = new Regex(filters.IncludePattern, RegexOptions.IgnoreCase);
                    result = result.Where(f => includeRegex.IsMatch(Path.GetFileName(f))).ToList();
                }
                catch (Exception ex)
                {
                    _logService.LogError($"Invalid include pattern: {ex.Message}");
                }
            }

            if (!string.IsNullOrEmpty(filters.ExcludePattern))
            {
                try
                {
                    Regex excludeRegex = new Regex(filters.ExcludePattern, RegexOptions.IgnoreCase);
                    result = result.Where(f => !excludeRegex.IsMatch(Path.GetFileName(f))).ToList();
                }
                catch (Exception ex)
                {
                    _logService.LogError($"Invalid exclude pattern: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// Handles validation of files after transfer (e.g., checking file integrity)
        /// </summary>
        public bool ValidateFile(string sourcePath, string destinationPath, syncer.core.ValidationOptions options)
        {
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destinationPath))
                return false;

            try
            {
                if (options.HasFlag(ValidationOptions.Existence))
                {
                    if (!File.Exists(destinationPath))
                        return false;
                }

                if (options.HasFlag(ValidationOptions.FileSize))
                {
                    var sourceInfo = new FileInfo(sourcePath);
                    var destInfo = new FileInfo(destinationPath);
                    
                    if (sourceInfo.Length != destInfo.Length)
                        return false;
                }

                if (options.HasFlag(ValidationOptions.Timestamp) && options.HasFlag(ValidationOptions.PreserveTimestamp))
                {
                    var sourceInfo = new FileInfo(sourcePath);
                    var destInfo = new FileInfo(destinationPath);
                    
                    // Allow small difference (1 second) due to filesystem precision differences
                    TimeSpan difference = sourceInfo.LastWriteTime - destInfo.LastWriteTime;
                    if (Math.Abs(difference.TotalSeconds) > 1)
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError($"File validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ensure directories exist in the path, creating them as needed
        /// </summary>
        public bool EnsureDirectoryExists(string path, bool isLocal)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            if (isLocal)
            {
                try
                {
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    return true;
                }
                catch (Exception ex)
                {
                    _logService.LogError($"Failed to create directory {path}: {ex.Message}");
                    return false;
                }
            }
            
            // For remote paths, the directory creation is handled by the transfer client
            return true;
        }

        /// <summary>
        /// Relocates files after a successful transfer if relocation options are specified
        /// </summary>
        public bool RelocateFile(string sourceFilePath, syncer.core.RelocationOptions options)
        {
            if (string.IsNullOrEmpty(sourceFilePath) || !File.Exists(sourceFilePath))
                return false;

            try
            {
                if (options == RelocationOptions.None)
                    return true;

                string targetFolder = null;
                string fileName = Path.GetFileName(sourceFilePath);
                
                if (options == syncer.core.RelocationOptions.Delete)
                {
                    File.Delete(sourceFilePath);
                    return true;
                }
                else if (options == syncer.core.RelocationOptions.Archive)
                {
                    targetFolder = Path.Combine(Path.GetDirectoryName(sourceFilePath), "Archive");
                }
                else if (options == syncer.core.RelocationOptions.CustomFolder && !string.IsNullOrEmpty(Path.GetDirectoryName(sourceFilePath)))
                {
                    // For custom folder option we'll need the custom path from the filter settings
                    // which is not available in this context - we'll use the parent directory for now
                    targetFolder = Path.GetDirectoryName(sourceFilePath);
                }

                if (targetFolder != null)
                {
                    if (!Directory.Exists(targetFolder))
                        Directory.CreateDirectory(targetFolder);

                    string targetPath = Path.Combine(targetFolder, fileName);
                    
                    // Handle file already exists in target
                    if (File.Exists(targetPath))
                    {
                        string newName = Path.GetFileNameWithoutExtension(fileName) + "_" + 
                                        DateTime.Now.ToString("yyyyMMdd_HHmmss") + 
                                        Path.GetExtension(fileName);
                        targetPath = Path.Combine(targetFolder, newName);
                    }
                    
                    File.Move(sourceFilePath, targetPath);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Failed to relocate file {sourceFilePath}: {ex.Message}");
                return false;
            }
        }
    }

    [Flags]
    public enum ValidationOptions
    {
        None = 0,
        Existence = 1,
        FileSize = 2,
        Timestamp = 4,
        PreserveTimestamp = 8,
        All = Existence | FileSize | Timestamp
    }

    public enum RelocationOptions
    {
        None,
        Delete,
        Archive,
        CustomFolder
    }
}
