using System;
using System.Collections.Generic;
using System.IO;

namespace syncer.core
{
    public class FileEnumerator : IFileEnumerator
    {
        public List<string> EnumerateFiles(string rootPath, FilterSettings filters, bool includeSubfolders)
        {
            var results = new List<string>();
            
            try
            {
                if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
                {
                    return results;
                }

                // If the filter settings have include or exclude patterns, use the pattern-based enumeration
                if (filters != null && (!string.IsNullOrEmpty(filters.IncludePattern) || !string.IsNullOrEmpty(filters.ExcludePattern)))
                {
                    return EnumerateFilesWithPattern(
                        rootPath, 
                        filters.IncludePattern, 
                        filters.ExcludePattern, 
                        includeSubfolders);
                }

                var searchOption = includeSubfolders 
                    ? SearchOption.AllDirectories 
                    : SearchOption.TopDirectoryOnly;

                string[] files = Directory.GetFiles(rootPath, "*", searchOption);

                foreach (string filePath in files)
                {
                    try
                    {
                        if (ShouldIncludeFile(filePath, filters))
                        {
                            results.Add(filePath);
                        }
                    }
                    catch (Exception)
                    {
                        // Log individual file errors but continue processing
                    }
                }
            }
            catch (Exception)
            {
                // Return what we have so far
            }

            return results;
        }

        public List<string> EnumerateDirectories(string rootPath, FilterSettings filters, bool includeSubfolders)
        {
            var results = new List<string>();
            
            try
            {
                if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
                {
                    return results;
                }

                var searchOption = includeSubfolders 
                    ? SearchOption.AllDirectories 
                    : SearchOption.TopDirectoryOnly;

                string[] directories = Directory.GetDirectories(rootPath, "*", searchOption);

                foreach (string dirPath in directories)
                {
                    try
                    {
                        if (ShouldIncludeDirectory(dirPath, filters))
                        {
                            results.Add(dirPath);
                        }
                    }
                    catch (Exception)
                    {
                        // Log individual directory errors but continue processing
                    }
                }
            }
            catch (Exception)
            {
                // Return what we have so far
            }

            return results;
        }

        /// <summary>
        /// Enumerates files with more robust pattern matching support
        /// </summary>
        /// <param name="rootPath">Root directory to start search from</param>
        /// <param name="includePattern">Pattern to include files (semicolon-separated)</param>
        /// <param name="excludePattern">Pattern to exclude files (semicolon-separated)</param>
        /// <param name="includeSubfolders">Whether to include subdirectories in search</param>
        /// <returns>List of file paths matching the criteria</returns>
        public List<string> EnumerateFilesWithPattern(string rootPath, string includePattern, string excludePattern, bool includeSubfolders)
        {
            List<string> files = new List<string>();
            
            try
            {
                if (File.Exists(rootPath))
                {
                    // It's a single file - check if it matches our patterns
                    if (ShouldIncludeFileByPattern(Path.GetFileName(rootPath), includePattern, excludePattern))
                        files.Add(rootPath);
                }
                else if (Directory.Exists(rootPath))
                {
                    // Process all files in current directory
                    foreach (string file in Directory.GetFiles(rootPath))
                    {
                        if (ShouldIncludeFileByPattern(Path.GetFileName(file), includePattern, excludePattern))
                            files.Add(file);
                    }
                    
                    // Recursively process subdirectories if requested
                    if (includeSubfolders)
                    {
                        foreach (string subDir in Directory.GetDirectories(rootPath))
                        {
                            files.AddRange(EnumerateFilesWithPattern(subDir, includePattern, excludePattern, true));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with partial results
                Console.WriteLine($"Error enumerating files in {rootPath}: {ex.Message}");
            }
            
            return files;
        }

        /// <summary>
        /// Determines if a file should be included based on pattern matching
        /// </summary>
        private bool ShouldIncludeFileByPattern(string fileName, string includePattern, string excludePattern)
        {
            // First check exclude pattern - if file matches an exclude pattern, skip it
            if (!string.IsNullOrEmpty(excludePattern))
            {
                foreach (string pattern in excludePattern.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (IsWildcardMatch(fileName, pattern.Trim()))
                        return false;
                }
            }
            
            // If include pattern is empty or null, include all files that weren't excluded
            if (string.IsNullOrEmpty(includePattern))
                return true;
                
            // Check if file matches any include pattern
            foreach (string pattern in includePattern.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (IsWildcardMatch(fileName, pattern.Trim()))
                    return true;
            }
            
            return false;
        }

        /// <summary>
        /// Improved wildcard matching for file patterns
        /// </summary>
        private bool IsWildcardMatch(string text, string pattern)
        {
            // Simple wildcard matching for *, ? style patterns
            if (pattern == "*") return true;
            
            if (pattern.StartsWith("*") && pattern.EndsWith("*") && pattern.Length > 2)
            {
                string middle = pattern.Substring(1, pattern.Length - 2);
                return text.IndexOf(middle, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            else if (pattern.StartsWith("*") && pattern.Length > 1)
            {
                string end = pattern.Substring(1);
                return text.EndsWith(end, StringComparison.OrdinalIgnoreCase);
            }
            else if (pattern.EndsWith("*") && pattern.Length > 1)
            {
                string start = pattern.Substring(0, pattern.Length - 1);
                return text.StartsWith(start, StringComparison.OrdinalIgnoreCase);
            }
            else if (pattern.Contains("?"))
            {
                // Process ? wildcards (single character)
                return MatchWithQuestionMark(text, pattern);
            }
            else
            {
                return string.Equals(text, pattern, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Handles pattern matching with question mark wildcards
        /// </summary>
        private bool MatchWithQuestionMark(string text, string pattern)
        {
            if (text.Length != pattern.Length)
                return false;
                
            for (int i = 0; i < text.Length; i++)
            {
                if (pattern[i] != '?' && !char.Equals(char.ToUpperInvariant(text[i]), char.ToUpperInvariant(pattern[i])))
                    return false;
            }
            
            return true;
        }

        private bool ShouldIncludeFile(string filePath, FilterSettings filters)
        {
            if (filters == null) return true;

            try
            {
                var fileInfo = new FileInfo(filePath);

                // Check file attributes
                if (!filters.IncludeHidden && HasAttribute(fileInfo, FileAttributes.Hidden))
                    return false;
                
                if (!filters.IncludeSystem && HasAttribute(fileInfo, FileAttributes.System))
                    return false;
                
                if (!filters.IncludeReadOnly && HasAttribute(fileInfo, FileAttributes.ReadOnly))
                    return false;

                // Check file size
                if (filters.MinSizeBytes >= 0 && fileInfo.Length < filters.MinSizeBytes)
                    return false;
                
                if (filters.MaxSizeBytes >= 0 && fileInfo.Length > filters.MaxSizeBytes)
                    return false;

                // Check modification date
                if (filters.ModifiedAfter.HasValue && fileInfo.LastWriteTime < filters.ModifiedAfter.Value)
                    return false;
                
                if (filters.ModifiedBefore.HasValue && fileInfo.LastWriteTime > filters.ModifiedBefore.Value)
                    return false;

                // Check file extensions
                if (filters.FileExtensions != null && filters.FileExtensions.Count > 0)
                {
                    string extension = fileInfo.Extension;
                    bool matchesExtension = false;
                    
                    foreach (string allowedExt in filters.FileExtensions)
                    {
                        if (string.Equals(extension, allowedExt, StringComparison.OrdinalIgnoreCase))
                        {
                            matchesExtension = true;
                            break;
                        }
                    }
                    
                    if (!matchesExtension) return false;
                }

                // Check exclude patterns
                if (filters.ExcludePatterns != null && filters.ExcludePatterns.Count > 0)
                {
                    string fileName = fileInfo.Name;
                    string fullPath = fileInfo.FullName;
                    
                    foreach (string pattern in filters.ExcludePatterns)
                    {
                        if (string.IsNullOrEmpty(pattern)) continue;
                        
                        if (MatchesPattern(fileName, pattern) || MatchesPattern(fullPath, pattern))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false; // Exclude files that can't be analyzed
            }
        }

        private bool ShouldIncludeDirectory(string dirPath, FilterSettings filters)
        {
            if (filters == null) return true;

            try
            {
                var dirInfo = new DirectoryInfo(dirPath);

                // Check directory attributes
                if (!filters.IncludeHidden && HasAttribute(dirInfo, FileAttributes.Hidden))
                    return false;
                
                if (!filters.IncludeSystem && HasAttribute(dirInfo, FileAttributes.System))
                    return false;

                // Check exclude patterns
                if (filters.ExcludePatterns != null && filters.ExcludePatterns.Count > 0)
                {
                    string dirName = dirInfo.Name;
                    string fullPath = dirInfo.FullName;
                    
                    foreach (string pattern in filters.ExcludePatterns)
                    {
                        if (string.IsNullOrEmpty(pattern)) continue;
                        
                        if (MatchesPattern(dirName, pattern) || MatchesPattern(fullPath, pattern))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false; // Exclude directories that can't be analyzed
            }
        }

        private static bool HasAttribute(FileSystemInfo info, FileAttributes attribute)
        {
            return (info.Attributes & attribute) == attribute;
        }

        private static bool MatchesPattern(string text, string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return false;
            
            // Simple pattern matching - supports * wildcard
            pattern = pattern.Replace("*", ".*");
            
            try
            {
                return System.Text.RegularExpressions.Regex.IsMatch(text, pattern, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            catch
            {
                // If regex fails, use simple contains check
                return text.IndexOf(pattern.Replace(".*", ""), StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }
    }
}
