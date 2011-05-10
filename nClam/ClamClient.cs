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
        /// <param name="fileBytes">The bytes of the stream to send to the ClamAV server.</param>
        /// <param name="stream">The communication channel to the ClamAV server.</param>
        private void SendStreamFileChunks(byte[] fileBytes, Stream stream)
        {
            var cursor = 0;
            var size = MaxChunkSize;
            while (cursor < fileBytes.Length)
            {
                if (cursor + size >= fileBytes.Length)
                {
                    size = fileBytes.Length - cursor;
                }

                var sizeBytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(size));  //convert size to NetworkOrder!
                stream.Write(sizeBytes, 0, sizeBytes.Length);
                stream.Write(fileBytes, cursor, size);
                cursor += size;
            }
            
            var newMessage = BitConverter.GetBytes(0);
            stream.Write(newMessage, 0, newMessage.Length);
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
            return new ClamScanResult(ExecuteClamCommand("INSTREAM", stream => SendStreamFileChunks(fileData, stream)));
        }

        /// <summary>
        /// Reads the file from the path and then sends it to the ClamAV server as a stream.
        /// </summary>
        /// <param name="filePath">Path to the file/directory.</param>
        public ClamScanResult SendAndScanFile(string filePath)
        {
            return SendAndScanFile(File.ReadAllBytes(filePath));
        }
    }
}
