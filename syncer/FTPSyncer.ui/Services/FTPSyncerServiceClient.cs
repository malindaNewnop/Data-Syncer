using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Xml.Serialization;

namespace FTPSyncer.ui.Services
{
    /// <summary>
    /// Client to communicate with the FTPSyncer Windows service
    /// Enables the UI to start/stop jobs and check their status
    /// </summary>
    public class FTPSyncerServiceClient
    {
        private const string PIPE_NAME = "FTPSyncerServicePipe";
        private const int PIPE_TIMEOUT = 5000; // 5 seconds

        /// <summary>
        /// Start a job via the Windows service
        /// </summary>
        public ServiceOperationResult StartJob(string jobId)
        {
            var request = new ServiceRequest
            {
                Command = "start_job",
                JobId = jobId
            };

            return SendRequest(request);
        }

        /// <summary>
        /// Stop a job via the Windows service
        /// </summary>
        public ServiceOperationResult StopJob(string jobId)
        {
            var request = new ServiceRequest
            {
                Command = "stop_job",
                JobId = jobId
            };

            return SendRequest(request);
        }

        /// <summary>
        /// Check if a job is currently running
        /// </summary>
        public ServiceOperationResult IsJobRunning(string jobId)
        {
            var request = new ServiceRequest
            {
                Command = "is_job_running",
                JobId = jobId
            };

            return SendRequest(request);
        }

        /// <summary>
        /// Get list of currently running job IDs
        /// </summary>
        public ServiceOperationResult GetRunningJobs()
        {
            var request = new ServiceRequest
            {
                Command = "get_running_jobs"
            };

            return SendRequest(request);
        }

        /// <summary>
        /// Check if the Windows service is running and responsive
        /// </summary>
        public ServiceOperationResult PingService()
        {
            var request = new ServiceRequest
            {
                Command = "ping"
            };

            return SendRequest(request);
        }

        /// <summary>
        /// Check if the Windows service is installed and running
        /// </summary>
        public bool IsServiceAvailable()
        {
            try
            {
                // Check if service is installed and running
                using (var serviceController = new System.ServiceProcess.ServiceController("FTPSyncerService"))
                {
                    return serviceController.Status == System.ServiceProcess.ServiceControllerStatus.Running;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Send a request to the service and get response
        /// </summary>
        private ServiceOperationResult SendRequest(ServiceRequest request)
        {
            try
            {
                using (var pipeClient = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.InOut))
                {
                    // Connect to the service
                    pipeClient.Connect(PIPE_TIMEOUT);

                    // Serialize and send request
                    string requestXml = SerializeRequest(request);
                    byte[] requestBytes = Encoding.UTF8.GetBytes(requestXml);
                    pipeClient.Write(requestBytes, 0, requestBytes.Length);
                    pipeClient.Flush();

                    // Read response
                    byte[] responseBuffer = new byte[1024];
                    int bytesRead = pipeClient.Read(responseBuffer, 0, responseBuffer.Length);
                    string responseXml = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

                    // Deserialize response
                    ServiceResponse response = DeserializeResponse(responseXml);
                    
                    return new ServiceOperationResult
                    {
                        Success = response != null && response.Success,
                        Message = response?.Message ?? "Unknown error",
                        Data = response?.Data,
                        ServiceAvailable = true
                    };
                }
            }
            catch (TimeoutException)
            {
                return new ServiceOperationResult
                {
                    Success = false,
                    Message = "Service communication timeout. The Windows service may not be running.",
                    ServiceAvailable = false
                };
            }
            catch (Exception ex)
            {
                return new ServiceOperationResult
                {
                    Success = false,
                    Message = string.Format("Service communication error: {0}", ex.Message),
                    ServiceAvailable = false
                };
            }
        }

        private string SerializeRequest(ServiceRequest request)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ServiceRequest));
                using (StringWriter writer = new StringWriter())
                {
                    serializer.Serialize(writer, request);
                    return writer.ToString();
                }
            }
            catch
            {
                return "<ServiceRequest><Command>ping</Command></ServiceRequest>";
            }
        }

        private ServiceResponse DeserializeResponse(string xml)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ServiceResponse));
                using (StringReader reader = new StringReader(xml))
                {
                    return (ServiceResponse)serializer.Deserialize(reader);
                }
            }
            catch
            {
                return new ServiceResponse { Success = false, Message = "Failed to parse service response" };
            }
        }
    }

    /// <summary>
    /// Result of a service operation
    /// </summary>
    public class ServiceOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
        public bool ServiceAvailable { get; set; }
    }

    /// <summary>
    /// Request to send to service - must match service definitions
    /// </summary>
    [Serializable]
    public class ServiceRequest
    {
        public string Command { get; set; }
        public string JobId { get; set; }
        public string Data { get; set; }
    }

    /// <summary>
    /// Response from service - must match service definitions
    /// </summary>
    [Serializable]
    public class ServiceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
    }
}





