using System;
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

            Assert.Equal(1, result.InfectedFiles.Count);

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
        public void before_tests()
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
        public void after_tests()
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
    }
}
