using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nClam.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length != 1)
            {
                Console.WriteLine("Invalid arguments.  Usage: nClam.ConsoleTest [FileName]");
                return;
            }

            var fileInfo = new FileInfo(args[0]);
            const string ClamServer = "localhost";

            SendSyncExample(fileInfo, ClamServer);
            Console.WriteLine();

            Task.WaitAll(SendAsyncExample(fileInfo, ClamServer).ToArray());
            Console.WriteLine();

            Console.WriteLine("Finished, Press <enter> to quit.");
            Console.ReadLine();
        }

        /// <summary>
        /// Returns true if the given file path is a folder.
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>True if a folder</returns>
        public static bool IsFolder(string path)
        {
            return ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory);
        }

        static void SendSyncExample(FileInfo fileInfo, string clamServer)
        {
            Console.WriteLine("############## Sending Examples Synchronously ##############");
            var client = new ClamClient(clamServer);

            Console.WriteLine("GetVersion(): {0}", client.GetVersion());
            Console.WriteLine("GetPing(): {0}", client.Ping());

            if (!fileInfo.Exists)
            {
                Console.WriteLine("{0} could not be found.  Exiting.", fileInfo.FullName);
                return;
            }

            Console.WriteLine("ScanFileOnServer(): {0}", client.ScanFileOnServer(fileInfo.FullName));
            Console.WriteLine("ScanFileOnServerMultithreaded(): {0}", client.ScanFileOnServerMultithreaded(fileInfo.FullName));

            if (!IsFolder(fileInfo.FullName))
            {
                Console.WriteLine("SendAndScanFile(string): {0}", client.SendAndScanFile(fileInfo.FullName));
                Console.WriteLine("SendAndScanFile(byte[]): {0}", client.SendAndScanFile(File.ReadAllBytes(fileInfo.FullName)));
            }
            else
            {
                Console.WriteLine("SendAndScanFile(): Not run because argument is a folder, not a file.");
            }
        }

        static IEnumerable<Task> SendAsyncExample(FileInfo fileInfo, string clamServer)
        {
            Console.WriteLine("############## Sending Examples Asynchronously ##############");
            var client = new ClamClient(clamServer);

            Console.WriteLine("Begin GetVersionAsync()");
            yield return client.GetVersionAsync().ContinueWith(x => Console.WriteLine("Complete GetVersionAsync(): {0}", x.Result));

            Console.WriteLine("Begin PingAsync()");
            yield return client.PingAsync().ContinueWith(x => Console.WriteLine("Complete PingAsync(): {0}", x.Result));

            if (!fileInfo.Exists)
            {
                Console.WriteLine("{0} could not be found.  Exiting.", fileInfo.FullName);
                yield return Task.FromResult(0);
            }
            else
            {
                Console.WriteLine("Begin ScanFileOnServerAsync()");
                yield return client.ScanFileOnServerAsync(fileInfo.FullName).ContinueWith(x => Console.WriteLine("Complete ScanFileOnServerAsync(): {0}", x.Result));

                Console.WriteLine("Begin ScanFileOnServerMultithreadedAsync()");
                yield return client.ScanFileOnServerMultithreadedAsync(fileInfo.FullName).ContinueWith(x => Console.WriteLine("Complete ScanFileOnServerMultithreadedAsync(): {0}", x.Result));

                if (!IsFolder(fileInfo.FullName))
                {
                    Console.WriteLine("Begin SendAndScanFileAsync(string):");
                    yield return client.SendAndScanFileAsync(fileInfo.FullName).ContinueWith(x => Console.WriteLine("Complete SendAndScanFileAsync(string): {0}", x.Result));

                    Console.WriteLine("Begin SendAndScanFileAsync(byte[])");
                    yield return client.SendAndScanFileAsync(File.ReadAllBytes(fileInfo.FullName)).ContinueWith(x => Console.WriteLine("Complete SendAndScanFileAsync(byte[]): {0}", x.Result));
                }
                else
                {
                    Console.WriteLine("Begin SendAndScanFile(): Not run because argument is a folder, not a file.");
                }
            }
        }
    }
}