using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace nClam
{
    public class ClamClient
    {

        /// <summary>
        /// Maximum size (in bytes) which streams will be broken up to when sending to the ClamAV server.  Used in the SendAndScanFile methods.  128kb is the default size.
        /// </summary>
        public int MaxChunkSize {get; set;}

        /// <summary>
        /// Maximum size (in bytes) that can be streamed to the ClamAV server before it will terminate the connection. Used in the SendAndScanFile methods. 25mb is the default size.
        /// </summary>
        public long MaxStreamSize { get; set; }

        /// <summary>
        /// Address to the ClamAV server
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Port which the ClamAV server is listening on
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// A class to connect to a ClamAV server and request virus scans
        /// </summary>
        /// <param name="server">Address to the ClamAV server</param>
        /// <param name="port">Port which the ClamAV server is listening on</param>
        public ClamClient(string server, int port)
        {
            MaxChunkSize = 131072; //128k
            MaxStreamSize = 26214400; //25mb
            Server = server;
            Port = port;
        }

        /// <summary>
        /// Helper method which connects to the ClamAV Server, performs the command and returns the result.
        /// </summary>
        /// <param name="command">The command to execute on the ClamAV Server</param>
        /// <param name="additionalCommand">Action to define additional server communications.  Executed after the command is sent and before the response is read.</param>
        /// <returns>The full response from the ClamAV server.</returns>
        private string ExecuteClamCommand(string command, Action<NetworkStream> additionalCommand = null)
        {
#if DEBUG
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
#endif
            string result;

            var clam = new TcpClient();
            try
            {
                clam.Connect(Server, Port);

                using (var stream = clam.GetStream())
                {
                    var commandText = String.Format("z{0}\0", command);
                    var commandBytes = Encoding.ASCII.GetBytes(commandText);
                    stream.Write(commandBytes, 0, commandBytes.Length);

                    if (additionalCommand != null)
                    {
                        additionalCommand.Invoke(stream);
                    }

                    using (var reader = new StreamReader(stream))
                    {
                        result = reader.ReadToEnd();

                        if(!String.IsNullOrEmpty(result))
                        {
                            //if we have a result, trim off the terminating null character
                            result = result.TrimEnd('\0');
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // catch all exceptions and return empty value
#if DEBUG
                System.Diagnostics.Debug.WriteLine(ex.ToString());
#endif
                result = null;
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
        private void SendStreamFileChunks(Stream sourceStream, Stream clamStream)
        {
            var size = MaxChunkSize;
            var bytes = new byte[size];

            while ((size = sourceStream.Read(bytes, 0, size)) > 0)
            {
                if (sourceStream.Position > MaxStreamSize)
                {
                    throw new MaxStreamSizeExceededException(MaxStreamSize);
                }

                var sizeBytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(size));  //convert size to NetworkOrder!
                clamStream.Write(sizeBytes, 0, sizeBytes.Length);
                clamStream.Write(bytes, 0, size);
            }
            
            var newMessage = BitConverter.GetBytes(0);
            clamStream.Write(newMessage, 0, newMessage.Length);
        }

        /// <summary>
        /// Gets the ClamAV server version
        /// </summary>
        public string GetVersion()
        {
            return ExecuteClamCommand("VERSION");
        }

        /// <summary>
        /// Executes a PING command on the ClamAV server.
        /// </summary>
        /// <returns>If the server responds with PONG, returns true.  Otherwise returns false.</returns>
        public bool Ping()
        {
            return ExecuteClamCommand("PING").ToLowerInvariant() == "pong";
        }

        /// <summary>
        /// Scans a file/directory on the ClamAV Server.
        /// </summary>
        /// <param name="filePath">Path to the file/directory on the ClamAV server.</param>
        public ClamScanResult ScanFileOnServer(string filePath)
        {
            return new ClamScanResult(ExecuteClamCommand(String.Format("SCAN {0}", filePath)));
        }

        /// <summary>
        /// Scans a file/directory on the ClamAV Server using multiple threads on the server.
        /// </summary>
        /// <param name="filePath">Path to the file/directory on the ClamAV server.</param>
        public ClamScanResult ScanFileOnServerMultithreaded(string filePath)
        {
            return new ClamScanResult(ExecuteClamCommand(String.Format("MULTISCAN {0}", filePath)));
        }

        /// <summary>
        /// Sends the data to the ClamAV server as a stream.
        /// </summary>
        /// <param name="fileData">Byte array containing the data from a file.</param>
        /// <returns></returns>
        public ClamScanResult SendAndScanFile(byte[] fileData)
        {
            var sourceStream = new MemoryStream(fileData);
            return new ClamScanResult(ExecuteClamCommand("INSTREAM", stream => SendStreamFileChunks(sourceStream, stream)));
        }

        /// <summary>
        /// Sends the data to the ClamAV server as a stream.
        /// </summary>
        /// <param name="sourceStream">Stream containing the data to scan.</param>
        /// <returns></returns>
        public ClamScanResult SendAndScanFile(Stream sourceStream)
        {
            return new ClamScanResult(ExecuteClamCommand("INSTREAM", stream => SendStreamFileChunks(sourceStream, stream)));
        }

        /// <summary>
        /// Reads the file from the path and then sends it to the ClamAV server as a stream.
        /// </summary>
        /// <param name="filePath">Path to the file/directory.</param>
        public ClamScanResult SendAndScanFile(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return SendAndScanFile(stream);
            }
        }
    }
}
