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
        public int MaxChunkSize {get; set;} 

        public string Server { get; set; }
        public int Port { get; set; }

        public ClamClient(string server, int port)
        {
            MaxChunkSize = 2048;
            Server = server;
            Port = port;
        }

        private string ExecuteClamCommand(string command, Action<NetworkStream> additionalCommand = null)
        {
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            string result = null;

            TcpClient clam = new TcpClient();
            try
            {
                clam.Connect(Server, Port);

                using (NetworkStream stream = clam.GetStream())
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
            stopWatch.Stop();
            System.Diagnostics.Debug.WriteLine("Command {0} took: {1}", command, stopWatch.Elapsed);
            return result;
        }

        private void SendStreamFileChunks(byte[] fileBytes, Stream stream)
        {
            int cursor = 0;
            int size = MaxChunkSize;
            while (cursor < fileBytes.Length)
            {
                if (cursor + size >= fileBytes.Length)
                {
                    size = fileBytes.Length - cursor;
                }

                var sizeBytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(size));
                stream.Write(sizeBytes, 0, sizeBytes.Length);
                stream.Write(fileBytes, cursor, size);
                cursor += size;
            }
            
            var newMessage = BitConverter.GetBytes(0);
            stream.Write(newMessage, 0, newMessage.Length);
        }

        public string GetVersion()
        {
            return ExecuteClamCommand("VERSION");
        }

        public string GetPing()
        {
            return ExecuteClamCommand("PING");
        }

        public string ScanFileOnServer(string filePath)
        {
            return ExecuteClamCommand(String.Format("SCAN {0}", filePath));
        }

        public string ScanFileOnServerMultithreaded(string filePath)
        {
            return ExecuteClamCommand(String.Format("MULTISCAN {0}", filePath));
        }

        public string SendAndScanFile(byte[] fileData)
        {
            return ExecuteClamCommand("INSTREAM", stream => SendStreamFileChunks(fileData, stream));
        }

        public string SendAndScanFile(string filePath)
        {
            return SendAndScanFile(File.ReadAllBytes(filePath));
        }

        
    }
}
