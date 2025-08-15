using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace syncer.core
{
    /// <summary>
    /// SQLite-based repository for SyncJobs
    /// This replaces the XML persistence with a more robust SQLite database approach
    /// </summary>
    public class SqliteJobRepository : IJobRepository
    {
        private readonly object _lockObject = new object();
        private readonly ILogService _logService;

        public SqliteJobRepository(ILogService logService = null)
        {
            _logService = logService;
            DatabaseHelper.InitializeDatabase();
            MigrateXmlToSqlite();
        }

        private void MigrateXmlToSqlite()
        {
            try
            {
                // First check if we have any jobs in the database
                bool hasExistingJobs = false;
                
                using (var connection = DatabaseHelper.CreateConnection())
                {
                    using (var command = new SQLiteCommand("SELECT COUNT(*) FROM Jobs", connection))
                    {
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        hasExistingJobs = count > 0;
                    }
                }
                
                // If we already have jobs in the database, don't migrate
                if (hasExistingJobs)
                {
                    return;
                }
                
                if (File.Exists(Paths.JobsFile) && new FileInfo(Paths.JobsFile).Length > 0)
                {
                    LogInfo("Found existing XML jobs file, attempting to migrate to SQLite");
                    
                    // Read jobs from XML
                    List<SyncJob> xmlJobs = new List<SyncJob>();
                    try
                    {
                        using (var fs = File.OpenRead(Paths.JobsFile))
                        {
                            var xs = new System.Xml.Serialization.XmlSerializer(typeof(List<SyncJob>));
                            xmlJobs = (List<SyncJob>)xs.Deserialize(fs);
                        }
                        
                        if (xmlJobs != null && xmlJobs.Count > 0)
                        {
                            LogInfo($"Found {xmlJobs.Count} jobs in XML file, migrating to SQLite");
                            
                            // Save jobs to SQLite
                            foreach (var job in xmlJobs)
                            {
                                Save(job);
                            }
                            
                            LogInfo($"Successfully migrated {xmlJobs.Count} jobs to SQLite");
                            
                            // Backup and rename the XML file
                            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                            string backupFile = Paths.JobsFile + ".migrated." + timestamp;
                            File.Copy(Paths.JobsFile, backupFile, true);
                            LogInfo($"Backed up XML file to {backupFile}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error migrating from XML: {ex.Message}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in migration process: {ex.Message}", ex);
            }
        }

        public List<SyncJob> LoadAll()
        {
            lock (_lockObject)
            {
                try
                {
                    var jobs = new List<SyncJob>();
                    
                    using (var connection = DatabaseHelper.CreateConnection())
                    {
                        using (var command = new SQLiteCommand("SELECT * FROM Jobs", connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    try
                                    {
                                        var job = new SyncJob
                                        {
                                            Id = reader["Id"].ToString(),
                                            Name = reader["Name"].ToString(),
                                            Description = reader["Description"].ToString(),
                                            IsEnabled = Convert.ToBoolean(reader["IsEnabled"]),
                                            IsScheduled = Convert.ToBoolean(reader["IsScheduled"]),
                                            SourcePath = reader["SourcePath"].ToString(),
                                            DestinationPath = reader["DestinationPath"].ToString(),
                                            IncludeSubfolders = Convert.ToBoolean(reader["IncludeSubfolders"]),
                                            OverwriteExisting = Convert.ToBoolean(reader["OverwriteExisting"]),
                                            Direction = (TransferDirection)Convert.ToInt32(reader["Direction"]),
                                            TransferMode = (TransferMode)Convert.ToInt32(reader["TransferMode"]),
                                            CreatedDate = !string.IsNullOrEmpty(reader["CreatedDate"].ToString()) ? 
                                                        DateTime.Parse(reader["CreatedDate"].ToString()) : DateTime.MinValue,
                                            LastRun = !string.IsNullOrEmpty(reader["LastRun"].ToString()) ? 
                                                    DateTime.Parse(reader["LastRun"].ToString()) : DateTime.MinValue,
                                            NextRun = !string.IsNullOrEmpty(reader["NextRun"].ToString()) ? 
                                                    (DateTime?)DateTime.Parse(reader["NextRun"].ToString()) : null,
                                            LastStatus = reader["LastStatus"].ToString(),
                                            LastTransferCount = Convert.ToInt32(reader["LastTransferCount"]),
                                            LastTransferBytes = Convert.ToInt64(reader["LastTransferBytes"]),
                                            LastDuration = TimeSpan.FromMilliseconds(Convert.ToInt64(reader["LastDuration"])),
                                            LastError = reader["LastError"].ToString(),
                                            IsRunning = Convert.ToBoolean(reader["IsRunning"]),
                                            RetryCount = Convert.ToInt32(reader["RetryCount"]),
                                            MaxRetries = Convert.ToInt32(reader["MaxRetries"]),
                                            RetryIntervalMinutes = Convert.ToInt32(reader["RetryIntervalMinutes"]),
                                            Connection = JsonConvert.DeserializeObject<ConnectionSettings>(reader["ConnectionJson"].ToString()),
                                            DestinationConnection = JsonConvert.DeserializeObject<ConnectionSettings>(reader["DestinationConnectionJson"].ToString()),
                                            Filters = JsonConvert.DeserializeObject<FilterSettings>(reader["FiltersJson"].ToString()),
                                            Schedule = JsonConvert.DeserializeObject<ScheduleSettings>(reader["ScheduleJson"].ToString()),
                                            PostProcess = JsonConvert.DeserializeObject<PostProcessSettings>(reader["PostProcessJson"].ToString())
                                        };
                                        
                                        jobs.Add(job);
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError($"Error deserializing job: {ex.Message}", ex);
                                    }
                                }
                            }
                        }
                    }
                    
                    LogInfo($"Successfully loaded {jobs.Count} jobs from repository");
                    return jobs;
                }
                catch (Exception ex)
                {
                    LogError($"Error loading jobs: {ex.Message}", ex);
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
                    // Create a backup first
                    DatabaseHelper.BackupDatabase();
                    
                    using (var connection = DatabaseHelper.CreateConnection())
                    {
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // First delete all existing jobs
                                using (var command = new SQLiteCommand("DELETE FROM Jobs", connection, transaction))
                                {
                                    command.ExecuteNonQuery();
                                }
                                
                                // Then insert all jobs
                                foreach (var job in jobs)
                                {
                                    InsertOrUpdateJob(job, connection, transaction);
                                }
                                
                                transaction.Commit();
                                LogInfo($"Successfully saved {jobs.Count} jobs to repository");
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                LogError($"Error in transaction: {ex.Message}", ex);
                                throw;
                            }
                        }
                    }
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
            lock (_lockObject)
            {
                try
                {
                    using (var connection = DatabaseHelper.CreateConnection())
                    {
                        using (var command = new SQLiteCommand("SELECT * FROM Jobs WHERE Id = @Id", connection))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    try
                                    {
                                        var job = new SyncJob
                                        {
                                            Id = reader["Id"].ToString(),
                                            Name = reader["Name"].ToString(),
                                            Description = reader["Description"].ToString(),
                                            IsEnabled = Convert.ToBoolean(reader["IsEnabled"]),
                                            IsScheduled = Convert.ToBoolean(reader["IsScheduled"]),
                                            SourcePath = reader["SourcePath"].ToString(),
                                            DestinationPath = reader["DestinationPath"].ToString(),
                                            IncludeSubfolders = Convert.ToBoolean(reader["IncludeSubfolders"]),
                                            OverwriteExisting = Convert.ToBoolean(reader["OverwriteExisting"]),
                                            Direction = (TransferDirection)Convert.ToInt32(reader["Direction"]),
                                            TransferMode = (TransferMode)Convert.ToInt32(reader["TransferMode"]),
                                            CreatedDate = !string.IsNullOrEmpty(reader["CreatedDate"].ToString()) ? 
                                                        DateTime.Parse(reader["CreatedDate"].ToString()) : DateTime.MinValue,
                                            LastRun = !string.IsNullOrEmpty(reader["LastRun"].ToString()) ? 
                                                    DateTime.Parse(reader["LastRun"].ToString()) : DateTime.MinValue,
                                            NextRun = !string.IsNullOrEmpty(reader["NextRun"].ToString()) ? 
                                                    (DateTime?)DateTime.Parse(reader["NextRun"].ToString()) : null,
                                            LastStatus = reader["LastStatus"].ToString(),
                                            LastTransferCount = Convert.ToInt32(reader["LastTransferCount"]),
                                            LastTransferBytes = Convert.ToInt64(reader["LastTransferBytes"]),
                                            LastDuration = TimeSpan.FromMilliseconds(Convert.ToInt64(reader["LastDuration"])),
                                            LastError = reader["LastError"].ToString(),
                                            IsRunning = Convert.ToBoolean(reader["IsRunning"]),
                                            RetryCount = Convert.ToInt32(reader["RetryCount"]),
                                            MaxRetries = Convert.ToInt32(reader["MaxRetries"]),
                                            RetryIntervalMinutes = Convert.ToInt32(reader["RetryIntervalMinutes"]),
                                            Connection = JsonConvert.DeserializeObject<ConnectionSettings>(reader["ConnectionJson"].ToString()),
                                            DestinationConnection = JsonConvert.DeserializeObject<ConnectionSettings>(reader["DestinationConnectionJson"].ToString()),
                                            Filters = JsonConvert.DeserializeObject<FilterSettings>(reader["FiltersJson"].ToString()),
                                            Schedule = JsonConvert.DeserializeObject<ScheduleSettings>(reader["ScheduleJson"].ToString()),
                                            PostProcess = JsonConvert.DeserializeObject<PostProcessSettings>(reader["PostProcessJson"].ToString())
                                        };
                                        
                                        return job;
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError($"Error deserializing job: {ex.Message}", ex);
                                    }
                                }
                            }
                        }
                    }
                    
                    return null;
                }
                catch (Exception ex)
                {
                    LogError($"Error getting job by ID {id}: {ex.Message}", ex);
                    return null;
                }
            }
        }

        public void Save(SyncJob job)
        {
            lock (_lockObject)
            {
                try
                {
                    // Create a backup first
                    DatabaseHelper.BackupDatabase();
                    
                    using (var connection = DatabaseHelper.CreateConnection())
                    {
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // Check if job already exists
                                bool jobExists = false;
                                using (var checkCommand = new SQLiteCommand("SELECT COUNT(*) FROM Jobs WHERE Id = @Id", connection, transaction))
                                {
                                    checkCommand.Parameters.AddWithValue("@Id", job.Id);
                                    int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                                    jobExists = count > 0;
                                }
                                
                                if (jobExists)
                                {
                                    LogInfo($"Updating existing job: {job.Id} - {job.Name}");
                                }
                                else
                                {
                                    LogInfo($"Adding new job: {job.Id} - {job.Name}");
                                }
                                
                                // Insert or update the job
                                InsertOrUpdateJob(job, connection, transaction);
                                
                                // Commit the transaction
                                transaction.Commit();
                                LogInfo($"Job saved successfully: {job.Id} - {job.Name}");
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                LogError($"Error in transaction: {ex.Message}", ex);
                                throw;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error saving job {job.Id}: {ex.Message}", ex);
                    throw;
                }
            }
        }

        private void InsertOrUpdateJob(SyncJob job, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            string sql = @"
                INSERT OR REPLACE INTO Jobs (
                    Id, Name, Description, IsEnabled, IsScheduled, SourcePath, DestinationPath, 
                    IncludeSubfolders, OverwriteExisting, Direction, TransferMode, CreatedDate, 
                    LastRun, NextRun, LastStatus, LastTransferCount, LastTransferBytes, 
                    LastDuration, LastError, IsRunning, RetryCount, MaxRetries, RetryIntervalMinutes,
                    ConnectionJson, DestinationConnectionJson, FiltersJson, ScheduleJson, PostProcessJson
                ) VALUES (
                    @Id, @Name, @Description, @IsEnabled, @IsScheduled, @SourcePath, @DestinationPath, 
                    @IncludeSubfolders, @OverwriteExisting, @Direction, @TransferMode, @CreatedDate, 
                    @LastRun, @NextRun, @LastStatus, @LastTransferCount, @LastTransferBytes, 
                    @LastDuration, @LastError, @IsRunning, @RetryCount, @MaxRetries, @RetryIntervalMinutes,
                    @ConnectionJson, @DestinationConnectionJson, @FiltersJson, @ScheduleJson, @PostProcessJson
                )";
                
            using (var command = new SQLiteCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@Id", job.Id);
                command.Parameters.AddWithValue("@Name", job.Name);
                command.Parameters.AddWithValue("@Description", job.Description ?? "");
                command.Parameters.AddWithValue("@IsEnabled", job.IsEnabled ? 1 : 0);
                command.Parameters.AddWithValue("@IsScheduled", job.IsScheduled ? 1 : 0);
                command.Parameters.AddWithValue("@SourcePath", job.SourcePath ?? "");
                command.Parameters.AddWithValue("@DestinationPath", job.DestinationPath ?? "");
                command.Parameters.AddWithValue("@IncludeSubfolders", job.IncludeSubfolders ? 1 : 0);
                command.Parameters.AddWithValue("@OverwriteExisting", job.OverwriteExisting ? 1 : 0);
                command.Parameters.AddWithValue("@Direction", (int)job.Direction);
                command.Parameters.AddWithValue("@TransferMode", (int)job.TransferMode);
                command.Parameters.AddWithValue("@CreatedDate", job.CreatedDate.ToString("o"));
                command.Parameters.AddWithValue("@LastRun", job.LastRun != DateTime.MinValue ? job.LastRun.ToString("o") : "");
                command.Parameters.AddWithValue("@NextRun", job.NextRun.HasValue ? job.NextRun.Value.ToString("o") : "");
                command.Parameters.AddWithValue("@LastStatus", job.LastStatus ?? "");
                command.Parameters.AddWithValue("@LastTransferCount", job.LastTransferCount);
                command.Parameters.AddWithValue("@LastTransferBytes", job.LastTransferBytes);
                command.Parameters.AddWithValue("@LastDuration", job.LastDuration.TotalMilliseconds);
                command.Parameters.AddWithValue("@LastError", job.LastError ?? "");
                command.Parameters.AddWithValue("@IsRunning", job.IsRunning ? 1 : 0);
                command.Parameters.AddWithValue("@RetryCount", job.RetryCount);
                command.Parameters.AddWithValue("@MaxRetries", job.MaxRetries);
                command.Parameters.AddWithValue("@RetryIntervalMinutes", job.RetryIntervalMinutes);
                command.Parameters.AddWithValue("@ConnectionJson", JsonConvert.SerializeObject(job.Connection ?? new ConnectionSettings()));
                command.Parameters.AddWithValue("@DestinationConnectionJson", JsonConvert.SerializeObject(job.DestinationConnection ?? new ConnectionSettings()));
                command.Parameters.AddWithValue("@FiltersJson", JsonConvert.SerializeObject(job.Filters ?? new FilterSettings()));
                command.Parameters.AddWithValue("@ScheduleJson", JsonConvert.SerializeObject(job.Schedule ?? new ScheduleSettings()));
                command.Parameters.AddWithValue("@PostProcessJson", JsonConvert.SerializeObject(job.PostProcess ?? new PostProcessSettings()));
                
                command.ExecuteNonQuery();
            }
        }

        public void Delete(string id)
        {
            lock (_lockObject)
            {
                try
                {
                    using (var connection = DatabaseHelper.CreateConnection())
                    {
                        using (var command = new SQLiteCommand("DELETE FROM Jobs WHERE Id = @Id", connection))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            int rowsAffected = command.ExecuteNonQuery();
                            
                            if (rowsAffected > 0)
                            {
                                LogInfo($"Deleted job with ID: {id}");
                            }
                            else
                            {
                                LogInfo($"Job with ID {id} not found for deletion");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error deleting job {id}: {ex.Message}", ex);
                    throw;
                }
            }
        }
        
        private void LogInfo(string message)
        {
            if (_logService != null)
            {
                _logService.LogInfo(message, "SqliteJobRepository");
            }
        }
        
        private void LogError(string message, Exception ex = null)
        {
            if (_logService != null)
            {
                _logService.LogError(message + (ex != null ? ": " + ex.ToString() : ""), "SqliteJobRepository");
            }
        }
    }
}
