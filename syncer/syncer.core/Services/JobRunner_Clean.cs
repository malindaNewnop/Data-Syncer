using System;
using System.Threading;

namespace syncer.core
{
    public class JobRunner : IJobRunner
    {
        private readonly ITransferClientFactory _transferClientFactory;
        private readonly ILogService _logService;
        private readonly IFileEnumerator _fileEnumerator;
        private bool _cancellationRequested;
        private bool _isRunning;

        public event EventHandler<JobStartedEventArgs> JobStarted;
        public event EventHandler<JobProgressEventArgs> JobProgress;
        public event EventHandler<JobCompletedEventArgs> JobCompleted;
        public event EventHandler<FileTransferEventArgs> FileTransferStarted;
        public event EventHandler<FileTransferEventArgs> FileTransferCompleted;

        public JobRunner(ITransferClientFactory transferClientFactory, ILogService logService, IFileEnumerator fileEnumerator)
        {
            _transferClientFactory = transferClientFactory ?? throw new ArgumentNullException("transferClientFactory");
            _logService = logService ?? throw new ArgumentNullException("logService");
            _fileEnumerator = fileEnumerator ?? throw new ArgumentNullException("fileEnumerator");
        }

        public bool IsRunning
        {
            get { return _isRunning; }
        }

        public void RunJob(SyncJob job)
        {
            if (_isRunning)
            {
                throw new InvalidOperationException("Job runner is already running.");
            }

            _isRunning = true;
            _cancellationRequested = false;

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
                    Errors = new System.Collections.Generic.List<string>()
                };

                OnJobStarted(new JobStartedEventArgs { Job = job, Result = result });
                _logService.LogJobStart(job.Id, job.Name);

                ExecuteJob(job, result);

                result.EndTime = DateTime.Now;
                result.Duration = result.EndTime - result.StartTime;
                result.Status = _cancellationRequested ? 
                    TransferStatus.Cancelled : 
                    (result.FailedFiles > 0 ? TransferStatus.CompletedWithErrors : TransferStatus.Completed);

                _logService.LogJobEnd(job.Id, result.Status.ToString(), result.ProcessedFiles, result.FailedFiles);
                OnJobCompleted(new JobCompletedEventArgs { Job = job, Result = result });
            }
            catch (Exception ex)
            {
                _logService.LogError(job.Id, "Job execution failed: " + ex.Message);
                throw;
            }
            finally
            {
                _isRunning = false;
            }
        }

        public void CancelJob()
        {
            _cancellationRequested = true;
        }

        private void ExecuteJob(SyncJob job, TransferResult result)
        {
            var filesToProcess = GetFilesToProcess(job);
            result.TotalFiles = filesToProcess.Count;

            for (int i = 0; i < filesToProcess.Count && !_cancellationRequested; i++)
            {
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
                    _logService.LogError(job.Id, "Failed to process file '" + sourceFile + "': " + ex.Message);
                }

                result.ProcessedFiles++;
                var progressPercent = (result.ProcessedFiles * 100) / result.TotalFiles;
                OnJobProgress(new JobProgressEventArgs { Job = job, Result = result, ProgressPercent = progressPercent });
            }
        }

        private System.Collections.Generic.List<string> GetFilesToProcess(SyncJob job)
        {
            var files = new System.Collections.Generic.List<string>();

            if (job.Connection.Protocol == ProtocolType.Local)
            {
                if (System.IO.File.Exists(job.SourcePath))
                {
                    files.Add(job.SourcePath);
                }
                else if (System.IO.Directory.Exists(job.SourcePath))
                {
                    files.AddRange(_fileEnumerator.EnumerateFiles(job.SourcePath, job.Filters, job.IncludeSubfolders));
                }
            }
            else
            {
                var client = _transferClientFactory.Create(job.Connection.Protocol);
                string error;
                System.Collections.Generic.List<string> remoteFiles;
                
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
                DateTime transferStart = DateTime.Now;
                long fileSize = 0;

                // Get file size for logging
                try
                {
                    if (File.Exists(sourceFile))
                    {
                        fileSize = new FileInfo(sourceFile).Length;
                    }
                }
                catch
                {
                    fileSize = 0;
                }

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

                TimeSpan transferDuration = DateTime.Now - transferStart;
                string fileName = Path.GetFileName(sourceFile);

                if (!success)
                {
                    throw new Exception(error ?? "Transfer failed");
                }

                // Log successful transfer with proper details
                LogTransferWithDetails(job.Name, fileName, fileSize, transferDuration, sourceFile, destinationFile, true, null);

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
                TimeSpan transferDuration = DateTime.Now - transferStart;
                string fileName = Path.GetFileName(sourceFile);
                
                try
                {
                    if (File.Exists(sourceFile) && fileSize == 0)
                    {
                        fileSize = new FileInfo(sourceFile).Length;
                    }
                }
                catch
                {
                    fileSize = 0;
                }

                // Log failed transfer with proper details
                LogTransferWithDetails(job.Name, fileName, fileSize, transferDuration, sourceFile, destinationFile, false, ex.Message);

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

        /// <summary>
        /// Logs transfer with detailed information including file size, duration, and paths
        /// </summary>
        private void LogTransferWithDetails(string jobName, string fileName, long fileSize, TimeSpan duration, 
            string sourcePath, string destinationPath, bool success, string error)
        {
            try
            {
                // Use the enhanced logging method with all details
                if (_logService is FileLogService fileLogService)
                {
                    fileLogService.LogTransferDetailed(jobName, fileName, fileSize, duration, 
                        destinationPath, sourcePath, success, error);
                }
                else
                {
                    // Fallback to standard transfer logging
                    _logService.LogTransfer(jobName, fileName, fileSize, success, error);
                }
            }
            catch (Exception ex)
            {
                // Don't let logging errors affect the transfer process
                Console.WriteLine("Error logging transfer: " + ex.Message);
            }
        }
    }
}
