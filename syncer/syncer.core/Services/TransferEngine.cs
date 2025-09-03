using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using syncer.core;
using syncer.core.Services;
using syncer.core.Transfers;

namespace syncer.core.Services
{
    /// <summary>
    /// Comprehensive Transfer Engine with job management, retry logic, and progress tracking
    /// .NET 3.5 Compatible
    /// </summary>
    public class TransferEngine : IDisposable
    {
        private readonly ITransferClientFactory _clientFactory;
        private readonly ILogService _logService;
        private readonly EnhancedSettingsService _settingsService;
        private readonly IFileFilterService _fileFilterService;
        private readonly Dictionary<string, TransferJob> _activeJobs;
        private readonly object _jobLock = new object();
        private volatile bool _disposed = false;

        public event EventHandler<TransferProgressEventArgs> TransferProgress;
        public event EventHandler<TransferCompletedEventArgs> TransferCompleted;
        public event EventHandler<TransferStartedEventArgs> TransferStarted;

        public TransferEngine(ITransferClientFactory clientFactory, ILogService logService, 
                         EnhancedSettingsService settingsService, IFileFilterService fileFilterService)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException("clientFactory");
            _logService = logService ?? throw new ArgumentNullException("logService");
            _settingsService = settingsService ?? throw new ArgumentNullException("settingsService");
            _fileFilterService = fileFilterService ?? throw new ArgumentNullException("fileFilterService");
            _activeJobs = new Dictionary<string, TransferJob>();
        }

        /// <summary>
        /// Execute a sync job with comprehensive error handling and retry logic
        /// </summary>
        public TransferResultEnhanced ExecuteJob(SyncJob job)
        {
            if (job == null)
                throw new ArgumentNullException("job");

            var result = new TransferResultEnhanced
            {
                JobId = job.Id.ToString(),
                JobName = job.Name,
                StartTime = DateTime.Now,
                SourcePath = job.SourcePath,
                DestinationPath = job.DestinationPath,
                TransferMode = job.TransferMode.ToString(),
                SourceProtocol = job.SourceConnection.Protocol,
                DestinationProtocol = job.DestinationConnection.Protocol
            };

            var transferJob = new TransferJob
            {
                JobId = job.Id.ToString(),
                Job = job,
                Result = result,
                CancellationToken = new CancellationToken(),
                StartTime = DateTime.Now
            };

            lock (_jobLock)
            {
                if (_activeJobs.ContainsKey(transferJob.JobId))
                {
                    result.Success = false;
                    result.LastError = "Job is already running";
                    return result;
                }
                _activeJobs[transferJob.JobId] = transferJob;
            }

            try
            {
                _logService.LogJobStart(job);
                OnTransferStarted(new TransferStartedEventArgs { Job = job, Result = result });

                // Execute the transfer
                ExecuteTransfer(transferJob);

                result.EndTime = DateTime.Now;
                result.Duration = result.EndTime - result.StartTime;

                // Calculate final statistics
                if (result.Duration.TotalSeconds > 0)
                {
                    result.AverageSpeedBytesPerSecond = result.TransferredBytes / result.Duration.TotalSeconds;
                }

                if (result.Errors.Count == 0 && result.FailedFiles == 0)
                {
                    result.Status = TransferStatus.Completed;
                    result.Success = true;
                    _logService.LogJobSuccess(job, $"Job completed successfully. Transferred {result.SuccessfulFiles} files ({result.GetFormattedDataSize()}).");
                }
                else if (result.SuccessfulFiles > 0)
                {
                    result.Status = TransferStatus.CompletedWithErrors;
                    result.Success = true; // Partial success
                    _logService.LogJobProgress(job, $"Job completed with errors. Transferred {result.SuccessfulFiles} files, {result.FailedFiles} failed.");
                }
                else
                {
                    result.Status = TransferStatus.Failed;
                    result.Success = false;
                    _logService.LogJobError(job, "Job failed - no files were transferred successfully.", null);
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Status = TransferStatus.Failed;
                result.Success = false;
                result.LastError = ex.Message;
                result.Errors.Add(ex.Message);
                
                _logService.LogJobError(job, $"Job execution failed: {ex.Message}", ex);
                return result;
            }
            finally
            {
                lock (_jobLock)
                {
                    _activeJobs.Remove(transferJob.JobId);
                }

                OnTransferCompleted(new TransferCompletedEventArgs { Job = job, Result = result });
            }
        }

        /// <summary>
        /// Execute the actual file transfer
        /// </summary>
        private void ExecuteTransfer(TransferJob transferJob)
        {
            var job = transferJob.Job;
            var result = transferJob.Result;

            // Get source files
            var sourceFiles = GetSourceFiles(job);
            result.TotalFiles = sourceFiles.Count;

            if (sourceFiles.Count == 0)
            {
                result.Warnings.Add("No source files found to transfer");
                return;
            }

            // Create transfer clients
            var sourceClient = _clientFactory.Create(job.SourceConnection.Protocol);
            var destClient = _clientFactory.Create(job.DestinationConnection.Protocol);

            // Set up progress callbacks
            var totalFiles = sourceFiles.Count;
            var currentFileIndex = 0;

            sourceClient.SetProgressCallback((progress) =>
            {
                var overallProgress = ((currentFileIndex * 100) + progress) / totalFiles;
                OnTransferProgress(new TransferProgressEventArgs
                {
                    Job = job,
                    Result = result,
                    ProgressPercent = Math.Min(overallProgress, 100),
                    CurrentFile = result.TransferredFiles.Count > 0 ? result.TransferredFiles[result.TransferredFiles.Count - 1] : "",
                    FileProgress = progress
                });
            });

            // Process each file
            foreach (var sourceFile in sourceFiles)
            {
                if (transferJob.CancellationToken.IsCancellationRequested)
                {
                    result.Status = TransferStatus.Cancelled;
                    result.LastError = "Transfer was cancelled";
                    break;
                }

                try
                {
                    var success = TransferSingleFile(job, sourceFile, sourceClient, destClient, result);
                    
                    if (success)
                    {
                        result.SuccessfulFiles++;
                        result.TransferredFiles.Add(sourceFile);
                    }
                    else
                    {
                        result.FailedFiles++;
                        result.FailedFilesList.Add(sourceFile);
                    }
                    
                    result.ProcessedFiles++;
                    currentFileIndex++;

                    // Update progress
                    var overallProgress = (currentFileIndex * 100) / totalFiles;
                    OnTransferProgress(new TransferProgressEventArgs
                    {
                        Job = job,
                        Result = result,
                        ProgressPercent = overallProgress,
                        CurrentFile = sourceFile,
                        FileProgress = 100
                    });
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing {sourceFile}: {ex.Message}");
                    result.FailedFiles++;
                    result.FailedFilesList.Add(sourceFile);
                    result.ProcessedFiles++;
                    currentFileIndex++;
                    
                    _logService.LogJobError(job, $"Failed to transfer file {sourceFile}", ex);
                }
            }
        }

        /// <summary>
        /// Transfer a single file between source and destination
        /// </summary>
        private bool TransferSingleFile(SyncJob job, string sourceFile, ITransferClient sourceClient, ITransferClient destClient, TransferResultEnhanced result)
        {
            try
            {
                // Validate destination path
                if (string.IsNullOrEmpty(job.DestinationPath))
                {
                    result.Errors.Add($"Destination path is empty or null for file {sourceFile}");
                    return false;
                }

                string destinationFile;
                string fileName;

                // Handle path construction for local source only
                if (job.SourceConnection.Protocol == ProtocolType.Local)
                {
                    // Local source: standard path handling
                    fileName = Path.GetFileName(sourceFile);
                    
                    if (job.IncludeSubFolders && sourceFile.StartsWith(job.SourcePath))
                    {
                        // Preserve directory structure
                        var relativePath = sourceFile.Substring(job.SourcePath.Length).TrimStart('\\', '/');
                        destinationFile = Path.Combine(job.DestinationPath, relativePath);
                    }
                    else
                    {
                        destinationFile = Path.Combine(job.DestinationPath, fileName);
                    }
                }
                else
                {
                    // Remote source file - support Remote to Local transfers
                    fileName = GetFileNameFromPath(sourceFile);
                    
                    if (job.IncludeSubFolders && sourceFile.StartsWith(job.SourcePath))
                    {
                        // Preserve directory structure for remote to local
                        var normalizedSourcePath = job.SourcePath.Replace('\\', '/');
                        var normalizedSourceFile = sourceFile.Replace('\\', '/');
                        
                        if (normalizedSourceFile.StartsWith(normalizedSourcePath))
                        {
                            var relativePath = normalizedSourceFile.Substring(normalizedSourcePath.Length).TrimStart('/');
                            relativePath = relativePath.Replace('/', '\\'); // Convert to Windows path for local storage
                            destinationFile = Path.Combine(job.DestinationPath, relativePath);
                        }
                        else
                        {
                            destinationFile = Path.Combine(job.DestinationPath, fileName);
                        }
                    }
                    else
                    {
                        destinationFile = Path.Combine(job.DestinationPath, fileName);
                    }
                }

                string error;

                // Only support Local to Remote and Local to Local transfers
                if (job.SourceConnection.Protocol == ProtocolType.Local && job.DestinationConnection.Protocol != ProtocolType.Local)
                {
                    // Local to Remote: Upload
                    // For remote destinations, construct Unix-style path
                    string remoteDestinationFile;
                    if (job.DestinationConnection.Protocol != ProtocolType.Local)
                    {
                        var normalizedDestBase = job.DestinationPath.Replace('\\', '/');
                        if (!normalizedDestBase.EndsWith("/"))
                            normalizedDestBase += "/";
                        
                        if (job.IncludeSubFolders && sourceFile.StartsWith(job.SourcePath))
                        {
                            // Preserve directory structure to remote
                            var relativePath = sourceFile.Substring(job.SourcePath.Length).TrimStart('\\', '/');
                            relativePath = relativePath.Replace('\\', '/'); // Convert to Unix path
                            remoteDestinationFile = normalizedDestBase + relativePath;
                        }
                        else
                        {
                            remoteDestinationFile = normalizedDestBase + fileName;
                        }
                    }
                    else
                    {
                        remoteDestinationFile = destinationFile;
                    }
                    
                    var success = destClient.UploadFile(job.DestinationConnection, sourceFile, remoteDestinationFile, job.OverwriteExisting, out error);
                    if (!success)
                    {
                        result.Errors.Add($"Upload failed for {sourceFile}: {error}");
                        return false;
                    }
                }
                else if (job.SourceConnection.Protocol == ProtocolType.Local && job.DestinationConnection.Protocol == ProtocolType.Local)
                {
                    // Local to Local: Copy
                    if (!job.OverwriteExisting && File.Exists(destinationFile))
                    {
                        result.SkippedFiles++;
                        result.SkippedFilesList.Add(sourceFile);
                        return true; // Count as success
                    }

                    File.Copy(sourceFile, destinationFile, job.OverwriteExisting);
                }
                else if (job.SourceConnection.Protocol != ProtocolType.Local && job.DestinationConnection.Protocol == ProtocolType.Local)
                {
                    // Remote to Local: Download
                    var success = sourceClient.DownloadFile(job.SourceConnection, sourceFile, destinationFile, job.OverwriteExisting, out error);
                    if (!success)
                    {
                        result.Errors.Add($"Download failed for {sourceFile}: {error}");
                        return false;
                    }
                }
                else
                {
                    // Remote to Remote transfers are not supported yet
                    result.Errors.Add($"Remote to Remote transfers are not currently supported: {sourceFile}");
                    return false;
                }

                // Update byte statistics
                long fileSize = 0;
                if (job.SourceConnection.Protocol == ProtocolType.Local)
                {
                    var fileInfo = new FileInfo(sourceFile);
                    fileSize = fileInfo.Length;
                }
                else if (job.DestinationConnection.Protocol == ProtocolType.Local)
                {
                    // For remote to local transfers, get size from destination file after transfer
                    var destFileInfo = new FileInfo(destinationFile);
                    if (destFileInfo.Exists)
                    {
                        fileSize = destFileInfo.Length;
                    }
                }

                result.TransferredBytes += fileSize;
                result.TotalBytes += fileSize;

                // Basic file validation if local destination
                if (job.DestinationConnection.Protocol == ProtocolType.Local)
                {
                    var destFileInfo = new FileInfo(destinationFile);
                    if (!destFileInfo.Exists)
                    {
                        result.Warnings.Add($"Transferred file not found at destination: {destinationFile}");
                    }
                    else if (job.SourceConnection.Protocol == ProtocolType.Local && fileSize > 0 && fileSize != destFileInfo.Length)
                    {
                        // Only compare sizes for local to remote/local transfers where we can get source size
                        result.Warnings.Add($"File size mismatch for {fileName}: Source={fileSize}, Dest={destFileInfo.Length}");
                    }
                }

                // Delete source file after successful transfer if requested
                if (ShouldDeleteSourceAfterTransfer(job))
                {
                    try
                    {
                        string deleteError;
                        bool deleteSuccess = sourceClient.DeleteFile(job.SourceConnection, sourceFile, out deleteError);
                        
                        if (deleteSuccess)
                        {
                            _logService.LogJobProgress(job, $"Source file deleted after successful transfer: {fileName}");
                        }
                        else
                        {
                            result.Warnings.Add($"Failed to delete source file {fileName} after transfer: {deleteError ?? "Unknown error"}");
                            _logService.LogJobError(job, $"Failed to delete source file {fileName}: {deleteError ?? "Unknown error"}", null);
                        }
                    }
                    catch (Exception deleteEx)
                    {
                        result.Warnings.Add($"Failed to delete source file {fileName} after transfer: {deleteEx.Message}");
                        _logService.LogJobError(job, $"Failed to delete source file {fileName}: {deleteEx.Message}", deleteEx);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Transfer failed for {sourceFile}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get list of source files based on job configuration
        /// </summary>
        private List<string> GetSourceFiles(SyncJob job)
        {
            var files = new List<string>();

            try
            {
                // Debug logging to diagnose path handling issue
                _logService.LogJobProgress(job, $"DEBUG: Source protocol = {job.SourceConnection.Protocol}");
                _logService.LogJobProgress(job, $"DEBUG: Source path = {job.SourcePath}");
                
                if (job.SourceConnection.Protocol == ProtocolType.Local) // Local
                {
                    if (File.Exists(job.SourcePath))
                    {
                        files.Add(job.SourcePath);
                    }
                    else if (Directory.Exists(job.SourcePath))
                    {
                        var searchOption = job.IncludeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                        files.AddRange(Directory.GetFiles(job.SourcePath, "*.*", searchOption));
                    }
                    else
                    {
                        _logService.LogJobError(job, $"Local source path does not exist: {job.SourcePath}", null);
                    }
                }
                else // Remote
                {
                    var sourceClient = _clientFactory.Create(job.SourceConnection.Protocol);
                    List<string> remoteFiles;
                    string error;
                    
                    _logService.LogJobProgress(job, $"Listing remote files from: {job.SourcePath}");
                    
                    if (sourceClient.ListFiles(job.SourceConnection, job.SourcePath, out remoteFiles, out error))
                    {
                        files.AddRange(remoteFiles);
                        _logService.LogJobProgress(job, $"Found {remoteFiles.Count} remote files");
                    }
                    else
                    {
                        _logService.LogJobError(job, $"Failed to list remote files: {error}", null);
                        return files; // Return empty list on error
                    }
                }

                // Apply file filtering if enabled
                if (ShouldApplyFilters(job))
                {
                    var filterSettings = CreateFilterSettings(job);
                    var filteredFiles = _fileFilterService.ApplyFilters(files, filterSettings);
                    
                    _logService.LogJobProgress(job, string.Format("Filtering applied: {0} files reduced to {1} files", 
                        files.Count, filteredFiles.Count));
                    
                    files = filteredFiles;
                }

                _logService.LogJobProgress(job, string.Format("Found {0} files to process", files.Count));
            }
            catch (Exception ex)
            {
                _logService.LogJobError(job, $"Failed to get source files: {ex.Message}", ex);
            }

            return files;
        }

        /// <summary>
        /// Check if filters should be applied to this job
        /// </summary>
        private bool ShouldApplyFilters(SyncJob job)
        {
            // Check if job has the UI model property for EnableFilters
            var uiJobType = job.GetType();
            var enableFiltersProperty = uiJobType.GetProperty("EnableFilters");
            if (enableFiltersProperty != null)
            {
                return (bool)enableFiltersProperty.GetValue(job, null);
            }
            
            return false;
        }

        /// <summary>
        /// Create FilterSettings from job configuration
        /// </summary>
        private FilterSettings CreateFilterSettings(SyncJob job)
        {
            var filterSettings = new FilterSettings();
            
            var uiJobType = job.GetType();
            
            // Get include file types
            var includeProperty = uiJobType.GetProperty("IncludeFileTypes");
            if (includeProperty != null)
            {
                var includeTypes = (string)includeProperty.GetValue(job, null);
                if (!string.IsNullOrEmpty(includeTypes))
                {
                    var extensions = includeTypes.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var ext in extensions)
                    {
                        var cleanExt = ext.Trim();
                        if (!string.IsNullOrEmpty(cleanExt))
                        {
                            filterSettings.IncludeExtensions.Add(cleanExt);
                        }
                    }
                }
            }
            
            // Get exclude file types
            var excludeProperty = uiJobType.GetProperty("ExcludeFileTypes");
            if (excludeProperty != null)
            {
                var excludeTypes = (string)excludeProperty.GetValue(job, null);
                if (!string.IsNullOrEmpty(excludeTypes))
                {
                    var extensions = excludeTypes.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var ext in extensions)
                    {
                        var cleanExt = ext.Trim();
                        if (!string.IsNullOrEmpty(cleanExt))
                        {
                            filterSettings.ExcludeExtensions.Add(cleanExt);
                        }
                    }
                }
            }
            
            return filterSettings;
        }

        /// <summary>
        /// Cancel a running job
        /// </summary>
        public bool CancelJob(string jobId)
        {
            lock (_jobLock)
            {
                if (_activeJobs.TryGetValue(jobId, out TransferJob job))
                {
                    job.CancellationToken = new CancellationToken(true);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get status of all active jobs
        /// </summary>
        public List<TransferJob> GetActiveJobs()
        {
            lock (_jobLock)
            {
                return new List<TransferJob>(_activeJobs.Values);
            }
        }

        /// <summary>
        /// Check if a job is currently running
        /// </summary>
        public bool IsJobRunning(string jobId)
        {
            lock (_jobLock)
            {
                return _activeJobs.ContainsKey(jobId);
            }
        }

        protected virtual void OnTransferStarted(TransferStartedEventArgs e)
        {
            TransferStarted?.Invoke(this, e);
        }

        protected virtual void OnTransferProgress(TransferProgressEventArgs e)
        {
            TransferProgress?.Invoke(this, e);
        }

        protected virtual void OnTransferCompleted(TransferCompletedEventArgs e)
        {
            TransferCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// Check if source files should be deleted after successful transfer
        /// Handles both UI SyncJob (DeleteSourceAfterTransfer) and Core SyncJob (PostProcess.DeleteSourceAfterTransfer)
        /// </summary>
        private bool ShouldDeleteSourceAfterTransfer(SyncJob job)
        {
            // Check if job has the UI model property
            var uiJobType = job.GetType();
            var deleteProperty = uiJobType.GetProperty("DeleteSourceAfterTransfer");
            if (deleteProperty != null)
            {
                return (bool)deleteProperty.GetValue(job, null);
            }
            
            // Check if job has core model's PostProcess property
            var postProcessProperty = uiJobType.GetProperty("PostProcess");
            if (postProcessProperty != null)
            {
                var postProcess = postProcessProperty.GetValue(job, null);
                if (postProcess != null)
                {
                    var postProcessType = postProcess.GetType();
                    var deleteAfterTransferProperty = postProcessType.GetProperty("DeleteSourceAfterTransfer");
                    if (deleteAfterTransferProperty != null)
                    {
                        return (bool)deleteAfterTransferProperty.GetValue(postProcess, null);
                    }
                }
            }
            
            return false;
        }

        /// <summary>
        /// Extract filename from a remote path (handles both Unix and Windows paths)
        /// </summary>
        private string GetFileNameFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            
            // Handle both forward and backward slashes
            var lastSlashIndex = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
            return lastSlashIndex >= 0 ? path.Substring(lastSlashIndex + 1) : path;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Cancel all active jobs
                lock (_jobLock)
                {
                    foreach (var job in _activeJobs.Values)
                    {
                        job.CancellationToken = new CancellationToken(true);
                    }
                    _activeJobs.Clear();
                }

                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Represents an active transfer job
    /// </summary>
    public class TransferJob
    {
        public string JobId { get; set; }
        public SyncJob Job { get; set; }
        public TransferResultEnhanced Result { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public DateTime StartTime { get; set; }
    }

    /// <summary>
    /// Event arguments for transfer events
    /// </summary>
    public class TransferStartedEventArgs : EventArgs
    {
        public SyncJob Job { get; set; }
        public TransferResultEnhanced Result { get; set; }
    }

    public class TransferProgressEventArgs : EventArgs
    {
        public SyncJob Job { get; set; }
        public TransferResultEnhanced Result { get; set; }
        public int ProgressPercent { get; set; }
        public string CurrentFile { get; set; }
        public int FileProgress { get; set; }
    }

    public class TransferCompletedEventArgs : EventArgs
    {
        public SyncJob Job { get; set; }
        public TransferResultEnhanced Result { get; set; }
    }

    /// <summary>
    /// Simple cancellation token for .NET 3.5 compatibility
    /// </summary>
    public class CancellationToken
    {
        private volatile bool _isCancellationRequested;

        public CancellationToken(bool isCancellationRequested = false)
        {
            _isCancellationRequested = isCancellationRequested;
        }

        public bool IsCancellationRequested
        {
            get { return _isCancellationRequested; }
        }
    }
}
