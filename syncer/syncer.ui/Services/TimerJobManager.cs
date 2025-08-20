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
            public string FolderPath { get; set; }
            public string RemotePath { get; set; }
            public System.Timers.Timer Timer { get; set; }
            public double IntervalMs { get; set; }
            public bool IsRunning { get; set; }
            public DateTime? LastUploadTime { get; set; }
            public ITransferClient TransferClient { get; set; }
            public syncer.core.ConnectionSettings ConnectionSettings { get; set; }
        }
        
        public TimerJobManager()
        {
            _timerJobs = new Dictionary<long, TimerJobInfo>();
            _connectionService = ServiceLocator.ConnectionService;
            _logService = ServiceLocator.LogService;
        }
        
        public bool RegisterTimerJob(long jobId, string folderPath, string remotePath, double intervalMs)
        {
            try
            {
                // Check if the job already exists
                if (_timerJobs.ContainsKey(jobId))
                {
                    // Update existing job
                    var job = _timerJobs[jobId];
                    job.FolderPath = folderPath;
                    job.RemotePath = remotePath;
                    job.IntervalMs = intervalMs;
                    
                    // Update timer interval if running
                    if (job.Timer != null && job.IsRunning)
                    {
                        job.Timer.Interval = intervalMs;
                    }
                    
                    _logService.LogInfo(string.Format("Updated timer job {0} for folder {1}", jobId, folderPath));
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
                    FolderPath = folderPath,
                    RemotePath = remotePath,
                    IntervalMs = intervalMs,
                    IsRunning = false,
                    LastUploadTime = null,
                    TransferClient = transferClient,
                    ConnectionSettings = coreSettings
                };
                
                _timerJobs.Add(jobId, newJob);
                
                _logService.LogInfo(string.Format("Registered timer job {0} for folder {1}", jobId, folderPath));
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
        
        private void OnTimerElapsed(long jobId)
        {
            try
            {
                if (!_timerJobs.ContainsKey(jobId))
                {
                    return;
                }
                
                TimerJobInfo job = _timerJobs[jobId];
                
                _logService.LogInfo(string.Format("Timer elapsed for job {0} - starting folder upload", jobId));
                
                // Get all files in the directory, including any newly added files
                string[] currentFiles = Directory.GetFiles(job.FolderPath, "*", SearchOption.AllDirectories);
                
                if (currentFiles.Length == 0)
                {
                    _logService.LogWarning(string.Format("No files found in folder {0} for timer job {1}", job.FolderPath, jobId));
                    return;
                }
                
                // Perform the upload
                PerformFolderUpload(job, currentFiles);
                
                // Update last upload time
                job.LastUploadTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Error in timer job {0}: {1}", jobId, ex.Message));
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
                
                // Dictionary to track created remote folders
                Dictionary<string, bool> createdRemoteFolders = new Dictionary<string, bool>();
                
                foreach (string localFile in localFilePaths)
                {
                    try
                    {
                        // Skip if the file doesn't exist
                        if (!File.Exists(localFile))
                        {
                            _logService.LogWarning(string.Format("File does not exist: {0}", localFile));
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
                        }
                        else
                        {
                            _logService.LogInfo(string.Format("Successfully uploaded: {0}", fileName));
                        }
                    }
                    catch (Exception fileEx)
                    {
                        _logService.LogError(string.Format("Error uploading file: {0}", fileEx.Message));
                    }
                }
                
                _logService.LogInfo(string.Format("Folder upload completed for job {0}", job.JobId));
            }
            catch (Exception ex)
            {
                _logService.LogError(string.Format("Error in folder upload for job {0}: {1}", job.JobId, ex.Message));
            }
        }
    }
}
