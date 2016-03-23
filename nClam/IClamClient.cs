using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace nClam
{
    public interface IClamClient
    {
        /// <summary>
        /// Maximum size (in bytes) which streams will be broken up to when sending to the ClamAV server.  Used in the SendAndScanFile methods.  128kb is the default size.
        /// </summary>
        int MaxChunkSize { get; set; }

        /// <summary>
        /// Maximum size (in bytes) that can be streamed to the ClamAV server before it will terminate the connection. Used in the SendAndScanFile methods. 25mb is the default size.
        /// </summary>
        long MaxStreamSize { get; set; }

        /// <summary>
        /// Address to the ClamAV server
        /// </summary>
        string Server { get; set; }

        /// <summary>
        /// Port which the ClamAV server is listening on
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// Gets the ClamAV server version
        /// </summary>
        Task<string> GetVersionAsync();

        /// <summary>
        /// Gets the ClamAV server version
        /// </summary>
        Task<string> GetVersionAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Executes a PING command on the ClamAV server.
        /// </summary>
        /// <returns>If the server responds with PONG, returns true.  Otherwise returns false.</returns>
        Task<bool> PingAsync();

        /// <summary>
        /// Executes a PING command on the ClamAV server.
        /// </summary>
        /// <returns>If the server responds with PONG, returns true.  Otherwise returns false.</returns>
        Task<bool> PingAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Scans a file/directory on the ClamAV Server.
        /// </summary>
        /// <param name="filePath">Path to the file/directory on the ClamAV server.</param>
        Task<ClamScanResult> ScanFileOnServerAsync(string filePath);

        /// <summary>
        /// Scans a file/directory on the ClamAV Server.
        /// </summary>
        /// <param name="filePath">Path to the file/directory on the ClamAV server.</param>
        /// <param name="cancellationToken">cancellation token used for request</param>
        Task<ClamScanResult> ScanFileOnServerAsync(string filePath, CancellationToken cancellationToken);

        /// <summary>
        /// Scans a file/directory on the ClamAV Server using multiple threads on the server.
        /// </summary>
        /// <param name="filePath">Path to the file/directory on the ClamAV server.</param>
        Task<ClamScanResult> ScanFileOnServerMultithreadedAsync(string filePath);

        /// <summary>
        /// Scans a file/directory on the ClamAV Server using multiple threads on the server.
        /// </summary>
        /// <param name="filePath">Path to the file/directory on the ClamAV server.</param>
        /// <param name="cancellationToken">cancellation token used for request</param>
        Task<ClamScanResult> ScanFileOnServerMultithreadedAsync(string filePath, CancellationToken cancellationToken);

        /// <summary>
        /// Sends the data to the ClamAV server as a stream.
        /// </summary>
        /// <param name="fileData">Byte array containing the data from a file.</param>
        /// <returns></returns>
        Task<ClamScanResult> SendAndScanFileAsync(byte[] fileData);

        /// <summary>
        /// Sends the data to the ClamAV server as a stream.
        /// </summary>
        /// <param name="fileData">Byte array containing the data from a file.</param>
        /// <param name="cancellationToken">cancellation token used for request</param>
        /// <returns></returns>
        Task<ClamScanResult> SendAndScanFileAsync(byte[] fileData, CancellationToken cancellationToken);

        /// <summary>
        /// Sends the data to the ClamAV server as a stream.
        /// </summary>
        /// <param name="sourceStream">Stream containing the data to scan.</param>
        /// <returns></returns>
        Task<ClamScanResult> SendAndScanFileAsync(Stream sourceStream);

        /// <summary>
        /// Sends the data to the ClamAV server as a stream.
        /// </summary>
        /// <param name="sourceStream">Stream containing the data to scan.</param>
        /// <param name="cancellationToken">cancellation token used for request</param>
        /// <returns></returns>
        Task<ClamScanResult> SendAndScanFileAsync(Stream sourceStream, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the file from the path and then sends it to the ClamAV server as a stream.
        /// </summary>
        /// <param name="filePath">Path to the file/directory.</param>
        Task<ClamScanResult> SendAndScanFileAsync(string filePath);

        /// <summary>
        /// Reads the file from the path and then sends it to the ClamAV server as a stream.
        /// </summary>
        /// <param name="filePath">Path to the file/directory.</param>
        /// <param name="cancellationToken">cancellation token used for request</param>
        Task<ClamScanResult> SendAndScanFileAsync(string filePath, CancellationToken cancellationToken);
    }
}