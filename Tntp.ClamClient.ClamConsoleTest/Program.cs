using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nClam.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ClamClient("localhost", 3310);
            Console.WriteLine("GetVersion(): {0}", client.GetVersion());
            Console.WriteLine("GetPing(): {0}", client.GetPing());
            Console.WriteLine("ScanFileOnServer(): {0}", client.ScanFileOnServer("C:\\clamav\\test.txt"));
            Console.WriteLine("ScanFileOnServerMultithreaded(): {0}", client.ScanFileOnServerMultithreaded("C:\\clamav\\test.txt"));
            Console.WriteLine("SendAndScanFile(): {0}", client.SendAndScanFile("C:\\clamav\\test.txt"));
            Console.WriteLine("Finished, Press <enter> to quit.");
            Console.ReadLine();
        }
    }
}
