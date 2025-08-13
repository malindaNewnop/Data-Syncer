using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace syncer.core
{
    public class FtpTransferClient : ITransferClient
    {
        private Action<int> _progressCallback;

        public ProtocolType Protocol { get { return ProtocolType.Ftp; } }

        public bool TestConnection(ConnectionSettings settings, out string error)
        {
            error = null;
            try
            {
                if (settings == null)
                {
                    error = "Connection settings are required for FTP";
                    return false;
                }

                var request = CreateFtpRequest(settings, "/", WebRequestMethods.Ftp.ListDirectory);
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == FtpStatusCode.OpeningData || 
                           response.StatusCode == FtpStatusCode.DataAlreadyOpen;
                }
            }
            catch (Exception ex)
            {
                error = "FTP connection test failed: " + ex.Message;
                return false;
            }
        }

        public bool EnsureDirectory(ConnectionSettings settings, string remoteDir, out string error)
        {
            error = null;
            try
            {
                // Normalize path separators
                remoteDir = remoteDir.Replace('\\', '/');
                if (!remoteDir.StartsWith("/"))
                    remoteDir = "/" + remoteDir;

                // Check if directory exists first
                var request = CreateFtpRequest(settings, remoteDir, WebRequestMethods.Ftp.ListDirectory);
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    return true; // Directory exists
                }
            }
            catch (WebException)
            {
                // Directory doesn't exist, try to create it
                try
                {
                    var createRequest = CreateFtpRequest(settings, remoteDir, WebRequestMethods.Ftp.MakeDirectory);
                    using (var response = (FtpWebResponse)createRequest.GetResponse())
                    {
                        return response.StatusCode == FtpStatusCode.PathnameCreated;
                    }
                }
                catch (Exception createEx)
                {
                    error = "Failed to create directory '" + remoteDir + "': " + createEx.Message;
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = "Failed to ensure directory '" + remoteDir + "': " + ex.Message;
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
                    error = "Source file does not exist: " + localPath;
                    return false;
                }

                // Normalize remote path
                remotePath = remotePath.Replace('\\', '/');
                if (!remotePath.StartsWith("/"))
                    remotePath = "/" + remotePath;

                // Check if file exists and overwrite is false
                if (!overwrite)
                {
                    bool exists;
                    string checkError;
                    if (FileExists(settings, remotePath, out exists, out checkError))
                    {
                        if (exists)
                        {
                            error = "Destination file already exists: " + remotePath;
                            return false;
                        }
                    }
                }

                // Ensure remote directory exists
                var remoteDir = Path.GetDirectoryName(remotePath).Replace('\\', '/');
                if (!string.IsNullOrEmpty(remoteDir))
                {
                    string dirError;
                    if (!EnsureDirectory(settings, remoteDir, out dirError))
                    {
                        error = "Failed to create remote directory: " + dirError;
                        return false;
                    }
                }

                // Upload the file
                var request = CreateFtpRequest(settings, remotePath, WebRequestMethods.Ftp.UploadFile);
                using (var requestStream = request.GetRequestStream())
                using (var fileStream = File.OpenRead(localPath))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                    }
                }

                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == FtpStatusCode.ClosingData || 
                           response.StatusCode == FtpStatusCode.FileActionOK;
                }
            }
            catch (Exception ex)
            {
                error = "Upload failed: " + ex.Message;
                return false;
            }
        }

        public bool DownloadFile(ConnectionSettings settings, string remotePath, string localPath, bool overwrite, out string error)
        {
            error = null;
            try
            {
                // Check if remote file exists
                bool exists;
                string checkError;
                if (!FileExists(settings, remotePath, out exists, out checkError))
                {
                    error = "Failed to check if remote file exists: " + checkError;
                    return false;
                }

                if (!exists)
                {
                    error = "Source file does not exist: " + remotePath;
                    return false;
                }

                if (!overwrite && File.Exists(localPath))
                {
                    error = "Destination file already exists: " + localPath;
                    return false;
                }

                // Ensure local directory exists
                var localDir = Path.GetDirectoryName(localPath);
                if (!Directory.Exists(localDir))
                    Directory.CreateDirectory(localDir);

                // Download the file
                var request = CreateFtpRequest(settings, remotePath, WebRequestMethods.Ftp.DownloadFile);
                using (var response = (FtpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var fileStream = File.Create(localPath))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                error = "Download failed: " + ex.Message;
                return false;
            }
        }

        public bool FileExists(ConnectionSettings settings, string remotePath, out bool exists, out string error)
        {
            exists = false;
            error = null;

            try
            {
                var request = CreateFtpRequest(settings, remotePath, WebRequestMethods.Ftp.GetFileSize);
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    exists = true;
                    return true;
                }
            }
            catch (WebException ex)
            {
                var ftpResponse = ex.Response as FtpWebResponse;
                if (ftpResponse != null && ftpResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    exists = false;
                    return true; // File doesn't exist, but operation succeeded
                }
                error = "Failed to check if file exists: " + ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                error = "Failed to check if file exists: " + ex.Message;
                return false;
            }
        }

        public bool DeleteFile(ConnectionSettings settings, string remotePath, out string error)
        {
            error = null;
            try
            {
                var request = CreateFtpRequest(settings, remotePath, WebRequestMethods.Ftp.DeleteFile);
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == FtpStatusCode.FileActionOK;
                }
            }
            catch (Exception ex)
            {
                error = "Failed to delete file: " + ex.Message;
                return false;
            }
        }

        public bool ListFiles(ConnectionSettings settings, string remoteDir, out List<string> files, out string error)
        {
            files = new List<string>();
            error = null;

            try
            {
                // Normalize directory path
                if (!remoteDir.StartsWith("/"))
                    remoteDir = "/" + remoteDir;

                var request = CreateFtpRequest(settings, remoteDir, WebRequestMethods.Ftp.ListDirectory);
                using (var response = (FtpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                                                if (!StringExtensions.IsNullOrWhiteSpace(line))
                        {
                            // Simple file listing - in real implementation you might want to parse detailed listing
                            files.Add(line.Trim());
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                error = "Failed to list files: " + ex.Message;
                return false;
            }
        }

        public void SetProgressCallback(Action<int> callback)
        {
            _progressCallback = callback;
        }

        private FtpWebRequest CreateFtpRequest(ConnectionSettings settings, string path, string method)
        {
            var uri = "ftp://" + settings.Host + ":" + settings.Port + path;
            var request = (FtpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.Credentials = new NetworkCredential(settings.Username, settings.Password);
            request.UseBinary = true;
            request.UsePassive = true;
            request.KeepAlive = false;
            return request;
        }
    }
}
