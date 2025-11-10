using System;
using System.Timers;
using System.Collections.Generic;

namespace FTPSyncer.ui.Interfaces
{
    /// <summary>
    /// Interface for managing timer-based jobs in the application
    /// </summary>
    public interface ITimerJobManager
    {
        /// <summary>
        /// Registers a timer job that will run automatically
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <param name="folderPath">The local folder path to monitor and upload</param>
        /// <param name="remotePath">The remote path to upload to</param>
        /// <param name="intervalMs">The timer interval in milliseconds</param>
        /// <returns>True if the job was registered successfully</returns>
        bool RegisterTimerJob(long jobId, string folderPath, string remotePath, double intervalMs);
        
        /// <summary>
        /// Registers a timer job that will run automatically with a custom name
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <param name="jobName">The custom name for the job</param>
        /// <param name="folderPath">The local folder path to monitor and upload</param>
        /// <param name="remotePath">The remote path to upload to</param>
        /// <param name="intervalMs">The timer interval in milliseconds</param>
        /// <returns>True if the job was registered successfully</returns>
        bool RegisterTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs);
        
        /// <summary>
        /// Registers a timer job with subfolder inclusion option
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <param name="jobName">The custom name for the job</param>
        /// <param name="folderPath">The local folder path to monitor and upload</param>
        /// <param name="remotePath">The remote path to upload to</param>
        /// <param name="intervalMs">The timer interval in milliseconds</param>
        /// <param name="includeSubfolders">Whether to include subfolders in file enumeration</param>
        /// <returns>True if the job was registered successfully</returns>
        bool RegisterTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders);
        
        /// <summary>
        /// Registers a timer job with subfolder inclusion and delete source after transfer options
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <param name="jobName">The custom name for the job</param>
        /// <param name="folderPath">The local folder path to monitor and upload</param>
        /// <param name="remotePath">The remote path to upload to</param>
        /// <param name="intervalMs">The timer interval in milliseconds</param>
        /// <param name="includeSubfolders">Whether to include subfolders in file enumeration</param>
        /// <param name="deleteSourceAfterTransfer">Whether to delete source files after successful transfer</param>
        /// <returns>True if the job was registered successfully</returns>
        bool RegisterTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders, bool deleteSourceAfterTransfer);
        
        /// <summary>
        /// Registers a timer job with complete filter support
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <param name="jobName">The name of the job</param>
        /// <param name="folderPath">The path to the folder to monitor</param>
        /// <param name="remotePath">The remote path to upload to</param>
        /// <param name="intervalMs">The interval in milliseconds</param>
        /// <param name="includeSubfolders">Whether to include subfolders</param>
        /// <param name="deleteSourceAfterTransfer">Whether to delete source files after transfer</param>
        /// <param name="enableFilters">Whether to enable file filtering</param>
        /// <param name="includeExtensions">List of file extensions to include</param>
        /// <param name="excludeExtensions">List of file extensions to exclude</param>
        /// <returns>True if the job was registered successfully</returns>
        bool RegisterTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders, bool deleteSourceAfterTransfer, bool enableFilters, List<string> includeExtensions, List<string> excludeExtensions);
        
        /// <summary>
        /// Starts a timer job
        /// </summary>
        /// <param name="jobId">The ID of the job to start</param>
        /// <returns>True if the job was started successfully</returns>
        bool StartTimerJob(long jobId);
        
        /// <summary>
        /// Starts multiple timer jobs in parallel
        /// </summary>
        /// <param name="jobIds">List of job IDs to start</param>
        /// <returns>Dictionary with job IDs as keys and success status as values</returns>
        Dictionary<long, bool> StartMultipleTimerJobs(List<long> jobIds);
        
        /// <summary>
        /// Stops a timer job
        /// </summary>
        /// <param name="jobId">The ID of the job to stop</param>
        /// <returns>True if the job was stopped successfully</returns>
        bool StopTimerJob(long jobId);
        
        /// <summary>
        /// Removes a timer job
        /// </summary>
        /// <param name="jobId">The ID of the job to remove</param>
        /// <returns>True if the job was removed successfully</returns>
        bool RemoveTimerJob(long jobId);
        
        /// <summary>
        /// Gets the status of a timer job
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>True if the job is running</returns>
        bool IsTimerJobRunning(long jobId);
        
        /// <summary>
        /// Gets a list of all registered timer job IDs
        /// </summary>
        /// <returns>List of job IDs</returns>
        List<long> GetRegisteredTimerJobs();
        
        /// <summary>
        /// Gets the last upload time for a job
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>DateTime of the last upload or null if never uploaded</returns>
        DateTime? GetLastUploadTime(long jobId);
        
        /// <summary>
        /// Gets the folder path for a timer job
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>The folder path or null if job not found</returns>
        string GetTimerJobFolderPath(long jobId);
        
        /// <summary>
        /// Gets the remote path for a timer job
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>The remote path or null if job not found</returns>
        string GetTimerJobRemotePath(long jobId);
        
        /// <summary>
        /// Gets the interval in milliseconds for a timer job
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>The interval in milliseconds or 0 if job not found</returns>
        double GetTimerJobInterval(long jobId);
        
        /// <summary>
        /// Gets the job name for a timer job
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>The job name or null if job not found</returns>
        string GetTimerJobName(long jobId);
        
        /// <summary>
        /// Updates a timer job with new settings
        /// </summary>
        /// <param name="jobId">The ID of the job to update</param>
        /// <param name="jobName">The new job name</param>
        /// <param name="folderPath">The new local folder path</param>
        /// <param name="remotePath">The new remote path</param>
        /// <param name="intervalMs">The new timer interval in milliseconds</param>
        /// <returns>True if the job was updated successfully</returns>
        bool UpdateTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs);
        
        /// <summary>
        /// Updates a timer job with new settings including subfolder inclusion
        /// </summary>
        /// <param name="jobId">The ID of the job to update</param>
        /// <param name="jobName">The new job name</param>
        /// <param name="folderPath">The new local folder path</param>
        /// <param name="remotePath">The new remote path</param>
        /// <param name="intervalMs">The new timer interval in milliseconds</param>
        /// <param name="includeSubfolders">Whether to include subfolders in file enumeration</param>
        /// <returns>True if the job was updated successfully</returns>
        bool UpdateTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders);
        
        /// <summary>
        /// Updates a timer job with new settings including subfolder inclusion and delete source after transfer options
        /// </summary>
        /// <param name="jobId">The ID of the job to update</param>
        /// <param name="jobName">The new job name</param>
        /// <param name="folderPath">The new local folder path</param>
        /// <param name="remotePath">The new remote path</param>
        /// <param name="intervalMs">The new timer interval in milliseconds</param>
        /// <param name="includeSubfolders">Whether to include subfolders in file enumeration</param>
        /// <param name="deleteSourceAfterTransfer">Whether to delete source files after successful transfer</param>
        /// <returns>True if the job was updated successfully</returns>
        bool UpdateTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders, bool deleteSourceAfterTransfer);
        
        /// <summary>
        /// Updates a timer job with complete filter support
        /// </summary>
        /// <param name="jobId">The ID of the job to update</param>
        /// <param name="jobName">The name of the job</param>
        /// <param name="folderPath">The path to the folder to monitor</param>
        /// <param name="remotePath">The remote path to upload to</param>
        /// <param name="intervalMs">The interval in milliseconds</param>
        /// <param name="includeSubfolders">Whether to include subfolders</param>
        /// <param name="deleteSourceAfterTransfer">Whether to delete source files after transfer</param>
        /// <param name="enableFilters">Whether to enable file filtering</param>
        /// <param name="includeExtensions">List of file extensions to include</param>
        /// <param name="excludeExtensions">List of file extensions to exclude</param>
        /// <returns>True if the job was updated successfully</returns>
        bool UpdateTimerJob(long jobId, string jobName, string folderPath, string remotePath, double intervalMs, bool includeSubfolders, bool deleteSourceAfterTransfer, bool enableFilters, List<string> includeExtensions, List<string> excludeExtensions);
        
        /// <summary>
        /// Gets all running timer jobs
        /// </summary>
        /// <returns>Dictionary with job IDs as keys and job info as values</returns>
        Dictionary<long, object> GetRunningJobs();
        
        /// <summary>
        /// Gets all timer jobs (running and stopped)
        /// </summary>
        /// <returns>Dictionary with job IDs as keys and job info as values</returns>
        Dictionary<long, object> GetAllJobs();
        
        /// <summary>
        /// Checks if a timer job is currently performing an upload
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>True if the job is currently uploading</returns>
        bool IsTimerJobUploading(long jobId);
        
        /// <summary>
        /// Gets the upload start time for a timer job
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>DateTime when the current upload started, or null if not uploading</returns>
        DateTime? GetTimerJobUploadStartTime(long jobId);
        
        // Download-specific methods
        
        /// <summary>
        /// Registers a download timer job that will download files from remote to local
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <param name="jobName">The name of the download job</param>
        /// <param name="remoteFolderPath">The remote folder path to download from</param>
        /// <param name="localDestinationPath">The local destination path to save files to</param>
        /// <param name="intervalMs">The timer interval in milliseconds</param>
        /// <param name="includeSubfolders">Whether to include subfolders in download</param>
        /// <param name="deleteSourceAfterTransfer">Whether to delete remote files after successful download</param>
        /// <param name="enableFilters">Whether to enable file filtering</param>
        /// <param name="includeExtensions">List of file extensions to include</param>
        /// <param name="excludeExtensions">List of file extensions to exclude</param>
        /// <returns>True if the download job was registered successfully</returns>
        bool RegisterDownloadTimerJob(long jobId, string jobName, string remoteFolderPath, string localDestinationPath, double intervalMs, bool includeSubfolders, bool deleteSourceAfterTransfer, bool enableFilters, List<string> includeExtensions, List<string> excludeExtensions);
        
        /// <summary>
        /// Gets the last download time for a job
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>DateTime of the last download or null if never downloaded</returns>
        DateTime? GetLastDownloadTime(long jobId);
        
        /// <summary>
        /// Checks if a timer job is currently performing a download
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>True if the job is currently downloading</returns>
        bool IsTimerJobDownloading(long jobId);
        
        /// <summary>
        /// Gets the download start time for a timer job
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>DateTime when the current download started, or null if not downloading</returns>
        DateTime? GetTimerJobDownloadStartTime(long jobId);
        
        /// <summary>
        /// Checks if a timer job is a download job (remote to local)
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>True if the job is a download job</returns>
        bool IsTimerJobDownloadJob(long jobId);
        
        /// <summary>
        /// Gets whether the timer job includes subfolders
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>True if subfolders are included</returns>
        bool GetTimerJobIncludeSubfolders(long jobId);
        
        /// <summary>
        /// Gets whether the timer job deletes source files after transfer
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>True if source files are deleted after transfer</returns>
        bool GetTimerJobDeleteSourceAfterTransfer(long jobId);
        
        /// <summary>
        /// Gets whether the timer job has file filtering enabled
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>True if file filtering is enabled</returns>
        bool GetTimerJobEnableFilters(long jobId);
        
        /// <summary>
        /// Gets the list of file extensions to include for the timer job
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>List of file extensions to include</returns>
        List<string> GetTimerJobIncludeExtensions(long jobId);
        
        /// <summary>
        /// Gets the list of file extensions to exclude for the timer job
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>List of file extensions to exclude</returns>
        List<string> GetTimerJobExcludeExtensions(long jobId);
        
        /// <summary>
        /// Gets whether the timer job should run on application startup
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>True if job should auto-start on application startup</returns>
        bool GetTimerJobRunOnStartup(long jobId);
        
        /// <summary>
        /// Sets whether the timer job should run on application startup
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <param name="runOnStartup">True to auto-start job on application startup</param>
        /// <returns>True if the setting was updated successfully</returns>
        bool SetTimerJobRunOnStartup(long jobId, bool runOnStartup);
        
        /// <summary>
        /// Saves the current state of all timer jobs to persistent storage
        /// </summary>
        void SaveTimerJobsState();
        
        /// <summary>
        /// Gets timer job info for a specific job (for internal use)
        /// </summary>
        /// <param name="jobId">The ID of the job</param>
        /// <returns>Timer job info object or null if not found</returns>
        object GetTimerJobInfo(long jobId);
    }
}





