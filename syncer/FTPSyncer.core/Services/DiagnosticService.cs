using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using FTPSyncer.core.Models;

namespace FTPSyncer.core.Services
{
    /// <summary>
    /// Service for testing connections, simulating transfers, and diagnosing issues
    /// </summary>
    public class DiagnosticService
    {
        private readonly ILogService _logService;
        private readonly ITransferClientFactory _clientFactory;

        public DiagnosticService(ILogService logService, ITransferClientFactory clientFactory)
        {
            _logService = logService ?? throw new ArgumentNullException("logService");
            _clientFactory = clientFactory ?? throw new ArgumentNullException("clientFactory");
        }

        /// <summary>
        /// Test connection to a server using the specified protocol and settings
        /// </summary>
        public ConnectionTestResult TestConnection(ConnectionSettings settings)
        {
            var result = new ConnectionTestResult
            {
                Protocol = settings.Protocol,
                Host = settings.Host,
                Port = settings.Port,
                Success = false
            };

            _logService.LogInfo($"Testing connection to {settings.Protocol}://{settings.Host}:{settings.Port}", "DiagnosticService");
            
            try
            {
                // First check if host is reachable via ping
                var pingSuccess = PingHost(settings.Host);
                result.PingSuccess = pingSuccess;
                
                if (!pingSuccess)
                {
                    result.Details = "Host could not be reached via ping. Check if the server is online.";
                    return result;
                }
                
                // Now test with actual protocol client
                var client = _clientFactory.Create(settings.Protocol);
                string error;
                
                var connectionSuccess = client.TestConnection(settings, out error);
                result.Success = connectionSuccess;
                
                if (!connectionSuccess)
                {
                    result.Details = $"Connection failed: {error}";
                    _logService.LogError($"Connection test failed for {settings.Protocol}://{settings.Host}:{settings.Port}: {error}");
                }
                else
                {
                    result.Details = "Connection successful";
                    _logService.LogInfo($"Connection test successful for {settings.Protocol}://{settings.Host}:{settings.Port}");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Details = $"Error testing connection: {ex.Message}";
                _logService.LogError($"Error testing connection: {ex.Message}", ex);
            }
            
            return result;
        }

        /// <summary>
        /// Verify file path validity
        /// </summary>
        public PathValidationResult ValidatePath(string path, bool isLocal)
        {
            var result = new PathValidationResult
            {
                Path = path,
                IsLocal = isLocal,
                IsValid = false
            };

            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    result.Details = "Path is empty";
                    return result;
                }
                
                if (isLocal)
                {
                    // Local path validation
                    if (Directory.Exists(path))
                    {
                        result.IsValid = true;
                        result.IsDirectory = true;
                        result.Details = "Directory exists";
                    }
                    else if (File.Exists(path))
                    {
                        result.IsValid = true;
                        result.IsDirectory = false;
                        result.Details = "File exists";
                    }
                    else
                    {
                        // Check if parent directory exists
                        string parentDir = Path.GetDirectoryName(path);
                        if (string.IsNullOrEmpty(parentDir) || Directory.Exists(parentDir))
                        {
                            result.IsValid = true;
                            result.Details = "Path is valid but doesn't exist yet";
                        }
                        else
                        {
                            result.Details = "Path is invalid. Parent directory doesn't exist.";
                        }
                    }
                }
                else
                {
                    // Remote paths are valid as long as they're not empty and don't contain invalid characters
                    char[] invalidChars = new char[] { '<', '>', '\"', '|' };
                    if (path.IndexOfAny(invalidChars) >= 0)
                    {
                        result.Details = "Path contains invalid characters";
                    }
                    else
                    {
                        result.IsValid = true;
                        result.Details = "Remote path format is valid";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Details = $"Error validating path: {ex.Message}";
                _logService.LogError($"Error validating path {path}: {ex.Message}", ex);
            }
            
            return result;
        }

        /// <summary>
        /// Simulate a file transfer to estimate performance without actually transferring
        /// </summary>
        public SimulatedTransferResult SimulateTransfer(SyncJob job, int sampleCount = 5)
        {
            var result = new SimulatedTransferResult
            {
                JobName = job.Name,
                SourcePath = job.SourcePath,
                DestinationPath = job.DestinationPath,
                ProtocolFrom = job.SourceConnection.Protocol,
                ProtocolTo = job.DestinationConnection.Protocol,
                Success = false
            };

            _logService.LogInfo($"Simulating transfer for job '{job.Name}'", "DiagnosticService");
            
            try
            {
                // Get source files for analysis
                List<string> sourceFiles = new List<string>();
                long totalBytes = 0;
                
                if (job.SourceConnection.Protocol == ProtocolType.Local)
                {
                    if (Directory.Exists(job.SourcePath))
                    {
                        var searchOption = job.IncludeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                        var files = Directory.GetFiles(job.SourcePath, "*.*", searchOption);
                        
                        sourceFiles.AddRange(files);
                        
                        foreach (var file in files)
                        {
                            var info = new FileInfo(file);
                            totalBytes += info.Length;
                        }
                    }
                    else if (File.Exists(job.SourcePath))
                    {
                        sourceFiles.Add(job.SourcePath);
                        var info = new FileInfo(job.SourcePath);
                        totalBytes += info.Length;
                    }
                }
                else
                {
                    var client = _clientFactory.Create(job.SourceConnection.Protocol);
                    string error;
                    
                    if (client.ListFiles(job.SourceConnection, job.SourcePath, out sourceFiles, out error))
                    {
                        // Can't get size of remote files without downloading them
                        result.HasSizeEstimate = false;
                    }
                    else
                    {
                        result.Details = $"Could not list source files: {error}";
                        return result;
                    }
                }
                
                result.FileCount = sourceFiles.Count;
                result.TotalBytes = totalBytes;
                
                // Test connection to destination
                var destTestResult = TestConnection(job.DestinationConnection);
                if (!destTestResult.Success)
                {
                    result.Details = $"Destination connection failed: {destTestResult.Details}";
                    return result;
                }
                
                // Simulate transfer by sending small test files
                var testResults = new List<TransferSpeedResult>();
                for (int i = 0; i < sampleCount; i++)
                {
                    var speedTest = MeasureTransferSpeed(job.SourceConnection, job.DestinationConnection);
                    if (speedTest.Success)
                    {
                        testResults.Add(speedTest);
                    }
                }
                
                if (testResults.Count > 0)
                {
                    // Calculate average speed
                    double totalSpeed = 0;
                    foreach (var test in testResults)
                    {
                        totalSpeed += test.BytesPerSecond;
                    }
                    
                    double avgSpeed = totalSpeed / testResults.Count;
                    result.EstimatedSpeedBytesPerSecond = avgSpeed;
                    
                    if (totalBytes > 0)
                    {
                        // Calculate estimated transfer time
                        result.EstimatedTransferTimeSeconds = totalBytes / avgSpeed;
                    }
                    
                    result.Success = true;
                    result.HasSpeedEstimate = true;
                }
                else
                {
                    result.Details = "Could not estimate transfer speed";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Details = $"Error simulating transfer: {ex.Message}";
                _logService.LogError($"Error simulating transfer: {ex.Message}", ex);
            }
            
            return result;
        }
        
        /// <summary>
        /// Ping a host to check if it's reachable
        /// </summary>
        private bool PingHost(string nameOrAddress)
        {
            try
            {
                Ping pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress, 2000); // 2-second timeout
                return reply?.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Measure transfer speed between two endpoints
        /// </summary>
        private TransferSpeedResult MeasureTransferSpeed(ConnectionSettings source, ConnectionSettings destination)
        {
            var result = new TransferSpeedResult
            {
                Success = false
            };
            
            try
            {
                // Create test file of approximately 100 KB
                byte[] testData = new byte[102400]; // 100 KB
                new Random().NextBytes(testData); // Fill with random data
                
                string tempSourcePath = Path.GetTempFileName();
                string tempDestPath = Path.GetTempFileName();
                
                try
                {
                    // Write test file
                    File.WriteAllBytes(tempSourcePath, testData);
                    
                    // Measure transfer time
                    DateTime startTime = DateTime.Now;
                    
                    if (destination.Protocol == ProtocolType.Local)
                    {
                        // Transfer to local
                        File.Copy(tempSourcePath, tempDestPath, true);
                    }
                    else
                    {
                        // Transfer to remote
                        var client = _clientFactory.Create(destination.Protocol);
                        string error;
                        
                        if (!client.UploadFile(destination, tempSourcePath, "test_file", true, out error))
                        {
                            result.Details = $"Upload test failed: {error}";
                            return result;
                        }
                    }
                    
                    DateTime endTime = DateTime.Now;
                    TimeSpan duration = endTime - startTime;
                    
                    // Calculate speed
                    result.BytesPerSecond = testData.Length / duration.TotalSeconds;
                    result.Success = true;
                }
                finally
                {
                    // Clean up
                    if (File.Exists(tempSourcePath))
                        File.Delete(tempSourcePath);
                        
                    if (File.Exists(tempDestPath))
                        File.Delete(tempDestPath);
                }
            }
            catch (Exception ex)
            {
                result.Details = ex.Message;
            }
            
            return result;
        }
    }

    /// <summary>
    /// Result of connection test
    /// </summary>
    public class ConnectionTestResult
    {
        public ProtocolType Protocol { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool Success { get; set; }
        public bool PingSuccess { get; set; }
        public string Details { get; set; }
    }

    /// <summary>
    /// Result of path validation
    /// </summary>
    public class PathValidationResult
    {
        public string Path { get; set; }
        public bool IsLocal { get; set; }
        public bool IsValid { get; set; }
        public bool IsDirectory { get; set; }
        public string Details { get; set; }
    }

    /// <summary>
    /// Result of simulated transfer
    /// </summary>
    public class SimulatedTransferResult
    {
        public string JobName { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public ProtocolType ProtocolFrom { get; set; }
        public ProtocolType ProtocolTo { get; set; }
        public bool Success { get; set; }
        public int FileCount { get; set; }
        public long TotalBytes { get; set; }
        public bool HasSizeEstimate { get; set; }
        public bool HasSpeedEstimate { get; set; }
        public double EstimatedSpeedBytesPerSecond { get; set; }
        public double EstimatedTransferTimeSeconds { get; set; }
        public string Details { get; set; }
    }

    /// <summary>
    /// Result of transfer speed measurement
    /// </summary>
    private class TransferSpeedResult
    {
        public bool Success { get; set; }
        public double BytesPerSecond { get; set; }
        public string Details { get; set; }
    }
}





