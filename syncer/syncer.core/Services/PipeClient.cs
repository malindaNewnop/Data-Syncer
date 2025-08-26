using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace syncer.core
{
    public class PipeClient : IPipeClient
    {
        private NamedPipeClientStream _pipeClient;
        private Thread _readerThread;
        private bool _isConnected;
        private readonly object _lockObject = new object();

        public event EventHandler<PipeMessageEventArgs> MessageReceived;
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public bool IsConnected
        {
            get 
            { 
                lock (_lockObject)
                {
                    return _isConnected && _pipeClient != null && _pipeClient.IsConnected;
                }
            }
        }

        public bool Connect(int timeoutMs = 5000)
        {
            lock (_lockObject)
            {
                if (_isConnected)
                    return true;

                try
                {
                    _pipeClient = new NamedPipeClientStream(
                        ".",
                        Paths.PipeName,
                        PipeDirection.InOut);

                    _pipeClient.Connect(timeoutMs);

                    if (_pipeClient.IsConnected)
                    {
                        _isConnected = true;

                        // Start reader thread
                        _readerThread = new Thread(ReaderLoop)
                        {
                            IsBackground = true,
                            Name = "PipeClientReaderThread"
                        };
                        _readerThread.Start();

                        OnConnected();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to connect to pipe server: {ex.Message}");
                    
                    try
                    {
                        _pipeClient?.Close();
                        _pipeClient?.Dispose();
                    }
                    catch { }
                    
                    _pipeClient = null;
                }

                return false;
            }
        }

        public void Disconnect()
        {
            lock (_lockObject)
            {
                if (!_isConnected)
                    return;

                _isConnected = false;

                try
                {
                    _pipeClient?.Close();
                    _pipeClient?.Dispose();
                }
                catch { }

                if (_readerThread != null && _readerThread.IsAlive)
                {
                    if (!_readerThread.Join(5000)) // Wait 5 seconds
                    {
                        try
                        {
                            _readerThread.Abort();
                        }
                        catch { }
                    }
                }

                _pipeClient = null;
                _readerThread = null;

                OnDisconnected();
            }
        }

        public bool SendMessage(string message)
        {
            lock (_lockObject)
            {
                if (!_isConnected || _pipeClient == null || !_pipeClient.IsConnected)
                    return false;

                try
                {
                    var messageBytes = Encoding.UTF8.GetBytes(message);
                    var lengthBytes = BitConverter.GetBytes(messageBytes.Length);
                    
                    _pipeClient.Write(lengthBytes, 0, lengthBytes.Length);
                    _pipeClient.Write(messageBytes, 0, messageBytes.Length);
                    _pipeClient.Flush();
                    
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to send message: {ex.Message}");
                    
                    // Connection might be broken, disconnect
                    Disconnect();
                    return false;
                }
            }
        }

        private void ReaderLoop()
        {
            try
            {
                while (_isConnected && _pipeClient != null && _pipeClient.IsConnected)
                {
                    // Read message length
                    var lengthBuffer = new byte[4];
                    var bytesRead = _pipeClient.Read(lengthBuffer, 0, 4);
                    
                    if (bytesRead == 0)
                        break; // Server disconnected

                    if (bytesRead != 4)
                        continue; // Invalid message

                    var messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                    if (messageLength <= 0 || messageLength > 1024 * 1024) // Max 1MB message
                        continue; // Invalid message length

                    // Read message content
                    var messageBuffer = new byte[messageLength];
                    var totalRead = 0;
                    
                    while (totalRead < messageLength && _isConnected)
                    {
                        bytesRead = _pipeClient.Read(messageBuffer, totalRead, messageLength - totalRead);
                        if (bytesRead == 0)
                            break; // Server disconnected
                        totalRead += bytesRead;
                    }

                    if (totalRead == messageLength)
                    {
                        var message = Encoding.UTF8.GetString(messageBuffer);
                        OnMessageReceived(new PipeMessageEventArgs 
                        { 
                            Message = message, 
                            ClientId = "Server" 
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                if (_isConnected)
                {
                    System.Diagnostics.Debug.WriteLine($"Reader loop error: {ex.Message}");
                }
            }
            finally
            {
                // Connection lost, clean up
                if (_isConnected)
                {
                    Disconnect();
                }
            }
        }

        protected virtual void OnMessageReceived(PipeMessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        protected virtual void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
