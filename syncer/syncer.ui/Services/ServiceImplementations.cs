using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Timers;
using System.Threading;
using System.Text.RegularExpressions;

namespace syncer.ui.Services
{
    // Full implementation of sync job service with actual scheduling functionality
    public class SyncJobService : ISyncJobService
    {
        private List<SyncJob> _jobs;
        private Dictionary<int, System.Timers.Timer> _jobTimers;
        private static readonly string JobsStateFilePath = Path.Combine(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FTPSyncer"),
            "SyncJobsState.xml");

        public SyncJobService()
        {
            _jobs = new List<SyncJob>();
            _jobTimers = new Dictionary<int, System.Timers.Timer>();
            
            // Restore jobs from previous session
            LoadJobsFromPersistence();
            
            ServiceLocator.LogService.LogInfo("SyncJobService initialized with scheduling support");
        }

        public List<SyncJob> GetAllJobs()
        {
            return new List<SyncJob>(_jobs);
        }

        public SyncJob GetJobById(int id)
        {
            for (int i = 0; i < _jobs.Count; i++)
            {
                if (_jobs[i].Id == id) return _jobs[i];
            }
            return null;
        }

        public int CreateJob(SyncJob job)
        {
            job.Id = _jobs.Count > 0 ? _jobs[_jobs.Count - 1].Id + 1 : 1;
            job.CreatedDate = DateTime.Now;
            _jobs.Add(job);
            
            // Schedule the job if it's enabled
            if (job.IsEnabled)
            {
                ScheduleJob(job);
            }
            
            // Save jobs to persistence
            SaveJobsToPersistence();
            
            ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' created with ID {1}", job.Name, job.Id));
            return job.Id;
        }

        public bool UpdateJob(SyncJob job)
        {
            SyncJob existing = GetJobById(job.Id);
            if (existing == null) return false;
            
            // Stop existing timer if any
            StopJobTimer(job.Id);
            
            int idx = _jobs.IndexOf(existing);
            _jobs[idx] = job;
            
            // Reschedule if enabled
            if (job.IsEnabled)
            {
                ScheduleJob(job);
            }
            
            // Save jobs to persistence
            SaveJobsToPersistence();
            
            ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' updated", job.Name));
            return true;
        }

        public bool DeleteJob(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return false;
            
            // Stop timer first
            StopJobTimer(id);
            
            _jobs.Remove(job);
            
            // Save jobs to persistence
            SaveJobsToPersistence();
            
            ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' deleted", job.Name));
            return true;
        }

        public bool StartJob(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return false;
            
            job.IsEnabled = true;
            ScheduleJob(job);
            
            // Save jobs to persistence
            SaveJobsToPersistence();
            
            ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' started", job.Name));
            return true;
        }

        public bool StopJob(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return false;
            
            job.IsEnabled = false;
            StopJobTimer(id);
            
            // Save jobs to persistence
            SaveJobsToPersistence();
            
            ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' stopped", job.Name));
            return true;
        }

        public string GetJobStatus(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return "Not Found";
            
            if (!job.IsEnabled) return "Disabled";
            if (_jobTimers.ContainsKey(id)) return "Scheduled";
            return "Enabled";
        }

        private void ScheduleJob(SyncJob job)
        {
            if (job == null || !job.IsEnabled) return;
            
            // Stop existing timer
            StopJobTimer(job.Id);
            
            // Calculate next run time
            DateTime nextRun = CalculateNextRunTime(job);
            double millisecondsToNextRun = (nextRun - DateTime.Now).TotalMilliseconds;
            
            if (millisecondsToNextRun <= 0)
            {
                // If time has passed, schedule for the next interval
                millisecondsToNextRun = GetIntervalInMilliseconds(job);
            }
            
            System.Timers.Timer timer = new System.Timers.Timer(millisecondsToNextRun);
            timer.Elapsed += (sender, e) => OnTimerElapsed(job.Id);
            timer.AutoReset = false; // Single shot, we'll reschedule after execution
            timer.Start();
            
            _jobTimers[job.Id] = timer;
            
            ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' scheduled to run in {1} minutes", 
                job.Name, (int)(millisecondsToNextRun / 60000)));
        }

        private void StopJobTimer(int jobId)
        {
            if (_jobTimers.ContainsKey(jobId))
            {
                _jobTimers[jobId].Stop();
                _jobTimers[jobId].Dispose();
                _jobTimers.Remove(jobId);
            }
        }

        private void OnTimerElapsed(int jobId)
        {
            SyncJob job = GetJobById(jobId);
            if (job == null || !job.IsEnabled) return;
            
            try
            {
                ServiceLocator.LogService.LogInfo(string.Format("Executing scheduled job: {0}", job.Name));
                
                // Execute the job
                ExecuteJob(job);
                
                // Update last run time and status
                job.LastRun = DateTime.Now;
                job.LastStatus = "Completed Successfully";
                
                // Reschedule for next run
                if (job.IsEnabled)
                {
                    ScheduleJob(job);
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError(string.Format("Error executing job '{0}': {1}", job.Name, ex.Message));
                job.LastRun = DateTime.Now;
                job.LastStatus = "Failed: " + ex.Message;
                
                // Still reschedule for next attempt
                if (job.IsEnabled)
                {
                    ScheduleJob(job);
                }
            }
        }

        private void ExecuteJob(SyncJob job)
        {
            ServiceLocator.LogService.LogInfo(string.Format("Executing job '{0}': {1} -> {2}", 
                job.Name, job.SourcePath, job.DestinationPath));
            
            try
            {
                // Check transfer mode first, then fall back to connection-based logic
                if (job.TransferMode == "Download")
                {
                    // Explicit download transfer
                    ExecuteDownloadTransfer(job);
                }
                else if (job.TransferMode == "Upload")
                {
                    // Explicit upload transfer
                    ExecuteUploadTransfer(job);
                }
                else
                {
                    // Legacy logic based on connection settings
                    var sourceConnection = job.SourceConnection ?? new ConnectionSettings();
                    var destConnection = job.DestinationConnection ?? new ConnectionSettings();
                    
                    // For local transfers, create appropriate transfer clients
                    if (sourceConnection.Protocol == "LOCAL" && destConnection.Protocol == "LOCAL")
                    {
                        // Local to local transfer
                        ExecuteLocalToLocalTransfer(job);
                    }
                    else if (sourceConnection.Protocol == "LOCAL")
                    {
                        // Local to remote upload
                        ExecuteUploadTransfer(job);
                    }
                    else if (destConnection.Protocol == "LOCAL")
                    {
                        // Remote to local download
                        ExecuteDownloadTransfer(job);
                    }
                    else
                    {
                        // Remote to remote transfer
                        ExecuteRemoteToRemoteTransfer(job);
                    }
                }
                
                ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' executed successfully", job.Name));
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError(string.Format("Job '{0}' execution failed: {1}", job.Name, ex.Message));
                throw;
            }
        }

        private void ExecuteLocalToLocalTransfer(SyncJob job)
        {
            if (!Directory.Exists(job.SourcePath))
            {
                throw new Exception(string.Format("Source directory does not exist: {0}", job.SourcePath));
            }

            if (!Directory.Exists(job.DestinationPath))
            {
                Directory.CreateDirectory(job.DestinationPath);
            }

            // Get all files without filtering
            List<string> files = GetFilteredFiles(job.SourcePath, job.IncludeSubFolders);

            // If no files found, log it but don't treat as error (empty directory is valid)
            if (files.Count == 0)
            {
                ServiceLocator.LogService.LogInfo(string.Format("No files found in source directory (or all files filtered out): {0}", job.SourcePath));
                return; // Exit gracefully, nothing to sync
            }

            foreach (string sourceFile in files)
            {
                try
                {
                    string relativePath = sourceFile.Substring(job.SourcePath.Length + 1);
                    string destFile = Path.Combine(job.DestinationPath, relativePath);
                    
                    string destDir = Path.GetDirectoryName(destFile);
                    if (!Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    if (job.OverwriteExisting || !File.Exists(destFile))
                    {
                        File.Copy(sourceFile, destFile, true);
                        ServiceLocator.LogService.LogInfo(string.Format("Copied: {0} -> {1}", sourceFile, destFile));
                        
                        // Delete source file after successful transfer if requested
                        if (job.DeleteSourceAfterTransfer)
                        {
                            try
                            {
                                File.Delete(sourceFile);
                                ServiceLocator.LogService.LogInfo(string.Format("Source file deleted after successful transfer: {0}", sourceFile));
                            }
                            catch (Exception deleteEx)
                            {
                                ServiceLocator.LogService.LogError(string.Format("Failed to delete source file {0} after transfer: {1}", sourceFile, deleteEx.Message));
                                // Don't fail the entire transfer just because delete failed
                            }
                        }
                    }
                    else
                    {
                        ServiceLocator.LogService.LogInfo(string.Format("Skipped (exists): {0}", destFile));
                    }
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogError(string.Format("Failed to copy file {0}: {1}", sourceFile, ex.Message));
                }
            }
        }

        private List<string> GetFilteredFiles(string sourcePath, bool includeSubFolders)
        {
            var allFiles = Directory.GetFiles(sourcePath, "*", 
                includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            
            // Return all files (no filtering)
            return new List<string>(allFiles);
        }

        private void ExecuteUploadTransfer(SyncJob job)
        {
            try
            {
                ServiceLocator.LogService.LogInfo(string.Format("Starting upload transfer job: {0}", job.Name));
                
                // Create appropriate transfer client based on destination protocol
                syncer.core.ITransferClient transferClient = null;
                var destConnection = job.DestinationConnection;
                
                if (destConnection == null)
                {
                    throw new Exception("Destination connection settings are missing");
                }
                
                // Initialize transfer client based on protocol
                switch (destConnection.Protocol.ToUpper())
                {
                    case "FTP":
                        transferClient = new syncer.core.Transfers.EnhancedFtpTransferClient();
                        break;
                    case "SFTP":
                        transferClient = new syncer.core.Transfers.ProductionSftpTransferClient();
                        break;
                    default:
                        throw new Exception(string.Format("Unsupported destination protocol: {0}", destConnection.Protocol));
                }
                
                // Convert UI connection settings to core connection settings
                var coreDestConnection = ConvertToConnectionSettings(destConnection);
                
                // Test connection first
                string testError;
                if (!transferClient.TestConnection(coreDestConnection, out testError))
                {
                    throw new Exception(string.Format("Connection test failed: {0}", testError));
                }
                
                // Get files to upload (no filtering)
                List<string> filesToUploadList;
                if (Directory.Exists(job.SourcePath))
                {
                    // Upload directory contents
                    filesToUploadList = GetFilteredFiles(job.SourcePath, job.IncludeSubFolders);
                }
                else if (File.Exists(job.SourcePath))
                {
                    // Upload single file
                    filesToUploadList = new List<string> { job.SourcePath };
                }
                else
                {
                    throw new Exception(string.Format("Source path does not exist: {0}", job.SourcePath));
                }
                
                string[] filesToUpload = filesToUploadList.ToArray();
                
                ServiceLocator.LogService.LogInfo(string.Format("Found {0} files to upload", filesToUpload.Length));
                
                int successCount = 0;
                long totalBytes = 0;
                
                foreach (string localFile in filesToUpload)
                {
                    try
                    {
                        // Calculate relative remote path
                        string relativePath = job.IncludeSubFolders ? 
                            localFile.Substring(job.SourcePath.Length).TrimStart('\\', '/') :
                            Path.GetFileName(localFile);
                        
                        string remoteFile = job.DestinationPath.TrimEnd('/', '\\') + "/" + relativePath.Replace('\\', '/');
                        
                        ServiceLocator.LogService.LogInfo(string.Format("Uploading: {0} -> {1}", localFile, remoteFile));
                        
                        string uploadError;
                        if (transferClient.UploadFile(coreDestConnection, localFile, remoteFile, job.OverwriteExisting, out uploadError))
                        {
                            successCount++;
                            totalBytes += new FileInfo(localFile).Length;
                            ServiceLocator.LogService.LogInfo(string.Format("Successfully uploaded: {0}", Path.GetFileName(localFile)));
                        }
                        else
                        {
                            ServiceLocator.LogService.LogError(string.Format("Failed to upload {0}: {1}", Path.GetFileName(localFile), uploadError));
                        }
                    }
                    catch (Exception fileEx)
                    {
                        ServiceLocator.LogService.LogError(string.Format("Error uploading {0}: {1}", Path.GetFileName(localFile), fileEx.Message));
                    }
                }
                
                // Update job statistics
                job.LastFileCount = successCount;
                job.LastTransferSize = totalBytes;
                
                if (successCount == filesToUpload.Length)
                {
                    ServiceLocator.LogService.LogInfo(string.Format("Upload job completed successfully. Uploaded {0} files ({1} bytes)", successCount, totalBytes));
                }
                else
                {
                    throw new Exception(string.Format("Upload partially failed. {0} of {1} files uploaded successfully", successCount, filesToUpload.Length));
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError(string.Format("Upload transfer job failed: {0}", ex.Message));
                throw; // Re-throw to update job status
            }
        }

        private syncer.core.ConnectionSettings ConvertToConnectionSettings(ConnectionSettings uiSettings)
        {
            return new syncer.core.ConnectionSettings
            {
                Protocol = ConvertProtocolType(uiSettings.Protocol),
                Host = uiSettings.Host,
                Port = uiSettings.Port,
                Username = uiSettings.Username,
                Password = uiSettings.Password,
                UsePassiveMode = uiSettings.UsePassiveMode,
                SshKeyPath = uiSettings.SshKeyPath,
                Timeout = uiSettings.Timeout
            };
        }
        
        private syncer.core.ProtocolType ConvertProtocolType(string protocol)
        {
            switch (protocol.ToUpper())
            {
                case "LOCAL": return syncer.core.ProtocolType.Local;
                case "FTP": return syncer.core.ProtocolType.Ftp;
                case "SFTP": return syncer.core.ProtocolType.Sftp;
                default: return syncer.core.ProtocolType.Local;
            }
        }

        private void ExecuteDownloadTransfer(SyncJob job)
        {
            try
            {
                ServiceLocator.LogService.LogInfo(string.Format("Starting download transfer job: {0}", job.Name));
                
                // Create appropriate transfer client based on source protocol
                syncer.core.ITransferClient transferClient = null;
                var sourceConnection = job.SourceConnection;
                
                if (sourceConnection == null)
                {
                    throw new Exception("Source connection settings are missing");
                }
                
                // Initialize transfer client based on protocol
                switch (sourceConnection.Protocol.ToUpper())
                {
                    case "FTP":
                        transferClient = new syncer.core.Transfers.EnhancedFtpTransferClient();
                        break;
                    case "SFTP":
                        transferClient = new syncer.core.Transfers.ProductionSftpTransferClient();
                        break;
                    case "LOCAL":
                        transferClient = new syncer.core.LocalTransferClient();
                        break;
                    default:
                        throw new Exception(string.Format("Unsupported source protocol: {0}", sourceConnection.Protocol));
                }
                
                // Convert UI connection settings to core connection settings
                var coreSourceConnection = ConvertToConnectionSettings(sourceConnection);
                
                // Test connection first
                string testError;
                if (!transferClient.TestConnection(coreSourceConnection, out testError))
                {
                    throw new Exception(string.Format("Connection test failed: {0}", testError));
                }
                
                // Get files to download from remote source
                List<string> filesToDownload;
                string listError;
                
                if (!transferClient.ListFiles(coreSourceConnection, job.SourcePath, out filesToDownload, out listError))
                {
                    throw new Exception(string.Format("Failed to list remote files: {0}", listError));
                }
                
                if (filesToDownload.Count == 0)
                {
                    ServiceLocator.LogService.LogInfo("No files found to download");
                    job.LastFileCount = 0;
                    job.LastTransferSize = 0;
                    return;
                }
                
                ServiceLocator.LogService.LogInfo(string.Format("Found {0} files to download", filesToDownload.Count));
                
                // Ensure destination directory exists
                if (!Directory.Exists(job.DestinationPath))
                {
                    Directory.CreateDirectory(job.DestinationPath);
                    ServiceLocator.LogService.LogInfo(string.Format("Created destination directory: {0}", job.DestinationPath));
                }
                
                int successCount = 0;
                long totalBytes = 0;
                
                foreach (string remoteFile in filesToDownload)
                {
                    try
                    {
                        // Calculate local destination path
                        string fileName = Path.GetFileName(remoteFile);
                        if (string.IsNullOrEmpty(fileName)) continue;
                        
                        string localFile = Path.Combine(job.DestinationPath, fileName);
                        
                        ServiceLocator.LogService.LogInfo(string.Format("Downloading: {0} -> {1}", remoteFile, localFile));
                        
                        string downloadError;
                        if (transferClient.DownloadFile(coreSourceConnection, remoteFile, localFile, job.OverwriteExisting, out downloadError))
                        {
                            successCount++;
                            if (File.Exists(localFile))
                            {
                                totalBytes += new FileInfo(localFile).Length;
                            }
                            ServiceLocator.LogService.LogInfo(string.Format("Successfully downloaded: {0}", fileName));
                            
                            // Delete source file if requested
                            if (job.DeleteSourceAfterTransfer)
                            {
                                string deleteError;
                                if (transferClient.DeleteFile(coreSourceConnection, remoteFile, out deleteError))
                                {
                                    ServiceLocator.LogService.LogInfo(string.Format("Source file deleted after download: {0}", fileName));
                                }
                                else
                                {
                                    ServiceLocator.LogService.LogWarning(string.Format("Failed to delete source file {0}: {1}", fileName, deleteError));
                                }
                            }
                        }
                        else
                        {
                            ServiceLocator.LogService.LogError(string.Format("Failed to download {0}: {1}", fileName, downloadError));
                        }
                    }
                    catch (Exception fileEx)
                    {
                        ServiceLocator.LogService.LogError(string.Format("Error downloading {0}: {1}", Path.GetFileName(remoteFile), fileEx.Message));
                    }
                }
                
                // Update job statistics
                job.LastFileCount = successCount;
                job.LastTransferSize = totalBytes;
                
                if (successCount == filesToDownload.Count)
                {
                    ServiceLocator.LogService.LogInfo(string.Format("Download job completed successfully. Downloaded {0} files ({1} bytes)", successCount, totalBytes));
                }
                else if (successCount > 0)
                {
                    ServiceLocator.LogService.LogWarning(string.Format("Download partially completed. {0} of {1} files downloaded successfully", successCount, filesToDownload.Count));
                }
                else
                {
                    throw new Exception("Download job failed - no files were downloaded successfully");
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError(string.Format("Download transfer job failed: {0}", ex.Message));
                
                // Update job statistics to reflect the error
                if (job != null)
                {
                    job.LastRun = DateTime.Now;
                    job.LastStatus = "Error: " + ex.Message;
                    job.LastFileCount = 0;
                    job.LastTransferSize = 0;
                }
                throw; // Re-throw to update job status
            }
        }

        private void ExecuteRemoteToRemoteTransfer(SyncJob job)
        {
            // This would require access to transfer clients
            // For now, just log the operation  
            ServiceLocator.LogService.LogInfo(string.Format("Remote-to-remote transfer from {0} to {1} (not fully implemented)", 
                job.SourcePath, job.DestinationPath));
        }

        private DateTime CalculateNextRunTime(SyncJob job)
        {
            DateTime baseTime = job.LastRun.HasValue && job.LastRun.Value != DateTime.MinValue 
                ? job.LastRun.Value 
                : job.StartTime;
                
            if (baseTime < DateTime.Now.AddDays(-1))
            {
                baseTime = DateTime.Now;
            }
            
            switch (job.IntervalType)
            {
                case "Minutes":
                    return baseTime.AddMinutes(job.IntervalValue);
                case "Hours":
                    return baseTime.AddHours(job.IntervalValue);
                case "Days":
                    return baseTime.AddDays(job.IntervalValue);
                default:
                    return baseTime.AddMinutes(job.IntervalValue);
            }
        }

        private double GetIntervalInMilliseconds(SyncJob job)
        {
            switch (job.IntervalType?.ToLower())
            {
                case "seconds":
                    return job.IntervalValue * 1000;
                case "minutes":
                    return job.IntervalValue * 60 * 1000;
                case "hours":
                    return job.IntervalValue * 60 * 60 * 1000;
                case "days":
                    return job.IntervalValue * 24 * 60 * 60 * 1000;
                default:
                    return job.IntervalValue * 60 * 1000; // Default to minutes
            }
        }
        
        // Public methods for service manager to control the scheduler
        public void StartScheduler()
        {
            try
            {
                // Start all enabled jobs that have scheduling configured
                foreach (var job in _jobs)
                {
                    if (job.IsEnabled && job.IntervalValue > 0 && !string.IsNullOrEmpty(job.IntervalType))
                    {
                        ScheduleJob(job);
                    }
                }
                ServiceLocator.LogService.LogInfo("Job scheduler started");
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Failed to start scheduler: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Load jobs from persistent storage on startup
        /// </summary>
        private void LoadJobsFromPersistence()
        {
            try
            {
                if (!File.Exists(JobsStateFilePath))
                {
                    ServiceLocator.LogService.LogInfo("No saved sync jobs state found - starting fresh");
                    return;
                }

                string xmlContent = File.ReadAllText(JobsStateFilePath);
                if (StringExtensions.IsNullOrWhiteSpace(xmlContent))
                {
                    ServiceLocator.LogService.LogInfo("Sync jobs state file is empty - starting fresh");
                    return;
                }

                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<SyncJob>));
                using (var reader = new StringReader(xmlContent))
                {
                    var restoredJobs = (List<SyncJob>)serializer.Deserialize(reader);
                    if (restoredJobs != null && restoredJobs.Count > 0)
                    {
                        _jobs.AddRange(restoredJobs);
                        
                        // Restore enabled jobs with scheduling
                        int restoredCount = 0;
                        foreach (var job in restoredJobs)
                        {
                            if (job.IsEnabled)
                            {
                                try
                                {
                                    ScheduleJob(job);
                                    restoredCount++;
                                }
                                catch (Exception scheduleEx)
                                {
                                    ServiceLocator.LogService.LogError(string.Format("Failed to reschedule job '{0}': {1}", job.Name, scheduleEx.Message));
                                }
                            }
                        }
                        
                        ServiceLocator.LogService.LogInfo(string.Format("Restored {0} sync jobs from previous session ({1} scheduled)", 
                            restoredJobs.Count, restoredCount));
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Failed to load sync jobs from persistence: " + ex.Message);
            }
        }

        /// <summary>
        /// Save jobs to persistent storage
        /// </summary>
        private void SaveJobsToPersistence()
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(JobsStateFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<SyncJob>));
                using (var writer = new StreamWriter(JobsStateFilePath))
                {
                    serializer.Serialize(writer, _jobs);
                }
                
                ServiceLocator.LogService.LogInfo(string.Format("Saved {0} sync jobs to persistent storage", _jobs.Count));
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Failed to save sync jobs to persistence: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Manually force jobs to be reloaded from persistence - useful for recovery scenarios
        /// </summary>
        public void ReloadJobsFromPersistence()
        {
            try
            {
                // Stop all current timers
                foreach (var kvp in _jobTimers)
                {
                    try
                    {
                        kvp.Value.Stop();
                        kvp.Value.Dispose();
                    }
                    catch { }
                }
                _jobTimers.Clear();
                
                // Clear current jobs
                _jobs.Clear();
                
                // Reload from persistence
                LoadJobsFromPersistence();
                
                ServiceLocator.LogService.LogInfo("Jobs manually reloaded from persistence for recovery");
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Failed to reload jobs from persistence: " + ex.Message);
            }
        }
        
        public void StopScheduler()
        {
            try
            {
                // Save jobs before stopping
                SaveJobsToPersistence();
                
                // Stop all job timers
                foreach (var kvp in _jobTimers)
                {
                    try
                    {
                        kvp.Value.Stop();
                        kvp.Value.Dispose();
                    }
                    catch { }
                }
                _jobTimers.Clear();
                ServiceLocator.LogService.LogInfo("Job scheduler stopped and jobs saved");
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Failed to stop scheduler: " + ex.Message);
            }
        }
    }

    // Enhanced connection service with persistent storage
    public class ConnectionService : IConnectionService
    {
        private ConnectionSettings _settings;

        public ConnectionService()
        {
            // Try to load default connection on startup
            _settings = LoadDefaultConnectionFromRegistry() ?? new ConnectionSettings();
            
            // Ensure connection is properly restored for restart scenarios
            if (_settings != null && !StringExtensions.IsNullOrWhiteSpace(_settings.Host))
            {
                // Mark as connected if we have valid connection details
                _settings.IsConnected = true;
                ServiceLocator.LogService.LogInfo(string.Format("Connection settings loaded and marked as connected on startup: {0}@{1}:{2}", 
                    _settings.Username, _settings.Host, _settings.Port));
            }
            else
            {
                ServiceLocator.LogService.LogWarning("No valid connection settings found for startup restoration");
            }
        }

        public ConnectionSettings GetConnectionSettings()
        {
            return _settings;
        }

        public bool SaveConnectionSettings(ConnectionSettings settings)
        {
            _settings = settings;
            
            // Mark as connected if we have valid settings
            if (_settings != null && !StringExtensions.IsNullOrWhiteSpace(_settings.Host) && 
                !StringExtensions.IsNullOrWhiteSpace(_settings.Username))
            {
                _settings.IsConnected = true;
                
                // Auto-save this connection to registry for restart recovery
                try
                {
                    string connectionName = string.Format("{0}@{1}", _settings.Username, _settings.Host);
                    SaveConnection(connectionName, _settings, true); // Set as default
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogWarning("Failed to auto-save connection to registry: " + ex.Message);
                }
                
                ServiceLocator.LogService.LogInfo(string.Format("Connection settings saved and marked as connected: {0}@{1}:{2}", 
                    _settings.Username, _settings.Host, _settings.Port));
            }
            
            return true;
        }

        public bool TestConnection(ConnectionSettings settings)
        {
            if (StringExtensions.IsNullOrWhiteSpace(settings.Host) ||
                StringExtensions.IsNullOrWhiteSpace(settings.Username))
            {
                return false;
            }
            return true;
        }

        public bool IsConnected()
        {
            return _settings.IsConnected;
        }

        // Enhanced connection management methods
        public bool SaveConnection(string connectionName, ConnectionSettings settings, bool setAsDefault = false)
        {
            if (StringExtensions.IsNullOrWhiteSpace(connectionName) || settings == null)
                return false;

            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\DataSyncer\\Connections"))
                {
                    using (Microsoft.Win32.RegistryKey connectionKey = key.CreateSubKey(connectionName))
                    {
                        connectionKey.SetValue("Protocol", settings.Protocol ?? "LOCAL");
                        connectionKey.SetValue("ProtocolType", settings.ProtocolType);
                        connectionKey.SetValue("Host", settings.Host ?? "");
                        connectionKey.SetValue("Port", settings.Port);
                        connectionKey.SetValue("Username", settings.Username ?? "");
                        connectionKey.SetValue("SshKeyPath", settings.SshKeyPath ?? "");
                        connectionKey.SetValue("Timeout", settings.Timeout);
                        connectionKey.SetValue("UsePassiveMode", settings.UsePassiveMode);
                        connectionKey.SetValue("IsConnected", settings.IsConnected);
                        
                        // Store encrypted password for restart recovery
                        if (!StringExtensions.IsNullOrWhiteSpace(settings.Password))
                        {
                            try
                            {
                                // Simple base64 encoding for basic obfuscation (not secure encryption)
                                string encodedPassword = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(settings.Password));
                                connectionKey.SetValue("EncodedPassword", encodedPassword);
                            }
                            catch (Exception pwdEx)
                            {
                                ServiceLocator.LogService.LogWarning("Failed to store password: " + pwdEx.Message);
                            }
                        }
                        
                        // Set as default if requested
                        if (setAsDefault)
                        {
                            connectionKey.SetValue("IsDefault", true);
                            _settings = settings; // Update current settings
                        }
                    }
                }
                
                ServiceLocator.LogService.LogInfo(string.Format("Connection '{0}' saved successfully", connectionName));
                return true;
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError(string.Format("Failed to save connection '{0}': {1}", connectionName, ex.Message));
                return false;
            }
        }

        public ConnectionSettings GetConnection(string connectionName)
        {
            if (StringExtensions.IsNullOrWhiteSpace(connectionName))
                return null;

            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\DataSyncer\\Connections"))
                {
                    if (key != null)
                    {
                        using (Microsoft.Win32.RegistryKey connectionKey = key.OpenSubKey(connectionName))
                        {
                            if (connectionKey != null)
                            {
                                var settings = new ConnectionSettings
                                {
                                    Protocol = connectionKey.GetValue("Protocol", "LOCAL").ToString(),
                                    ProtocolType = Convert.ToInt32(connectionKey.GetValue("ProtocolType", 0)),
                                    Host = connectionKey.GetValue("Host", "").ToString(),
                                    Port = Convert.ToInt32(connectionKey.GetValue("Port", 21)),
                                    Username = connectionKey.GetValue("Username", "").ToString(),
                                    SshKeyPath = connectionKey.GetValue("SshKeyPath", "").ToString(),
                                    Timeout = Convert.ToInt32(connectionKey.GetValue("Timeout", 30)),
                                    UsePassiveMode = Convert.ToBoolean(connectionKey.GetValue("UsePassiveMode", false)),
                                    IsConnected = Convert.ToBoolean(connectionKey.GetValue("IsConnected", false))
                                };
                                
                                // Retrieve and decode password
                                try
                                {
                                    string encodedPassword = connectionKey.GetValue("EncodedPassword", "").ToString();
                                    if (!StringExtensions.IsNullOrWhiteSpace(encodedPassword))
                                    {
                                        settings.Password = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedPassword));
                                    }
                                }
                                catch (Exception pwdEx)
                                {
                                    ServiceLocator.LogService.LogWarning("Failed to decode stored password: " + pwdEx.Message);
                                }
                                
                                // If not explicitly marked as connected in registry, check if we have valid details
                                if (!settings.IsConnected && !StringExtensions.IsNullOrWhiteSpace(settings.Host) && 
                                    !StringExtensions.IsNullOrWhiteSpace(settings.Username))
                                {
                                    settings.IsConnected = true;
                                }
                                
                                return settings;
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        public List<SavedConnection> GetAllConnections()
        {
            var connections = new List<SavedConnection>();
            
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\DataSyncer\\Connections"))
                {
                    if (key != null)
                    {
                        foreach (string connectionName in key.GetSubKeyNames())
                        {
                            var settings = GetConnection(connectionName);
                            if (settings != null)
                            {
                                connections.Add(new SavedConnection
                                {
                                    Name = connectionName,
                                    Settings = settings,
                                    CreatedDate = DateTime.Now,
                                    LastUsed = DateTime.Now,
                                    IsDefault = false
                                });
                            }
                        }
                    }
                }
            }
            catch { }

            return connections;
        }

        public ConnectionSettings GetDefaultConnection()
        {
            var connections = GetAllConnections();
            if (connections.Count > 0)
            {
                return connections[0].Settings;
            }
            return LoadDefaultConnectionFromRegistry();
        }

        public bool SetDefaultConnection(string connectionName)
        {
            var connection = GetConnection(connectionName);
            if (connection != null)
            {
                _settings = connection;
                return true;
            }
            return false;
        }

        public bool DeleteConnection(string connectionName)
        {
            if (StringExtensions.IsNullOrWhiteSpace(connectionName))
                return false;

            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\DataSyncer\\Connections", true))
                {
                    if (key != null)
                    {
                        key.DeleteSubKey(connectionName, false);
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        public bool ConnectionExists(string connectionName)
        {
            return GetConnection(connectionName) != null;
        }

        public List<string> GetConnectionNames()
        {
            var names = new List<string>();
            
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\DataSyncer\\Connections"))
                {
                    if (key != null)
                    {
                        foreach (string connectionName in key.GetSubKeyNames())
                        {
                            names.Add(connectionName);
                        }
                    }
                }
            }
            catch { }

            return names;
        }

        public ConnectionSettings LoadConnectionForStartup()
        {
            var defaultConnection = GetDefaultConnection();
            if (defaultConnection != null)
            {
                _settings = defaultConnection;
                return defaultConnection;
            }
            return _settings;
        }
        
        /// <summary>
        /// Force reconnection and ensure IsConnected is properly set - useful for restart recovery
        /// </summary>
        public bool ForceReconnect()
        {
            try
            {
                if (_settings != null && !StringExtensions.IsNullOrWhiteSpace(_settings.Host) && 
                    !StringExtensions.IsNullOrWhiteSpace(_settings.Username))
                {
                    // Test the connection
                    if (TestConnection(_settings))
                    {
                        _settings.IsConnected = true;
                        ServiceLocator.LogService.LogInfo(string.Format("Force reconnect successful: {0}@{1}:{2}", 
                            _settings.Username, _settings.Host, _settings.Port));
                        return true;
                    }
                    else
                    {
                        _settings.IsConnected = false;
                        ServiceLocator.LogService.LogWarning(string.Format("Force reconnect failed - connection test unsuccessful: {0}@{1}:{2}", 
                            _settings.Username, _settings.Host, _settings.Port));
                        return false;
                    }
                }
                else
                {
                    ServiceLocator.LogService.LogWarning("Force reconnect failed - insufficient connection details");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Error during force reconnect: " + ex.Message);
                return false;
            }
        }

        private ConnectionSettings LoadDefaultConnectionFromRegistry()
        {
            try
            {
                var connections = GetAllConnections();
                if (connections.Count > 0)
                {
                    // First try to find a connection explicitly marked as default
                    foreach (var conn in connections)
                    {
                        try
                        {
                            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\DataSyncer\\Connections\\" + conn.Name))
                            {
                                if (key != null && Convert.ToBoolean(key.GetValue("IsDefault", false)))
                                {
                                    ServiceLocator.LogService.LogInfo(string.Format("Loaded default connection: {0}", conn.Name));
                                    return conn.Settings;
                                }
                            }
                        }
                        catch { }
                    }
                    
                    // If no explicit default, return the first one
                    ServiceLocator.LogService.LogInfo(string.Format("Using first available connection: {0} connections found", connections.Count));
                    return connections[0].Settings;
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Failed to load default connection from registry: " + ex.Message);
            }
            
            return null;
        }
    }

    // Stub implementation of log service - will be replaced with actual backend implementation
    public class LogService : ILogService
    {
        private DataTable _logsTable;

        public LogService()
        {
            InitializeLogTable();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            // Add some sample log entries for testing
            _logsTable.Rows.Add(DateTime.Now.AddMinutes(-10), "INFO", "File Sync Job", "document.pdf", "Success", "File transferred successfully");
            _logsTable.Rows.Add(DateTime.Now.AddMinutes(-8), "WARNING", "Backup Job", "data.xml", "Retry", "Connection timeout, retrying...");
            _logsTable.Rows.Add(DateTime.Now.AddMinutes(-5), "ERROR", "Upload Task", "image.jpg", "Failed", "Authentication failed");
            _logsTable.Rows.Add(DateTime.Now.AddMinutes(-2), "INFO", "Cleanup Job", "", "Success", "Temporary files cleaned");
        }

        private void InitializeLogTable()
        {
            _logsTable = new DataTable();
            _logsTable.Columns.Add("DateTime", typeof(DateTime));
            _logsTable.Columns.Add("Level", typeof(string));
            _logsTable.Columns.Add("Job", typeof(string));
            _logsTable.Columns.Add("File", typeof(string));
            _logsTable.Columns.Add("Status", typeof(string));
            _logsTable.Columns.Add("Message", typeof(string));
        }

        public DataTable GetLogs(DateTime? fromDate, DateTime? toDate, string logLevel)
        {
            DataTable filtered = _logsTable.Clone();
            foreach (DataRow row in _logsTable.Rows)
            {
                DateTime logDate = (DateTime)row["DateTime"];
                string level = row["Level"].ToString();
                bool includeRow = true;
                if (fromDate.HasValue && logDate < fromDate.Value) includeRow = false;
                if (toDate.HasValue && logDate > toDate.Value) includeRow = false;
                if (!StringExtensions.IsNullOrWhiteSpace(logLevel) && logLevel != "All" && level != logLevel) includeRow = false;
                if (includeRow) filtered.ImportRow(row);
            }
            return filtered;
        }

        public DataTable GetLogs()
        {
            return _logsTable.Copy();
        }

        public bool ClearLogs()
        {
            _logsTable.Clear();
            return true;
        }

        public bool ExportLogs(string filePath)
        {
            return ExportLogs(filePath, null, null);
        }

        public bool ExportLogs(string filePath, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                DataTable logs = GetLogs(fromDate, toDate, null);
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("DateTime,Level,Job,File,Status,Message");
                    foreach (DataRow row in logs.Rows)
                    {
                        string line = row["DateTime"].ToString() + "," +
                                      EscapeCsvField(row["Level"].ToString()) + "," +
                                      EscapeCsvField(row["Job"].ToString()) + "," +
                                      EscapeCsvField(row["File"].ToString()) + "," +
                                      EscapeCsvField(row["Status"].ToString()) + "," +
                                      EscapeCsvField(row["Message"].ToString());
                        writer.WriteLine(line);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string EscapeCsvField(string field)
        {
            if (field.IndexOf(',') >= 0 || field.IndexOf('"') >= 0 || field.IndexOf('\n') >= 0)
            {
                return '"' + field.Replace("\"", "\"\"") + '"';
            }
            return field;
        }

        public void LogInfo(string message)
        {
            LogInfo(message, string.Empty);
        }

        public void LogInfo(string message, string jobName)
        {
            AddLog("INFO", message, jobName);
        }

        public void LogInfo(string message, string source, string jobId)
        {
            AddLog("INFO", message, source ?? string.Empty, jobId ?? string.Empty);
        }

        public void LogWarning(string message)
        {
            LogWarning(message, string.Empty);
        }

        public void LogWarning(string message, string jobName)
        {
            AddLog("WARNING", message, jobName);
        }

        public void LogWarning(string message, string source, string jobId)
        {
            AddLog("WARNING", message, source ?? string.Empty, jobId ?? string.Empty);
        }

        public void LogError(string message)
        {
            LogError(message, string.Empty);
        }

        public void LogError(string message, string jobName)
        {
            AddLog("ERROR", message, jobName);
        }

        public void LogError(string message, string source, string jobId)
        {
            AddLog("ERROR", message, source ?? string.Empty, jobId ?? string.Empty);
        }

        private void AddLog(string level, string message, string jobName)
        {
            _logsTable.Rows.Add(DateTime.Now, level, jobName, string.Empty, string.Empty, message);
        }

        private void AddLog(string level, string message, string source, string jobId)
        {
            _logsTable.Rows.Add(DateTime.Now, level, source, string.Empty, string.Empty, message);
        }

        #region Real-time Logging Interface Implementation (No-op for UI LogService)
        
        // Removed: unused event RealTimeLogEntry
        
        /// <summary>
        /// Enable real-time logging (no-op implementation for UI LogService)
        /// </summary>
        public void EnableRealTimeLogging(string customFilePath)
        {
            // UI LogService doesn't support real-time logging to custom directories
            // This is a no-op implementation
        }

        /// <summary>
        /// Disable real-time logging (no-op implementation for UI LogService)
        /// </summary>
        public void DisableRealTimeLogging()
        {
            // UI LogService doesn't support real-time logging to custom directories
            // This is a no-op implementation
        }

        /// <summary>
        /// Check if real-time logging is enabled (always false for UI LogService)
        /// </summary>
        public bool IsRealTimeLoggingEnabled()
        {
            return false; // UI LogService doesn't support real-time logging
        }

        /// <summary>
        /// Get real-time log path (always null for UI LogService)
        /// </summary>
        public string GetRealTimeLogPath()
        {
            return null; // UI LogService doesn't support real-time logging
        }

        #endregion
    }

    // Full implementation of service manager that properly manages job scheduling
    public class ServiceManager : IServiceManager
    {
        private bool _isRunning;
        private SyncJobService _jobService;
        
        public ServiceManager()
        {
            _jobService = ServiceLocator.SyncJobService as SyncJobService;
        }
        
        public bool StartService() 
        { 
            try
            {
                // First check if Windows service is available and start it if needed
                try
                {
                    using (var service = new System.ServiceProcess.ServiceController("FTPSyncerService"))
                    {
                        service.Refresh();
                        if (service.Status == System.ServiceProcess.ServiceControllerStatus.Stopped)
                        {
                            ServiceLocator.LogService?.LogInfo("Starting Windows service...");
                            service.Start();
                            service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                            ServiceLocator.LogService?.LogInfo("Windows service started successfully");
                        }
                        else if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                        {
                            ServiceLocator.LogService?.LogInfo("Windows service is already running");
                        }
                    }
                }
                catch (Exception serviceEx)
                {
                    ServiceLocator.LogService?.LogWarning("Could not start Windows service (running in local mode): " + serviceEx.Message);
                }
                
                if (!_isRunning)
                {
                    _isRunning = true;
                    
                    // Start the job scheduler if we have jobs
                    if (_jobService != null)
                    {
                        _jobService.StartScheduler();
                        ServiceLocator.LogService.LogInfo("FTPSyncer service started - job scheduling active");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Failed to start service: " + ex.Message);
                return false;
            }
        }
        
        public bool StopService() 
        { 
            try
            {
                if (_isRunning)
                {
                    _isRunning = false;
                    
                    // Stop the job scheduler
                    if (_jobService != null)
                    {
                        _jobService.StopScheduler();
                        ServiceLocator.LogService.LogInfo("FTPSyncer service stopped - job scheduling inactive");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Failed to stop service: " + ex.Message);
                return false;
            }
        }
        
        public bool IsServiceRunning() 
        { 
            // First check if we have the Windows service
            try
            {
                using (var service = new System.ServiceProcess.ServiceController("FTPSyncerService"))
                {
                    service.Refresh();
                    bool windowsServiceRunning = service.Status == System.ServiceProcess.ServiceControllerStatus.Running;
                    
                    // If Windows service is running, sync our local state
                    if (windowsServiceRunning && !_isRunning)
                    {
                        _isRunning = true;
                        ServiceLocator.LogService?.LogInfo("Synchronized with running Windows service");
                    }
                    else if (!windowsServiceRunning && _isRunning)
                    {
                        _isRunning = false;
                        ServiceLocator.LogService?.LogInfo("Synchronized with stopped Windows service");
                    }
                    
                    return windowsServiceRunning;
                }
            }
            catch
            {
                // If Windows service is not available, fall back to local state
                return _isRunning;
            }
        }
        
        public string GetServiceStatus() 
        { 
            // Check Windows service status first
            try
            {
                using (var service = new System.ServiceProcess.ServiceController("FTPSyncerService"))
                {
                    service.Refresh();
                    var status = service.Status;
                    
                    switch (status)
                    {
                        case System.ServiceProcess.ServiceControllerStatus.Running:
                            // Get job count if possible
                            var timerJobManager = ServiceLocator.TimerJobManager;
                            if (timerJobManager != null)
                            {
                                var runningJobs = timerJobManager.GetRunningJobs();
                                if (runningJobs.Count > 0)
                                {
                                    return string.Format("Running ({0} active jobs)", runningJobs.Count);
                                }
                                else
                                {
                                    return "Running (idle)";
                                }
                            }
                            return "Running";
                            
                        case System.ServiceProcess.ServiceControllerStatus.Stopped:
                            return "Stopped";
                            
                        case System.ServiceProcess.ServiceControllerStatus.Paused:
                            return "Paused";
                            
                        default:
                            return status.ToString();
                    }
                }
            }
            catch
            {
                // If Windows service is not available, fall back to local state
                return _isRunning ? "Running (Local)" : "Stopped (Local)";
            }
        }
        
        public void Dispose()
        {
            StopService();
        }
    }

    // Full implementation of configuration service using Windows Registry for .NET 3.5 compatibility
    public class ConfigurationService : IConfigurationService
    {
        private Dictionary<string, object> _settings = new Dictionary<string, object>();
        private const string REGISTRY_KEY = @"SOFTWARE\DataSyncer\Settings";
        private bool _registryAvailable = true;
        
        public ConfigurationService() 
        { 
            LoadAllSettings(); 
        }
        
        public T GetSetting<T>(string key, T defaultValue)
        {
            if (_settings.ContainsKey(key))
            {
                try 
                { 
                    object value = _settings[key];
                    if (value is T)
                        return (T)value;
                    return (T)Convert.ChangeType(value, typeof(T));
                } 
                catch 
                { 
                    return defaultValue; 
                }
            }
            return defaultValue;
        }
        
        public bool SaveSetting<T>(string key, T value) 
        { 
            try
            {
                _settings[key] = value;
                SaveToRegistry(key, value);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public bool DeleteSetting(string key) 
        { 
            try
            {
                bool removed = _settings.Remove(key);
                if (removed && _registryAvailable)
                {
                    using (Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true))
                    {
                        if (regKey != null)
                        {
                            regKey.DeleteValue(key, false);
                        }
                    }
                }
                return removed;
            }
            catch
            {
                return false;
            }
        }
        
        public void SaveAllSettings() 
        { 
            try
            {
                if (!_registryAvailable) return;
                
                foreach (var kvp in _settings)
                {
                    SaveToRegistry(kvp.Key, kvp.Value);
                }
            }
            catch
            {
                // Ignore errors during bulk save
            }
        }
        
        public void LoadAllSettings() 
        { 
            try
            {
                _settings.Clear();
                
                using (Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(REGISTRY_KEY))
                {
                    if (regKey != null)
                    {
                        foreach (string valueName in regKey.GetValueNames())
                        {
                            object value = regKey.GetValue(valueName);
                            if (value != null)
                            {
                                _settings[valueName] = value;
                            }
                        }
                    }
                }
            }
            catch
            {
                _registryAvailable = false;
                // Fall back to default settings if registry is not available
                SetDefaultSettings();
            }
        }
        
        private void SaveToRegistry<T>(string key, T value)
        {
            try
            {
                if (!_registryAvailable) return;
                
                using (Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(REGISTRY_KEY))
                {
                    if (regKey != null)
                    {
                        // Convert to appropriate registry type
                        if (value is bool)
                            regKey.SetValue(key, (bool)(object)value ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                        else if (value is int)
                            regKey.SetValue(key, (int)(object)value, Microsoft.Win32.RegistryValueKind.DWord);
                        else
                            regKey.SetValue(key, value.ToString(), Microsoft.Win32.RegistryValueKind.String);
                    }
                }
            }
            catch
            {
                // Ignore registry save errors
            }
        }
        
        private void SetDefaultSettings()
        {
            _settings["NotificationsEnabled"] = true;
            _settings["NotificationDelay"] = 3000;
            _settings["MinimizeToTray"] = true;
            _settings["StartMinimized"] = false;
        }
    }
}
