namespace nClam
{
    /// <summary>
    /// The results of an infected file.
    /// </summary>
    public class ClamScanInfectedFile
    {
        /// <summary>
        /// The file name scaned, as returned by the CalmAV server
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The name of the virus detected by the ClamAV server
        /// </summary>
        public string VirusName { get; set; }
    }
}