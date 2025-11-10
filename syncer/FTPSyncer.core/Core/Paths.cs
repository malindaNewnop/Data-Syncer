using System;
using System.IO;

namespace FTPSyncer.core
{
    public static class Paths
    {
        public static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DataSyncer");
        public static readonly string JobsFile = Path.Combine(AppDataFolder, "jobs.xml");
        public static readonly string LogsFile = Path.Combine(AppDataFolder, "logs.csv");
        public static readonly string SettingsFile = Path.Combine(AppDataFolder, "settings.json");
        public static readonly string TempFolder = Path.Combine(AppDataFolder, "temp");
        public static readonly string ArchiveFolder = Path.Combine(AppDataFolder, "archive");
        public static readonly string LogArchiveFolder = Path.Combine(AppDataFolder, "logs" + Path.DirectorySeparatorChar + "archive");

        // Named Pipe Names
        public static readonly string PipeName = "DataSyncerPipe";
        public static readonly string PipeFullName = @"\\.\pipe\" + PipeName;

        static Paths()
        {
            EnsureDirectoriesExist();
        }

        private static void EnsureDirectoriesExist()
        {
            try
            {
                if (!Directory.Exists(AppDataFolder)) Directory.CreateDirectory(AppDataFolder);
                if (!Directory.Exists(TempFolder)) Directory.CreateDirectory(TempFolder);
                if (!Directory.Exists(ArchiveFolder)) Directory.CreateDirectory(ArchiveFolder);
                if (!Directory.Exists(LogArchiveFolder)) Directory.CreateDirectory(LogArchiveFolder);
            }
            catch
            {
                // Ignore directory creation errors
            }
        }

        public static string GetLogArchivePath(DateTime date)
        {
            return Path.Combine(LogArchiveFolder, "logs_" + date.ToString("yyyy_MM") + ".csv");
        }

        public static string GetTempFilePath(string fileName)
        {
            return Path.Combine(TempFolder, fileName);
        }
    }
}





