using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace nClam.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args == null || args.Length != 1)
            {
                Console.WriteLine("Invalid arguments.  Usage: nClam.ConsoleTest [FileName]");
                return;
            }

            var fileName = args[0];
            var client = new ClamClient("localhost", 3310);
            Console.WriteLine("GetVersion(): {0}", client.GetVersion());
            Console.WriteLine("GetPing(): {0}", client.Ping());
            Console.WriteLine("ScanFileOnServer(): {0}", client.ScanFileOnServer(fileName));
            Console.WriteLine("ScanFileOnServerMultithreaded(): {0}", client.ScanFileOnServerMultithreaded(fileName));

            if (!IsFolder(fileName))
            {
                try
                {
                    Console.WriteLine("SendAndScanFile(string): {0}", client.SendAndScanFile(fileName));
                    Console.WriteLine("SendAndScanFile(byte[]): {0}", client.SendAndScanFile(File.ReadAllBytes(fileName)));
                }
                catch (MaxStreamSizeExceededException msee)
                {
                    Console.WriteLine(msee.Message);
                }
            }
            else
            {
                Console.WriteLine("SendAndScanFile(): Not run because argument is a folder, not a file.");
            }
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
    }
}
