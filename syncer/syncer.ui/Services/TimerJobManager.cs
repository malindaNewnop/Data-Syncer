using System;
using System.Timers;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using syncer.ui.Interfaces;
using syncer.core;
using syncer.core.Configuration;
using syncer.core.Transfers;

namespace syncer.ui.Services
{
    /// <summary>
    /// Manages timer-based jobs that run in the background
    /// </summary>
    public class TimerJobManager : ITimerJobManager
    {
        private Dictionary<long, TimerJobInfo> _timerJobs;
        private IConnectionService _connectionService;
        private ILogService _logService;
        
        /// <summary>
        /// Represents a timer job with its settings and state
        /// </summary>
        private class TimerJobInfo
        {
            public long JobId { get; set; }
            public string JobName { get; set; }
            public string FolderPath { get; set; }
            public string RemotePath { get; set; }
            public System.Timers.Timer Timer { get; set; }
            public double IntervalMs { get; set; }
            public bool IsRunning { get; set; }
            public DateTime? LastJobTime { get; set; } // Changed from LastUploadTime to be generic
            public ITransferClient TransferClient { get; set; }
            public syncer.core.ConnectionSettings ConnectionSettings { get; set; }
            public bool IsJobInProgress { get; set; } // Prevent overlapping jobs (changed from IsUploadInProgress)
            public DateTime? JobStartTime { get; set; } // Track job duration (changed from UploadStartTime)
            public bool IncludeSubfolders { get; set; } // Whether to include subfolders in file enumeration
            public bool IsDownloadJob { get; set; } // Whether this job is a download (remote to local) or upload (local to remote)
            public bool DeleteSourceAfterTransfer { get; set; } // Whether to delete source files after successful transfer
            public FilterSettings FilterSettings { get; set; } // File filtering settings
        }
        
        public TimerJobManager()
        {
            _timerJobs = new Dictionary<long, TimerJobInfo>();
            _connectionService = ServiceLocator.ConnectionService;
            _logService = ServiceLocator.LogService;
        }
        
        public bool RegisterTimerJob(long jobId, string folderPath, string remotePath, double intervalMs)
        {
            return RegisterTimerJob(jobId, "Timer Job " + jobId, folderPath, remotePath, intervalMs, true, false, null); // Default to include subfolders
        }
        
        public bool RegisterTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs)
        {
            return RegisterTimerJob(jobId, jobName, folderPath, remotePath, intervalMs, true, false, null); // Default to include subfolders
        }
        
        public bool RegisterTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders)
        {
            return RegisterTimerJob(jobId, jobName, folderPath, remotePath, intervalMs, includeSubfolders, false, null); // Default no delete source, no filters
        }
        
        public bool RegisterTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders, bool deleteSourceAfterTransfer = false, FilterSettings filterSettings = null)
        {
            try
            {
                // Check if the job already exists
                if (_timerJobs.ContainsKey(jobId))
                {
                    // Update existing job
                    var job = _timerJobs[jobId];
                    job.JobName = jobName ?? ("Timer Job " + jobId);
                    job.FolderPath = folderPath;
                    job.RemotePath = remotePath;
                    job.IntervalMs = intervalMs;
                    job.IncludeSubfolders = includeSubfolders; // Update subfolder setting
                    job.DeleteSourceAfterTransfer = deleteSourceAfterTransfer;
                    job.FilterSettings = filterSettings;
                    
                    // Initialize new properties if they don't exist
                    if (!job.IsJobInProgress) job.IsJobInProgress = false;
                    if (job.JobStartTime == null) job.JobStartTime = null;
                    
                    // Update timer interval if running
                    if (job.Timer != null && job.IsRunning)
                    {
                        job.Timer.Interval = intervalMs;
                    }
                    
                    _logService.LogInfo(string.Format("Updated timer job {0} ({1}) for folder {2}", jobId, jobName, folderPath));
                    return true;
                }
                
                // Create a new job
                ConnectionSettings connectionSettings = _connectionService.GetConnectionSettings();
                if (connectionSettings == null || !connectionSettings.IsRemoteConnection)
                {
                    _logService.LogError("Cannot register timer job: No remote connection settings available");
                    return false;
                }
                
                // Convert UI ConnectionSettings to Core ConnectionSettings
                var coreSettings = new syncer.core.ConnectionSettings
                {
                    Protocol = connectionSettings.Protocol == "SFTP" ? 
                        syncer.core.ProtocolType.Sftp : 
                        connectionSettings.Protocol == "FTP" ? 
                            syncer.core.ProtocolType.Ftp : 
                            syncer.core.ProtocolType.Local,
                    Host = connectionSettings.Host,
                    Port = connectionSettings.Port,
                    Username = connectionSettings.Username,
                    Password = connectionSettings.Password,
                    SshKeyPath = connectionSettings.SshKeyPath,
                    Timeout = connectionSettings.Timeout
                };
                
                // Get the appropriate transfer client using the factory
                syncer.core.TransferClientFactory factory = new syncer.core.TransferClientFactory();
                ITransferClient transferClient = factory.Create(coreSettings.Protocol);
                
                // Create the job
                var newJob = new TimerJobInfo
                {
                    JobId = jobId,
                    JobName = jobName ?? ("Timer Job " + jobId),
                    FolderPath = folderPath,
                    RemotePath = remotePath,
                    IntervalMs = intervalMs,
                    IsRunning = false,
                    LastJobTime = null,
                    TransferClient = transferClient,
                    ConnectionSettings = coreSettings,
                    IsJobInProgress = false,
                    JobStartTime = null,
                    IsDownloadJob = false,
                    DeleteSourceAfterTransfer = deleteSourceAfterTransfer,
                    IncludeSubfolders = includeSubfolders,
                    FilterSettings = filterSettings
                };
                
                _timerJobs.Add(jobId, newJob);
                
                _logService.LogInfo(string.Format("Registered upload timer job {0} ({1}) for folder {2}", jobId, jobName, folderPath));
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Failed to register timer job: {0}", ex.Message));
                return false;
            }
        }
        
        // Interface overload method to support object type for FilterSettings
        public bool RegisterTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders, bool deleteSourceAfterTransfer, object filterSettings)
        {
            // Cast the object back to FilterSettings
            FilterSettings filters = filterSettings as FilterSettings;
            return RegisterTimerJob(jobId, jobName, folderPath, remotePath, intervalMs, includeSubfolders, deleteSourceAfterTransfer, filters);
        }
        
        /// <summary>
        /// Register a download timer job (remote to local)
        /// </summary>
        public bool RegisterDownloadTimerJob(long jobId, string remotePath, string localFolderPath, double intervalMs, bool deleteSourceAfterTransfer = false)
        {
            return RegisterDownloadTimerJob(jobId, "Timer Download Job " + jobId, remotePath, localFolderPath, intervalMs, true, deleteSourceAfterTransfer, null);
        }
        
        /// <summary>
        /// Register a download timer job (remote to local) with custom name
        /// </summary>
        public bool RegisterDownloadTimerJob(long jobId, string jobName, string remotePath, string localFolderPath, double intervalMs, bool includeSubfolders, bool deleteSourceAfterTransfer = false)
        {
            return RegisterDownloadTimerJob(jobId, jobName, remotePath, localFolderPath, intervalMs, includeSubfolders, deleteSourceAfterTransfer, null);
        }
        
        /// <summary>
        /// Register a download timer job (remote to local) with custom name
        /// </summary>
        public bool RegisterDownloadTimerJob(long jobId, string jobName, string remotePath, string localFolderPath, double intervalMs, bool includeSubfolders, bool deleteSourceAfterTransfer = false, FilterSettings filterSettings = null)
        {
            try
            {
                // Check if the job already exists
                if (_timerJobs.ContainsKey(jobId))
                {
                    // Update existing job
                    var job = _timerJobs[jobId];
                    job.JobName = jobName ?? ("Timer Download Job " + jobId);
                    job.FolderPath = localFolderPath;  // Local destination
                    job.RemotePath = remotePath;       // Remote source
                    job.IntervalMs = intervalMs;
                    job.IsDownloadJob = true;          // Mark as download job
                    job.DeleteSourceAfterTransfer = deleteSourceAfterTransfer;
                    job.IncludeSubfolders = includeSubfolders;
                    job.FilterSettings = filterSettings;
                    
                    // Initialize other properties
                    if (!job.IsJobInProgress) job.IsJobInProgress = false;
                    if (job.JobStartTime == null) job.JobStartTime = null;
                    
                    // Update timer interval if running
                    if (job.Timer != null && job.IsRunning)
                    {
                        job.Timer.Interval = intervalMs;
                    }
                    
                    _logService.LogInfo(string.Format("Updated download timer job {0} ({1}) from {2} to {3}", 
                        jobId, jobName, remotePath, localFolderPath));
                    return true;
                }
                
                // Create a new job
                ConnectionSettings connectionSettings = _connectionService.GetConnectionSettings();
                if (connectionSettings == null || !connectionSettings.IsRemoteConnection)
                {
                    _logService.LogError("Cannot register download timer job: No remote connection settings available");
                    return false;
                }
                
                // Convert UI ConnectionSettings to Core ConnectionSettings
                var coreSettings = new syncer.core.ConnectionSettings
                {
                    Protocol = connectionSettings.Protocol == "SFTP" ? 
                        syncer.core.ProtocolType.Sftp : 
                        connectionSettings.Protocol == "FTP" ? 
                            syncer.core.ProtocolType.Ftp : 
                            syncer.core.ProtocolType.Local,
                    Host = connectionSettings.Host,
                    Port = connectionSettings.Port,
                    Username = connectionSettings.Username,
                    Password = connectionSettings.Password,
                    SshKeyPath = connectionSettings.SshKeyPath,
                    Timeout = connectionSettings.Timeout
                };
                
                // Get the appropriate transfer client using the factory
                syncer.core.TransferClientFactory factory = new syncer.core.TransferClientFactory();
                ITransferClient transferClient = factory.Create(coreSettings.Protocol);
                
                // Create the job
                var newJob = new TimerJobInfo
                {
                    JobId = jobId,
                    JobName = jobName ?? ("Timer Download Job " + jobId),
                    FolderPath = localFolderPath,   // Local destination
                    RemotePath = remotePath,        // Remote source
                    IntervalMs = intervalMs,
                    IsRunning = false,
                    LastJobTime = null,
                    TransferClient = transferClient,
                    ConnectionSettings = coreSettings,
                    IsJobInProgress = false,
                    JobStartTime = null,
                    IsDownloadJob = true,           // Mark as download job
                    DeleteSourceAfterTransfer = deleteSourceAfterTransfer,
                    IncludeSubfolders = includeSubfolders,
                    FilterSettings = filterSettings
                };
                
                _timerJobs.Add(jobId, newJob);
                
                _logService.LogInfo(string.Format("Registered download timer job {0} ({1}) from {2} to {3}", 
                    jobId, jobName, remotePath, localFolderPath));
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Failed to register download timer job: {0}", ex.Message));
                return false;
            }
        }
        
        // Interface overload method to support object type for FilterSettings
        public bool RegisterDownloadTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders, bool deleteSourceAfterTransfer, object filterSettings)
        {
            // Cast the object back to FilterSettings
            FilterSettings filters = filterSettings as FilterSettings;
            return RegisterDownloadTimerJob(jobId, jobName, remotePath, folderPath, intervalMs, includeSubfolders, deleteSourceAfterTransfer, filters);
        }
        
        public bool StartTimerJob(long jobId)
        {
            try
            {
                if (!_timerJobs.ContainsKey(jobId))
                {
                    _logService.LogError(string.Format("Cannot start timer job {0}: Job not found", jobId));
                    return false;
                }
                
                TimerJobInfo job = _timerJobs[jobId];
                
                // Create timer if it doesn't exist
                if (job.Timer == null)
                {
                    job.Timer = new System.Timers.Timer();
                    job.Timer.Elapsed += (sender, e) => OnTimerElapsed(jobId);
                    job.Timer.AutoReset = true;
                    job.Timer.Interval = job.IntervalMs;
                }
                
                // Start the timer
                job.Timer.Start();
                job.IsRunning = true;
                
                _logService.LogInfo(string.Format("Started timer job {0} with interval {1}ms", jobId, job.IntervalMs));
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Failed to start timer job {0}: {1}", jobId, ex.Message));
                return false;
            }
        }
        
        public bool StopTimerJob(long jobId)
        {
            try
            {
                if (!_timerJobs.ContainsKey(jobId))
                {
                    _logService.LogError(string.Format("Cannot stop timer job {0}: Job not found", jobId));
                    return false;
                }
                
                TimerJobInfo job = _timerJobs[jobId];
                
                // Stop the timer if it exists
                if (job.Timer != null)
                {
                    job.Timer.Stop();
                    job.IsRunning = false;
                    _logService.LogInfo(string.Format("Stopped timer job {0}", jobId));
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Failed to stop timer job {0}: {1}", jobId, ex.Message));
                return false;
            }
        }
        
        public bool RemoveTimerJob(long jobId)
        {
            try
            {
                if (!_timerJobs.ContainsKey(jobId))
                {
                    return false;
                }
                
                TimerJobInfo job = _timerJobs[jobId];
                
                // Stop and dispose the timer if it exists
                if (job.Timer != null)
                {
                    job.Timer.Stop();
                    job.Timer.Dispose();
                }
                
                _timerJobs.Remove(jobId);
                
                _logService.LogInfo(string.Format("Removed timer job {0}", jobId));
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Failed to remove timer job {0}: {1}", jobId, ex.Message));
                return false;
            }
        }
        
        public bool IsTimerJobRunning(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return false;
            }
            
            return _timerJobs[jobId].IsRunning;
        }
        
        public List<long> GetRegisteredTimerJobs()
        {
            return new List<long>(_timerJobs.Keys);
        }
        
        public DateTime? GetLastUploadTime(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return null;
            }
            
            return _timerJobs[jobId].LastJobTime;
        }
        
        public string GetTimerJobFolderPath(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return null;
            }
            
            return _timerJobs[jobId].FolderPath;
        }
        
        public string GetTimerJobRemotePath(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return null;
            }
            
            return _timerJobs[jobId].RemotePath;
        }
        
        public double GetTimerJobInterval(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return 0;
            }
            
            return _timerJobs[jobId].IntervalMs;
        }
        
        public string GetTimerJobName(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return null;
            }
            
            return _timerJobs[jobId].JobName ?? ("Timer Job " + jobId);
        }
        
        /// <summary>
        /// Checks if a timer job is currently performing an upload
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>True if the job is currently uploading</returns>
        public bool IsTimerJobUploading(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return false;
            }
            
            return _timerJobs[jobId].IsJobInProgress;
        }
        
        /// <summary>
        /// Gets the job start time for a timer job
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>DateTime when the current job started, or null if not running</returns>
        public DateTime? GetTimerJobUploadStartTime(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return null;
            }
            
            return _timerJobs[jobId].JobStartTime;
        }
        
        public bool UpdateTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs)
        {
            return UpdateTimerJob(jobId, jobName, folderPath, remotePath, intervalMs, true); // Default to include subfolders for backward compatibility
        }
        
        public bool UpdateTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders)
        {
            try
            {
                if (!_timerJobs.ContainsKey(jobId))
                {
                    _logService.LogError(string.Format("Cannot update timer job {0}: Job not found", jobId));
                    return false;
                }
                
                TimerJobInfo job = _timerJobs[jobId];
                bool wasRunning = job.IsRunning;
                
                // Stop the timer temporarily if it's running
                if (wasRunning && job.Timer != null)
                {
                    job.Timer.Stop();
                }
                
                // Update job properties
                job.JobName = jobName ?? ("Timer Job " + jobId);
                job.FolderPath = folderPath;
                job.RemotePath = remotePath;
                job.IntervalMs = intervalMs;
                job.IncludeSubfolders = includeSubfolders;
                
                // Update timer interval if the timer exists
                if (job.Timer != null)
                {
                    job.Timer.Interval = intervalMs;
                    
                    // Restart the timer if it was running
                    if (wasRunning)
                    {
                        job.Timer.Start();
                    }
                }
                
                _logService.LogInfo(string.Format("Updated timer job {0}: folder={1}, remote={2}, interval={3}ms", 
                    jobId, folderPath, remotePath, intervalMs));
                
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Failed to update timer job {0}: {1}", jobId, ex.Message));
                return false;
            }
        }
        
        private void OnTimerElapsed(long jobId)
        {
            try
            {
                if (!_timerJobs.ContainsKey(jobId))
                {
                    return;
                }
                
                TimerJobInfo job = _timerJobs[jobId];
                
                // Prevent overlapping jobs
                if (job.IsJobInProgress)
                {
                    TimeSpan jobDuration = DateTime.Now - (job.JobStartTime ?? DateTime.Now);
                    _logService.LogWarning(string.Format("Skipping timer cycle for job {0} - previous {1} still in progress (running for {2:mm\\:ss})", 
                        jobId, job.IsDownloadJob ? "download" : "upload", jobDuration));
                    return;
                }
                
                _logService.LogInfo(string.Format("Timer elapsed for job {0} - starting {1}", 
                    jobId, job.IsDownloadJob ? "folder download" : "folder upload"));
                
                // Handle differently based on job type
                if (job.IsDownloadJob)
                {
                    // For download jobs, we mark it as in progress and process it
                    job.IsJobInProgress = true;
                    job.JobStartTime = DateTime.Now;
                    
                    // Process download asynchronously
                    ProcessDownloadTimerJob(job);
                    return;
                }
                
                // For upload jobs, continue with the original flow
                SearchOption searchOption = job.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                string[] allFiles = Directory.GetFiles(job.FolderPath, "*", searchOption);
                
                // Apply filters if configured
                string[] currentFiles = allFiles;
                if (job.FilterSettings != null && job.FilterSettings.FiltersEnabled)
                {
                    string[] filteredFiles = ApplyFilters(allFiles, job.FilterSettings);
                    currentFiles = filteredFiles;
                    _logService.LogInfo(string.Format("Applied filters: {0} files remain from {1} total files", currentFiles.Length, allFiles.Length));
                }
                else
                {
                    _logService.LogInfo(string.Format("No filters applied: processing all {0} files", currentFiles.Length));
                }
                
                // Also ensure empty directories are created on remote (only if including subfolders)
                string[] allDirectories = job.IncludeSubfolders ? 
                    Directory.GetDirectories(job.FolderPath, "*", SearchOption.AllDirectories) : 
                    new string[0]; // No subdirectories if not including subfolders
                
                _logService.LogInfo(string.Format("Found {0} files and {1} directories in folder {2} (subfolders: {3})", 
                    currentFiles.Length, allDirectories.Length, job.FolderPath, job.IncludeSubfolders ? "included" : "excluded"));
                
                // Create empty directories first
                foreach (string directory in allDirectories)
                {
                    try
                    {
                        string relativeDirPath = directory.Substring(job.FolderPath.Length);
                        if (relativeDirPath.StartsWith("\\") || relativeDirPath.StartsWith("/"))
                        {
                            relativeDirPath = relativeDirPath.Substring(1);
                        }
                        
                        string normalizedRemotePath = job.RemotePath.Replace('\\', '/');
                        if (!normalizedRemotePath.EndsWith("/")) normalizedRemotePath += "/";
                        
                        string remoteDirectory = normalizedRemotePath + relativeDirPath.Replace('\\', '/');
                        
                        string dirError = null;
                        if (job.TransferClient.EnsureDirectory(job.ConnectionSettings, remoteDirectory, out dirError))
                        {
                            _logService.LogInfo(string.Format("Created/ensured remote directory: {0}", remoteDirectory));
                        }
                        else
                        {
                            _logService.LogWarning(string.Format("Failed to create remote directory {0}: {1}", 
                                remoteDirectory, dirError ?? "Unknown error"));
                        }
                    }
                    catch (Exception dirEx)
                    {
                        _logService.LogError(string.Format("Error processing directory {0}: {1}", directory, dirEx.Message));
                    }
                }
                
                if (currentFiles.Length == 0 && allDirectories.Length == 0)
                {
                    _logService.LogWarning(string.Format("No files or directories found in folder {0} for timer job {1}", job.FolderPath, jobId));
                    return;
                }
                
                if (currentFiles.Length == 0)
                {
                    _logService.LogInfo(string.Format("No files to upload, but {0} empty directories were processed for timer job {1}", allDirectories.Length, jobId));
                    return;
                }
                
                // Mark job as in progress
                job.IsJobInProgress = true;
                job.JobStartTime = DateTime.Now;
                
                // Choose between upload and download based on job type
                if (job.IsDownloadJob)
                {
                    // For download jobs, we need to get remote files first
                    System.Threading.ThreadPool.QueueUserWorkItem(state =>
                    {
                        try
                        {
                            // List remote files to download
                            List<string> remoteFiles;
                            string listError;
                            bool success = job.TransferClient.ListFiles(job.ConnectionSettings, job.RemotePath, out remoteFiles, out listError);
                            
                            if (!success)
                            {
                                _logService.LogError(string.Format("Failed to list remote files for job {0}: {1}", 
                                    jobId, listError ?? "Unknown error"));
                                return;
                            }
                            
                            if (remoteFiles == null || remoteFiles.Count == 0)
                            {
                                _logService.LogInfo(string.Format("No files found in remote folder {0} for job {1}", 
                                    job.RemotePath, jobId));
                                return;
                            }
                            
                            _logService.LogInfo(string.Format("Found {0} files to download from {1} for job {2}", 
                                remoteFiles.Count, job.RemotePath, jobId));
                                
                            // Perform the download
                            PerformFolderDownload(job, remoteFiles.ToArray());
                            
                            // Update last job time
                            job.LastJobTime = DateTime.Now;
                            
                            TimeSpan totalDuration = DateTime.Now - job.JobStartTime.Value;
                            _logService.LogInfo(string.Format("Download cycle completed for job {0} in {1:mm\\:ss}", 
                                jobId, totalDuration));
                        }
                        catch (Exception ex)
                        {
                            _logService.LogError(string.Format("Error in background download for job {0}: {1}", jobId, ex.Message));
                        }
                        finally
                        {
                            // Always reset the job in progress flag
                            job.IsJobInProgress = false;
                            job.JobStartTime = null;
                        }
                    });
                }
                else
                {
                    // Perform the upload asynchronously to prevent blocking the timer
                    System.Threading.ThreadPool.QueueUserWorkItem(state =>
                    {
                        try
                        {
                            PerformFolderUpload(job, currentFiles);
                            
                            // Update last job time
                            job.LastJobTime = DateTime.Now;
                            
                            TimeSpan totalDuration = DateTime.Now - job.JobStartTime.Value;
                            _logService.LogInfo(string.Format("Upload cycle completed for job {0} in {1:mm\\:ss}", 
                                jobId, totalDuration));
                        }
                        catch (Exception ex)
                        {
                            _logService.LogError(string.Format("Error in background upload for job {0}: {1}", jobId, ex.Message));
                        }
                        finally
                        {
                            // Always reset the job in progress flag
                            job.IsJobInProgress = false;
                            job.JobStartTime = null;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Error in timer job {0}: {1}", jobId, ex.Message));
                
                // Ensure we reset the flag even if there's an error
                if (_timerJobs.ContainsKey(jobId))
                {
                    _timerJobs[jobId].IsJobInProgress = false;
                    _timerJobs[jobId].JobStartTime = null;
                }
            }
        }
        
        private void PerformFolderUpload(TimerJobInfo job, string[] localFilePaths)
        {
            try
            {
                _logService.LogInfo(string.Format("Starting folder upload for job {0} with {1} files from {2} to {3}", 
                    job.JobId, localFilePaths.Length, job.FolderPath, job.RemotePath));
                
                _logService.LogInfo(string.Format("Connection: {0}@{1}:{2}", 
                    job.ConnectionSettings.Username, job.ConnectionSettings.Host, job.ConnectionSettings.Port));
                
                // Track upload statistics
                int successfulUploads = 0;
                int failedUploads = 0;
                int skippedFiles = 0;
                
                // Dictionary to track created remote folders
                Dictionary<string, bool> createdRemoteFolders = new Dictionary<string, bool>();
                
                // Process files in smaller batches to improve responsiveness
                const int batchSize = 5; // Process 5 files at a time
                int totalBatches = (int)Math.Ceiling((double)localFilePaths.Length / batchSize);
                
                for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
                {
                    int startIndex = batchIndex * batchSize;
                    int endIndex = Math.Min(startIndex + batchSize, localFilePaths.Length);
                    
                    _logService.LogInfo(string.Format("Processing batch {0}/{1} (files {2}-{3})", 
                        batchIndex + 1, totalBatches, startIndex + 1, endIndex));
                    
                    // Process files in current batch
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        string localFile = localFilePaths[i];
                        
                        try
                        {
                            // Skip if the file doesn't exist
                            if (!File.Exists(localFile))
                            {
                                _logService.LogWarning(string.Format("File does not exist: {0}", localFile));
                                skippedFiles++;
                                continue;
                            }
                            
                            // Skip files that are currently being written to or locked
                            try
                            {
                                using (FileStream stream = File.Open(localFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    // File is accessible
                                }
                            }
                            catch (IOException)
                            {
                                _logService.LogWarning(string.Format("File is locked or being used: {0}", localFile));
                                skippedFiles++;
                                continue;
                            }
                            
                            // Calculate relative path from base folder
                            string relativePath = localFile.Substring(job.FolderPath.Length);
                            if (relativePath.StartsWith("\\") || relativePath.StartsWith("/"))
                            {
                                relativePath = relativePath.Substring(1);
                            }
                            
                            // Fix path separator - ensure remote path uses forward slashes
                            string normalizedRemotePath = job.RemotePath.Replace('\\', '/');
                            if (!normalizedRemotePath.EndsWith("/")) normalizedRemotePath += "/";
                            
                            // Get the directory structure of the file
                            string remoteDirectory = Path.GetDirectoryName(relativePath);
                            
                            if (!string.IsNullOrEmpty(remoteDirectory))
                            {
                                // Replace backslashes with forward slashes
                                remoteDirectory = remoteDirectory.Replace('\\', '/');
                                
                                // Create the remote directory structure
                                string[] pathParts = remoteDirectory.Split('/');
                                string currentPath = normalizedRemotePath;
                                
                                foreach (string part in pathParts)
                                {
                                    currentPath += part + "/";
                                    
                                    // Check if we've already created this directory
                                    if (!createdRemoteFolders.ContainsKey(currentPath))
                                    {
                                        // Ensure directory exists (creates it if it doesn't exist)
                                        string dirError = null;
                                        if (job.TransferClient.EnsureDirectory(job.ConnectionSettings, currentPath, out dirError))
                                        {
                                            _logService.LogInfo(string.Format("Ensured remote directory exists: {0}", currentPath));
                                            createdRemoteFolders.Add(currentPath, true);
                                        }
                                        else
                                        {
                                            _logService.LogError(string.Format("Failed to create directory {0}: {1}", 
                                                currentPath, dirError ?? "Unknown error"));
                                            continue;
                                        }
                                    }
                                }
                            }
                            
                            // Calculate the remote file path
                            string remoteFile = normalizedRemotePath + relativePath.Replace('\\', '/');
                            
                            // Upload the file
                            string fileName = Path.GetFileName(localFile);
                            _logService.LogInfo(string.Format("Uploading: {0} -> {1}", localFile, remoteFile));
                            
                            FileInfo fileInfo = new FileInfo(localFile);
                            _logService.LogInfo(string.Format("Local file size: {0} bytes", fileInfo.Length));
                            
                            string error = null;
                            bool success = job.TransferClient.UploadFile(job.ConnectionSettings, localFile, remoteFile, true, out error);
                            
                            if (!success)
                            {
                                string errorMsg = string.Format("Failed to upload {0}: {1}", 
                                    fileName, error == null ? "Unknown error" : error);
                                _logService.LogError(errorMsg);
                                failedUploads++;
                                
                                // Don't break here - continue trying to upload other files
                                // This ensures all files get an attempt even if some fail
                            }
                            else
                            {
                                _logService.LogInfo(string.Format("Successfully uploaded: {0} ({1} bytes)", fileName, fileInfo.Length));
                                successfulUploads++;
                                
                                // Verify upload by checking if file exists remotely (optional)
                                try
                                {
                                    bool remoteFileExists = false;
                                    string verifyError = null;
                                    if (job.TransferClient.FileExists(job.ConnectionSettings, remoteFile, out remoteFileExists, out verifyError))
                                    {
                                        if (remoteFileExists)
                                        {
                                            _logService.LogInfo(string.Format("Upload verified: {0} exists on remote server", fileName));
                                        }
                                        else
                                        {
                                            _logService.LogWarning(string.Format("Upload completed but file not found on remote: {0}", fileName));
                                        }
                                    }
                                }
                                catch (Exception verifyEx)
                                {
                                    _logService.LogWarning(string.Format("Could not verify upload of {0}: {1}", fileName, verifyEx.Message));
                                }
                                
                                // Delete source file if specified and upload was successful (even if verification failed)
                                if (job.DeleteSourceAfterTransfer)
                                {
                                    try
                                    {
                                        File.Delete(localFile);
                                        _logService.LogInfo(string.Format("Source file deleted after successful upload: {0}", fileName));
                                    }
                                    catch (Exception deleteEx)
                                    {
                                        _logService.LogWarning(string.Format("Failed to delete local source file {0}: {1}", 
                                            fileName, deleteEx.Message));
                                    }
                                }
                            }
                        }
                        catch (Exception fileEx)
                        {
                            _logService.LogError(string.Format("Error uploading file: {0}", fileEx.Message));
                            failedUploads++;
                        }
                    }
                    
                    // Small delay between batches to prevent overwhelming the server
                    if (batchIndex < totalBatches - 1) // Don't delay after the last batch
                    {
                        System.Threading.Thread.Sleep(100); // 100ms delay between batches
                    }
                }
                
                _logService.LogInfo(string.Format("Folder upload completed for job {0}. Results: {1} successful, {2} failed, {3} skipped out of {4} total files", 
                    job.JobId, successfulUploads, failedUploads, skippedFiles, localFilePaths.Length));
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Error in folder upload for job {0}: {1}", job.JobId, ex.Message));
            }
        }
        
        /// <summary>
        /// Apply filters to a list of files based on FilterSettings
        /// </summary>
        private string[] ApplyFilters(string[] allFiles, FilterSettings filterSettings)
        {
            if (filterSettings == null)
            {
                return allFiles; // No filters, return all files
            }
            
            List<string> filteredFiles = new List<string>();
            
            foreach (string file in allFiles)
            {
                if (ShouldIncludeFile(file, filterSettings))
                {
                    filteredFiles.Add(file);
                }
            }
            
            return filteredFiles.ToArray();
        }
        
        /// <summary>
        /// Determine if a file should be included based on filter settings
        /// </summary>
        private bool ShouldIncludeFile(string filePath, FilterSettings filterSettings)
        {
            if (filterSettings == null || !filterSettings.FiltersEnabled) return true;
            
            string fileName = Path.GetFileName(filePath);
            string fileExtension = Path.GetExtension(filePath).ToLower();
            
            // Check exclude file patterns (e.g., .pdf, .tmp)
            if (!string.IsNullOrEmpty(filterSettings.ExcludeFilePatterns))
            {
                string[] excludePatterns = filterSettings.ExcludeFilePatterns.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string excludePattern in excludePatterns)
                {
                    string cleanPattern = excludePattern.Trim();
                    
                    // If it starts with a dot, treat it as an extension
                    if (cleanPattern.StartsWith("."))
                    {
                        if (fileExtension == cleanPattern.ToLower())
                        {
                            return false; // Exclude this file extension
                        }
                    }
                    else
                    {
                        // Treat as wildcard pattern for filename
                        if (IsWildcardMatch(fileName, cleanPattern))
                        {
                            return false; // Exclude this file pattern
                        }
                    }
                }
            }
            
            // Check include file extensions (if specified, only include these extensions)
            if (!string.IsNullOrEmpty(filterSettings.IncludeFileExtensions))
            {
                string[] includeExtensions = filterSettings.IncludeFileExtensions.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                bool matchesInclude = false;
                foreach (string includeExt in includeExtensions)
                {
                    string cleanExt = includeExt.Trim().ToLower();
                    if (!cleanExt.StartsWith(".")) cleanExt = "." + cleanExt;
                    
                    if (fileExtension == cleanExt)
                    {
                        matchesInclude = true;
                        break;
                    }
                }
                if (!matchesInclude) return false; // Not in include list
            }
            
            return true; // File passes all filters
        }
        
        /// <summary>
        /// Simple wildcard pattern matching (* and ? supported)
        /// </summary>
        private bool IsWildcardMatch(string text, string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return true;
            if (string.IsNullOrEmpty(text)) return pattern == "*";
            
            // Convert wildcard pattern to regex
            string regexPattern = "^" + pattern.Replace("*", ".*").Replace("?", ".") + "$";
            return System.Text.RegularExpressions.Regex.IsMatch(text, regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        
        public Dictionary<long, object> GetRunningJobs()
        {
            var runningJobs = new Dictionary<long, object>();
            try
            {
                foreach (var kvp in _timerJobs)
                {
                    if (kvp.Value.IsRunning)
                    {
                        runningJobs.Add(kvp.Key, kvp.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Error getting running jobs: {0}", ex.Message));
            }
            return runningJobs;
        }
        
        public Dictionary<long, object> GetAllJobs()
        {
            var allJobs = new Dictionary<long, object>();
            try
            {
                foreach (var kvp in _timerJobs)
                {
                    allJobs.Add(kvp.Key, kvp.Value);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Error getting all jobs: {0}", ex.Message));
            }
            return allJobs;
        }
        
        /// <summary>
        /// Download files from remote to local
        /// </summary>
        /// <param name="job">The timer job</param>
        /// <param name="remoteFilePaths">Array of remote file paths to download</param>
        private void PerformFolderDownload(TimerJobInfo job, string[] remoteFilePaths)
        {
            try
            {
                _logService.LogInfo(string.Format("Starting folder download for job {0} with {1} files from {2} to {3}", 
                    job.JobId, remoteFilePaths.Length, job.RemotePath, job.FolderPath));
                
                _logService.LogInfo(string.Format("Connection: {0}@{1}:{2}", 
                    job.ConnectionSettings.Username, job.ConnectionSettings.Host, job.ConnectionSettings.Port));
                
                // Track download statistics
                int successfulDownloads = 0;
                int failedDownloads = 0;
                long totalBytes = 0;
                
                // Ensure local destination directory exists - Create all directories in the path
                try
                {
                    if (!Directory.Exists(job.FolderPath))
                    {
                        // Create all directories in the path
                        Directory.CreateDirectory(job.FolderPath);
                        _logService.LogInfo(string.Format("Created local destination directory: {0}", job.FolderPath));
                    }
                }
                catch (Exception dirEx)
                {
                    _logService.LogError(string.Format("Error creating local destination directory {0}: {1}", job.FolderPath, dirEx.Message));
                    
                    // Try a different approach to create the entire directory path
                    try
                    {
                        // Break down the path and try to create each segment
                        string path = job.FolderPath;
                        if (path.EndsWith("\\") || path.EndsWith("/"))
                            path = path.Substring(0, path.Length - 1);
                            
                        // Start with the drive root
                        string drivePart = Path.GetPathRoot(path);
                        string remainingPath = path.Substring(drivePart.Length);
                        string currentPath = drivePart;
                        
                        // Split by directory separator and create each segment
                        foreach (string segment in remainingPath.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, 
                            StringSplitOptions.RemoveEmptyEntries))
                        {
                            currentPath = Path.Combine(currentPath, segment);
                            if (!Directory.Exists(currentPath))
                            {
                                Directory.CreateDirectory(currentPath);
                                _logService.LogInfo(string.Format("Created directory segment: {0}", currentPath));
                            }
                        }
                        
                        _logService.LogInfo(string.Format("Successfully created full directory path: {0}", job.FolderPath));
                    }
                    catch (Exception segmentEx)
                    {
                        _logService.LogError(string.Format("Failed to create directory path segments for {0}: {1}", job.FolderPath, segmentEx.Message));
                        throw; // Re-throw to be caught by outer try/catch
                    }
                }
                
                // Process files in smaller batches to improve responsiveness
                const int batchSize = 5; // Process 5 files at a time
                int totalBatches = (int)Math.Ceiling((double)remoteFilePaths.Length / batchSize);
                
                for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
                {
                    int startIndex = batchIndex * batchSize;
                    int endIndex = Math.Min(startIndex + batchSize, remoteFilePaths.Length);
                    
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        string remoteFile = remoteFilePaths[i];
                        string fileName = Path.GetFileName(remoteFile);
                        
                        // Calculate relative path if needed for subdirectories
                        string relativePath = "";
                        string remotePath = job.RemotePath.Replace('\\', '/');
                        if (!remotePath.EndsWith("/")) remotePath += "/";
                        
                        // Normalize remoteFile path to handle different path formats
                        string normalizedRemoteFile = remoteFile.Replace('\\', '/');
                        
                        if (normalizedRemoteFile.StartsWith(remotePath, StringComparison.OrdinalIgnoreCase))
                        {
                            // Extract relative path (subfolders) from remote file path
                            string remoteRelative = normalizedRemoteFile.Substring(remotePath.Length);
                            relativePath = Path.GetDirectoryName(remoteRelative);
                            
                            if (!string.IsNullOrEmpty(relativePath))
                            {
                                relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
                                _logService.LogInfo(string.Format("Found subfolder structure: {0}", relativePath));
                            }
                        }
                        else
                        {
                            // Check if we can extract the file name if the path doesn't match exactly
                            // This can happen due to different formats of slashes or different directory representations
                            _logService.LogInfo(string.Format("Remote file path format doesn't match base path. Trying to extract filename from: {0}", normalizedRemoteFile));
                            
                            // Try to get just the filename component if it's just a filename without path
                            if (!normalizedRemoteFile.Contains("/"))
                            {
                                fileName = normalizedRemoteFile; // Use as-is if it's just a filename
                                _logService.LogInfo(string.Format("Using filename without path: {0}", fileName));
                            }
                            else
                            {
                                // Try to extract the relative path by finding the common part
                                int lastSlashPos = normalizedRemoteFile.LastIndexOf('/');
                                if (lastSlashPos >= 0)
                                {
                                    string remoteDir = normalizedRemoteFile.Substring(0, lastSlashPos + 1);
                                    fileName = normalizedRemoteFile.Substring(lastSlashPos + 1);
                                    
                                    // If remote path is a parent of the remoteDir, extract the relative path
                                    if (remoteDir.StartsWith(remotePath, StringComparison.OrdinalIgnoreCase))
                                    {
                                        relativePath = remoteDir.Substring(remotePath.Length).Replace('/', Path.DirectorySeparatorChar);
                                        if (relativePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                                            relativePath = relativePath.Substring(0, relativePath.Length - 1);
                                        _logService.LogInfo(string.Format("Extracted subfolder: {0}", relativePath));
                                    }
                                }
                            }
                            
                            _logService.LogInfo(string.Format("Will use filename: {0} with relative path: {1}", fileName, relativePath));
                        }
                        
                        // Calculate full local file path (including any subfolders)
                        string localFile;
                        if (string.IsNullOrEmpty(relativePath))
                        {
                            localFile = Path.Combine(job.FolderPath, fileName);
                        }
                        else
                        {
                            // Create subdirectory structure if needed
                            string localSubDir = Path.Combine(job.FolderPath, relativePath);
                            if (!Directory.Exists(localSubDir))
                            {
                                try
                                {
                                    Directory.CreateDirectory(localSubDir);
                                    _logService.LogInfo(string.Format("Created local subdirectory: {0}", localSubDir));
                                }
                                catch (Exception dirEx)
                                {
                                    _logService.LogError(string.Format("Error creating local subdirectory {0}: {1}", localSubDir, dirEx.Message));
                                }
                            }
                            localFile = Path.Combine(localSubDir, fileName);
                        }
                        
                        _logService.LogInfo(string.Format("Downloading: {0} -> {1}", remoteFile, localFile));
                        
                        try
                        {
                            string downloadError;
                            bool success = job.TransferClient.DownloadFile(job.ConnectionSettings, remoteFile, localFile, true, out downloadError);
                            
                            if (!success)
                            {
                                string errorMsg = string.Format("Failed to download {0}: {1}", 
                                    fileName, downloadError == null ? "Unknown error" : downloadError);
                                _logService.LogError(errorMsg);
                                failedDownloads++;
                                
                                // Don't break here - continue trying to download other files
                            }
                            else
                            {
                                _logService.LogInfo(string.Format("Successfully downloaded: {0}", fileName));
                                successfulDownloads++;
                                
                                // Get file size for statistics
                                if (File.Exists(localFile))
                                {
                                    FileInfo fileInfo = new FileInfo(localFile);
                                    totalBytes += fileInfo.Length;
                                }
                                
                                // Delete source file from remote if specified
                                if (job.DeleteSourceAfterTransfer)
                                {
                                    try
                                    {
                                        string deleteError;
                                        bool deleteSuccess = job.TransferClient.DeleteFile(job.ConnectionSettings, remoteFile, out deleteError);
                                        
                                        if (deleteSuccess)
                                        {
                                            _logService.LogInfo(string.Format("Source file deleted from remote after successful download: {0}", fileName));
                                        }
                                        else
                                        {
                                            _logService.LogWarning(string.Format("Failed to delete remote source file {0}: {1}", 
                                                fileName, deleteError ?? "Unknown error"));
                                        }
                                    }
                                    catch (Exception deleteEx)
                                    {
                                        _logService.LogWarning(string.Format("Failed to delete remote source file {0}: {1}", 
                                            fileName, deleteEx.Message));
                                    }
                                }
                            }
                        }
                        catch (Exception fileEx)
                        {
                            _logService.LogError(string.Format("Error downloading {0}: {1}", fileName, fileEx.Message));
                            failedDownloads++;
                        }
                    }
                }
                
                _logService.LogInfo(string.Format("Folder download completed: {0} files successful, {1} failed, {2} bytes transferred", 
                    successfulDownloads, failedDownloads, totalBytes));
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Error in folder download: {0}", ex.Message));
            }
        }
        
        /// <summary>
        /// Process a download timer job asynchronously
        /// </summary>
        private void ProcessDownloadTimerJob(TimerJobInfo job)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    // List remote files to download
                    List<string> remoteFiles;
                    string listError;
                    bool success;
                    
                    // We're just using the basic listing capability provided by the client
                    _logService.LogInfo(string.Format("Listing files from {0} for job {1}", 
                        job.RemotePath, job.JobId));
                    
                    success = job.TransferClient.ListFiles(job.ConnectionSettings, job.RemotePath, out remoteFiles, out listError);
                    
                    if (!success)
                    {
                        _logService.LogError(string.Format("Failed to list remote files for job {0}: {1}", 
                            job.JobId, listError ?? "Unknown error"));
                        return;
                    }
                    
                    if (remoteFiles == null || remoteFiles.Count == 0)
                    {
                        _logService.LogInfo(string.Format("No files found in remote folder {0} for job {1}", 
                            job.RemotePath, job.JobId));
                        return;
                    }
                    
                    _logService.LogInfo(string.Format("Found {0} files to download from {1} for job {2}", 
                        remoteFiles.Count, job.RemotePath, job.JobId));
                        
                    // Perform the download
                    PerformFolderDownload(job, remoteFiles.ToArray());
                    
                    // Update last job time
                    job.LastJobTime = DateTime.Now;
                    
                    TimeSpan totalDuration = DateTime.Now - job.JobStartTime.Value;
                    _logService.LogInfo(string.Format("Download cycle completed for job {0} in {1:mm\\:ss}", 
                        job.JobId, totalDuration));
                }
                catch (Exception ex)
                {
                    _logService.LogError(string.Format("Error in background download for job {0}: {1}", job.JobId, ex.Message));
                }
                finally
                {
                    // Always reset the job in progress flag
                    job.IsJobInProgress = false;
                    job.JobStartTime = null;
                }
            });
        }
    }
}
