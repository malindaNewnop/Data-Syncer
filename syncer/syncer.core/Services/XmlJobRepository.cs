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

        public List<SyncJob> LoadAll()
        {
            lock (_lockObject)
            {
                try
                {
                    if (!File.Exists(Paths.JobsFile)) return new List<SyncJob>();
                    
                    using (var fs = File.OpenRead(Paths.JobsFile))
                    {
                        var xs = new XmlSerializer(typeof(List<SyncJob>));
                        object obj = xs.Deserialize(fs);
                        return obj as List<SyncJob> ?? new List<SyncJob>();
                    }
                }
                catch (Exception)
                {
                    // If file is corrupted, backup and return empty list
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
                    
                    using (var fs = File.Create(Paths.JobsFile))
                    {
                        var xs = new XmlSerializer(typeof(List<SyncJob>));
                        xs.Serialize(fs, jobs);
                    }
                }
                catch (Exception ex)
                {
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
            var jobs = LoadAll();
            var existing = jobs.FirstOrDefault(j => j.Id == job.Id);
            
            if (existing != null)
            {
                var index = jobs.IndexOf(existing);
                jobs[index] = job;
            }
            else
            {
                jobs.Add(job);
            }
            
            SaveAll(jobs);
        }

        public void Delete(string id)
        {
            var jobs = LoadAll();
            jobs.RemoveAll(j => j.Id == id);
            SaveAll(jobs);
        }

        private void CreateBackup()
        {
            try
            {
                if (File.Exists(Paths.JobsFile))
                {
                    string backupPath = Paths.JobsFile + ".backup";
                    File.Copy(Paths.JobsFile, backupPath, true);
                }
            }
            catch
            {
                // Ignore backup errors
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
                }
            }
            catch
            {
                // Ignore backup errors
            }
        }
    }
}
