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

        public SyncJobService()
        {
            _jobs = new List<SyncJob>();
            _jobTimers = new Dictionary<int, System.Timers.Timer>();
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
            ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' deleted", job.Name));
            return true;
        }

        public bool StartJob(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return false;
            
            job.IsEnabled = true;
            ScheduleJob(job);
            ServiceLocator.LogService.LogInfo(string.Format("Job '{0}' started", job.Name));
            return true;
        }

        public bool StopJob(int id)
        {
            SyncJob job = GetJobById(id);
            if (job == null) return false;
            
            job.IsEnabled = false;
            StopJobTimer(id);
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

            // Use the job's own filter settings instead of global settings
            if (job.FilterSettings == null)
            {
                // Fallback to global filter settings if job doesn't have its own
                var filterService = ServiceLocator.FilterService;
                if (filterService != null)
                {
                    job.FilterSettings = filterService.GetFilterSettings();
                }
            }

            // Get files with filtering applied using job-specific settings
            List<string> files = GetFilteredFiles(job.SourcePath, job.FilterSettings, job.IncludeSubFolders);
            Console.WriteLine("Found " + files.Count + " files after applying job-specific filters");

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

        private List<string> GetFilteredFiles(string sourcePath, FilterSettings filterSettings, bool includeSubFolders)
        {
            var allFiles = Directory.GetFiles(sourcePath, "*", 
                includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            
            var filteredFiles = new List<string>();
            
            // If filters are disabled, return all files
            if (filterSettings == null || !filterSettings.FiltersEnabled)
            {
                Console.WriteLine("Filters disabled, returning all " + allFiles.Length + " files");
                filteredFiles.AddRange(allFiles);
                return filteredFiles;
            }
            
            Console.WriteLine("Applying filters to " + allFiles.Length + " files");
            
            foreach (string file in allFiles)
            {
                if (ShouldIncludeFile(file, filterSettings))
                {
                    filteredFiles.Add(file);
                }
                else
                {
                    Console.WriteLine("Excluded file: " + Path.GetFileName(file));
                }
            }
            
            return filteredFiles;
        }

        private bool ShouldIncludeFile(string filePath, FilterSettings filterSettings)
        {
            if (filterSettings == null || !filterSettings.FiltersEnabled)
                return true;
            
            try
            {
                var fileInfo = new FileInfo(filePath);
                
                // Check file attributes
                if (!filterSettings.IncludeHiddenFiles && (fileInfo.Attributes & FileAttributes.Hidden) != 0)
                    return false;
                
                if (!filterSettings.IncludeSystemFiles && (fileInfo.Attributes & FileAttributes.System) != 0)
                    return false;
                
                if (!filterSettings.IncludeReadOnlyFiles && (fileInfo.Attributes & FileAttributes.ReadOnly) != 0)
                    return false;
                
                // Check file size (handle different units - assume MB if no unit specified)
                long fileSizeBytes = fileInfo.Length;
                long minSizeBytes = (long)(filterSettings.MinFileSize * 1024 * 1024); // Default to MB
                long maxSizeBytes = (long)(filterSettings.MaxFileSize * 1024 * 1024); // Default to MB
                
                if (minSizeBytes > 0 && fileSizeBytes < minSizeBytes)
                    return false;
                
                if (maxSizeBytes > 0 && fileSizeBytes > maxSizeBytes)
                    return false;
                
                // Check file extensions (new simple filtering approach)
                if (!string.IsNullOrEmpty(filterSettings.IncludeFileExtensions))
                {
                    string fileExtension = fileInfo.Extension.ToLower();
                    string[] includeExtensions = filterSettings.IncludeFileExtensions.Split(',');
                    bool matchesInclude = false;

                    foreach (string pattern in includeExtensions)
                    {
                        string cleanPattern = pattern.Trim().ToLower();
                        if (string.IsNullOrEmpty(cleanPattern)) continue;

                        // Handle patterns like *.txt, .txt, txt
                        if (cleanPattern.StartsWith("*."))
                        {
                            string ext = cleanPattern.Substring(1); // Remove the *
                            if (fileExtension == ext)
                            {
                                matchesInclude = true;
                                break;
                            }
                        }
                        else if (cleanPattern.StartsWith("."))
                        {
                            if (fileExtension == cleanPattern)
                            {
                                matchesInclude = true;
                                break;
                            }
                        }
                        else
                        {
                            string ext = "." + cleanPattern;
                            if (fileExtension == ext)
                            {
                                matchesInclude = true;
                                break;
                            }
                        }
                    }

                    if (!matchesInclude)
                    {
                        Console.WriteLine("File " + Path.GetFileName(filePath) + " extension " + fileExtension + " not in include extensions");
                        return false;
                    }
                }
                // Check file extensions (legacy approach)
                else if (filterSettings.AllowedFileTypes != null && filterSettings.AllowedFileTypes.Length > 0)
                {
                    string fileExtension = fileInfo.Extension;
                    bool matchesExtension = false;
                    
                    foreach (string allowedType in filterSettings.AllowedFileTypes)
                    {
                        // Extract extension from format like ".txt - Text files"
                        string allowedExt = allowedType.Split(' ')[0].Trim();
                        if (string.Equals(fileExtension, allowedExt, StringComparison.OrdinalIgnoreCase))
                        {
                            matchesExtension = true;
                            break;
                        }
                    }
                    
                    if (!matchesExtension)
                    {
                        Console.WriteLine("File " + Path.GetFileName(filePath) + " extension " + fileExtension + " not in allowed types");
                        return false;
                    }
                }
                
                // Check exclude patterns (new simple filtering approach)
                if (!string.IsNullOrEmpty(filterSettings.ExcludeFilePatterns))
                {
                    string fileName = fileInfo.Name.ToLower();
                    string[] excludePatterns = filterSettings.ExcludeFilePatterns.Split(',');

                    foreach (string pattern in excludePatterns)
                    {
                        string cleanPattern = pattern.Trim().ToLower();
                        if (string.IsNullOrEmpty(cleanPattern)) continue;

                        // Handle patterns like *.tmp, .tmp, tmp, temp*
                        if (cleanPattern.StartsWith("*."))
                        {
                            string ext = cleanPattern.Substring(1); // Remove the *
                            if (fileInfo.Extension.ToLower() == ext)
                                return false;
                        }
                        else if (cleanPattern.StartsWith("."))
                        {
                            if (fileInfo.Extension.ToLower() == cleanPattern)
                                return false;
                        }
                        else if (cleanPattern.EndsWith("*"))
                        {
                            string prefix = cleanPattern.Substring(0, cleanPattern.Length - 1);
                            if (fileName.StartsWith(prefix))
                                return false;
                        }
                        else if (cleanPattern.Contains("*"))
                        {
                            // Simple wildcard matching
                            string regexPattern = "^" + Regex.Escape(cleanPattern).Replace("\\*", ".*") + "$";
                            if (Regex.IsMatch(fileName, regexPattern))
                                return false;
                        }
                        else
                        {
                            // Exact filename or extension match
                            string ext = "." + cleanPattern;
                            if (fileInfo.Extension.ToLower() == ext || fileName == cleanPattern)
                                return false;
                        }
                    }
                }
                // Check exclude patterns (legacy approach)
                else if (!string.IsNullOrEmpty(filterSettings.ExcludePatterns))
                {
                    string fileName = fileInfo.Name;
                    string[] patterns = filterSettings.ExcludePatterns.Split(',', ';');
                    
                    foreach (string pattern in patterns)
                    {
                        string trimmedPattern = pattern.Trim();
                        if (!string.IsNullOrEmpty(trimmedPattern))
                        {
                            // Simple wildcard matching
                            if (trimmedPattern.Contains("*"))
                            {
                                // Convert to regex pattern
                                string regexPattern = trimmedPattern.Replace("*", ".*");
                                if (Regex.IsMatch(fileName, regexPattern, RegexOptions.IgnoreCase))
                                {
                                    Console.WriteLine("File " + fileName + " matched exclude pattern: " + trimmedPattern);
                                    return false;
                                }
                            }
                            else if (fileName.IndexOf(trimmedPattern, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                Console.WriteLine("File " + fileName + " matched exclude pattern: " + trimmedPattern);
                                return false;
                            }
                        }
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking file " + filePath + ": " + ex.Message);
                return false; // Exclude files that can't be analyzed
            }
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
                
                // Use the job's own filter settings instead of global settings
                if (job.FilterSettings == null)
                {
                    // Fallback to global filter settings if job doesn't have its own
                    var filterService = ServiceLocator.FilterService;
                    if (filterService != null)
                    {
                        job.FilterSettings = filterService.GetFilterSettings();
                    }
                }

                // Get files to upload with filtering applied
                List<string> filesToUploadList;
                if (Directory.Exists(job.SourcePath))
                {
                    // Upload directory contents with filtering
                    filesToUploadList = GetFilteredFiles(job.SourcePath, job.FilterSettings, job.IncludeSubFolders);
                }
                else if (File.Exists(job.SourcePath))
                {
                    // Upload single file (check if it passes filter)
                    if (ShouldIncludeFile(job.SourcePath, job.FilterSettings))
                    {
                        filesToUploadList = new List<string> { job.SourcePath };
                    }
                    else
                    {
                        throw new Exception("Source file does not match current filter settings");
                    }
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
                
                // Get connection settings
                ConnectionSettings uiSettings = ServiceLocator.ConnectionService.GetConnectionSettings();
                if (uiSettings == null)
                {
                    throw new Exception("No connection settings available for download transfer");
                }
                
                // Convert to core connection settings
                var coreConnectionSettings = new syncer.core.ConnectionSettings
                {
                    Protocol = uiSettings.Protocol == "SFTP" ? syncer.core.ProtocolType.Sftp :
                              uiSettings.Protocol == "FTP" ? syncer.core.ProtocolType.Ftp :
                              syncer.core.ProtocolType.Local,
                    Host = uiSettings.Host,
                    Port = uiSettings.Port,
                    Username = uiSettings.Username,
                    Password = uiSettings.Password,
                    SshKeyPath = uiSettings.SshKeyPath,
                    Timeout = uiSettings.Timeout
                };
                
                // Create transfer client
                var factory = syncer.core.ServiceFactory.CreateTransferClientFactory();
                var transferClient = factory.Create(coreConnectionSettings.Protocol);
                
                // Get list of files from remote directory
                List<string> remoteFiles;
                string error;
                bool success = transferClient.ListFiles(coreConnectionSettings, job.SourcePath, out remoteFiles, out error);
                
                if (!success)
                {
                    throw new Exception($"Failed to list remote files: {error}");
                }
                
                if (remoteFiles == null || remoteFiles.Count == 0)
                {
                    ServiceLocator.LogService.LogInfo("No files found in remote directory for download");
                    job.LastFileCount = 0;
                    job.LastTransferSize = 0;
                    return;
                }
                
                ServiceLocator.LogService.LogInfo(string.Format("Found {0} files to download", remoteFiles.Count));
                
                // Ensure local destination directory exists
                if (!Directory.Exists(job.DestinationPath))
                {
                    Directory.CreateDirectory(job.DestinationPath);
                    ServiceLocator.LogService.LogInfo($"Created local destination directory: {job.DestinationPath}");
                }
                
                int successCount = 0;
                long totalBytes = 0;
                
                // Download each file
                foreach (string remoteFile in remoteFiles)
                {
                    try
                    {
                        string fileName = Path.GetFileName(remoteFile);
                        string localFile = Path.Combine(job.DestinationPath, fileName);
                        
                        ServiceLocator.LogService.LogInfo(string.Format("Downloading: {0} -> {1}", remoteFile, localFile));
                        
                        string downloadError;
                        if (transferClient.DownloadFile(coreConnectionSettings, remoteFile, localFile, job.OverwriteExisting, out downloadError))
                        {
                            successCount++;
                            
                            // Get file size for statistics
                            if (File.Exists(localFile))
                            {
                                FileInfo fileInfo = new FileInfo(localFile);
                                totalBytes += fileInfo.Length;
                            }
                            
                            ServiceLocator.LogService.LogInfo(string.Format("Successfully downloaded: {0}", fileName));
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
                job.LastRun = DateTime.Now;
                job.LastStatus = $"Downloaded {successCount} files successfully";
                
                ServiceLocator.LogService.LogInfo(string.Format("Download job completed successfully. Downloaded {0} files ({1} bytes)", successCount, totalBytes));
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError(string.Format("Download transfer job failed: {0}", ex.Message));
                if (job != null)
                {
                    job.LastRun = DateTime.Now;
                    job.LastStatus = "Failed: " + ex.Message;
                }
                throw;
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
            switch (job.IntervalType)
            {
                case "Minutes":
                    return job.IntervalValue * 60 * 1000;
                case "Hours":
                    return job.IntervalValue * 60 * 60 * 1000;
                case "Days":
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
        
        public void StopScheduler()
        {
            try
            {
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
                ServiceLocator.LogService.LogInfo("Job scheduler stopped");
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
        }

        public ConnectionSettings GetConnectionSettings()
        {
            return _settings;
        }

        public bool SaveConnectionSettings(ConnectionSettings settings)
        {
            _settings = settings;
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
                    }
                }
                return true;
            }
            catch
            {
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
                                    Timeout = Convert.ToInt32(connectionKey.GetValue("Timeout", 30))
                                };
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

        private ConnectionSettings LoadDefaultConnectionFromRegistry()
        {
            var connections = GetAllConnections();
            if (connections.Count > 0)
            {
                return connections[0].Settings;
            }
            return null;
        }
    }

    // Stub implementation of filter service - will be replaced with actual backend implementation
    public class FilterService : IFilterService
    {
        private FilterSettings _settings;
        private string _configPath;

        public FilterService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dataSyncerPath = Path.Combine(appDataPath, "DataSyncer");
            _configPath = Path.Combine(dataSyncerPath, "filter_settings.json");
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    // Since we don't have Newtonsoft.Json available in .NET 3.5 stub, use simple parsing
                    _settings = ParseFilterSettings(json);
                }
                else
                {
                    _settings = new FilterSettings();
                }
            }
            catch
            {
                _settings = new FilterSettings();
            }
        }

        private void SaveSettings()
        {
            try
            {
                string directory = Path.GetDirectoryName(_configPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                string json = SerializeFilterSettings(_settings);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving filter settings: " + ex.Message);
            }
        }

        private string SerializeFilterSettings(FilterSettings settings)
        {
            // Simple JSON serialization for .NET 3.5 compatibility
            var json = "{\n";
            json += "  \"FiltersEnabled\": " + settings.FiltersEnabled.ToString().ToLower() + ",\n";
            json += "  \"MinFileSize\": " + settings.MinFileSize + ",\n";
            json += "  \"MaxFileSize\": " + settings.MaxFileSize + ",\n";
            json += "  \"IncludeHiddenFiles\": " + settings.IncludeHiddenFiles.ToString().ToLower() + ",\n";
            json += "  \"IncludeSystemFiles\": " + settings.IncludeSystemFiles.ToString().ToLower() + ",\n";
            json += "  \"IncludeReadOnlyFiles\": " + settings.IncludeReadOnlyFiles.ToString().ToLower() + ",\n";
            json += "  \"ExcludePatterns\": \"" + (settings.ExcludePatterns ?? "") + "\",\n";
            json += "  \"AllowedFileTypes\": [";
            if (settings.AllowedFileTypes != null)
            {
                for (int i = 0; i < settings.AllowedFileTypes.Length; i++)
                {
                    json += "\"" + settings.AllowedFileTypes[i] + "\"";
                    if (i < settings.AllowedFileTypes.Length - 1) json += ",";
                }
            }
            json += "]\n";
            json += "}";
            return json;
        }

        private FilterSettings ParseFilterSettings(string json)
        {
            var settings = new FilterSettings();
            
            try
            {
                // Simple JSON parsing for .NET 3.5 compatibility
                if (json.Contains("\"FiltersEnabled\": true")) settings.FiltersEnabled = true;
                
                // Parse numeric values
                var minSizeMatch = System.Text.RegularExpressions.Regex.Match(json, "\"MinFileSize\":\\s*(\\d+\\.?\\d*)");
                if (minSizeMatch.Success) settings.MinFileSize = decimal.Parse(minSizeMatch.Groups[1].Value);
                
                var maxSizeMatch = System.Text.RegularExpressions.Regex.Match(json, "\"MaxFileSize\":\\s*(\\d+\\.?\\d*)");
                if (maxSizeMatch.Success) settings.MaxFileSize = decimal.Parse(maxSizeMatch.Groups[1].Value);
                
                // Parse boolean values
                if (json.Contains("\"IncludeHiddenFiles\": true")) settings.IncludeHiddenFiles = true;
                if (json.Contains("\"IncludeSystemFiles\": true")) settings.IncludeSystemFiles = true;
                if (json.Contains("\"IncludeReadOnlyFiles\": false")) settings.IncludeReadOnlyFiles = false;
                else settings.IncludeReadOnlyFiles = true; // default
                
                // Parse exclude patterns
                var excludeMatch = System.Text.RegularExpressions.Regex.Match(json, "\"ExcludePatterns\":\\s*\"([^\"]*)\"");
                if (excludeMatch.Success) settings.ExcludePatterns = excludeMatch.Groups[1].Value;
                
                // Parse allowed file types array
                var typesMatch = System.Text.RegularExpressions.Regex.Match(json, "\"AllowedFileTypes\":\\s*\\[([^\\]]*)\\]");
                if (typesMatch.Success)
                {
                    string typesStr = typesMatch.Groups[1].Value;
                    var typeMatches = System.Text.RegularExpressions.Regex.Matches(typesStr, "\"([^\"]*)\"");
                    var typesList = new List<string>();
                    foreach (System.Text.RegularExpressions.Match match in typeMatches)
                    {
                        typesList.Add(match.Groups[1].Value);
                    }
                    settings.AllowedFileTypes = typesList.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing filter settings: " + ex.Message);
            }
            
            return settings;
        }

        public FilterSettings GetFilterSettings()
        {
            return _settings;
        }

        public bool SaveFilterSettings(FilterSettings settings)
        {
            _settings = settings;
            SaveSettings();
            Console.WriteLine("Filter settings saved - Enabled: " + settings.FiltersEnabled + ", FileTypes: " + (settings.AllowedFileTypes != null ? settings.AllowedFileTypes.Length : 0));
            return true;
        }

        public string[] GetDefaultFileTypes()
        {
            return new string[]
            {
                ".txt - Text files",
                ".doc, .docx - Word documents",
                ".xls, .xlsx - Excel files",
                ".pdf - PDF documents",
                ".jpg, .jpeg - JPEG images",
                ".png - PNG images",
                ".gif - GIF images",
                ".mp4 - Video files",
                ".mp3 - Audio files",
                ".zip, .rar - Archive files",
                ".exe - Executable files",
                ".dll - Library files",
                ".log - Log files",
                ".csv - CSV files",
                ".xml - XML files",
                ".json - JSON files"
            };
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

        public void LogWarning(string message)
        {
            LogWarning(message, string.Empty);
        }

        public void LogWarning(string message, string jobName)
        {
            AddLog("WARNING", message, jobName);
        }

        public void LogError(string message)
        {
            LogError(message, string.Empty);
        }

        public void LogError(string message, string jobName)
        {
            AddLog("ERROR", message, jobName);
        }

        private void AddLog(string level, string message, string jobName)
        {
            _logsTable.Rows.Add(DateTime.Now, level, jobName, string.Empty, string.Empty, message);
        }

        #region Real-time Logging Interface Implementation (No-op for UI LogService)
        
        public event EventHandler<syncer.core.LogEntryEventArgs> RealTimeLogEntry;

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
                if (!_isRunning)
                {
                    _isRunning = true;
                    
                    // Start the job scheduler if we have jobs
                    if (_jobService != null)
                    {
                        _jobService.StartScheduler();
                        ServiceLocator.LogService.LogInfo("Data Syncer service started - job scheduling active");
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
                        ServiceLocator.LogService.LogInfo("Data Syncer service stopped - job scheduling inactive");
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
        
        public bool IsServiceRunning() { return _isRunning; }
        public string GetServiceStatus() { return _isRunning ? "Running" : "Stopped"; }
        
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
