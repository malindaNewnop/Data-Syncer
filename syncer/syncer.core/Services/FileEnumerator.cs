using System;
using System.Collections.Generic;
using System.IO;

namespace syncer.core
{
    public class FileEnumerator : IFileEnumerator
    {
        public List<string> EnumerateFiles(string rootPath, bool includeSubfolders)
        {
            return EnumerateFiles(rootPath, null, includeSubfolders);
        }

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
                Console.WriteLine("FileEnumerator: Found " + files.Length + " total files before filtering");

                // Convert to list for easier processing
                var allFiles = new List<string>(files);

                // Apply filters if provided
                if (filters != null)
                {
                    results = ApplyFileFilters(allFiles, filters);
                    Console.WriteLine("FileEnumerator: Applied filters, {0} files remain", results.Count);
                }
                else
                {
                    // No filters, include all files
                    foreach (string filePath in files)
                    {
                        try
                        {
                            results.Add(filePath);
                        }
                        catch (Exception)
                        {
                            // Log individual file errors but continue processing
                        }
                    }
                }
                
                Console.WriteLine("FileEnumerator: Included " + results.Count + " files after filtering");
            }
            catch (Exception)
            {
                // Return what we have so far
            }

            return results;
        }

        public List<string> EnumerateDirectories(string rootPath, bool includeSubfolders)
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
                        results.Add(dirPath);
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

        private List<string> ApplyFileFilters(List<string> files, FilterSettings filters)
        {
            if (files == null || files.Count == 0 || filters == null)
                return files ?? new List<string>();

            var result = new List<string>();

            // Apply include/exclude extensions (Include takes precedence)
            if (filters.IncludeExtensions != null && filters.IncludeExtensions.Count > 0)
            {
                // Convert include extensions to lowercase and normalize
                var normalizedIncludes = new List<string>();
                foreach (var ext in filters.IncludeExtensions)
                {
                    if (!string.IsNullOrEmpty(ext))
                    {
                        normalizedIncludes.Add(ext.TrimStart('.').ToLowerInvariant());
                    }
                }

                // Filter files to include only specified extensions
                foreach (string file in files)
                {
                    var fileExt = Path.GetExtension(file);
                    if (!string.IsNullOrEmpty(fileExt) && normalizedIncludes.Contains(fileExt.TrimStart('.').ToLowerInvariant()))
                    {
                        result.Add(file);
                    }
                }
                
                Console.WriteLine("Applied include filter: {0} files match included extensions [{1}]", 
                    result.Count, string.Join(", ", normalizedIncludes.ToArray()));
            }
            // If no include filter, apply exclude filter
            else if (filters.ExcludeExtensions != null && filters.ExcludeExtensions.Count > 0)
            {
                // Convert exclude extensions to lowercase and normalize
                var normalizedExcludes = new List<string>();
                foreach (var ext in filters.ExcludeExtensions)
                {
                    if (!string.IsNullOrEmpty(ext))
                    {
                        normalizedExcludes.Add(ext.TrimStart('.').ToLowerInvariant());
                    }
                }

                // Filter files to exclude specified extensions
                foreach (string file in files)
                {
                    var fileExt = Path.GetExtension(file);
                    if (string.IsNullOrEmpty(fileExt) || !normalizedExcludes.Contains(fileExt.TrimStart('.').ToLowerInvariant()))
                    {
                        result.Add(file);
                    }
                }
                
                Console.WriteLine("Applied exclude filter: {0} files remain after excluding extensions [{1}]", 
                    result.Count, string.Join(", ", normalizedExcludes.ToArray()));
            }
            else
            {
                // No filtering, return all files
                result.AddRange(files);
            }

            return result;
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
