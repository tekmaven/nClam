namespace nClam
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class ClamClient : IClamClient
    {
        /// <summary>
        /// Maximum size (in bytes) which streams will be broken up to when sending to the ClamAV server.  Used in the SendAndScanFile methods.  128kb is the default size.
        /// </summary>
        public int MaxChunkSize { get; set; }

        /// <summary>
        /// Maximum size (in bytes) that can be streamed to the ClamAV server before it will terminate the connection. Used in the SendAndScanFile methods. 25mb is the default size.
        /// </summary>
        public long MaxStreamSize { get; set; }

        /// <summary>
        /// Address to the ClamAV server
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// IP Address to the ClamAV server
        /// </summary>
        public IPAddress ServerIP { get; set; }

        /// <summary>
        /// Port which the ClamAV server is listening on
        /// </summary>
        public int Port { get; set; }

        private ClamClient()
        {
            MaxChunkSize = 131072; //128k
            MaxStreamSize = 26214400; //25mb
        }

        /// <summary>
        /// A class to connect to a ClamAV server and request virus scans
        /// </summary>
        /// <param name="server">Address to the ClamAV server</param>
        /// <param name="port">Port which the ClamAV server is listening on</param>
        public ClamClient(string server, int port = 3310) : this()
        {
            Server = server;
            Port = port;
        }

        /// <summary>
        /// A class to connect to a ClamAV server via IP and request virus scans
        /// </summary>
        /// <param name="serverIP">IP Address to the ClamAV server</param>
        /// <param name="port">Port which the ClamAV server is listening on</param>
        public ClamClient(IPAddress serverIP, int port = 3310) : this()
        {
            ServerIP = serverIP;
            Port = port;
        }

        /// <summary>
        /// Helper method which connects to the ClamAV Server, performs the command and returns the result.
        /// </summary>
        /// <param name="command">The command to execute on the ClamAV Server</param>
        /// <param name="cancellationToken">cancellation token used in requests</param>
        /// <param name="additionalCommand">Action to define additional server communications.  Executed after the command is sent and before the response is read.</param>
        /// <returns>The full response from the ClamAV server.</returns>
        private async Task<string> ExecuteClamCommandAsync(string command, CancellationToken cancellationToken, Func<Stream, CancellationToken, Task> additionalCommand = null)
        {
#if DEBUG
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
#endif
            string result;

            var clam = new TcpClient(AddressFamily.InterNetwork);

            try
            {
                using (var stream = await CreateConnection(clam))
                {
                    var commandText = String.Format("z{0}\0", command);
                    var commandBytes = Encoding.UTF8.GetBytes(commandText);
                    await stream.WriteAsync(commandBytes, 0, commandBytes.Length, cancellationToken).ConfigureAwait(false);

                    if (additionalCommand != null)
                    {
                        await additionalCommand(stream, cancellationToken).ConfigureAwait(false);
                    }

                    using (var reader = new StreamReader(stream))
                    {
                        result = await reader.ReadToEndAsync().ConfigureAwait(false);

                        if (!String.IsNullOrEmpty(result))
                        {
                            //if we have a result, trim off the terminating null character
                            result = result.TrimEnd('\0');
                        }
                    }
                }
            }
            finally
            {
                if (clam.Connected)
                {
                    clam.Close();
                }
            }
#if DEBUG
            stopWatch.Stop();
            System.Diagnostics.Debug.WriteLine("Command {0} took: {1}", command, stopWatch.Elapsed);
#endif
            return result;
        }

        /// <summary>
        /// Helper method to send a byte array over the wire to the ClamAV server, split up in chunks.
        /// </summary>
        /// <param name="sourceStream">The stream to send to the ClamAV server.</param>
        /// <param name="clamStream">The communication channel to the ClamAV server.</param>
        /// <param name="cancellationToken"></param>
        private async Task SendStreamFileChunksAsync(Stream sourceStream, Stream clamStream, CancellationToken cancellationToken)
        {
            var size = MaxChunkSize;
            var bytes = new byte[size];

            while ((size = await sourceStream.ReadAsync(bytes, 0, size, cancellationToken).ConfigureAwait(false)) > 0)
            {
                if (sourceStream.CanSeek && sourceStream.Position > MaxStreamSize)
                {
                    throw new MaxStreamSizeExceededException(MaxStreamSize);
                }

                var sizeBytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(size));  //convert size to NetworkOrder!
                await clamStream.WriteAsync(sizeBytes, 0, sizeBytes.Length, cancellationToken).ConfigureAwait(false);
                await clamStream.WriteAsync(bytes, 0, size, cancellationToken).ConfigureAwait(false);
            }

            var newMessage = BitConverter.GetBytes(0);
            await clamStream.WriteAsync(newMessage, 0, newMessage.Length, cancellationToken).ConfigureAwait(false);
        }

        protected async virtual Task<Stream> CreateConnection(TcpClient clam)
        {
            await (ServerIP == null ? clam.ConnectAsync(Server, Port) : clam.ConnectAsync(ServerIP, Port)).ConfigureAwait(false);

            return clam.GetStream();
        }

        /// <summary>
        /// Gets the ClamAV server version
        /// </summary>
        public Task<string> GetVersionAsync()
        {
            return GetVersionAsync(CancellationToken.None);
        }

        /// <summary>
        /// Gets the ClamAV server version
        /// </summary>
        public async Task<string> GetVersionAsync(CancellationToken cancellationToken)
        {
            var version = await ExecuteClamCommandAsync("VERSION", cancellationToken).ConfigureAwait(false);

            return version;
        }

        /// <summary>
        /// Executes a PING command on the ClamAV server.
        /// <para>Tip:when you call this method , please wrap your call in a try/catch ,because if return is not true,will throw a exception,or you can use <see cref="TryPingAsync"/></para>
        /// </summary>
        /// <returns>If the server responds with PONG, returns true.  Otherwise throw a exception.</returns>
        public Task<bool> PingAsync()
        {
            return PingAsync(CancellationToken.None);
        }

        /// <summary>
        /// Executes a PING command on the ClamAV server.
        /// <para>Tip:when you call this method , please wrap your call in a try/catch ,because if return is not true,will throw a exception,or you can use <see cref="TryPingAsync"/></para>
        /// </summary>
        /// <returns>If the server responds with PONG, returns true.  Otherwise throw a exception.</returns>
        public async Task<bool> PingAsync(CancellationToken cancellationToken)
        {
            var result = await ExecuteClamCommandAsync("PING", cancellationToken).ConfigureAwait(false);
            return result.ToLowerInvariant() == "pong";
        }

        /// <summary>
        /// Executes a PING command on the ClamAV server.
        /// </summary>
        /// <returns>If the server responds with PONG, returns true.  Otherwise returns false.</returns>
        public Task<bool> TryPingAsync()
        {
            return TryPingAsync(CancellationToken.None);
        }

        /// <summary>
        /// Executes a PING command on the ClamAV server.
        /// </summary>
        /// <returns>If the server responds with PONG, returns true.  Otherwise returns false.</returns>
        public async Task<bool> TryPingAsync(CancellationToken cancellationToken)
        {
            try
            {
                var result = await ExecuteClamCommandAsync("PING", cancellationToken).ConfigureAwait(false);
                return result.ToLowerInvariant() == "pong";
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Scans a file/directory on the ClamAV Server.
        /// </summary>
        /// <param name="filePath">Path to the file/directory on the ClamAV server.</param>
        public Task<ClamScanResult> ScanFileOnServerAsync(string filePath)
        {
            return ScanFileOnServerAsync(filePath, CancellationToken.None);
        }

        /// <summary>
        /// Scans a file/directory on the ClamAV Server.
        /// </summary>
        /// <param name="filePath">Path to the file/directory on the ClamAV server.</param>
        /// <param name="cancellationToken">cancellation token used for request</param>
        public async Task<ClamScanResult> ScanFileOnServerAsync(string filePath, CancellationToken cancellationToken)
        {
            return new ClamScanResult(await ExecuteClamCommandAsync(String.Format("SCAN {0}", filePath), cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Scans a file/directory on the ClamAV Server using multiple threads on the server.
        /// </summary>
        /// <param name="filePath">Path to the file/directory on the ClamAV server.</param>
        public Task<ClamScanResult> ScanFileOnServerMultithreadedAsync(string filePath)
        {
            return ScanFileOnServerMultithreadedAsync(filePath, CancellationToken.None);
        }

        /// <summary>
        /// Scans a file/directory on the ClamAV Server using multiple threads on the server.
        /// </summary>
        /// <param name="filePath">Path to the file/directory on the ClamAV server.</param>
        /// <param name="cancellationToken">cancellation token used for request</param>
        public async Task<ClamScanResult> ScanFileOnServerMultithreadedAsync(string filePath, CancellationToken cancellationToken)
        {
            return new ClamScanResult(await ExecuteClamCommandAsync(String.Format("MULTISCAN {0}", filePath), cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Sends the data to the ClamAV server as a stream.
        /// </summary>
        /// <param name="fileData">Byte array containing the data from a file.</param>
        /// <returns></returns>
        public Task<ClamScanResult> SendAndScanFileAsync(byte[] fileData)
        {
            return SendAndScanFileAsync(fileData, CancellationToken.None);
        }

        /// <summary>
        /// Sends the data to the ClamAV server as a stream.
        /// </summary>
        /// <param name="fileData">Byte array containing the data from a file.</param>
        /// <param name="cancellationToken">cancellation token used for request</param>
        /// <returns></returns>
        public async Task<ClamScanResult> SendAndScanFileAsync(byte[] fileData, CancellationToken cancellationToken)
        {
            var sourceStream = new MemoryStream(fileData);
            return new ClamScanResult(await ExecuteClamCommandAsync("INSTREAM", cancellationToken, (stream, token) => SendStreamFileChunksAsync(sourceStream, stream, token)).ConfigureAwait(false));
        }

        /// <summary>
        /// Sends the data to the ClamAV server as a stream.
        /// </summary>
        /// <param name="sourceStream">Stream containing the data to scan.</param>
        /// <returns></returns>
        public Task<ClamScanResult> SendAndScanFileAsync(Stream sourceStream)
        {
            return SendAndScanFileAsync(sourceStream, CancellationToken.None);
        }

        /// <summary>
        /// Sends the data to the ClamAV server as a stream.
        /// </summary>
        /// <param name="sourceStream">Stream containing the data to scan.</param>
        /// <param name="cancellationToken">cancellation token used for request</param>
        /// <returns></returns>
        public async Task<ClamScanResult> SendAndScanFileAsync(Stream sourceStream, CancellationToken cancellationToken)
        {
            return new ClamScanResult(await ExecuteClamCommandAsync("INSTREAM", cancellationToken, (stream, token) => SendStreamFileChunksAsync(sourceStream, stream, token)).ConfigureAwait(false));
        }

        /// <summary>
        /// Reads the file from the path and then sends it to the ClamAV server as a stream.
        /// </summary>
        /// <param name="filePath">Path to the file/directory.</param>
        public async Task<ClamScanResult> SendAndScanFileAsync(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return await SendAndScanFileAsync(stream).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Reads the file from the path and then sends it to the ClamAV server as a stream.
        /// </summary>
        /// <param name="filePath">Path to the file/directory.</param>
        /// <param name="cancellationToken">cancellation token used for request</param>
        public async Task<ClamScanResult> SendAndScanFileAsync(string filePath, CancellationToken cancellationToken)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return await SendAndScanFileAsync(stream, cancellationToken).ConfigureAwait(false);
            }
        }

		/// <summary>
		/// Shuts down the ClamAV server in an orderly fashion.
		/// </summary>
		public async Task Shutdown(CancellationToken cancellationToken)
		{
		    await ExecuteClamCommandAsync("SHUTDOWN", cancellationToken).ConfigureAwait(false);
		}

	}
}