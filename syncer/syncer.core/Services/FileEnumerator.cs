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
