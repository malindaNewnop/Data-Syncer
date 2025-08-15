using System;
using System.Collections.Generic;
using syncer.core;

namespace syncer.ui.Services
{
    /// <summary>
    /// Adapter to connect UI SyncJobService interface to Core JobRepository and JobRunner implementations
    /// </summary>
    public class CoreSyncJobServiceAdapter : ISyncJobService
    {
        private readonly syncer.core.IJobRepository _jobRepository;
        private readonly syncer.core.IJobRunner _jobRunner;
        private readonly syncer.core.ILogService _logService;

        public CoreSyncJobServiceAdapter()
        {
            try
            {
                _jobRepository = syncer.core.ServiceFactory.CreateJobRepository();
                _logService = syncer.core.ServiceFactory.CreateLogService();
                var transferClientFactory = syncer.core.ServiceFactory.CreateTransferClientFactory();
                var fileEnumerator = syncer.core.ServiceFactory.CreateFileEnumerator();
                _jobRunner = syncer.core.ServiceFactory.CreateJobRunner(transferClientFactory, _logService, fileEnumerator);
                
                DebugLogger.LogServiceActivity("CoreSyncJobServiceAdapter", "Initialized successfully");
            }
            catch (Exception ex)
            {
                DebugLogger.LogError(ex, "CoreSyncJobServiceAdapter initialization");
                throw;
            }
        }

        public List<SyncJob> GetAllJobs()
        {
            List<SyncJob> uiJobs = new List<SyncJob>();
            List<syncer.core.SyncJob> coreJobs = _jobRepository.GetAll();

            foreach (var coreJob in coreJobs)
            {
                uiJobs.Add(ConvertCoreJobToUIJob(coreJob));
            }

            return uiJobs;
        }

        public SyncJob GetJobById(int id)
        {
            var coreJobs = _jobRepository.GetAll();
            foreach (var coreJob in coreJobs)
            {
                if (coreJob.Id == id.ToString())
                {
                    return ConvertCoreJobToUIJob(coreJob);
                }
            }
            return null;
        }

        public int CreateJob(SyncJob job)
        {
            var coreJob = ConvertUIJobToCoreJob(job);
            
            // Generate a unique ID if needed
            if (string.IsNullOrEmpty(coreJob.Id))
            {
                coreJob.Id = Guid.NewGuid().ToString();
            }
            
            // Set creation date
            coreJob.CreatedDate = DateTime.Now;
            
            // Save the job
            _jobRepository.Save(coreJob);
            
            // Try to parse the ID as an integer
            int jobId;
            if (int.TryParse(coreJob.Id, out jobId))
            {
                return jobId;
            }
            else
            {
                // If the ID isn't an integer, generate a unique int ID
                return Math.Abs(coreJob.Id.GetHashCode());
            }
        }

        public bool UpdateJob(SyncJob job)
        {
            try
            {
                var coreJob = ConvertUIJobToCoreJob(job);
                _jobRepository.Save(coreJob);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteJob(int id)
        {
            try
            {
                _jobRepository.Delete(id.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool StartJob(int id)
        {
            try
            {
                var coreJob = _jobRepository.GetById(id.ToString());
                if (coreJob == null)
                    return false;

                coreJob.IsEnabled = true;
                _jobRepository.Save(coreJob);
                
                // Actually run the job
                _jobRunner.RunJob(coreJob);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool StopJob(int id)
        {
            try
            {
                var coreJob = _jobRepository.GetById(id.ToString());
                if (coreJob == null)
                    return false;

                coreJob.IsEnabled = false;
                _jobRepository.Save(coreJob);
                
                // If the job is running, cancel it
                if (_jobRunner.IsRunning)
                {
                    _jobRunner.CancelJob();
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetJobStatus(int id)
        {
            try
            {
                var coreJob = _jobRepository.GetById(id.ToString());
                if (coreJob == null)
                    return "Not Found";
                
                if (_jobRunner.IsRunning && coreJob.IsRunning)
                    return "Running";
                    
                return coreJob.IsEnabled ? "Enabled" : "Disabled";
            }
            catch
            {
                return "Unknown";
            }
        }

        // Helper methods to convert between UI and Core job models
        private SyncJob ConvertCoreJobToUIJob(syncer.core.SyncJob coreJob)
        {
            SyncJob uiJob = new SyncJob
            {
                Id = int.TryParse(coreJob.Id, out int id) ? id : Math.Abs(coreJob.Id.GetHashCode()),
                Name = coreJob.Name,
                Description = coreJob.Description,
                IsEnabled = coreJob.IsEnabled,
                SourcePath = coreJob.SourcePath,
                DestinationPath = coreJob.DestinationPath,
                IncludeSubFolders = coreJob.IncludeSubfolders,
                OverwriteExisting = coreJob.OverwriteExisting,
                CreatedDate = coreJob.CreatedDate,
                LastRun = coreJob.LastRun,
                LastStatus = coreJob.LastStatus,
                // Add schedule settings
                StartTime = coreJob.Schedule?.StartTime ?? DateTime.Now,
                IntervalValue = coreJob.Schedule?.RepeatEvery ?? 60,
                IntervalType = MapTimeUnitToString(coreJob.Schedule?.Unit ?? syncer.core.TimeUnit.Minutes)
            };

            return uiJob;
        }

        private syncer.core.SyncJob ConvertUIJobToCoreJob(SyncJob uiJob)
        {
            syncer.core.SyncJob coreJob = new syncer.core.SyncJob
            {
                Id = uiJob.Id.ToString(),
                Name = uiJob.Name,
                Description = uiJob.Description,
                IsEnabled = uiJob.IsEnabled,
                SourcePath = uiJob.SourcePath,
                DestinationPath = uiJob.DestinationPath,
                IncludeSubfolders = uiJob.IncludeSubFolders,
                OverwriteExisting = uiJob.OverwriteExisting,
                CreatedDate = uiJob.CreatedDate,
                LastRun = uiJob.LastRun.HasValue ? uiJob.LastRun.Value : DateTime.MinValue,
                LastStatus = uiJob.LastStatus,
                Connection = new syncer.core.ConnectionSettings { Protocol = syncer.core.ProtocolType.Local },
                DestinationConnection = new syncer.core.ConnectionSettings { Protocol = syncer.core.ProtocolType.Local },
                // Add schedule settings
                Schedule = new syncer.core.ScheduleSettings
                {
                    StartTime = uiJob.StartTime,
                    RepeatEvery = uiJob.IntervalValue,
                    Unit = MapStringToTimeUnit(uiJob.IntervalType),
                    IsEnabled = uiJob.IsEnabled
                }
            };

            return coreJob;
        }
        
        private string MapTimeUnitToString(syncer.core.TimeUnit unit)
        {
            switch (unit)
            {
                case syncer.core.TimeUnit.Minutes:
                    return "Minutes";
                case syncer.core.TimeUnit.Hours:
                    return "Hours";
                case syncer.core.TimeUnit.Days:
                    return "Days";
                default:
                    return "Minutes";
            }
        }
        
        private syncer.core.TimeUnit MapStringToTimeUnit(string intervalType)
        {
            switch (intervalType?.ToLower())
            {
                case "minutes":
                    return syncer.core.TimeUnit.Minutes;
                case "hours":
                    return syncer.core.TimeUnit.Hours;
                case "days":
                    return syncer.core.TimeUnit.Days;
                default:
                    return syncer.core.TimeUnit.Minutes;
            }
        }
    }
}
