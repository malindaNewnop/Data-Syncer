using System;
using System.IO;

namespace FTPSyncer.core
{
    /// <summary>
    /// Path utilities for the FTPSyncer application
    /// .NET 3.5 Compatible
    /// </summary>
    public static class Paths
    {
        private static readonly string _appDataPath;
        private static readonly string _tempPath;
        private static readonly string _logsPath;
        
        static Paths()
        {
            _appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DataSyncer");
            _tempPath = Path.Combine(_appDataPath, "Temp");
            _logsPath = Path.Combine(_appDataPath, "Logs");
            
            // Ensure directories exist
            EnsureDirectoryExists(_appDataPath);
            EnsureDirectoryExists(_tempPath);
            EnsureDirectoryExists(_logsPath);
        }

        /// <summary>
        /// Get the application data directory
        /// </summary>
        public static string AppDataFolder
        {
            get { return _appDataPath; }
        }

        /// <summary>
        /// Get the settings file path
        /// </summary>
        public static string SettingsFile
        {
            get { return Path.Combine(_appDataPath, "settings.json"); }
        }

        /// <summary>
        /// Get the temporary folder path
        /// </summary>
        public static string TempFolder
        {
            get { return _tempPath; }
        }

        /// <summary>
        /// Get the logs folder path
        /// </summary>
        public static string LogsFolder
        {
            get { return _logsPath; }
        }

        /// <summary>
        /// Get the main log file path
        /// </summary>
        public static string MainLogFile
        {
            get { return Path.Combine(_logsPath, "datasyncer.log"); }
        }

        /// <summary>
        /// Get the error log file path
        /// </summary>
        public static string ErrorLogFile
        {
            get { return Path.Combine(_logsPath, "errors.log"); }
        }

        /// <summary>
        /// Get the pipe name for inter-process communication
        /// </summary>
        public static string PipeName
        {
            get { return "DataSyncerPipe"; }
        }

        /// <summary>
        /// Get the database file path
        /// </summary>
        public static string DatabaseFile
        {
            get { return Path.Combine(_appDataPath, "datasyncer.db"); }
        }

        /// <summary>
        /// Get the job configuration file path
        /// </summary>
        public static string JobConfigFile
        {
            get { return Path.Combine(_appDataPath, "jobs.xml"); }
        }

        /// <summary>
        /// Get the backup folder path
        /// </summary>
        public static string BackupFolder
        {
            get 
            { 
                var backup = Path.Combine(_appDataPath, "Backups");
                EnsureDirectoryExists(backup);
                return backup;
            }
        }

        /// <summary>
        /// Get the SSH keys folder path
        /// </summary>
        public static string SshKeysFolder
        {
            get 
            { 
                var sshKeys = Path.Combine(_appDataPath, "SSH_Keys");
                EnsureDirectoryExists(sshKeys);
                return sshKeys;
            }
        }

        /// <summary>
        /// Get a temporary file path with specified extension
        /// </summary>
        public static string GetTempFile(string extension = ".tmp")
        {
            var fileName = Guid.NewGuid().ToString() + extension;
            return Path.Combine(_tempPath, fileName);
        }

        /// <summary>
        /// Get a backup file path for a given source file
        /// </summary>
        public static string GetBackupFilePath(string originalFile)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = Path.GetFileNameWithoutExtension(originalFile);
            var extension = Path.GetExtension(originalFile);
            var backupFileName = string.Format("{0}_{1}{2}", fileName, timestamp, extension);
            return Path.Combine(BackupFolder, backupFileName);
        }

        /// <summary>
        /// Get a log file path with specified name and date
        /// </summary>
        public static string GetLogFile(string logName, DateTime date)
        {
            var dateStr = date.ToString("yyyyMMdd");
            var fileName = string.Format("{0}_{1}.log", logName, dateStr);
            return Path.Combine(_logsPath, fileName);
        }

        /// <summary>
        /// Clean up old temporary files (older than specified days)
        /// </summary>
        public static void CleanupTempFiles(int olderThanDays = 7)
        {
            try
            {
                if (Directory.Exists(_tempPath))
                {
                    var cutoffDate = DateTime.Now.AddDays(-olderThanDays);
                    var tempFiles = Directory.GetFiles(_tempPath);
                    
                    foreach (var file in tempFiles)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(file);
                            if (fileInfo.CreationTime < cutoffDate)
                            {
                                File.Delete(file);
                            }
                        }
                        catch
                        {
                            // Ignore individual file deletion errors
                        }
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        /// <summary>
        /// Clean up old log files (older than specified days)
        /// </summary>
        public static void CleanupLogFiles(int keepDays = 30)
        {
            try
            {
                if (Directory.Exists(_logsPath))
                {
                    var cutoffDate = DateTime.Now.AddDays(-keepDays);
                    var logFiles = Directory.GetFiles(_logsPath, "*.log");
                    
                    foreach (var file in logFiles)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(file);
                            if (fileInfo.CreationTime < cutoffDate)
                            {
                                File.Delete(file);
                            }
                        }
                        catch
                        {
                            // Ignore individual file deletion errors
                        }
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        /// <summary>
        /// Get application version string for file names
        /// </summary>
        public static string GetVersionString()
        {
            try
            {
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            }
            catch
            {
                return "1.0.0";
            }
        }

        /// <summary>
        /// Ensure a directory exists, create if it doesn't
        /// </summary>
        private static void EnsureDirectoryExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch
            {
                // Ignore directory creation errors during static initialization
            }
        }

        /// <summary>
        /// Normalize path separators for the current platform
        /// </summary>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
                
            return path.Replace('\\', Path.DirectorySeparatorChar)
                      .Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Get relative path from one path to another
        /// </summary>
        public static string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath) || string.IsNullOrEmpty(toPath))
                return toPath;
                
            try
            {
                var fromUri = new Uri(fromPath);
                var toUri = new Uri(toPath);
                
                if (fromUri.Scheme != toUri.Scheme)
                    return toPath;
                    
                var relativeUri = fromUri.MakeRelativeUri(toUri);
                var relativePath = Uri.UnescapeDataString(relativeUri.ToString());
                
                return relativePath.Replace('/', Path.DirectorySeparatorChar);
            }
            catch
            {
                return toPath;
            }
        }

        /// <summary>
        /// Check if a path is accessible for reading/writing
        /// </summary>
        public static bool IsPathAccessible(string path, bool checkWrite = false)
        {
            try
            {
                if (File.Exists(path))
                {
                    using (var stream = File.OpenRead(path))
                    {
                        // Can read
                    }
                    
                    if (checkWrite)
                    {
                        using (var stream = File.OpenWrite(path))
                        {
                            // Can write
                        }
                    }
                    
                    return true;
                }
                else if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path);
                    
                    if (checkWrite)
                    {
                        var testFile = Path.Combine(path, Guid.NewGuid().ToString() + ".tmp");
                        File.WriteAllText(testFile, "test");
                        File.Delete(testFile);
                    }
                    
                    return true;
                }
            }
            catch
            {
                // Path is not accessible
            }
            
            return false;
        }
    }
}





