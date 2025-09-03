using System;
using System.IO;

namespace syncer.core
{
    public class LocalTransferClient : ITransferClient
    {
        private Action<int> _progressCallback;

        public ProtocolType Protocol { get { return ProtocolType.Local; } }

        public bool TestConnection(ConnectionSettings settings, out string error)
        {
            error = null;
            try
            {
                // For local transfers, just check if we can access the temp directory
                var tempPath = Paths.TempFolder;
                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }
                
                // Test write access
                var testFile = Path.Combine(tempPath, "test_" + Guid.NewGuid().ToString() + ".tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                
                return true;
            }
            catch (Exception ex)
            {
                error = $"Local file system test failed: {ex.Message}";
                return false;
            }
        }

        public bool EnsureDirectory(ConnectionSettings settings, string remoteDir, out string error)
        {
            error = null;
            try
            {
                if (!Directory.Exists(remoteDir))
                {
                    Directory.CreateDirectory(remoteDir);
                }
                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to create directory '{remoteDir}': {ex.Message}";
                return false;
            }
        }

        public bool UploadFile(ConnectionSettings settings, string localPath, string remotePath, bool overwrite, out string error)
        {
            error = null;
            try
            {
                if (!File.Exists(localPath))
                {
                    error = $"Source file does not exist: {localPath}";
                    return false;
                }

                if (!overwrite && File.Exists(remotePath))
                {
                    error = $"Destination file already exists: {remotePath}";
                    return false;
                }

                // Ensure destination directory exists
                var destDir = Path.GetDirectoryName(remotePath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                // Copy with progress reporting for large files
                CopyFileWithProgress(localPath, remotePath, overwrite);
                
                return true;
            }
            catch (Exception ex)
            {
                error = $"Upload failed: {ex.Message}";
                return false;
            }
        }

        public bool DownloadFile(ConnectionSettings settings, string remotePath, string localPath, bool overwrite, out string error)
        {
            error = null;
            try
            {
                if (!File.Exists(remotePath))
                {
                    error = $"Source file does not exist: {remotePath}";
                    return false;
                }

                if (!overwrite && File.Exists(localPath))
                {
                    error = $"Destination file already exists: {localPath}";
                    return false;
                }

                // Ensure local directory exists
                var localDir = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(localDir) && !Directory.Exists(localDir))
                {
                    Directory.CreateDirectory(localDir);
                }

                // Copy with progress reporting for large files (remote to local in local transfer means file copy)
                CopyFileWithProgress(remotePath, localPath, overwrite);
                
                return true;
            }
            catch (Exception ex)
            {
                error = $"Download failed: {ex.Message}";
                return false;
            }
        }

        public bool FileExists(ConnectionSettings settings, string remotePath, out bool exists, out string error)
        {
            error = null;
            try
            {
                exists = File.Exists(remotePath);
                return true;
            }
            catch (Exception ex)
            {
                exists = false;
                error = $"Failed to check if file exists: {ex.Message}";
                return false;
            }
        }

        public bool DeleteFile(ConnectionSettings settings, string remotePath, out string error)
        {
            error = null;
            try
            {
                if (File.Exists(remotePath))
                {
                    File.Delete(remotePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to delete file: {ex.Message}";
                return false;
            }
        }

        public bool ListFiles(ConnectionSettings settings, string remoteDir, out System.Collections.Generic.List<string> files, out string error)
        {
            files = new System.Collections.Generic.List<string>();
            error = null;
            
            try
            {
                if (!Directory.Exists(remoteDir))
                {
                    error = $"Directory does not exist: {remoteDir}";
                    return false;
                }

                string[] fileArray = Directory.GetFiles(remoteDir);
                files.AddRange(fileArray);
                
                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to list files: {ex.Message}";
                return false;
            }
        }

        public void SetProgressCallback(Action<int> progressCallback)
        {
            _progressCallback = progressCallback;
        }

        private void CopyFileWithProgress(string sourcePath, string destPath, bool overwrite)
        {
            const int bufferSize = 64 * 1024; // 64KB buffer
            
            var sourceInfo = new FileInfo(sourcePath);
            long totalBytes = sourceInfo.Length;
            long copiedBytes = 0;

            using (var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            using (var dest = new FileStream(destPath, overwrite ? FileMode.Create : FileMode.CreateNew))
            {
                byte[] buffer = new byte[bufferSize];
                int bytesRead;
                
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    dest.Write(buffer, 0, bytesRead);
                    copiedBytes += bytesRead;
                    
                    // Report progress for files larger than 1MB
                    if (totalBytes > 1024 * 1024 && _progressCallback != null)
                    {
                        int progress = (int)((copiedBytes * 100) / totalBytes);
                        _progressCallback(progress);
                    }
                }
            }
            
            // Preserve file timestamps
            File.SetCreationTime(destPath, sourceInfo.CreationTime);
            File.SetLastWriteTime(destPath, sourceInfo.LastWriteTime);
            File.SetLastAccessTime(destPath, sourceInfo.LastAccessTime);
        }
    }
}
