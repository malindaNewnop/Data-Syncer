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
        private static int _nextJobId = 1;

        public CoreSyncJobServiceAdapter()
        {
            try
            {
                _jobRepository = syncer.core.ServiceFactory.CreateJobRepository();
                _logService = syncer.core.ServiceFactory.CreateLogService();
                
                // Use the MultiJobRunner for concurrent job execution
                _jobRunner = syncer.core.ServiceFactory.CreateJobRunnerFromConfiguration();
                
                // Initialize the next job ID based on existing jobs
                InitializeJobIdSequence();
                
                DebugLogger.LogServiceActivity("CoreSyncJobServiceAdapter", "Initialized successfully with MultiJobRunner");
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
            try
            {
                // Apply current filter settings to the job before creating it
                ApplyCurrentFilterSettings(job);
                
                var coreJob = ConvertUIJobToCoreJob(job);
                
                // Generate a unique sequential integer ID
                int newJobId = GetNextJobId();
                coreJob.Id = newJobId.ToString();
                
                // Set creation date
                coreJob.CreatedDate = DateTime.Now;
                
                // Save the job
                _jobRepository.Save(coreJob);
                
                DebugLogger.LogServiceActivity("CoreSyncJobServiceAdapter", 
                    string.Format("Created job '{0}' with ID {1}", coreJob.Name, coreJob.Id));
                
                return newJobId;
            }
            catch (Exception ex)
            {
                DebugLogger.LogError(ex, "CoreSyncJobServiceAdapter.CreateJob");
                throw;
            }
        }
        
        private void InitializeJobIdSequence()
        {
            try
            {
                var existingJobs = _jobRepository.GetAll();
                int maxId = 0;
                
                foreach (var job in existingJobs)
                {
                    int id;
                    if (int.TryParse(job.Id, out id) && id > maxId)
                    {
                        maxId = id;
                    }
                }
                
                _nextJobId = maxId + 1;
                
                DebugLogger.LogServiceActivity("CoreSyncJobServiceAdapter", 
                    string.Format("Initialized job ID sequence. Next ID: {0}", _nextJobId));
            }
            catch (Exception ex)
            {
                DebugLogger.LogError(ex, "CoreSyncJobServiceAdapter.InitializeJobIdSequence");
                _nextJobId = 1; // Fallback to 1
            }
        }
        
        private int GetNextJobId()
        {
            return _nextJobId++;
        }

        public bool UpdateJob(SyncJob job)
        {
            try
            {
                // Apply current filter settings to the job before updating it
                ApplyCurrentFilterSettings(job);
                
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
            catch (Exception ex)
            {
                DebugLogger.LogError(ex, $"Failed to start job {id}");
                return false;
            }
        }

        public bool StopJob(int id)
        {
            try
            {
                return _jobRunner.CancelJob(id.ToString());
            }
            catch (Exception ex)
            {
                DebugLogger.LogError(ex, $"Failed to stop job {id}");
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
                IntervalType = MapTimeUnitToString(coreJob.Schedule?.Unit ?? syncer.core.TimeUnit.Minutes),
                // Convert connection settings
                SourceConnection = ConvertCoreConnectionToUIConnection(coreJob.Connection),
                DestinationConnection = ConvertCoreConnectionToUIConnection(coreJob.DestinationConnection),
                // Convert filter settings
                FilterSettings = ConvertCoreFilterToUIFilter(coreJob.Filters)
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
                Connection = ConvertUIConnectionToCoreConnection(uiJob.SourceConnection),
                DestinationConnection = ConvertUIConnectionToCoreConnection(uiJob.DestinationConnection),
                // Add schedule settings
                Schedule = new syncer.core.ScheduleSettings
                {
                    StartTime = uiJob.StartTime,
                    RepeatEvery = uiJob.IntervalValue,
                    Unit = MapStringToTimeUnit(uiJob.IntervalType),
                    IsEnabled = uiJob.IsEnabled
                },
                // Convert filter settings
                Filters = ConvertUIFilterToCoreFilter(uiJob.FilterSettings)
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

        /// <summary>
        /// Converts Core ConnectionSettings to UI ConnectionSettings
        /// </summary>
        private ConnectionSettings ConvertCoreConnectionToUIConnection(syncer.core.ConnectionSettings coreConnection)
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
        private syncer.core.ConnectionSettings ConvertUIConnectionToCoreConnection(ConnectionSettings uiConnection)
        {
            if (uiConnection == null)
            {
                return new syncer.core.ConnectionSettings { Protocol = syncer.core.ProtocolType.Local };
            }

            var coreConnection = new syncer.core.ConnectionSettings
            {
                Protocol = (syncer.core.ProtocolType)uiConnection.ProtocolType,
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
        /// Converts Core FilterSettings to UI FilterSettings
        /// </summary>
        private FilterSettings ConvertCoreFilterToUIFilter(syncer.core.FilterSettings coreFilter)
        {
            if (coreFilter == null)
            {
                return new FilterSettings(); // Return default filter
            }

            var uiFilter = new FilterSettings
            {
                FiltersEnabled = coreFilter.IncludeExtensions.Count > 0 || coreFilter.ExcludeExtensions.Count > 0 || 
                                coreFilter.MinSizeKB > 0 || coreFilter.MaxSizeKB > 0 ||
                                !string.IsNullOrEmpty(coreFilter.IncludePattern) || !string.IsNullOrEmpty(coreFilter.ExcludePattern),
                MinFileSize = (decimal)(coreFilter.MinSizeKB / 1024.0), // Convert KB to MB
                MaxFileSize = (decimal)(coreFilter.MaxSizeKB / 1024.0), // Convert KB to MB
                ExcludePatterns = coreFilter.ExcludePattern ?? "",
                IncludeHiddenFiles = coreFilter.IncludeHidden,
                IncludeSystemFiles = coreFilter.IncludeSystem,
                IncludeReadOnlyFiles = coreFilter.IncludeReadOnly
            };

            // Convert include extensions to simple format
            if (coreFilter.IncludeExtensions.Count > 0)
            {
                var extensions = new List<string>();
                foreach (var ext in coreFilter.IncludeExtensions)
                {
                    extensions.Add("*." + ext);
                }
                uiFilter.IncludeFileExtensions = string.Join(",", extensions.ToArray());
            }

            // Convert exclude pattern to simple format
            uiFilter.ExcludeFilePatterns = coreFilter.ExcludePattern ?? "";

            // Convert include extensions to allowed file types (legacy support)
            var fileTypes = new List<string>();
            foreach (var ext in coreFilter.IncludeExtensions)
            {
                fileTypes.Add("." + ext + " - " + GetFileTypeDescription(ext));
            }
            uiFilter.AllowedFileTypes = fileTypes.ToArray();

            return uiFilter;
        }

        /// <summary>
        /// Converts UI FilterSettings to Core FilterSettings
        /// </summary>
        private syncer.core.FilterSettings ConvertUIFilterToCoreFilter(FilterSettings uiFilter)
        {
            if (uiFilter == null)
            {
                DebugLogger.LogServiceActivity("CoreSyncJobServiceAdapter", "UI Filter is null, creating default core filter");
                return new syncer.core.FilterSettings(); // Return default filter
            }

            var coreFilter = new syncer.core.FilterSettings
            {
                MinSizeKB = (long)(uiFilter.MinFileSize * 1024), // Convert MB to KB
                MaxSizeKB = (long)(uiFilter.MaxFileSize * 1024), // Convert MB to KB
                ExcludePattern = uiFilter.ExcludePatterns,
                IncludeHidden = uiFilter.IncludeHiddenFiles,
                IncludeSystem = uiFilter.IncludeSystemFiles,
                IncludeReadOnly = uiFilter.IncludeReadOnlyFiles,
                RecursiveSearch = true,
                ValidateAfterTransfer = true
            };

            // Convert allowed file types to include extensions
            if (uiFilter.FiltersEnabled && uiFilter.AllowedFileTypes != null)
            {
                DebugLogger.LogServiceActivity("CoreSyncJobServiceAdapter", 
                    string.Format("Converting {0} allowed file types (Filters enabled: {1})", 
                        uiFilter.AllowedFileTypes.Length, uiFilter.FiltersEnabled));
                        
                foreach (var fileType in uiFilter.AllowedFileTypes)
                {
                    // Extract extension from format like ".txt - Text files"
                    if (fileType.StartsWith("."))
                    {
                        string ext = fileType.Split(' ')[0].TrimStart('.');
                        if (!string.IsNullOrEmpty(ext))
                        {
                            coreFilter.IncludeExtensions.Add(ext.ToLowerInvariant());
                            // Also add to FileExtensions for the FileEnumerator
                            coreFilter.FileExtensions.Add("." + ext.ToLowerInvariant());
                            
                            DebugLogger.LogServiceActivity("CoreSyncJobServiceAdapter", 
                                string.Format("Added file extension: {0}", ext));
                        }
                    }
                }
            }
            // Convert simple include extensions (new feature)
            else if (uiFilter.FiltersEnabled && !string.IsNullOrEmpty(uiFilter.IncludeFileExtensions))
            {
                DebugLogger.LogServiceActivity("CoreSyncJobServiceAdapter", 
                    string.Format("Converting simple include extensions: {0}", uiFilter.IncludeFileExtensions));
                    
                string[] extensions = uiFilter.IncludeFileExtensions.Split(',');
                foreach (string ext in extensions)
                {
                    string cleanExt = ext.Trim().ToLowerInvariant();
                    if (string.IsNullOrEmpty(cleanExt)) continue;
                    
                    // Handle patterns like *.txt, .txt, txt
                    if (cleanExt.StartsWith("*."))
                    {
                        string extension = cleanExt.Substring(2); // Remove *.
                        coreFilter.IncludeExtensions.Add(extension);
                        coreFilter.FileExtensions.Add("." + extension);
                    }
                    else if (cleanExt.StartsWith("."))
                    {
                        string extension = cleanExt.Substring(1); // Remove .
                        coreFilter.IncludeExtensions.Add(extension);
                        coreFilter.FileExtensions.Add(cleanExt);
                    }
                    else
                    {
                        coreFilter.IncludeExtensions.Add(cleanExt);
                        coreFilter.FileExtensions.Add("." + cleanExt);
                    }
                }
            }
            
            // Convert simple exclude patterns (new feature)
            if (uiFilter.FiltersEnabled && !string.IsNullOrEmpty(uiFilter.ExcludeFilePatterns))
            {
                // Use the existing exclude pattern field
                coreFilter.ExcludePattern = uiFilter.ExcludeFilePatterns;
            }
            
            if (!uiFilter.FiltersEnabled)
            {
                DebugLogger.LogServiceActivity("CoreSyncJobServiceAdapter", "Filters are disabled, clearing extension filters");
                // When filters are disabled, clear any existing filters to allow all files
                coreFilter.FileExtensions.Clear();
                coreFilter.IncludeExtensions.Clear();
            }

            return coreFilter;
        }

        /// <summary>
        /// Apply current filter settings from the filter service to a job
        /// </summary>
        private void ApplyCurrentFilterSettings(SyncJob job)
        {
            try
            {
                var filterService = ServiceLocator.FilterService;
                if (filterService != null)
                {
                    var currentFilters = filterService.GetFilterSettings();
                    if (currentFilters != null)
                    {
                        job.FilterSettings = currentFilters;
                        
                        // Debug logging
                        DebugLogger.LogServiceActivity("CoreSyncJobServiceAdapter", 
                            string.Format("Applied filter settings to job '{0}': Enabled={1}, FileTypes={2}", 
                                job.Name, 
                                currentFilters.FiltersEnabled,
                                currentFilters.AllowedFileTypes != null ? currentFilters.AllowedFileTypes.Length : 0));
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogError(ex, "ApplyCurrentFilterSettings");
                // Don't throw - just continue without filters
            }
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
    }
}
