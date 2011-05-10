#nClam Description
nClam is a library which helps you scan files or directories using a ClamAV server.  It contains a simple API which encapsulates the communication with the ClamAV server as well as the parsing of its results.  The library is licensed under the Apache License 2.0.

##Dependencies
ClamAV Server, also known as clamd.  It is a free, open-source virus scanner.  Win32 ports can be obtained here: http://oss.netfarm.it/clamav/

##Directions
1. Add the library as a reference in your application.
2. Create a nClam.ClamClient object, passing it the hostname and port of the ClamAV server.
3. Scan!

#Code Example
```csharp
using System;
using System.Linq;
using nClam;

class Program
{
    static void Main(string[] args)
    {

        var clam = new ClamClient("localhost", 3310);
        var scanResult = clam.ScanFileOnServer("C:\\test.txt");  //any file you would like!

        switch(scanResult.Result)
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
```