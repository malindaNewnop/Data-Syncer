using System;
using System.Collections.Generic;
using FTPSyncer.core;

namespace FTPSyncer.ui.Services
{
    /// <summary>
    /// Adapter to connect UI SyncJobService interface to Core JobRepository and JobRunner implementations
    /// </summary>
    public class CoreSyncJobServiceAdapter : ISyncJobService
    {
        private readonly FTPSyncer.core.IJobRepository _jobRepository;
        private readonly FTPSyncer.core.IJobRunner _jobRunner;
        private readonly FTPSyncer.core.ILogService _logService;

        public CoreSyncJobServiceAdapter()
        {
            try
            {
                _jobRepository = FTPSyncer.core.ServiceFactory.CreateJobRepository();
                _logService = FTPSyncer.core.ServiceFactory.CreateLogService();
                
                // Use the MultiJobRunner for concurrent job execution
                _jobRunner = FTPSyncer.core.ServiceFactory.CreateJobRunnerFromConfiguration();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<SyncJob> GetAllJobs()
        {
            List<SyncJob> uiJobs = new List<SyncJob>();
            List<FTPSyncer.core.SyncJob> coreJobs = _jobRepository.GetAll();

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
            try
            {
                var coreJob = ConvertUIJobToCoreJob(job);
                
                // Generate a unique job ID using centralized generator
                long numericJobId = JobIdGenerator.Instance.GenerateNumericJobId();
                coreJob.Id = numericJobId.ToString();
                
                // Set creation date
                coreJob.CreatedDate = DateTime.Now;
                
                // Save the job
                _jobRepository.Save(coreJob);
                
                // Return as int for backward compatibility
                return (int)numericJobId;
            }
            catch (Exception)
            {
                throw;
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
                return _jobRunner.StartJob(id.ToString());
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool StopJob(int id)
        {
            try
            {
                return _jobRunner.CancelJob(id.ToString());
            }
            catch (Exception)
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
                
                if (_jobRunner.IsRunning(id.ToString()))
                    return "Running";
                    
                return coreJob.IsEnabled ? "Enabled" : "Disabled";
            }
            catch
            {
                return "Unknown";
            }
        }

        // Helper methods to convert between UI and Core job models
        private SyncJob ConvertCoreJobToUIJob(FTPSyncer.core.SyncJob coreJob)
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
                IntervalType = MapTimeUnitToString(coreJob.Schedule?.Unit ?? FTPSyncer.core.TimeUnit.Minutes),
                // Convert connection settings
                SourceConnection = ConvertCoreConnectionToUIConnection(coreJob.Connection),
                DestinationConnection = ConvertCoreConnectionToUIConnection(coreJob.DestinationConnection)
            };

            // Convert filter settings from Core to UI
            ConvertFilterSettingsToUI(coreJob, uiJob);

            return uiJob;
        }

        private FTPSyncer.core.SyncJob ConvertUIJobToCoreJob(SyncJob uiJob)
        {
        
            FTPSyncer.core.SyncJob coreJob = new FTPSyncer.core.SyncJob
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
                Connection = ConvertUIConnectionToCoreConnection(uiJob.SourceConnection), // Legacy field for compatibility
                SourceConnection = ConvertUIConnectionToCoreConnection(uiJob.SourceConnection),
                DestinationConnection = ConvertUIConnectionToCoreConnection(uiJob.DestinationConnection),
                Schedule = new FTPSyncer.core.ScheduleSettings
                {
                    StartTime = uiJob.StartTime,
                    RepeatEvery = uiJob.IntervalValue,
                    Unit = MapStringToTimeUnit(uiJob.IntervalType),
                    IsEnabled = uiJob.IsEnabled
                }
            };

            // Convert filter settings from UI to Core
            ConvertFilterSettings(uiJob, coreJob);

            return coreJob;
        }
        
        private string MapTimeUnitToString(FTPSyncer.core.TimeUnit unit)
        {
            switch (unit)
            {
                case FTPSyncer.core.TimeUnit.Minutes:
                    return "Minutes";
                case FTPSyncer.core.TimeUnit.Hours:
                    return "Hours";
                case FTPSyncer.core.TimeUnit.Days:
                    return "Days";
                default:
                    return "Minutes";
            }
        }
        
        private FTPSyncer.core.TimeUnit MapStringToTimeUnit(string intervalType)
        {
            switch (intervalType?.ToLower())
            {
                case "minutes":
                    return FTPSyncer.core.TimeUnit.Minutes;
                case "hours":
                    return FTPSyncer.core.TimeUnit.Hours;
                case "days":
                    return FTPSyncer.core.TimeUnit.Days;
                default:
                    return FTPSyncer.core.TimeUnit.Minutes;
            }
        }

        /// <summary>
        /// Converts filter settings from UI model to Core model
        /// </summary>
        private void ConvertFilterSettings(SyncJob uiJob, FTPSyncer.core.SyncJob coreJob)
        {
            // Only create and populate filters if they are enabled
            if (uiJob.EnableFilters)
            {
                if (coreJob.Filters == null)
                    coreJob.Filters = new FilterSettings();

                // Convert include file types from comma-separated string to list
                coreJob.Filters.IncludeExtensions.Clear();
                if (!string.IsNullOrEmpty(uiJob.IncludeFileTypes))
                {
                    string[] includeTypes = uiJob.IncludeFileTypes.Split(',');
                    foreach (string type in includeTypes)
                    {
                        string cleanType = type.Trim().TrimStart('.');
                        if (!string.IsNullOrEmpty(cleanType))
                        {
                            coreJob.Filters.IncludeExtensions.Add(cleanType);
                        }
                    }
                }

                // Convert exclude file types from comma-separated string to list
                coreJob.Filters.ExcludeExtensions.Clear();
                if (!string.IsNullOrEmpty(uiJob.ExcludeFileTypes))
                {
                    string[] excludeTypes = uiJob.ExcludeFileTypes.Split(',');
                    foreach (string type in excludeTypes)
                    {
                        string cleanType = type.Trim().TrimStart('.');
                        if (!string.IsNullOrEmpty(cleanType))
                        {
                            coreJob.Filters.ExcludeExtensions.Add(cleanType);
                        }
                    }
                }
            }
            else
            {
                // Filters disabled, set to null to indicate no filtering should occur
                coreJob.Filters = null;
            }
        }

        /// <summary>
        /// Converts filter settings from Core model to UI model
        /// </summary>
        private void ConvertFilterSettingsToUI(FTPSyncer.core.SyncJob coreJob, SyncJob uiJob)
        {
            if (coreJob.Filters != null)
            {
                // Check if filters are enabled (if any extensions exist)
                uiJob.EnableFilters = coreJob.Filters.IncludeExtensions.Count > 0 || coreJob.Filters.ExcludeExtensions.Count > 0;

                // Convert include extensions to comma-separated string
                if (coreJob.Filters.IncludeExtensions.Count > 0)
                {
                    System.Text.StringBuilder includeBuilder = new System.Text.StringBuilder();
                    foreach (string extension in coreJob.Filters.IncludeExtensions)
                    {
                        if (includeBuilder.Length > 0)
                            includeBuilder.Append(",");
                        includeBuilder.Append(extension);
                    }
                    uiJob.IncludeFileTypes = includeBuilder.ToString();
                }

                // Convert exclude extensions to comma-separated string
                if (coreJob.Filters.ExcludeExtensions.Count > 0)
                {
                    System.Text.StringBuilder excludeBuilder = new System.Text.StringBuilder();
                    foreach (string extension in coreJob.Filters.ExcludeExtensions)
                    {
                        if (excludeBuilder.Length > 0)
                            excludeBuilder.Append(",");
                        excludeBuilder.Append(extension);
                    }
                    uiJob.ExcludeFileTypes = excludeBuilder.ToString();
                }
            }
            else
            {
                // No filters defined
                uiJob.EnableFilters = false;
                uiJob.IncludeFileTypes = string.Empty;
                uiJob.ExcludeFileTypes = string.Empty;
            }
        }

        /// <summary>
        /// Converts Core ConnectionSettings to UI ConnectionSettings
        /// </summary>
        private ConnectionSettings ConvertCoreConnectionToUIConnection(FTPSyncer.core.ConnectionSettings coreConnection)
        {
            if (coreConnection == null)
            {
                return new ConnectionSettings(); // Return default local connection
            }

            var uiConnection = new ConnectionSettings
            {
                ProtocolType = (int)coreConnection.Protocol,
                Protocol = coreConnection.Protocol.ToString().ToUpper(),
                Host = coreConnection.Host,
                Port = coreConnection.Port,
                Username = coreConnection.Username,
                Password = coreConnection.Password,
                UsePassiveMode = coreConnection.UsePassiveMode,
                UseSftp = coreConnection.UseSftp,
                SshKeyPath = coreConnection.SshKeyPath,
                Timeout = coreConnection.Timeout
            };

            return uiConnection;
        }

        /// <summary>
        /// Converts UI ConnectionSettings to Core ConnectionSettings
        /// </summary>
        private FTPSyncer.core.ConnectionSettings ConvertUIConnectionToCoreConnection(ConnectionSettings uiConnection)
        {
            if (uiConnection == null)
            {
                return new FTPSyncer.core.ConnectionSettings { Protocol = FTPSyncer.core.ProtocolType.Local };
            }

            var coreConnection = new FTPSyncer.core.ConnectionSettings
            {
                Protocol = (FTPSyncer.core.ProtocolType)uiConnection.ProtocolType,
                Host = uiConnection.Host,
                Port = uiConnection.Port,
                Username = uiConnection.Username,
                Password = uiConnection.Password,
                UsePassiveMode = uiConnection.UsePassiveMode,
                UseSftp = uiConnection.UseSftp,
                SshKeyPath = uiConnection.SshKeyPath,
                Timeout = uiConnection.Timeout
            };

            return coreConnection;
        }

        /// <summary>
        /// Get a description for a file extension
        /// </summary>
        private string GetFileTypeDescription(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case "txt": return "Text files";
                case "doc": case "docx": return "Word documents";
                case "xls": case "xlsx": return "Excel files";
                case "pdf": return "PDF documents";
                case "jpg": case "jpeg": return "JPEG images";
                case "png": return "PNG images";
                case "gif": return "GIF images";
                case "mp4": return "Video files";
                case "mp3": return "Audio files";
                case "zip": case "rar": return "Archive files";
                case "exe": return "Executable files";
                case "dll": return "Library files";
                case "log": return "Log files";
                case "csv": return "CSV files";
                case "xml": return "XML files";
                case "json": return "JSON files";
                default: return "Custom extension";
            }
        }

        public void ReloadJobsFromPersistence()
        {
            // This adapter doesn't use persistence like ServiceImplementations does
            // The core job repository handles its own persistence
            // This method is here to satisfy the interface but doesn't need implementation
        }
    }
}





