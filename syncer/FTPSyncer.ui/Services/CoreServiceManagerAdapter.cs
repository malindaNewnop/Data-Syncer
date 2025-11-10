using System;
using FTPSyncer.core;

namespace FTPSyncer.ui.Services
{
    /// <summary>
    /// Real service manager implementation that properly manages the job runner and scheduling
    /// </summary>
    public class CoreServiceManagerAdapter : IServiceManager
    {
        private readonly IMultiJobRunner _multiJobRunner;
        private readonly FTPSyncer.core.ILogService _coreLogService;
        private bool _isRunning = false;
        private readonly object _lockObject = new object();
        
        public CoreServiceManagerAdapter()
        {
            try
            {
                // Create the job runner and log service separately
                _coreLogService = FTPSyncer.core.ServiceFactory.CreateLogService();
                var jobRunner = FTPSyncer.core.ServiceFactory.CreateJobRunnerFromConfiguration();
                
                // Check if it's a multi-job runner
                _multiJobRunner = jobRunner as IMultiJobRunner;
                if (_multiJobRunner == null)
                {
                    // If we got a single job runner, create a multi-job runner instead
                    _multiJobRunner = FTPSyncer.core.ServiceFactory.CreateMultiJobRunner();
                }
                
                ServiceLocator.LogService.LogInfo("CoreServiceManagerAdapter initialized with MultiJobRunner", "ServiceManager");
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Failed to initialize CoreServiceManagerAdapter: " + ex.Message, "ServiceManager");
                throw;
            }
        }
        
        public bool StartService()
        {
            lock (_lockObject)
            {
                try
                {
                    if (_isRunning)
                    {
                        ServiceLocator.LogService.LogWarning("Service is already running", "ServiceManager");
                        return true;
                    }
                    
                    // Start all due jobs automatically when service starts
                    var startedJobs = _multiJobRunner.StartDueJobs();
                    
                    _isRunning = true;
                    ServiceLocator.LogService.LogInfo("FTPSyncer service started successfully", "ServiceManager");
                    
                    return true;
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogError("Failed to start service: " + ex.Message, "ServiceManager");
                    return false;
                }
            }
        }
        
        public bool StopService()
        {
            lock (_lockObject)
            {
                try
                {
                    if (!_isRunning)
                    {
                        ServiceLocator.LogService.LogWarning("Service is already stopped", "ServiceManager");
                        return true;
                    }
                    
                    // First stop the scheduler to prevent any new jobs from being started
                    if (_multiJobRunner is IDisposable disposableRunner)
                    {
                        // Stop the queue processor timer
                        disposableRunner.Dispose();
                        ServiceLocator.LogService.LogInfo("Job scheduler stopped", "ServiceManager");
                    }
                    
                    // Stop all running jobs
                    var runningJobs = _multiJobRunner.GetRunningJobIds();
                    foreach (var jobId in runningJobs)
                    {
                        try
                        {
                            _multiJobRunner.CancelJob(jobId);
                            ServiceLocator.LogService.LogInfo("Stopped job: " + jobId, "ServiceManager");
                        }
                        catch (Exception ex)
                        {
                            ServiceLocator.LogService.LogError("Failed to stop job " + jobId + ": " + ex.Message, "ServiceManager");
                        }
                    }
                    
                    // Wait a bit for jobs to stop gracefully
                    System.Threading.Thread.Sleep(2000);
                    
                    _isRunning = false;
                    ServiceLocator.LogService.LogInfo("FTPSyncer service stopped successfully", "ServiceManager");
                    
                    return true;
                }
                catch (Exception ex)
                {
                    ServiceLocator.LogService.LogError("Failed to stop service: " + ex.Message, "ServiceManager");
                    return false;
                }
            }
        }
        
        public bool IsServiceRunning()
        {
            lock (_lockObject)
            {
                return _isRunning;
            }
        }
        
        public string GetServiceStatus()
        {
            lock (_lockObject)
            {
                if (!_isRunning)
                    return "Stopped";
                    
                var runningJobs = _multiJobRunner.GetRunningJobIds();
                if (runningJobs.Count > 0)
                {
                    return string.Format("Running ({0} active jobs)", runningJobs.Count);
                }
                else
                {
                    return "Running (idle)";
                }
            }
        }
        
        /// <summary>
        /// Get the underlying multi-job runner for advanced operations
        /// </summary>
        public IMultiJobRunner GetMultiJobRunner()
        {
            return _multiJobRunner;
        }
        
        /// <summary>
        /// Start specific jobs manually
        /// </summary>
        public bool StartSelectedJobs(System.Collections.Generic.List<string> jobIds)
        {
            if (!_isRunning)
            {
                ServiceLocator.LogService.LogWarning("Cannot start jobs - service is not running", "ServiceManager");
                return false;
            }
            
            try
            {
                return _multiJobRunner.StartSelectedJobs(jobIds);
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Failed to start selected jobs: " + ex.Message, "ServiceManager");
                return false;
            }
        }
        
        /// <summary>
        /// Stop specific jobs manually
        /// </summary>
        public bool StopSelectedJobs(System.Collections.Generic.List<string> jobIds)
        {
            if (!_isRunning)
            {
                ServiceLocator.LogService.LogWarning("Cannot stop jobs - service is not running", "ServiceManager");
                return false;
            }
            
            try
            {
                foreach (var jobId in jobIds)
                {
                    _multiJobRunner.CancelJob(jobId);
                }
                return true;
            }
            catch (Exception ex)
            {
                ServiceLocator.LogService.LogError("Failed to stop selected jobs: " + ex.Message, "ServiceManager");
                return false;
            }
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
                    // Dispose managed resources
                    if (_multiJobRunner is IDisposable disposableRunner)
                    {
                        disposableRunner.Dispose();
                    }
                }
                
                // Free unmanaged resources
                
                _disposed = true;
            }
        }
        
        ~CoreServiceManagerAdapter()
        {
            Dispose(false);
        }
        
        #endregion
    }
}





