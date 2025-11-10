using System;
using System.IO;
using System.Threading;

namespace FTPSyncer.core.Utilities
{
    /// <summary>
    /// Handles locked files by creating temporary copies that can be transferred
    /// .NET 3.5 Compatible
    /// </summary>
    public static class LockedFileHandler
    {
        private const int DefaultBufferSize = 8192; // 8KB buffer
        private const int MaxRetryAttempts = 3;
        private const int RetryDelayMs = 500;

        /// <summary>
        /// Attempts to create a shadow/temporary copy of a potentially locked file
        /// </summary>
        /// <param name="sourceFilePath">Path to the original file</param>
        /// <param name="error">Error message if operation fails</param>
        /// <returns>Path to the temporary copy, or null if failed</returns>
        public static string CreateTempCopy(string sourceFilePath, out string error)
        {
            error = null;
            
            if (string.IsNullOrEmpty(sourceFilePath) || !File.Exists(sourceFilePath))
            {
                error = "Source file does not exist";
                return null;
            }

            // Generate unique temporary file path
            string tempFolder = Paths.TempFolder;
            string fileName = Path.GetFileName(sourceFilePath);
            string tempFilePath = Path.Combine(tempFolder, string.Format("temp_{0}_{1}", 
                DateTime.Now.Ticks, fileName));

            // Try to create a shadow copy using different methods
            for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
            {
                try
                {
                    // Method 1: Try normal file copy (works if file has shared read access)
                    if (TryCopyWithSharedAccess(sourceFilePath, tempFilePath, out error))
                    {
                        return tempFilePath;
                    }

                    // Method 2: Try Volume Shadow Copy (Windows only, requires elevation)
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        if (TryCopyUsingVSS(sourceFilePath, tempFilePath, out error))
                        {
                            return tempFilePath;
                        }
                    }

                    // Method 3: Wait and retry with exponential backoff
                    if (attempt < MaxRetryAttempts - 1)
                    {
                        Thread.Sleep(RetryDelayMs * (attempt + 1));
                    }
                }
                catch (Exception ex)
                {
                    error = string.Format("Attempt {0} failed: {1}", attempt + 1, ex.Message);
                }
            }

            error = string.Format("Failed to create temporary copy after {0} attempts: {1}", 
                MaxRetryAttempts, error ?? "Unknown error");
            return null;
        }

        /// <summary>
        /// Try to copy a file with shared read access
        /// </summary>
        private static bool TryCopyWithSharedAccess(string sourceFilePath, string destFilePath, out string error)
        {
            error = null;
            FileStream sourceStream = null;
            FileStream destStream = null;

            try
            {
                // Open source file with shared read access
                sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                
                // Create destination file
                destStream = new FileStream(destFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

                // Copy in chunks
                byte[] buffer = new byte[DefaultBufferSize];
                int bytesRead;
                
                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    destStream.Write(buffer, 0, bytesRead);
                }

                destStream.Flush();

                // Preserve file attributes and timestamps
                try
                {
                    FileInfo sourceInfo = new FileInfo(sourceFilePath);
                    FileInfo destInfo = new FileInfo(destFilePath);
                    
                    destInfo.CreationTime = sourceInfo.CreationTime;
                    destInfo.LastWriteTime = sourceInfo.LastWriteTime;
                    destInfo.LastAccessTime = sourceInfo.LastAccessTime;
                    destInfo.Attributes = sourceInfo.Attributes & ~FileAttributes.ReadOnly;
                }
                catch
                {
                    // Ignore attribute copy failures
                }

                return true;
            }
            catch (IOException ioEx)
            {
                error = string.Format("IO error: {0}", ioEx.Message);
                return false;
            }
            catch (UnauthorizedAccessException uaEx)
            {
                error = string.Format("Access denied: {0}", uaEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                error = string.Format("Error: {0}", ex.Message);
                return false;
            }
            finally
            {
                if (sourceStream != null)
                {
                    sourceStream.Close();
                    sourceStream.Dispose();
                }
                
                if (destStream != null)
                {
                    destStream.Close();
                    destStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Try to copy using Volume Shadow Copy Service (Windows only)
        /// Note: This is a simplified implementation. Full VSS requires COM interop.
        /// </summary>
        private static bool TryCopyUsingVSS(string sourceFilePath, string destFilePath, out string error)
        {
            error = "VSS not implemented in this version";
            // VSS implementation requires complex COM interop with Windows VSS API
            // This would require additional P/Invoke and COM interop code
            // For .NET 3.5, this is beyond the scope of this implementation
            return false;
        }

        /// <summary>
        /// Check if a file is locked by another process
        /// </summary>
        public static bool IsFileLocked(string filePath, out string error)
        {
            error = null;
            FileStream stream = null;

            try
            {
                // Try to open the file with exclusive access
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return false; // File is not locked
            }
            catch (IOException ioEx)
            {
                // Check if it's a sharing violation (file is locked)
                const int ERROR_SHARING_VIOLATION = 32;
                const int ERROR_LOCK_VIOLATION = 33;
                
                int errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(ioEx) & 0xFFFF;
                
                if (errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION)
                {
                    error = "File is being used by another process";
                    return true;
                }
                
                error = string.Format("IO error: {0}", ioEx.Message);
                return true;
            }
            catch (UnauthorizedAccessException uaEx)
            {
                error = string.Format("Access denied: {0}", uaEx.Message);
                return true;
            }
            catch (Exception ex)
            {
                error = string.Format("Error: {0}", ex.Message);
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }

        /// <summary>
        /// Clean up a temporary file after successful transfer
        /// </summary>
        public static void CleanupTempCopy(string tempFilePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
                {
                    // Remove read-only attribute if present
                    FileAttributes attributes = File.GetAttributes(tempFilePath);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(tempFilePath, attributes & ~FileAttributes.ReadOnly);
                    }
                    
                    File.Delete(tempFilePath);
                }
            }
            catch
            {
                // Ignore cleanup failures - temp files will be cleaned up later
            }
        }

        /// <summary>
        /// Verifies that a temporary copy matches the original file's size
        /// </summary>
        public static bool VerifyTempCopy(string originalPath, string tempPath, out string error)
        {
            error = null;
            
            try
            {
                if (!File.Exists(originalPath))
                {
                    error = "Original file no longer exists";
                    return false;
                }
                
                if (!File.Exists(tempPath))
                {
                    error = "Temporary file does not exist";
                    return false;
                }
                
                FileInfo originalInfo = new FileInfo(originalPath);
                FileInfo tempInfo = new FileInfo(tempPath);
                
                if (originalInfo.Length != tempInfo.Length)
                {
                    error = string.Format("Size mismatch: Original={0} bytes, Temp={1} bytes", 
                        originalInfo.Length, tempInfo.Length);
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                error = string.Format("Verification error: {0}", ex.Message);
                return false;
            }
        }
    }
}





