using System;
using System.IO;
using System.Net;
using Xunit;

namespace nClam.Tests
{
    public class ClamScanResultTests
    {
        [Fact]
        public void OK_Response()
        {
            var result = new ClamScanResult(@"C:\test.txt: OK");

            Assert.Equal(ClamScanResults.Clean, result.Result);
        }

        [Fact]
        public void Error_Response()
        {
            var result = new ClamScanResult("error");

            Assert.Equal(ClamScanResults.Error, result.Result);
        }

        [Fact]
        public void VirusDetected_Response()
        {
            var result = new ClamScanResult(@"\\?\C:\test.txt: Eicar-Test-Signature FOUND");

            Assert.Equal(ClamScanResults.VirusDetected, result.Result);

            Assert.Single(result.InfectedFiles);

            Assert.Equal(@"\\?\C:\test.txt", result.InfectedFiles[0].FileName);
            Assert.Equal(" Eicar-Test-Signature", result.InfectedFiles[0].VirusName);
        }

        [Fact]
        public void Non_Matching()
        {
            var result = new ClamScanResult(Guid.NewGuid().ToString());

            Assert.Equal(ClamScanResults.Unknown, result.Result);
        }

        [Fact]
        public void Before_Tests()
        {
            Assert.Equal(
                "test:test1",
                ClamScanResult.before("test:test1:test2")
                );

            Assert.Equal(
                "",
                ClamScanResult.before("test")
                );

            Assert.Equal(
                "test",
                ClamScanResult.before("test:test1")
                );
        }

        [Fact]
        public void After_Tests()
        {
            //current released behavior to have initial space
            //(probably a bug)

            Assert.Equal(
                " test1",
                ClamScanResult.after("test test1")
                );

            Assert.Equal(
                " test2",
                ClamScanResult.after("test test1 test2")
                );

            Assert.Equal(
                "",
                ClamScanResult.after("test")
                );
        }

		[Fact(Skip = "Requires ClamAV running on localhost:3310 ")]
		public void TestSendAsyncTest()
		{
			string Eicartestcase = @"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";
			var client = new ClamClient("localhost");
			var result = client.SendAndScanFileAsync(new MemoryStream(System.Text.Encoding.Default.GetBytes(Eicartestcase)));
			Assert.Equal(ClamScanResults.VirusDetected,result.Result.Result);
        }

        [Fact(Skip = "Requires ClamAV running on 127.0.0.1:3310 ")]
        public void TestSendIPAsyncTest()
        {
            string Eicartestcase = @"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";
            var client = new ClamClient(IPAddress.Parse("127.0.0.1"));
            var result = client.SendAndScanFileAsync(new MemoryStream(System.Text.Encoding.Default.GetBytes(Eicartestcase)));
            Assert.Equal(ClamScanResults.VirusDetected, result.Result.Result);
        }
    }
}
