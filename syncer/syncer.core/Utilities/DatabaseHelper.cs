using System;
using System.IO;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace syncer.core
{
    /// <summary>
    /// Helper class for SQLite database operations
    /// Provides centralized database initialization, connection management, and utilities
    /// </summary>
    public static class DatabaseHelper
    {
        private static readonly object _lockObject = new object();
        private const string DATABASE_FILENAME = "syncer.db";
        
        /// <summary>
        /// Gets the path to the SQLite database file
        /// </summary>
        public static string DatabasePath
        {
            get
            {
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "FTPSyncer");
                
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }
                
                return Path.Combine(appDataPath, DATABASE_FILENAME);
            }
        }
        
        /// <summary>
        /// Gets the connection string for the SQLite database
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                return $"Data Source={DatabasePath};Version=3;";
            }
        }
        
        /// <summary>
        /// Initializes the database, creating it if it doesn't exist
        /// and setting up all required tables
        /// </summary>
        public static void InitializeDatabase()
        {
            lock (_lockObject)
            {
                bool dbExists = File.Exists(DatabasePath);
                
                // Create database file if it doesn't exist
                if (!dbExists)
                {
                    SQLiteConnection.CreateFile(DatabasePath);
                }
                
                using (var connection = CreateConnection())
                {
                    // Create Jobs table
                    ExecuteNonQuery(connection, @"
                        CREATE TABLE IF NOT EXISTS Jobs (
                            Id TEXT PRIMARY KEY,
                            Name TEXT NOT NULL,
                            Description TEXT,
                            IsEnabled INTEGER NOT NULL,
                            IsScheduled INTEGER NOT NULL,
                            SourcePath TEXT,
                            DestinationPath TEXT,
                            IncludeSubfolders INTEGER NOT NULL,
                            OverwriteExisting INTEGER NOT NULL,
                            Direction INTEGER NOT NULL,
                            TransferMode INTEGER NOT NULL,
                            CreatedDate TEXT,
                            LastRun TEXT,
                            NextRun TEXT,
                            LastStatus TEXT,
                            LastTransferCount INTEGER,
                            LastTransferBytes INTEGER,
                            LastDuration INTEGER,
                            LastError TEXT,
                            IsRunning INTEGER NOT NULL,
                            RetryCount INTEGER,
                            MaxRetries INTEGER,
                            RetryIntervalMinutes INTEGER,
                            ConnectionJson TEXT,
                            DestinationConnectionJson TEXT,
                            FiltersJson TEXT,
                            ScheduleJson TEXT,
                            PostProcessJson TEXT
                        );
                    ");
                    
                    // Create Logs table
                    ExecuteNonQuery(connection, @"
                        CREATE TABLE IF NOT EXISTS Logs (
                            Id TEXT PRIMARY KEY,
                            Timestamp TEXT NOT NULL,
                            Level TEXT NOT NULL,
                            Source TEXT,
                            Message TEXT NOT NULL,
                            JobId TEXT,
                            Exception TEXT,
                            ExtraData TEXT
                        );
                    ");
                    
                    // Create index on Logs table for faster querying
                    ExecuteNonQuery(connection, @"
                        CREATE INDEX IF NOT EXISTS idx_logs_timestamp ON Logs (Timestamp);
                    ");
                    
                    // Create Settings table
                    ExecuteNonQuery(connection, @"
                        CREATE TABLE IF NOT EXISTS Settings (
                            Key TEXT PRIMARY KEY,
                            Value TEXT,
                            Description TEXT
                        );
                    ");
                    
                    // Initialize default settings if database was just created
                    if (!dbExists)
                    {
                        InitializeDefaultSettings(connection);
                    }
                }
            }
        }
        
        /// <summary>
        /// Creates a new SQLite connection to the database
        /// </summary>
        public static SQLiteConnection CreateConnection()
        {
            var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            return connection;
        }
        
        /// <summary>
        /// Executes a non-query SQL command against the connection
        /// </summary>
        private static void ExecuteNonQuery(SQLiteConnection connection, string sql)
        {
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        
        /// <summary>
        /// Initializes default settings in the database
        /// </summary>
        private static void InitializeDefaultSettings(SQLiteConnection connection)
        {
            var defaultSettings = new Dictionary<string, Tuple<string, string>>
            {
                { "LogRetentionDays", new Tuple<string, string>("30", "Number of days to retain log entries") },
                { "DatabaseBackupCount", new Tuple<string, string>("5", "Number of database backups to keep") },
                { "AutoBackupEnabled", new Tuple<string, string>("true", "Whether to automatically back up the database") },
                { "DatabaseVersion", new Tuple<string, string>("1.0", "Database schema version") }
            };
            
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    foreach (var setting in defaultSettings)
                    {
                        using (var command = new SQLiteCommand(
                            "INSERT OR IGNORE INTO Settings (Key, Value, Description) VALUES (@Key, @Value, @Description)", 
                            connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Key", setting.Key);
                            command.Parameters.AddWithValue("@Value", setting.Value.Item1);
                            command.Parameters.AddWithValue("@Description", setting.Value.Item2);
                            command.ExecuteNonQuery();
                        }
                    }
                    
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        
        /// <summary>
        /// Creates a backup of the database file
        /// </summary>
        public static void BackupDatabase()
        {
            lock (_lockObject)
            {
                try
                {
                    if (!File.Exists(DatabasePath))
                    {
                        return;
                    }
                    
                    string backupFolder = Path.Combine(
                        Path.GetDirectoryName(DatabasePath),
                        "Backups");
                    
                    if (!Directory.Exists(backupFolder))
                    {
                        Directory.CreateDirectory(backupFolder);
                    }
                    
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string backupFile = Path.Combine(
                        backupFolder,
                        Path.GetFileNameWithoutExtension(DATABASE_FILENAME) + 
                        "_backup_" + timestamp + 
                        Path.GetExtension(DATABASE_FILENAME));
                    
                    // Get the backup count setting
                    int backupCount = 5;
                    using (var connection = CreateConnection())
                    {
                        using (var command = new SQLiteCommand("SELECT Value FROM Settings WHERE Key = 'DatabaseBackupCount'", connection))
                        {
                            var result = command.ExecuteScalar();
                            if (result != null && !string.IsNullOrEmpty(result.ToString()))
                            {
                                int.TryParse(result.ToString(), out backupCount);
                            }
                        }
                    }
                    
                    // Copy the database file to the backup location
                    File.Copy(DatabasePath, backupFile, true);
                    
                    // Clean up old backups if we have too many
                    CleanupOldBackups(backupFolder, backupCount);
                }
                catch (Exception ex)
                {
                    // Log but don't rethrow - we don't want to interrupt operations for a backup failure
                }
            }
        }
        
        /// <summary>
        /// Cleans up old database backups, keeping only the most recent ones
        /// </summary>
        private static void CleanupOldBackups(string backupFolder, int keepCount)
        {
            try
            {
                var backupFiles = Directory.GetFiles(backupFolder, 
                    $"{Path.GetFileNameWithoutExtension(DATABASE_FILENAME)}_backup_*{Path.GetExtension(DATABASE_FILENAME)}")
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .Skip(keepCount)
                    .ToList();
                
                foreach (var file in backupFiles)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore errors deleting individual files
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        
        /// <summary>
        /// Performs database maintenance operations like vacuum and analyze
        /// </summary>
        public static void PerformMaintenance()
        {
            lock (_lockObject)
            {
                try
                {
                    using (var connection = CreateConnection())
                    {
                        // Vacuum the database to reclaim space
                        ExecuteNonQuery(connection, "VACUUM;");
                        
                        // Analyze the database to optimize query planning
                        ExecuteNonQuery(connection, "ANALYZE;");
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't rethrow
                }
            }
        }
        
        /// <summary>
        /// Checks if a table exists in the database
        /// </summary>
        public static bool TableExists(string tableName)
        {
            using (var connection = CreateConnection())
            {
                using (var command = new SQLiteCommand(
                    "SELECT name FROM sqlite_master WHERE type='table' AND name=@TableName", connection))
                {
                    command.Parameters.AddWithValue("@TableName", tableName);
                    var result = command.ExecuteScalar();
                    return result != null;
                }
            }
        }
        
        /// <summary>
        /// Gets a setting value from the database
        /// </summary>
        public static string GetSetting(string key, string defaultValue = null)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    using (var command = new SQLiteCommand("SELECT Value FROM Settings WHERE Key = @Key", connection))
                    {
                        command.Parameters.AddWithValue("@Key", key);
                        var result = command.ExecuteScalar();
                        return result != null ? result.ToString() : defaultValue;
                    }
                }
            }
            catch
            {
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Saves a setting value to the database
        /// </summary>
        public static void SaveSetting(string key, string value, string description = null)
        {
            using (var connection = CreateConnection())
            {
                using (var command = new SQLiteCommand(
                    "INSERT OR REPLACE INTO Settings (Key, Value, Description) VALUES (@Key, @Value, @Description)", 
                    connection))
                {
                    command.Parameters.AddWithValue("@Key", key);
                    command.Parameters.AddWithValue("@Value", value);
                    command.Parameters.AddWithValue("@Description", description ?? "");
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
