using System;
using System.Threading;
using System.Collections.Generic;

namespace syncer.core
{
    public class JobRunner : IJobRunner
    {
        private readonly ITransferClientFactory _transferClientFactory;
        private readonly ILogService _logService;
        private readonly IFileEnumerator _fileEnumerator;
        private readonly Dictionary<string, bool> _runningJobs = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> _cancellationRequests = new Dictionary<string, bool>();

        public event EventHandler<JobStartedEventArgs> JobStarted;
        public event EventHandler<JobProgressEventArgs> JobProgress;
        public event EventHandler<JobCompletedEventArgs> JobCompleted;
        public event EventHandler<FileTransferEventArgs> FileTransferStarted;
        public event EventHandler<FileTransferEventArgs> FileTransferCompleted;
        public event EventHandler<JobStatusEventArgs> JobStatusChanged;

        public JobRunner(ITransferClientFactory transferClientFactory, ILogService logService, IFileEnumerator fileEnumerator)
        {
            if (transferClientFactory == null)
                throw new ArgumentNullException("transferClientFactory");
            if (logService == null)
                throw new ArgumentNullException("logService");
            if (fileEnumerator == null)
                throw new ArgumentNullException("fileEnumerator");

            _transferClientFactory = transferClientFactory;
            _logService = logService;
            _fileEnumerator = fileEnumerator;
        }

        public bool IsRunning(string jobId)
        {
            lock (_runningJobs)
            {
                return _runningJobs.ContainsKey(jobId) && _runningJobs[jobId];
            }
        }

        public List<string> GetRunningJobIds()
        {
            lock (_runningJobs)
            {
                var runningIds = new List<string>();
                foreach (var kvp in _runningJobs)
                {
                    if (kvp.Value)
                        runningIds.Add(kvp.Key);
                }
                return runningIds;
            }
        }

        public bool StartJob(string jobId)
        {
            // This method should trigger job execution - implementation depends on your job management system
            // For now, throwing NotImplementedException as the original RunJob method handles this differently
            throw new NotImplementedException("Use RunJob(SyncJob) method instead");
        }

        public bool CancelJob(string jobId)
        {
            lock (_cancellationRequests)
            {
                if (_runningJobs.ContainsKey(jobId) && _runningJobs[jobId])
                {
                    _cancellationRequests[jobId] = true;
                    return true;
                }
                return false;
            }
        }

        public bool WaitForJob(string jobId, int timeoutMs)
        {
            var startTime = DateTime.Now;
            while (IsRunning(jobId) && (DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                Thread.Sleep(100);
            }
            return !IsRunning(jobId);
        }

        public void RunJob(SyncJob job)
        {
            // CRITICAL DEBUG - CHECK IF THIS METHOD IS CALLED AT ALL
            _logService.LogInfo("DEBUG: JobRunner.RunJob() called for job: " + (job != null ? job.Name ?? "unnamed" : "null"), "JobRunner");
            if (job != null && job.Filters != null)
            {
                _logService.LogInfo("DEBUG: Include extensions count: " + (job.Filters.IncludeExtensions?.Count ?? 0), "JobRunner");
                _logService.LogInfo("DEBUG: Exclude extensions count: " + (job.Filters.ExcludeExtensions?.Count ?? 0), "JobRunner");
            }
            else if (job != null)
            {
                _logService.LogInfo("DEBUG: Job has no filters (Filters is null)", "JobRunner");
            }
            
            if (IsRunning(job.Id))
            {
                throw new InvalidOperationException("Job is already running: " + job.Id);
            }

            lock (_runningJobs)
            {
                _runningJobs[job.Id] = true;
                _cancellationRequests[job.Id] = false;
            }

            try
            {
                var result = new TransferResult
                {
                    JobId = job.Id,
                    JobName = job.Name,
                    StartTime = DateTime.Now,
                    Status = TransferStatus.Running,
                    TotalFiles = 0,
                    ProcessedFiles = 0,
                    SuccessfulFiles = 0,
                    FailedFiles = 0,
                    TotalBytes = 0,
                    TransferredBytes = 0,
                    Errors = new List<string>()
                };

                OnJobStarted(new JobStartedEventArgs { Job = job, Result = result });
                _logService.LogJobStart(job);

                ExecuteJob(job, result);

                result.EndTime = DateTime.Now;
                result.Duration = result.EndTime - result.StartTime;

                bool wasCancelled;
                lock (_cancellationRequests)
                {
                    wasCancelled = _cancellationRequests.ContainsKey(job.Id) && _cancellationRequests[job.Id];
                }

                result.Status = wasCancelled ?
                    TransferStatus.Cancelled :
                    (result.FailedFiles > 0 ? TransferStatus.CompletedWithErrors : TransferStatus.Completed);

                if (result.Status == TransferStatus.Completed)
                {
                    _logService.LogJobSuccess(job, "Job completed successfully");
                }
                else if (result.Status == TransferStatus.CompletedWithErrors)
                {
                    _logService.LogJobError(job, "Job completed with errors", null);
                }

                OnJobCompleted(new JobCompletedEventArgs { Job = job, Result = result });
            }
            catch (Exception ex)
            {
                _logService.LogJobError(job, "Job execution failed: " + ex.Message, ex);
                throw;
            }
            finally
            {
                lock (_runningJobs)
                {
                    _runningJobs[job.Id] = false;
                }
            }
        }

        public void CancelJob()
        {
            // Legacy method - cancel all running jobs
            lock (_cancellationRequests)
            {
                var runningJobIds = GetRunningJobIds();
                foreach (var jobId in runningJobIds)
                {
                    _cancellationRequests[jobId] = true;
                }
            }
        }

        private void ExecuteJob(SyncJob job, TransferResult result)
        {
            var filesToProcess = GetFilesToProcess(job);
            result.TotalFiles = filesToProcess.Count;

            for (int i = 0; i < filesToProcess.Count; i++)
            {
                bool shouldCancel;
                lock (_cancellationRequests)
                {
                    shouldCancel = _cancellationRequests.ContainsKey(job.Id) && _cancellationRequests[job.Id];
                }

                if (shouldCancel)
                    break;

                var sourceFile = filesToProcess[i];
                try
                {
                    ProcessFile(job, sourceFile, result);
                    result.SuccessfulFiles++;
                }
                catch (Exception ex)
                {
                    result.FailedFiles++;
                    result.Errors.Add("File '" + sourceFile + "': " + ex.Message);
                    _logService.LogJobError(job, "Failed to process file '" + sourceFile + "': " + ex.Message, ex);
                }

                result.ProcessedFiles++;
                var progressPercent = (result.ProcessedFiles * 100) / result.TotalFiles;

                _logService.LogJobProgress(job, string.Format("Processing file {0} of {1} ({2}%)",
                    result.ProcessedFiles, result.TotalFiles, progressPercent));

                OnJobProgress(new JobProgressEventArgs { Job = job, Result = result, ProgressPercent = progressPercent });
            }
        }

        private List<string> GetFilesToProcess(SyncJob job)
        {
            var files = new List<string>();
            
            _logService.LogInfo("DEBUG: GetFilesToProcess called for source: " + job.SourcePath, "JobRunner");
            _logService.LogInfo("DEBUG: Job has filters: " + (job.Filters != null), "JobRunner");

            if (job.Connection.Protocol == ProtocolType.Local)
            {
                if (System.IO.File.Exists(job.SourcePath))
                {
                    files.Add(job.SourcePath);
                    _logService.LogInfo("DEBUG: Added single file: " + job.SourcePath, "JobRunner");
                }
                else if (System.IO.Directory.Exists(job.SourcePath))
                {
                    // Use the filter-aware overload when filters are provided
                    if (job.Filters != null)
                    {
                        _logService.LogInfo("DEBUG: Using FileEnumerator WITH filters", "JobRunner");
                        var includeList = job.Filters.IncludeExtensions ?? new List<string>();
                        var excludeList = job.Filters.ExcludeExtensions ?? new List<string>();
                        _logService.LogInfo("DEBUG: Include extensions: " + string.Join(",", includeList.ToArray()), "JobRunner");
                        _logService.LogInfo("DEBUG: Exclude extensions: " + string.Join(",", excludeList.ToArray()), "JobRunner");
                        files.AddRange(_fileEnumerator.EnumerateFiles(job.SourcePath, job.Filters, job.IncludeSubfolders));
                    }
                    else
                    {
                        _logService.LogInfo("DEBUG: Using FileEnumerator WITHOUT filters", "JobRunner");
                        files.AddRange(_fileEnumerator.EnumerateFiles(job.SourcePath, job.IncludeSubfolders));
                    }
                    _logService.LogInfo("DEBUG: FileEnumerator returned " + files.Count + " files", "JobRunner");
                }
            }
            else
            {
                var client = _transferClientFactory.Create(job.Connection.Protocol);
                string error;
                List<string> remoteFiles;

                if (client.ListFiles(job.Connection, job.SourcePath, out remoteFiles, out error))
                {
                    files.AddRange(remoteFiles);
                }
                else
                {
                    throw new Exception("Failed to list remote files: " + error);
                }
            }

            return files;
        }

        private void ProcessFile(SyncJob job, string sourceFile, TransferResult result)
        {
            var destinationFile = System.IO.Path.Combine(job.DestinationPath, System.IO.Path.GetFileName(sourceFile));

            OnFileTransferStarted(new FileTransferEventArgs
            {
                SourcePath = sourceFile,
                DestinationPath = destinationFile,
                Job = job
            });

            try
            {
                var sourceClient = _transferClientFactory.Create(job.Connection.Protocol);
                var destClient = _transferClientFactory.Create(job.DestinationConnection.Protocol);

                bool success = false;
                string error = null;

                if (job.Direction == TransferDirection.SourceToDestination)
                {
                    if (sourceClient.Protocol == ProtocolType.Local && destClient.Protocol == ProtocolType.Local)
                    {
                        success = sourceClient.UploadFile(null, sourceFile, destinationFile, job.OverwriteExisting, out error);
                    }
                    else if (sourceClient.Protocol == ProtocolType.Local)
                    {
                        success = destClient.UploadFile(job.DestinationConnection, sourceFile, destinationFile, job.OverwriteExisting, out error);
                    }
                    else if (destClient.Protocol == ProtocolType.Local)
                    {
                        success = sourceClient.DownloadFile(job.Connection, sourceFile, destinationFile, job.OverwriteExisting, out error);
                    }
                }

                if (!success)
                {
                    throw new Exception(error ?? "Transfer failed");
                }

                // Use a generic log method for transfer, or add LogTransfer to ILogService if needed
                _logService.LogJobProgress(job, $"Transferred file: {System.IO.Path.GetFileName(sourceFile)}");

                // Delete source file after successful transfer if requested
                if (ShouldDeleteSourceAfterTransfer(job))
                {
                    try
                    {
                        string deleteError;
                        bool deleteSuccess = sourceClient.DeleteFile(job.Connection, sourceFile, out deleteError);
                        
                        if (deleteSuccess)
                        {
                            _logService.LogJobProgress(job, $"Source file deleted after successful transfer: {System.IO.Path.GetFileName(sourceFile)}");
                        }
                        else
                        {
                            _logService.LogJobError(job, $"Failed to delete source file {System.IO.Path.GetFileName(sourceFile)} after transfer: {deleteError ?? "Unknown error"}", null);
                        }
                    }
                    catch (Exception deleteEx)
                    {
                        _logService.LogJobError(job, $"Failed to delete source file {System.IO.Path.GetFileName(sourceFile)} after transfer: {deleteEx.Message}", deleteEx);
                        // Don't fail the entire transfer just because delete failed
                    }
                }

                OnFileTransferCompleted(new FileTransferEventArgs
                {
                    SourcePath = sourceFile,
                    DestinationPath = destinationFile,
                    Job = job,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logService.LogJobError(job, $"Failed to transfer file: {System.IO.Path.GetFileName(sourceFile)} - {ex.Message}", ex);

                OnFileTransferCompleted(new FileTransferEventArgs
                {
                    SourcePath = sourceFile,
                    DestinationPath = destinationFile,
                    Job = job,
                    Success = false,
                    Error = ex.Message
                });

                throw;
            }
        }

        protected virtual void OnJobStarted(JobStartedEventArgs e)
        {
            if (JobStarted != null) JobStarted(this, e);
        }

        protected virtual void OnJobProgress(JobProgressEventArgs e)
        {
            if (JobProgress != null) JobProgress(this, e);
        }

        protected virtual void OnJobCompleted(JobCompletedEventArgs e)
        {
            if (JobCompleted != null) JobCompleted(this, e);
        }

        protected virtual void OnFileTransferStarted(FileTransferEventArgs e)
        {
            if (FileTransferStarted != null) FileTransferStarted(this, e);
        }

        protected virtual void OnFileTransferCompleted(FileTransferEventArgs e)
        {
            if (FileTransferCompleted != null) FileTransferCompleted(this, e);
        }

        protected virtual void OnJobStatusChanged(JobStatusEventArgs e)
        {
            if (JobStatusChanged != null) JobStatusChanged(this, e);
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
        
        #region IDisposable Implementation
        
        private bool _disposed = false;
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Cancel all running jobs
                    lock (_runningJobs)
                    {
                        foreach (string jobId in new List<string>(_runningJobs.Keys))
                        {
                            if (_runningJobs[jobId])
                            {
                                _cancellationRequests[jobId] = true;
                                _logService.LogInfo($"Cancelling job {jobId} during disposal", "JobRunner");
                            }
                        }
                        
                        // Wait a short time for jobs to respond to cancellation
                        Thread.Sleep(500);
                        
                        // Clear collections
                        _runningJobs.Clear();
                        _cancellationRequests.Clear();
                    }
                }
                
                _disposed = true;
            }
        }
        
        ~JobRunner()
        {
            Dispose(false);
        }
        
        #endregion
    }
}