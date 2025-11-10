using System;
using System.IO;
using FTPSyncer.core.Utilities;

namespace FTPSyncer.core
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
            string tempCopyPath = null;
            bool usedTempCopy = false;

            try
            {
                if (!File.Exists(localPath))
                {
                    error = string.Format("Source file does not exist: {0}", localPath);
                    return false;
                }

                if (!overwrite && File.Exists(remotePath))
                {
                    error = string.Format("Destination file already exists: {0}", remotePath);
                    return false;
                }

                // Ensure destination directory exists
                var destDir = Path.GetDirectoryName(remotePath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                // Check if source file is locked
                string lockError;
                if (LockedFileHandler.IsFileLocked(localPath, out lockError))
                {
                    // File is locked - try to create a temporary copy
                    tempCopyPath = LockedFileHandler.CreateTempCopy(localPath, out error);
                    
                    if (tempCopyPath == null)
                    {
                        error = string.Format("File is locked and temporary copy failed: {0}. Original error: {1}", 
                            error, lockError);
                        return false;
                    }
                    
                    // Verify the temp copy
                    string verifyError;
                    if (!LockedFileHandler.VerifyTempCopy(localPath, tempCopyPath, out verifyError))
                    {
                        LockedFileHandler.CleanupTempCopy(tempCopyPath);
                        error = string.Format("Temporary copy verification failed: {0}", verifyError);
                        return false;
                    }
                    
                    usedTempCopy = true;
                    
                    // Use the temp copy for transfer
                    CopyFileWithProgress(tempCopyPath, remotePath, overwrite);
                }
                else
                {
                    // File is not locked - proceed with normal copy
                    CopyFileWithProgress(localPath, remotePath, overwrite);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                error = string.Format("Upload failed: {0}", ex.Message);
                return false;
            }
            finally
            {
                // Clean up temporary copy if one was created
                if (usedTempCopy && tempCopyPath != null)
                {
                    LockedFileHandler.CleanupTempCopy(tempCopyPath);
                }
            }
        }

        public bool DownloadFile(ConnectionSettings settings, string remotePath, string localPath, bool overwrite, out string error)
        {
            error = null;
            string tempCopyPath = null;
            bool usedTempCopy = false;

            try
            {
                if (!File.Exists(remotePath))
                {
                    error = string.Format("Source file does not exist: {0}", remotePath);
                    return false;
                }

                if (!overwrite && File.Exists(localPath))
                {
                    error = string.Format("Destination file already exists: {0}", localPath);
                    return false;
                }

                // Ensure local directory exists
                var localDir = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(localDir) && !Directory.Exists(localDir))
                {
                    Directory.CreateDirectory(localDir);
                }

                // Check if source file is locked (remote in this case is also local)
                string lockError;
                if (LockedFileHandler.IsFileLocked(remotePath, out lockError))
                {
                    // File is locked - try to create a temporary copy
                    tempCopyPath = LockedFileHandler.CreateTempCopy(remotePath, out error);
                    
                    if (tempCopyPath == null)
                    {
                        error = string.Format("File is locked and temporary copy failed: {0}. Original error: {1}", 
                            error, lockError);
                        return false;
                    }
                    
                    // Verify the temp copy
                    string verifyError;
                    if (!LockedFileHandler.VerifyTempCopy(remotePath, tempCopyPath, out verifyError))
                    {
                        LockedFileHandler.CleanupTempCopy(tempCopyPath);
                        error = string.Format("Temporary copy verification failed: {0}", verifyError);
                        return false;
                    }
                    
                    usedTempCopy = true;
                    
                    // Use the temp copy for transfer
                    CopyFileWithProgress(tempCopyPath, localPath, overwrite);
                }
                else
                {
                    // File is not locked - proceed with normal copy
                    CopyFileWithProgress(remotePath, localPath, overwrite);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                error = string.Format("Download failed: {0}", ex.Message);
                return false;
            }
            finally
            {
                // Clean up temporary copy if one was created
                if (usedTempCopy && tempCopyPath != null)
                {
                    LockedFileHandler.CleanupTempCopy(tempCopyPath);
                }
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

        /// <summary>
        /// Get the last modified time of a file
        /// </summary>
        public bool GetFileModifiedTime(ConnectionSettings settings, string remotePath, out DateTime modifiedTime, out string error)
        {
            modifiedTime = DateTime.MinValue;
            error = null;

            try
            {
                if (string.IsNullOrEmpty(remotePath))
                {
                    error = "File path cannot be empty";
                    return false;
                }

                if (!File.Exists(remotePath))
                {
                    error = $"File not found: {remotePath}";
                    return false;
                }

                FileInfo fileInfo = new FileInfo(remotePath);
                modifiedTime = fileInfo.LastWriteTime;
                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to get file modified time for '{remotePath}': {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Get the size of a file
        /// </summary>
        public bool GetFileSize(ConnectionSettings settings, string remotePath, out long fileSize, out string error)
        {
            fileSize = 0;
            error = null;

            try
            {
                if (string.IsNullOrEmpty(remotePath))
                {
                    error = "File path cannot be empty";
                    return false;
                }

                if (!File.Exists(remotePath))
                {
                    error = $"File not found: {remotePath}";
                    return false;
                }

                FileInfo fileInfo = new FileInfo(remotePath);
                fileSize = fileInfo.Length;
                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to get file size for '{remotePath}': {ex.Message}";
                return false;
            }
        }
    }
}





