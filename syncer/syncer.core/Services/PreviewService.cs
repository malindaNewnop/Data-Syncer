using System;
using System.Collections.Generic;
using System.IO;

namespace syncer.core
{
    public class PreviewService : IPreviewService
    {
        private readonly IFileEnumerator _fileEnumerator;
        private readonly ITransferClientFactory _transferClientFactory;

        public PreviewService(IFileEnumerator fileEnumerator, ITransferClientFactory transferClientFactory)
        {
            _fileEnumerator = fileEnumerator ?? throw new ArgumentNullException("fileEnumerator");
            _transferClientFactory = transferClientFactory ?? throw new ArgumentNullException("transferClientFactory");
        }

        public PreviewResult GeneratePreview(SyncJob job)
        {
            var result = new PreviewResult
            {
                JobId = job.Id,
                JobName = job.Name,
                FilesToProcess = new List<FilePreviewItem>(),
                TotalFiles = 0,
                TotalSize = 0,
                EstimatedDuration = TimeSpan.Zero,
                HasErrors = false,
                Errors = new List<string>()
            };

            try
            {
                // Get source files based on job configuration
                var sourceFiles = GetSourceFiles(job, result);
                if (result.HasErrors)
                {
                    return result;
                }

                // Analyze each file for transfer preview
                AnalyzeFiles(job, sourceFiles, result);

                // Calculate estimates
                CalculateEstimates(result);

                return result;
            }
            catch (Exception ex)
            {
                result.HasErrors = true;
                result.Errors.Add($"Preview generation failed: {ex.Message}");
                return result;
            }
        }

        public bool ValidateJob(SyncJob job, out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            try
            {
                // Validate job configuration
                if (job == null)
                {
                    validationErrors.Add("Job cannot be null");
                    return false;
                }

                if (StringExtensions.IsNullOrWhiteSpace(job.Name))
                {
                    validationErrors.Add("Job name is required");
                }

                if (StringExtensions.IsNullOrWhiteSpace(job.SourcePath))
                {
                    validationErrors.Add("Source path is required");
                }

                if (StringExtensions.IsNullOrWhiteSpace(job.DestinationPath))
                {
                    validationErrors.Add("Destination path is required");
                }

                // Validate source path exists
                if (!StringExtensions.IsNullOrWhiteSpace(job.SourcePath))
                {
                    if (job.Connection.Protocol == ProtocolType.Local)
                    {
                        if (!Directory.Exists(job.SourcePath) && !File.Exists(job.SourcePath))
                        {
                            validationErrors.Add($"Source path does not exist: {job.SourcePath}");
                        }
                    }
                    else
                    {
                        // Test connection for remote protocols
                        var client = _transferClientFactory.Create(job.Connection.Protocol);
                        string connectionError;
                        if (!client.TestConnection(job.Connection, out connectionError))
                        {
                            validationErrors.Add($"Cannot connect to source: {connectionError}");
                        }
                    }
                }

                // Validate destination accessibility
                if (!StringExtensions.IsNullOrWhiteSpace(job.DestinationPath))
                {
                    if (job.DestinationConnection.Protocol == ProtocolType.Local)
                    {
                        try
                        {
                            var destDir = Path.GetDirectoryName(job.DestinationPath);
                            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                            {
                                // Try to create the directory to test access
                                Directory.CreateDirectory(destDir);
                            }
                        }
                        catch (Exception ex)
                        {
                            validationErrors.Add($"Cannot access destination path: {ex.Message}");
                        }
                    }
                    else
                    {
                        // Test destination connection
                        var client = _transferClientFactory.Create(job.DestinationConnection.Protocol);
                        string connectionError;
                        if (!client.TestConnection(job.DestinationConnection, out connectionError))
                        {
                            validationErrors.Add($"Cannot connect to destination: {connectionError}");
                        }
                    }
                }

                // Validate schedule if enabled
                if (job.IsScheduled && job.Schedule != null)
                {
                    if (StringExtensions.IsNullOrWhiteSpace(job.Schedule.CronExpression))
                    {
                        validationErrors.Add("Cron expression is required for scheduled jobs");
                    }
                    else
                    {
                        // Basic cron validation (you might want to use a cron library for proper validation)
                        var parts = job.Schedule.CronExpression.Split(' ');
                        if (parts.Length < 5 || parts.Length > 6)
                        {
                            validationErrors.Add("Invalid cron expression format");
                        }
                    }
                }

                return validationErrors.Count == 0;
            }
            catch (Exception ex)
            {
                validationErrors.Add($"Validation failed: {ex.Message}");
                return false;
            }
        }

        private List<string> GetSourceFiles(SyncJob job, PreviewResult result)
        {
            var files = new List<string>();

            try
            {
                if (job.Connection.Protocol == ProtocolType.Local)
                {
                    // Local file enumeration
                    if (File.Exists(job.SourcePath))
                    {
                        files.Add(job.SourcePath);
                    }
                    else if (Directory.Exists(job.SourcePath))
                    {
                        files.AddRange(_fileEnumerator.EnumerateFiles(
                            job.SourcePath, 
                            job.Filters, 
                            job.IncludeSubfolders));
                    }
                    else
                    {
                        result.HasErrors = true;
                        result.Errors.Add($"Source path does not exist: {job.SourcePath}");
                    }
                }
                else
                {
                    // Remote file enumeration
                    var client = _transferClientFactory.Create(job.Connection.Protocol);
                    string error;
                    List<string> remoteFiles;
                    
                    if (client.ListFiles(job.Connection, job.SourcePath, out remoteFiles, out error))
                    {
                        // Apply filters to remote files
                        foreach (var file in remoteFiles)
                        {
                            if (job.Filters == null || job.Filters.ShouldIncludeFile(Path.GetFileName(file)))
                            {
                                files.Add(file);
                            }
                        }
                    }
                    else
                    {
                        result.HasErrors = true;
                        result.Errors.Add($"Failed to list remote files: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                result.HasErrors = true;
                result.Errors.Add($"Failed to enumerate source files: {ex.Message}");
            }

            return files;
        }

        private void AnalyzeFiles(SyncJob job, List<string> sourceFiles, PreviewResult result)
        {
            foreach (var sourceFile in sourceFiles)
            {
                try
                {
                    var previewItem = new FilePreviewItem
                    {
                        SourcePath = sourceFile,
                        DestinationPath = DetermineDestinationPath(job, sourceFile),
                        Action = FileAction.Copy,
                        Size = 0,
                        LastModified = DateTime.MinValue,
                        Status = PreviewStatus.Pending,
                        Message = "Ready for transfer"
                    };

                    // Get file info
                    if (job.Connection.Protocol == ProtocolType.Local && File.Exists(sourceFile))
                    {
                        var fileInfo = new FileInfo(sourceFile);
                        previewItem.Size = fileInfo.Length;
                        previewItem.LastModified = fileInfo.LastWriteTime;
                    }

                    // Check if destination file exists and determine action
                    bool destExists = false;
                    if (job.DestinationConnection.Protocol == ProtocolType.Local)
                    {
                        destExists = File.Exists(previewItem.DestinationPath);
                    }
                    else
                    {
                        var client = _transferClientFactory.Create(job.DestinationConnection.Protocol);
                        string error;
                        client.FileExists(job.DestinationConnection, previewItem.DestinationPath, out destExists, out error);
                    }

                    if (destExists)
                    {
                        if (job.OverwriteExisting)
                        {
                            previewItem.Action = FileAction.Overwrite;
                            previewItem.Message = "Will overwrite existing file";
                        }
                        else
                        {
                            previewItem.Action = FileAction.Skip;
                            previewItem.Status = PreviewStatus.Skipped;
                            previewItem.Message = "File exists and overwrite is disabled";
                        }
                    }

                    result.FilesToProcess.Add(previewItem);
                    result.TotalFiles++;
                    result.TotalSize += previewItem.Size;
                }
                catch (Exception ex)
                {
                    result.HasErrors = true;
                    result.Errors.Add($"Error analyzing file '{sourceFile}': {ex.Message}");
                }
            }
        }

        private void CalculateEstimates(PreviewResult result)
        {
            // Simple estimation based on file sizes
            // In a real implementation, you might consider:
            // - Network speed
            // - File type (compression efficiency)
            // - Historical transfer data
            
            const long averageBytesPerSecond = 1024 * 1024; // 1 MB/s estimate
            
            if (result.TotalSize > 0)
            {
                var estimatedSeconds = result.TotalSize / averageBytesPerSecond;
                result.EstimatedDuration = TimeSpan.FromSeconds(estimatedSeconds);
            }
        }

        private string DetermineDestinationPath(SyncJob job, string sourceFile)
        {
            try
            {
                // Calculate relative path from source base
                var relativePath = GetRelativePath(job.SourcePath, sourceFile);
                
                // Combine with destination base
                if (job.DestinationConnection.Protocol == ProtocolType.Local)
                {
                    return Path.Combine(job.DestinationPath, relativePath);
                }
                else
                {
                    // For remote paths, use forward slashes
                    var destPath = job.DestinationPath.Replace('\\', '/');
                    if (!destPath.EndsWith("/"))
                        destPath += "/";
                    return destPath + relativePath.Replace('\\', '/');
                }
            }
            catch
            {
                // Fallback: use filename only
                return Path.Combine(job.DestinationPath, Path.GetFileName(sourceFile));
            }
        }

        private string GetRelativePath(string basePath, string fullPath)
        {
            if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(fullPath))
                return Path.GetFileName(fullPath);

            try
            {
                var baseUri = new Uri(basePath.EndsWith("\\") ? basePath : basePath + "\\");
                var fullUri = new Uri(fullPath);
                
                if (baseUri.Scheme != fullUri.Scheme)
                    return Path.GetFileName(fullPath);

                var relativeUri = baseUri.MakeRelativeUri(fullUri);
                return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', '\\');
            }
            catch
            {
                return Path.GetFileName(fullPath);
            }
        }
    }

    // Extension method for FilterSettings
    public static class FilterSettingsExtensions
    {
        public static bool ShouldIncludeFile(this FilterSettings filters, string fileName)
        {
            if (filters == null)
                return true;

            // Check include patterns
            if (!string.IsNullOrEmpty(filters.IncludePattern))
            {
                if (!MatchesPattern(fileName, filters.IncludePattern))
                    return false;
            }

            // Check exclude patterns
            if (!string.IsNullOrEmpty(filters.ExcludePattern))
            {
                if (MatchesPattern(fileName, filters.ExcludePattern))
                    return false;
            }

            return true;
        }

        private static bool MatchesPattern(string fileName, string pattern)
        {
            // Simple wildcard matching
            // In production, you might want to use Regex or more sophisticated pattern matching
            if (string.IsNullOrEmpty(pattern))
                return true;

            if (pattern == "*")
                return true;

            if (pattern.Contains("*"))
            {
                var parts = pattern.Split('*');
                if (parts.Length == 2)
                {
                    return fileName.StartsWith(parts[0]) && fileName.EndsWith(parts[1]);
                }
            }

            return fileName.Equals(pattern, StringComparison.OrdinalIgnoreCase);
        }
    }
}
