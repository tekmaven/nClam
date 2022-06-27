using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace nClam.ConsoleTest
{
    static class Program
    {
        static async Task Main()
        {
            Console.WriteLine("nClam Test Application");
            Console.WriteLine();

            Console.Write("\t* Testing connectivity: ");

            var clam = new ClamClient("localhost", 3310);
            var pingResult = await clam.TryPingAsync();

            if (!pingResult)
            {
                Console.WriteLine("test failed. Exiting.");
                return;
            }

            Console.WriteLine("connected.");

            Console.Write("\t* Scanning file: ");
            var fileToScan = ConfigurationManager.AppSettings["fileToScan"]; //any file you would like!
            var scanResult = await clam.SendAndScanFileAsync(fileToScan);

            switch (scanResult.Result)
            {
                case ClamScanResults.Clean:
                    Console.WriteLine("The file is clean!");
                    break;
                case ClamScanResults.VirusDetected:
                    Console.WriteLine("Virus Found!");
                    Console.WriteLine("Virus name: {0}", scanResult.InfectedFiles.First().VirusName);
                    break;
                case ClamScanResults.Error:
                    Console.WriteLine("Woah an error occured! Error: {0}", scanResult.RawResult);
                    break;
            }

        }
    }
}