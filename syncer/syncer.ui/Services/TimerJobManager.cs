using System;
using System.Timers;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
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
            public DateTime? LastUploadTime { get; set; }
            public DateTime? LastDownloadTime { get; set; } // Track download time
            public ITransferClient TransferClient { get; set; }
            public syncer.core.ConnectionSettings ConnectionSettings { get; set; }
            public bool IsUploadInProgress { get; set; } // Prevent overlapping uploads
            public bool IsDownloadInProgress { get; set; } // Prevent overlapping downloads
            public DateTime? UploadStartTime { get; set; } // Track upload duration
            public DateTime? DownloadStartTime { get; set; } // Track download duration
            public bool IncludeSubfolders { get; set; } // Whether to include subfolders in file enumeration
            public bool DeleteSourceAfterTransfer { get; set; } // Whether to delete source files after successful transfer
            public bool IsDownloadJob { get; set; } // Whether this is a download job (remote to local)

            // Filter settings for file filtering
            public bool EnableFilters { get; set; } // Whether file filtering is enabled
            public List<string> IncludeExtensions { get; set; } // Extensions to include (takes priority)
            public List<string> ExcludeExtensions { get; set; } // Extensions to exclude

        }

        public TimerJobManager()
        {
            _timerJobs = new Dictionary<long, TimerJobInfo>();
            _connectionService = ServiceLocator.ConnectionService;
            _logService = ServiceLocator.LogService;

        }

        public bool RegisterTimerJob(long jobId, string folderPath, string remotePath, double intervalMs)
        {
            return RegisterTimerJob(jobId, "Timer Job " + jobId, folderPath, remotePath, intervalMs, true, false); // Default to include subfolders, no delete
        }

        public bool RegisterTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs)
        {
            return RegisterTimerJob(jobId, jobName, folderPath, remotePath, intervalMs, true, false); // Default to include subfolders, no delete
        }

        public bool RegisterTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders)
        {
            return RegisterTimerJob(jobId, jobName, folderPath, remotePath, intervalMs, includeSubfolders, false); // No delete by default
        }

        public bool RegisterTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders, bool deleteSourceAfterTransfer)
        {
            return RegisterTimerJob(jobId, jobName, folderPath, remotePath, intervalMs, includeSubfolders, deleteSourceAfterTransfer, false, null, null);
        }

        public bool RegisterTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders, bool deleteSourceAfterTransfer, bool enableFilters, List<string> includeExtensions, List<string> excludeExtensions)
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
                    job.DeleteSourceAfterTransfer = deleteSourceAfterTransfer; // Update delete setting

                    // Update filter settings
                    job.EnableFilters = enableFilters;
                    job.IncludeExtensions = includeExtensions != null ? new List<string>(includeExtensions) : new List<string>();
                    job.ExcludeExtensions = excludeExtensions != null ? new List<string>(excludeExtensions) : new List<string>();

                    _logService.LogInfo(string.Format("Updated existing timer job {0} with FilterSettings - EnableFilters: {1}, Include: {2}, Exclude: {3}",
                        jobId, enableFilters,
                        includeExtensions != null ? string.Join(",", includeExtensions.ToArray()) : "none",
                        excludeExtensions != null ? string.Join(",", excludeExtensions.ToArray()) : "none"));

                    // Initialize new properties if they don't exist
                    if (!job.IsUploadInProgress) job.IsUploadInProgress = false;
                    if (job.UploadStartTime == null) job.UploadStartTime = null;

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
                    LastUploadTime = null,
                    TransferClient = transferClient,
                    ConnectionSettings = coreSettings,
                    IsUploadInProgress = false,
                    UploadStartTime = null,
                    IncludeSubfolders = includeSubfolders,
                    DeleteSourceAfterTransfer = deleteSourceAfterTransfer,
                    EnableFilters = enableFilters,
                    IncludeExtensions = includeExtensions != null ? new List<string>(includeExtensions) : new List<string>(),
                    ExcludeExtensions = excludeExtensions != null ? new List<string>(excludeExtensions) : new List<string>(),
                };

                _timerJobs.Add(jobId, newJob);

                _logService.LogInfo(string.Format("Created NEW timer job {0} with FilterSettings - EnableFilters: {1}, Include: {2}, Exclude: {3}",
                    jobId, enableFilters,
                    includeExtensions != null ? string.Join(",", includeExtensions.ToArray()) : "none",
                    excludeExtensions != null ? string.Join(",", excludeExtensions.ToArray()) : "none"));
                _logService.LogInfo(string.Format("Registered timer job {0} ({1}) for folder {2}", jobId, jobName, folderPath));
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Failed to register timer job: {0}", ex.Message));
                return false;
            }
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

            return _timerJobs[jobId].LastUploadTime;
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

            return _timerJobs[jobId].IsUploadInProgress;
        }

        /// <summary>
        /// Gets the upload start time for a timer job
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>DateTime when the current upload started, or null if not uploading</returns>
        public DateTime? GetTimerJobUploadStartTime(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return null;
            }

            return _timerJobs[jobId].UploadStartTime;
        }

        public bool UpdateTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs)
        {
            return UpdateTimerJob(jobId, jobName, folderPath, remotePath, intervalMs, true, false); // Default to include subfolders, no delete
        }

        public bool UpdateTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders)
        {
            return UpdateTimerJob(jobId, jobName, folderPath, remotePath, intervalMs, includeSubfolders, false); // No delete by default
        }

        public bool UpdateTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders, bool deleteSourceAfterTransfer)
        {
            return UpdateTimerJob(jobId, jobName, folderPath, remotePath, intervalMs, includeSubfolders, deleteSourceAfterTransfer, false, null, null);
        }

        public bool UpdateTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders, bool deleteSourceAfterTransfer, bool enableFilters, List<string> includeExtensions, List<string> excludeExtensions)
        {
            try
            {
                // Debug logging
                _logService.LogInfo(string.Format("=== UPDATING TIMER JOB {0} ===", jobId));

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
                job.DeleteSourceAfterTransfer = deleteSourceAfterTransfer;

                // Update filter settings
                job.EnableFilters = enableFilters;
                job.IncludeExtensions = includeExtensions != null ? new List<string>(includeExtensions) : new List<string>();
                job.ExcludeExtensions = excludeExtensions != null ? new List<string>(excludeExtensions) : new List<string>();

                _logService.LogInfo(string.Format("Updated timer job {0} with filters - EnableFilters: {1}, Include: {2}, Exclude: {3}",
                    jobId, enableFilters,
                    includeExtensions != null ? string.Join(",", includeExtensions.ToArray()) : "none",
                    excludeExtensions != null ? string.Join(",", excludeExtensions.ToArray()) : "none"));

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

                if (job.IsDownloadJob)
                {
                    // Handle download job (remote to local)
                    HandleDownloadJob(job, jobId);
                }
                else
                {
                    // Handle upload job (local to remote)
                    HandleUploadJob(job, jobId);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Error in OnTimerElapsed for job {0}: {1}", jobId, ex.Message));
            }
        }

        private void HandleDownloadJob(TimerJobInfo job, long jobId)
        {
            // Prevent overlapping downloads
            if (job.IsDownloadInProgress)
            {
                TimeSpan downloadDuration = DateTime.Now - (job.DownloadStartTime ?? DateTime.Now);
                _logService.LogWarning(string.Format("Skipping timer cycle for download job {0} - previous download still in progress (running for {1:mm\\:ss})",
                    jobId, downloadDuration));
                return;
            }

            _logService.LogInfo(string.Format("Timer elapsed for download job {0} - starting folder download", jobId));

            // List remote files
            List<string> remoteFiles;
            string error;
            if (!job.TransferClient.ListFiles(job.ConnectionSettings, job.RemotePath, out remoteFiles, out error))
            {
                _logService.LogError(string.Format("Failed to list remote files for job {0}: {1}", jobId, error ?? "Unknown error"));
                return;
            }

            // Apply file filtering if enabled
            string[] filteredFiles = FilterFiles(remoteFiles.ToArray(), job);

            if (filteredFiles.Length == 0)
            {
                _logService.LogInfo(string.Format("No files found (after filtering) in remote folder for download job {0}", jobId));
                return;
            }

            // Mark download as in progress and start background download
            job.IsDownloadInProgress = true;
            job.DownloadStartTime = DateTime.Now;

            // Perform the download asynchronously to prevent blocking the timer
            System.Threading.ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    PerformFolderDownload(job, filteredFiles);

                    // Update last download time
                    job.LastDownloadTime = DateTime.Now;

                    TimeSpan totalDuration = DateTime.Now - job.DownloadStartTime.Value;
                    _logService.LogInfo(string.Format("Download cycle completed for job {0} in {1:mm\\:ss}",
                        jobId, totalDuration));
                }
                catch (Exception ex)
                {
                    _logService.LogError(string.Format("Error in background download for job {0}: {1}", jobId, ex.Message));
                }
                finally
                {
                    // Always reset the download in progress flag
                    job.IsDownloadInProgress = false;
                    job.DownloadStartTime = null;
                }
            });
        }

        private void HandleUploadJob(TimerJobInfo job, long jobId)
        {
            try
            {
                // Prevent overlapping uploads
                if (job.IsUploadInProgress)
                {
                    TimeSpan uploadDuration = DateTime.Now - (job.UploadStartTime ?? DateTime.Now);
                    _logService.LogWarning(string.Format("Skipping timer cycle for job {0} - previous upload still in progress (running for {1:mm\\:ss})",
                        jobId, uploadDuration));
                    return;
                }

                _logService.LogInfo(string.Format("Timer elapsed for job {0} - starting folder upload", jobId));

                // Get files based on subfolder inclusion setting
                SearchOption searchOption = job.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                string[] allFiles = Directory.GetFiles(job.FolderPath, "*", searchOption);

                // Apply filtering if enabled
                string[] currentFiles;
                if (job.EnableFilters && (job.IncludeExtensions.Count > 0 || job.ExcludeExtensions.Count > 0))
                {
                    _logService.LogInfo(string.Format("TIMER JOB FILTER DEBUG: Applying filters - Include: {0}, Exclude: {1}",
                        string.Join(",", job.IncludeExtensions.ToArray()),
                        string.Join(",", job.ExcludeExtensions.ToArray())));

                    var filteredFiles = new List<string>();

                    foreach (string file in allFiles)
                    {
                        string fileName = Path.GetFileName(file);
                        string fileExt = Path.GetExtension(file);
                        if (!string.IsNullOrEmpty(fileExt))
                        {
                            fileExt = fileExt.TrimStart('.').ToLowerInvariant();
                        }

                        bool shouldInclude = true;

                        // Apply include filter (if any)
                        if (job.IncludeExtensions.Count > 0)
                        {
                            shouldInclude = false; // Default to exclude if include list exists
                            foreach (string includeExt in job.IncludeExtensions)
                            {
                                if (string.Equals(fileExt, includeExt.TrimStart('.').ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                                {
                                    shouldInclude = true;
                                    break;
                                }
                            }
                            _logService.LogInfo(string.Format("TIMER FILTER DEBUG: File '{0}' include check: {1}", fileName, shouldInclude));
                        }

                        // Apply exclude filter (only if not already excluded by include filter)
                        if (shouldInclude && job.ExcludeExtensions.Count > 0)
                        {
                            foreach (string excludeExt in job.ExcludeExtensions)
                            {
                                if (string.Equals(fileExt, excludeExt.TrimStart('.').ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                                {
                                    shouldInclude = false;
                                    break;
                                }
                            }
                            _logService.LogInfo(string.Format("TIMER FILTER DEBUG: File '{0}' exclude check: {1}", fileName, shouldInclude));
                        }

                        _logService.LogInfo(string.Format("TIMER FILTER DEBUG: Final decision for '{0}': {1}", fileName, shouldInclude ? "INCLUDE" : "EXCLUDE"));

                        if (shouldInclude)
                        {
                            filteredFiles.Add(file);
                        }
                    }

                    currentFiles = filteredFiles.ToArray();
                    _logService.LogInfo(string.Format("TIMER JOB FILTER RESULT: {0} files out of {1} total files match the filter criteria",
                        currentFiles.Length, allFiles.Length));
                }
                else
                {
                    // No filtering enabled - use all files
                    currentFiles = allFiles;
                    _logService.LogInfo("TIMER JOB: No file filtering applied - including all files");
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

                // Mark upload as in progress and start background upload
                job.IsUploadInProgress = true;
                job.UploadStartTime = DateTime.Now;

                // Perform the upload asynchronously to prevent blocking the timer
                System.Threading.ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        PerformFolderUpload(job, currentFiles);

                        // Update last upload time
                        job.LastUploadTime = DateTime.Now;

                        TimeSpan totalDuration = DateTime.Now - job.UploadStartTime.Value;
                        _logService.LogInfo(string.Format("Upload cycle completed for job {0} in {1:mm\\:ss}",
                            jobId, totalDuration));
                    }
                    catch (Exception ex)
                    {
                        _logService.LogError(string.Format("Error in background upload for job {0}: {1}", jobId, ex.Message));
                    }
                    finally
                    {
                        // Always reset the upload in progress flag
                        job.IsUploadInProgress = false;
                        job.UploadStartTime = null;
                    }
                });
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Error in timer job {0}: {1}", jobId, ex.Message));

                // Ensure we reset the flag even if there's an error
                if (_timerJobs.ContainsKey(jobId))
                {
                    _timerJobs[jobId].IsUploadInProgress = false;
                    _timerJobs[jobId].UploadStartTime = null;
                }
            }
        }

        /// <summary>
        /// Apply file filtering based on job settings
        /// </summary>
        private string[] FilterFiles(string[] files, TimerJobInfo job)
        {
            if (!job.EnableFilters || (job.IncludeExtensions.Count == 0 && job.ExcludeExtensions.Count == 0))
            {
                _logService.LogInfo("DOWNLOAD FILTER: No file filtering applied - including all files");
                return files;
            }

            _logService.LogInfo(string.Format("DOWNLOAD FILTER: Applying filters - Include: {0}, Exclude: {1}",
                string.Join(",", job.IncludeExtensions.ToArray()),
                string.Join(",", job.ExcludeExtensions.ToArray())));

            var filteredFiles = new List<string>();

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string fileExt = Path.GetExtension(fileName);
                if (!string.IsNullOrEmpty(fileExt))
                {
                    fileExt = fileExt.TrimStart('.').ToLowerInvariant();
                }

                bool shouldInclude = true;

                // Apply include filter (if any)
                if (job.IncludeExtensions.Count > 0)
                {
                    shouldInclude = false; // Default to exclude if include list exists
                    foreach (string includeExt in job.IncludeExtensions)
                    {
                        if (string.Equals(fileExt, includeExt.TrimStart('.').ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                        {
                            shouldInclude = true;
                            break;
                        }
                    }
                    _logService.LogInfo(string.Format("DOWNLOAD FILTER: File '{0}' include check: {1}", fileName, shouldInclude));
                }

                // Apply exclude filter (only if not already excluded by include filter)
                if (shouldInclude && job.ExcludeExtensions.Count > 0)
                {
                    foreach (string excludeExt in job.ExcludeExtensions)
                    {
                        if (string.Equals(fileExt, excludeExt.TrimStart('.').ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                        {
                            shouldInclude = false;
                            break;
                        }
                    }
                    _logService.LogInfo(string.Format("DOWNLOAD FILTER: File '{0}' exclude check: {1}", fileName, shouldInclude));
                }

                _logService.LogInfo(string.Format("DOWNLOAD FILTER: Final decision for '{0}': {1}", fileName, shouldInclude ? "INCLUDE" : "EXCLUDE"));

                if (shouldInclude)
                {
                    filteredFiles.Add(file);
                }
            }

            _logService.LogInfo(string.Format("DOWNLOAD FILTER RESULT: {0} files out of {1} total files match the filter criteria",
                filteredFiles.Count, files.Length));

            return filteredFiles.ToArray();
        }

        /// <summary>
        /// Perform folder download (remote to local) with comprehensive error handling
        /// </summary>
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
                int skippedFiles = 0;

                // Process files in smaller batches to improve responsiveness
                const int batchSize = 5; // Process 5 files at a time
                int totalBatches = (int)Math.Ceiling((double)remoteFilePaths.Length / batchSize);

                for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
                {
                    int startIndex = batchIndex * batchSize;
                    int endIndex = Math.Min(startIndex + batchSize, remoteFilePaths.Length);

                    _logService.LogInfo(string.Format("Processing download batch {0}/{1} (files {2}-{3})",
                        batchIndex + 1, totalBatches, startIndex + 1, endIndex));

                    // Process files in current batch
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        string remoteFile = remoteFilePaths[i];

                        try
                        {
                            // Calculate local file path
                            string fileName = Path.GetFileName(remoteFile);
                            string localFile;

                            if (job.IncludeSubfolders)
                            {
                                // Preserve directory structure for downloads
                                string normalizedRemotePath = job.RemotePath.Replace('\\', '/');
                                string normalizedRemoteFile = remoteFile.Replace('\\', '/');

                                string relativePath = "";
                                if (normalizedRemoteFile.StartsWith(normalizedRemotePath))
                                {
                                    relativePath = normalizedRemoteFile.Substring(normalizedRemotePath.Length);
                                    if (relativePath.StartsWith("/"))
                                        relativePath = relativePath.Substring(1);
                                }
                                else
                                {
                                    relativePath = fileName;
                                }

                                // Convert to Windows path for local storage
                                relativePath = relativePath.Replace('/', '\\');
                                localFile = Path.Combine(job.FolderPath, relativePath);
                            }
                            else
                            {
                                localFile = Path.Combine(job.FolderPath, fileName);
                            }

                            // Ensure local directory exists
                            string localDir = Path.GetDirectoryName(localFile);
                            if (!Directory.Exists(localDir))
                            {
                                Directory.CreateDirectory(localDir);
                                _logService.LogInfo(string.Format("Created local directory: {0}", localDir));
                            }

                            // Download the file
                            _logService.LogInfo(string.Format("Downloading: {0} -> {1}", remoteFile, localFile));

                            string error = null;
                            bool success = job.TransferClient.DownloadFile(job.ConnectionSettings, remoteFile, localFile, true, out error);

                            if (!success)
                            {
                                string errorMsg = string.Format("Failed to download {0}: {1}",
                                    fileName, error == null ? "Unknown error" : error);
                                _logService.LogError(errorMsg);
                                failedDownloads++;

                                // Don't break here - continue trying to download other files
                                // This ensures all files get an attempt even if some fail
                            }
                            else
                            {
                                FileInfo localFileInfo = new FileInfo(localFile);
                                _logService.LogInfo(string.Format("Successfully downloaded: {0} ({1} bytes)", fileName, localFileInfo.Length));
                                successfulDownloads++;

                                // Check if we should delete source file after successful download
                                if (job.DeleteSourceAfterTransfer)
                                {
                                    try
                                    {
                                        string deleteError;
                                        if (job.TransferClient.DeleteFile(job.ConnectionSettings, remoteFile, out deleteError))
                                        {
                                            _logService.LogInfo(string.Format("Source file deleted after successful download: {0}", remoteFile));
                                        }
                                        else
                                        {
                                            _logService.LogError(string.Format("Failed to delete remote source file {0} after download: {1}", remoteFile, deleteError));
                                        }
                                    }
                                    catch (Exception deleteEx)
                                    {
                                        _logService.LogError(string.Format("Failed to delete remote source file {0} after download: {1}", remoteFile, deleteEx.Message));
                                    }
                                }

                                // Verify download by checking if local file exists (optional)
                                try
                                {
                                    if (File.Exists(localFile))
                                    {
                                        _logService.LogInfo(string.Format("Download verified: {0} exists locally", fileName));
                                    }
                                    else
                                    {
                                        _logService.LogWarning(string.Format("Download completed but file not found locally: {0}", fileName));
                                    }
                                }
                                catch (Exception verifyEx)
                                {
                                    _logService.LogWarning(string.Format("Could not verify download of {0}: {1}", fileName, verifyEx.Message));
                                }
                            }
                        }
                        catch (Exception fileEx)
                        {
                            _logService.LogError(string.Format("Error downloading file: {0}", fileEx.Message));
                            failedDownloads++;
                        }
                    }

                    // Small delay between batches to prevent overwhelming the server
                    if (batchIndex < totalBatches - 1) // Don't delay after the last batch
                    {
                        System.Threading.Thread.Sleep(100); // 100ms delay between batches
                    }
                }

                _logService.LogInfo(string.Format("Folder download completed for job {0}. Results: {1} successful, {2} failed, {3} skipped out of {4} total files",
                    job.JobId, successfulDownloads, failedDownloads, skippedFiles, remoteFilePaths.Length));
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Error in folder download for job {0}: {1}", job.JobId, ex.Message));
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

                                // Check if we should delete source file after successful upload
                                if (job.DeleteSourceAfterTransfer)
                                {
                                    try
                                    {
                                        File.Delete(localFile);
                                        _logService.LogInfo(string.Format("Source file deleted after successful upload: {0}", localFile));
                                    }
                                    catch (Exception deleteEx)
                                    {
                                        _logService.LogError(string.Format("Failed to delete source file {0} after upload: {1}", localFile, deleteEx.Message));
                                    }
                                }

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
        
        // Download-specific methods implementation
        
        public bool RegisterDownloadTimerJob(long jobId, string jobName, string remoteFolderPath, string localDestinationPath, double intervalMs, bool includeSubfolders, bool deleteSourceAfterTransfer, bool enableFilters, List<string> includeExtensions, List<string> excludeExtensions)
        {
            try
            {
                // Check if the job already exists
                if (_timerJobs.ContainsKey(jobId))
                {
                    // Update existing job for download
                    var job = _timerJobs[jobId];
                    job.JobName = jobName ?? ("Download Job " + jobId);
                    job.RemotePath = remoteFolderPath; // Source is remote for downloads
                    job.FolderPath = localDestinationPath; // Destination is local for downloads
                    job.IntervalMs = intervalMs;
                    job.IncludeSubfolders = includeSubfolders;
                    job.DeleteSourceAfterTransfer = deleteSourceAfterTransfer;
                    job.IsDownloadJob = true; // Mark as download job
                    
                    // Update filter settings
                    job.EnableFilters = enableFilters;
                    job.IncludeExtensions = includeExtensions != null ? new List<string>(includeExtensions) : new List<string>();
                    job.ExcludeExtensions = excludeExtensions != null ? new List<string>(excludeExtensions) : new List<string>();
                    
                    _logService.LogInfo(string.Format("Updated existing download timer job {0} with FilterSettings - EnableFilters: {1}, Include: {2}, Exclude: {3}", 
                        jobId, enableFilters, 
                        includeExtensions != null ? string.Join(",", includeExtensions.ToArray()) : "none",
                        excludeExtensions != null ? string.Join(",", excludeExtensions.ToArray()) : "none"));
                    
                    // Initialize download-specific properties if they don't exist
                    if (!job.IsDownloadInProgress) job.IsDownloadInProgress = false;
                    if (job.DownloadStartTime == null) job.DownloadStartTime = null;
                    
                    // Update timer interval if running
                    if (job.Timer != null && job.IsRunning)
                    {
                        job.Timer.Interval = intervalMs;
                    }
                    
                    _logService.LogInfo(string.Format("Updated download timer job {0} ({1}) for remote folder {2} to local {3}", jobId, jobName, remoteFolderPath, localDestinationPath));
                    return true;
                }
                
                // Create a new download job
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
                
                // Create the download job
                var newJob = new TimerJobInfo
                {
                    JobId = jobId,
                    JobName = jobName ?? ("Download Job " + jobId),
                    RemotePath = remoteFolderPath, // Source is remote for downloads
                    FolderPath = localDestinationPath, // Destination is local for downloads
                    IntervalMs = intervalMs,
                    IsRunning = false,
                    LastDownloadTime = null,
                    TransferClient = transferClient,
                    ConnectionSettings = coreSettings,
                    IsDownloadInProgress = false,
                    DownloadStartTime = null,
                    IncludeSubfolders = includeSubfolders,
                    DeleteSourceAfterTransfer = deleteSourceAfterTransfer,
                    IsDownloadJob = true, // Mark as download job
                    EnableFilters = enableFilters,
                    IncludeExtensions = includeExtensions != null ? new List<string>(includeExtensions) : new List<string>(),
                    ExcludeExtensions = excludeExtensions != null ? new List<string>(excludeExtensions) : new List<string>(),
                };
                
                _timerJobs.Add(jobId, newJob);
                
                _logService.LogInfo(string.Format("Created NEW download timer job {0} with FilterSettings - EnableFilters: {1}, Include: {2}, Exclude: {3}", 
                    jobId, enableFilters, 
                    includeExtensions != null ? string.Join(",", includeExtensions.ToArray()) : "none",
                    excludeExtensions != null ? string.Join(",", excludeExtensions.ToArray()) : "none"));
                _logService.LogInfo(string.Format("Registered download timer job {0} ({1}) for remote folder {2} to local {3}", jobId, jobName, remoteFolderPath, localDestinationPath));
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Failed to register download timer job: {0}", ex.Message));
                return false;
            }
        }
        
        public DateTime? GetLastDownloadTime(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return null;
            }
            
            return _timerJobs[jobId].LastDownloadTime;
        }
        
        public bool IsTimerJobDownloading(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return false;
            }
            
            return _timerJobs[jobId].IsDownloadInProgress;
        }
        
        public DateTime? GetTimerJobDownloadStartTime(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return null;
            }
            
            return _timerJobs[jobId].DownloadStartTime;
        }
        
        public bool IsTimerJobDownloadJob(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return false;
            }
            
            return _timerJobs[jobId].IsDownloadJob;
        }
        
        public bool GetTimerJobIncludeSubfolders(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return true; // Default value
            }
            
            return _timerJobs[jobId].IncludeSubfolders;
        }
        
        public bool GetTimerJobDeleteSourceAfterTransfer(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return false; // Default value
            }
            
            return _timerJobs[jobId].DeleteSourceAfterTransfer;
        }
        
        public bool GetTimerJobEnableFilters(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return false; // Default value
            }
            
            return _timerJobs[jobId].EnableFilters;
        }
        
        public List<string> GetTimerJobIncludeExtensions(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return new List<string>(); // Default empty list
            }
            
            return _timerJobs[jobId].IncludeExtensions ?? new List<string>();
        }
        
        public List<string> GetTimerJobExcludeExtensions(long jobId)
        {
            if (!_timerJobs.ContainsKey(jobId))
            {
                return new List<string>(); // Default empty list
            }
            
            return _timerJobs[jobId].ExcludeExtensions ?? new List<string>();
        }
    }
}