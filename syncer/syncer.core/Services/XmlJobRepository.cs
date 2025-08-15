using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;

namespace syncer.core
{
    public class XmlJobRepository : IJobRepository
    {
        private readonly object _lockObject = new object();
        private readonly ILogService _logService;

        public XmlJobRepository(ILogService logService = null)
        {
            _logService = logService;
        }

        public List<SyncJob> LoadAll()
        {
            lock (_lockObject)
            {
                try
                {
                    if (!File.Exists(Paths.JobsFile)) 
                    {
                        LogInfo("Jobs file not found, returning empty list");
                        return new List<SyncJob>();
                    }
                    
                    using (var fs = File.OpenRead(Paths.JobsFile))
                    {
                        var xs = new XmlSerializer(typeof(List<SyncJob>));
                        object obj = xs.Deserialize(fs);
                        var jobs = obj as List<SyncJob> ?? new List<SyncJob>();
                        LogInfo($"Successfully loaded {jobs.Count} jobs from repository");
                        return jobs;
                    }
                }
                catch (Exception ex)
                {
                    // If file is corrupted, backup and return empty list
                    LogError($"Error loading jobs: {ex.Message}", ex);
                    BackupCorruptedFile();
                    return new List<SyncJob>();
                }
            }
        }

        public List<SyncJob> GetAll()
        {
            return LoadAll();
        }

        public void SaveAll(List<SyncJob> jobs)
        {
            lock (_lockObject)
            {
                try
                {
                    // Create backup before saving
                    CreateBackup();
                    
                    // Ensure parent directory exists
                    string directory = Path.GetDirectoryName(Paths.JobsFile);
                    if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                        LogInfo($"Created directory: {directory}");
                    }
                    
                    using (var fs = File.Create(Paths.JobsFile))
                    {
                        var xs = new XmlSerializer(typeof(List<SyncJob>));
                        xs.Serialize(fs, jobs);
                    }
                    
                    LogInfo($"Successfully saved {jobs.Count} jobs to repository");
                }
                catch (Exception ex)
                {
                    LogError($"Failed to save jobs: {ex.Message}", ex);
                    throw new Exception("Failed to save jobs: " + ex.Message, ex);
                }
            }
        }

        public SyncJob GetById(string id)
        {
            var jobs = LoadAll();
            return jobs.FirstOrDefault(j => j.Id == id);
        }

        public void Save(SyncJob job)
        {
            lock (_lockObject)
            {
                try
                {
                    var jobs = LoadAll();
                    var existing = jobs.FirstOrDefault(j => j.Id == job.Id);
                    
                    if (existing != null)
                    {
                        LogInfo($"Updating existing job: {job.Id} - {job.Name}");
                        var index = jobs.IndexOf(existing);
                        jobs[index] = job;
                    }
                    else
                    {
                        LogInfo($"Adding new job: {job.Id} - {job.Name}");
                        jobs.Add(job);
                    }
                    
                    SaveAll(jobs);
                }
                catch (Exception ex)
                {
                    LogError($"Error saving job {job.Id}: {ex.Message}", ex);
                    throw;
                }
            }
        }

        public void Delete(string id)
        {
            lock (_lockObject)
            {
                try
                {
                    var jobs = LoadAll();
                    int countBefore = jobs.Count;
                    jobs.RemoveAll(j => j.Id == id);
                    
                    if (jobs.Count < countBefore)
                    {
                        LogInfo($"Deleted job with ID: {id}");
                        SaveAll(jobs);
                    }
                    else
                    {
                        LogInfo($"Job with ID {id} not found for deletion");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error deleting job {id}: {ex.Message}", ex);
                    throw;
                }
            }
        }

        private void CreateBackup()
        {
            try
            {
                if (File.Exists(Paths.JobsFile))
                {
                    string backupPath = Paths.JobsFile + ".backup";
                    File.Copy(Paths.JobsFile, backupPath, true);
                    LogInfo($"Created backup at {backupPath}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to create backup: {ex.Message}", ex);
            }
        }

        private void BackupCorruptedFile()
        {
            try
            {
                if (File.Exists(Paths.JobsFile))
                {
                    string corruptPath = Paths.JobsFile + ".corrupted_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    File.Move(Paths.JobsFile, corruptPath);
                    LogInfo($"Backed up corrupted file to {corruptPath}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to backup corrupted file: {ex.Message}", ex);
            }
        }
        
        private void LogInfo(string message)
        {
            if (_logService != null)
            {
                _logService.LogInfo(message, "JobRepository");
            }
        }
        
        private void LogError(string message, Exception ex = null)
        {
            if (_logService != null)
            {
                _logService.LogError(message + (ex != null ? ": " + ex.ToString() : ""), "JobRepository");
            }
        }
    }
}
