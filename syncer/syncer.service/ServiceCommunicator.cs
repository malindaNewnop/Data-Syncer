using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace syncer.service
{
    /// <summary>
    /// Handles communication between the UI application and the Windows service
    /// Uses named pipes for .NET 3.5 compatibility
    /// </summary>
    public class ServiceCommunicator : IDisposable
    {
        private const string PIPE_NAME = "FTPSyncerServicePipe";
        private NamedPipeServerStream _pipeServer;
        private Thread _pipeThread;
        private bool _isRunning;
        private syncer.core.IJobRunner _jobRunner;
        private syncer.core.ILogService _logService;

        public event EventHandler<JobRequestEventArgs> JobStartRequested;
        public event EventHandler<JobRequestEventArgs> JobStopRequested;

        public ServiceCommunicator(syncer.core.IJobRunner jobRunner, syncer.core.ILogService logService)
        {
            _jobRunner = jobRunner;
            _logService = logService;
        }

        /// <summary>
        /// Start listening for communication from UI
        /// </summary>
        public void StartListening()
        {
            if (_isRunning) return;

            _isRunning = true;
            _pipeThread = new Thread(PipeThreadProc);
            _pipeThread.IsBackground = true;
            _pipeThread.Start();
            
            _logService?.LogInfo(null, "Service communicator started listening");
        }

        /// <summary>
        /// Stop listening for communication
        /// </summary>
        public void StopListening()
        {
            _isRunning = false;
            
            try
            {
                if (_pipeServer != null)
                {
                    _pipeServer.Close();
                    _pipeServer.Dispose();
                    _pipeServer = null;
                }
            }
            catch (Exception ex)
            {
                _logService?.LogError(null, "Error stopping service communicator: " + ex.Message);
            }

            if (_pipeThread != null && _pipeThread.IsAlive)
            {
                _pipeThread.Join(5000); // Wait up to 5 seconds
            }

            _logService?.LogInfo(null, "Service communicator stopped");
        }

        /// <summary>
        /// Main pipe communication thread
        /// </summary>
        private void PipeThreadProc()
        {
            while (_isRunning)
            {
                try
                {
                    _pipeServer = new NamedPipeServerStream(PIPE_NAME, PipeDirection.InOut, 1);
                    _pipeServer.WaitForConnection();

                    if (!_isRunning) break;

                    // Handle the client request
                    HandleClientRequest(_pipeServer);
                }
                catch (Exception ex)
                {
                    if (_isRunning) // Only log if we're supposed to be running
                    {
                        _logService?.LogError(null, "Pipe communication error: " + ex.Message);
                        Thread.Sleep(1000); // Wait before retrying
                    }
                }
                finally
                {
                    try
                    {
                        if (_pipeServer != null)
                        {
                            _pipeServer.Close();
                            _pipeServer.Dispose();
                            _pipeServer = null;
                        }
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Handle incoming request from UI
        /// </summary>
        private void HandleClientRequest(NamedPipeServerStream pipeServer)
        {
            try
            {
                // Read request
                byte[] buffer = new byte[1024];
                int bytesRead = pipeServer.Read(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                ServiceRequest serviceRequest = DeserializeRequest(request);
                ServiceResponse response = new ServiceResponse { Success = false, Message = "Unknown error" };

                if (serviceRequest != null)
                {
                    response = ProcessRequest(serviceRequest);
                }
                else
                {
                    response.Message = "Invalid request format";
                }

                // Send response
                string responseStr = SerializeResponse(response);
                byte[] responseBytes = Encoding.UTF8.GetBytes(responseStr);
                pipeServer.Write(responseBytes, 0, responseBytes.Length);
                pipeServer.Flush();
            }
            catch (Exception ex)
            {
                _logService?.LogError(null, "Error handling client request: " + ex.Message);
            }
        }

        /// <summary>
        /// Process the service request
        /// </summary>
        private ServiceResponse ProcessRequest(ServiceRequest request)
        {
            ServiceResponse response = new ServiceResponse();

            try
            {
                switch (request.Command.ToLower())
                {
                    case "start_job":
                        if (!string.IsNullOrEmpty(request.JobId))
                        {
                            bool started = _jobRunner.StartJob(request.JobId);
                            response.Success = started;
                            response.Message = started ? "Job started successfully" : "Failed to start job - may already be running";
                            
                            // Fire event for job tracking
                            if (started && JobStartRequested != null)
                            {
                                JobStartRequested(this, new JobRequestEventArgs { JobId = request.JobId });
                            }
                        }
                        else
                        {
                            response.Message = "Job ID is required";
                        }
                        break;

                    case "stop_job":
                        if (!string.IsNullOrEmpty(request.JobId))
                        {
                            bool stopped = _jobRunner.CancelJob(request.JobId);
                            response.Success = stopped;
                            response.Message = stopped ? "Job stopped successfully" : "Failed to stop job";

                            // Fire event for job tracking
                            if (stopped && JobStopRequested != null)
                            {
                                JobStopRequested(this, new JobRequestEventArgs { JobId = request.JobId });
                            }
                        }
                        else
                        {
                            response.Message = "Job ID is required";
                        }
                        break;

                    case "get_running_jobs":
                        response.Success = true;
                        response.Data = string.Join(",", _jobRunner.GetRunningJobIds().ToArray());
                        response.Message = "Running jobs retrieved successfully";
                        break;

                    case "is_job_running":
                        if (!string.IsNullOrEmpty(request.JobId))
                        {
                            bool isRunning = _jobRunner.IsRunning(request.JobId);
                            response.Success = true;
                            response.Data = isRunning.ToString();
                            response.Message = "Job status retrieved successfully";
                        }
                        else
                        {
                            response.Message = "Job ID is required";
                        }
                        break;

                    case "ping":
                        response.Success = true;
                        response.Message = "Service is running";
                        break;

                    default:
                        response.Message = "Unknown command: " + request.Command;
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error processing request: " + ex.Message;
                _logService?.LogError(null, "Error processing service request: " + ex.Message);
            }

            return response;
        }

        private ServiceRequest DeserializeRequest(string xml)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ServiceRequest));
                using (StringReader reader = new StringReader(xml))
                {
                    return (ServiceRequest)serializer.Deserialize(reader);
                }
            }
            catch
            {
                return null;
            }
        }

        private string SerializeResponse(ServiceResponse response)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ServiceResponse));
                using (StringWriter writer = new StringWriter())
                {
                    serializer.Serialize(writer, response);
                    return writer.ToString();
                }
            }
            catch
            {
                return "<ServiceResponse><Success>false</Success><Message>Serialization error</Message></ServiceResponse>";
            }
        }

        public void Dispose()
        {
            StopListening();
        }
    }

    /// <summary>
    /// Request from UI to service
    /// </summary>
    [Serializable]
    public class ServiceRequest
    {
        public string Command { get; set; }
        public string JobId { get; set; }
        public string Data { get; set; }
    }

    /// <summary>
    /// Response from service to UI
    /// </summary>
    [Serializable]
    public class ServiceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
    }

    /// <summary>
    /// Event args for job requests
    /// </summary>
    public class JobRequestEventArgs : EventArgs
    {
        public string JobId { get; set; }
    }
}
