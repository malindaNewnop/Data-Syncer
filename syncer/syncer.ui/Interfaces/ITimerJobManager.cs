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
    }
}
