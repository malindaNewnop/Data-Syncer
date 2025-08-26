using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace syncer.core
{
    public class PipeServer : IPipeServer
    {
        private NamedPipeServerStream _pipeServer;
        private Thread _serverThread;
        private bool _isRunning;
        private readonly object _lockObject = new object();

        public event EventHandler<PipeMessageEventArgs> MessageReceived;
        public event EventHandler<PipeClientEventArgs> ClientConnected;
        public event EventHandler<PipeClientEventArgs> ClientDisconnected;

        public bool IsRunning
        {
            get 
            { 
                lock (_lockObject)
                {
                    return _isRunning;
                }
            }
        }

        public void Start()
        {
            lock (_lockObject)
            {
                if (_isRunning)
                {
                    throw new InvalidOperationException("Pipe server is already running");
                }

                _isRunning = true;
                _serverThread = new Thread(ServerLoop)
                {
                    IsBackground = true,
                    Name = "PipeServerThread"
                };
                _serverThread.Start();
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                if (!_isRunning)
                    return;

                _isRunning = false;

                try
                {
                    _pipeServer?.Close();
                    _pipeServer?.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to close pipe server: " + ex.Message);
                }

                if (_serverThread != null && _serverThread.IsAlive)
                {
                    if (!_serverThread.Join(5000)) // Wait 5 seconds
                    {
                        try
                        {
                            _serverThread.Abort();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Failed to abort server thread: " + ex.Message);
                        }
                    }
                }

                _pipeServer = null;
                _serverThread = null;
            }
        }

        public bool SendMessage(string message)
        {
            lock (_lockObject)
            {
                if (!_isRunning || _pipeServer == null || !_pipeServer.IsConnected)
                    return false;

                try
                {
                    var messageBytes = Encoding.UTF8.GetBytes(message);
                    var lengthBytes = BitConverter.GetBytes(messageBytes.Length);
                    
                    _pipeServer.Write(lengthBytes, 0, lengthBytes.Length);
                    _pipeServer.Write(messageBytes, 0, messageBytes.Length);
                    _pipeServer.Flush();
                    
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to send message: {ex.Message}");
                    return false;
                }
            }
        }

        private void ServerLoop()
        {
            while (_isRunning)
            {
                try
                {
                    // Create a new pipe server instance
                    _pipeServer = new NamedPipeServerStream(
                        Paths.PipeName,
                        PipeDirection.InOut,
                        1, // Max 1 client at a time
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);

                    // Wait for client connection
                    _pipeServer.WaitForConnection();

                    if (!_isRunning)
                        break;

                    // Notify client connected
                    OnClientConnected(new PipeClientEventArgs { ClientId = "Client" });

                    // Handle client communication
                    HandleClient();

                    // Notify client disconnected
                    OnClientDisconnected(new PipeClientEventArgs { ClientId = "Client" });
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        System.Diagnostics.Debug.WriteLine($"Pipe server error: {ex.Message}");
                        Thread.Sleep(1000); // Wait before retrying
                    }
                }
                finally
                {
                    try
                    {
                        _pipeServer?.Close();
                        _pipeServer?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to dispose pipe server in finally block: " + ex.Message);
                    }
                }
            }
        }

        private void HandleClient()
        {
            try
            {
                while (_isRunning && _pipeServer.IsConnected)
                {
                    // Read message length
                    var lengthBuffer = new byte[4];
                    var bytesRead = _pipeServer.Read(lengthBuffer, 0, 4);
                    
                    if (bytesRead == 0)
                        break; // Client disconnected

                    if (bytesRead != 4)
                        continue; // Invalid message

                    var messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                    if (messageLength <= 0 || messageLength > 1024 * 1024) // Max 1MB message
                        continue; // Invalid message length

                    // Read message content
                    var messageBuffer = new byte[messageLength];
                    var totalRead = 0;
                    
                    while (totalRead < messageLength)
                    {
                        bytesRead = _pipeServer.Read(messageBuffer, totalRead, messageLength - totalRead);
                        if (bytesRead == 0)
                            break; // Client disconnected
                        totalRead += bytesRead;
                    }

                    if (totalRead == messageLength)
                    {
                        var message = Encoding.UTF8.GetString(messageBuffer);
                        OnMessageReceived(new PipeMessageEventArgs 
                        { 
                            Message = message, 
                            ClientId = "Client" 
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                if (_isRunning)
                {
                    System.Diagnostics.Debug.WriteLine($"Client handling error: {ex.Message}");
                }
            }
        }

        protected virtual void OnMessageReceived(PipeMessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        protected virtual void OnClientConnected(PipeClientEventArgs e)
        {
            ClientConnected?.Invoke(this, e);
        }

        protected virtual void OnClientDisconnected(PipeClientEventArgs e)
        {
            ClientDisconnected?.Invoke(this, e);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
