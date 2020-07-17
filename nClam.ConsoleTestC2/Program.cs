﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using nClam;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("nClam Test Application");
        Console.WriteLine();

        Console.Write("\t• Testing connectivity: ");

        var clam = new ClamClient("localhost", 3310);
        // or var clam = new ClamClient(IPAddress.Parse("127.0.0.1"), 3310);   
        var pingResult = await clam.PingAsync();

        if(!pingResult) {
            Console.WriteLine("test failed. Exiting.");
            return;
        }

        Console.WriteLine("connected.");
        
        Console.Write("\t• Scanning file: ");
        var scanResult = await clam.ScanFileOnServerAsync("C:\\test.txt");  //any file you would like!

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