using System;
using System.IO;

namespace FTPSyncer.ui.Services
{
    /// <summary>
    /// Helper class to check file system permissions
    /// </summary>
    public static class FileSystemPermissionChecker
    {
        public static bool CheckFolderPermissions(string path)
        {
            try
            {
                // Check if folder exists, if not try to create it
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                
                // Check if we can write to the folder
                string testFile = Path.Combine(path, "permission_test_" + Guid.NewGuid().ToString() + ".tmp");
                File.WriteAllText(testFile, "Test");
                File.Delete(testFile);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Permission check failed for path: " + path);
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
        }
        
        public static bool CheckAllRequiredFolders()
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string dataSyncerPath = Path.Combine(appDataPath, "DataSyncer");
                
                if (!CheckFolderPermissions(dataSyncerPath))
                    return false;
                
                string[] subFolders = new string[] { "logs", "temp", "archive" };
                foreach (string folder in subFolders)
                {
                    string path = Path.Combine(dataSyncerPath, folder);
                    if (!CheckFolderPermissions(path))
                        return false;
                }
                
                // For nested folders like "logs/archive", we need to combine paths in steps
                string logsFolder = Path.Combine(dataSyncerPath, "logs");
                string logArchiveFolder = Path.Combine(logsFolder, "archive");
                if (!CheckFolderPermissions(logArchiveFolder))
                    return false;
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking required folders: " + ex.Message);
                return false;
            }
        }
    }
}





