using System;
using System.Timers;
using System.Collections.Generic;

namespace syncer.ui.Interfaces
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
        /// Starts a timer job
        /// </summary>
        /// <param name="jobId">The ID of the job to start</param>
        /// <returns>True if the job was started successfully</returns>
        bool StartTimerJob(long jobId);
        
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
    }
}
