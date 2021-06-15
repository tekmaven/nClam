namespace nClam
{
    /// <summary>
    /// The results of an infected file.
    /// </summary>
    public record ClamScanInfectedFile
    {
        public ClamScanInfectedFile(string fileName, string virusName)
        {
            FileName = fileName;
            VirusName = virusName;
        }

        /// <summary>
        /// The file name scaned, as returned by the CalmAV server
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The name of the virus detected by the ClamAV server
        /// </summary>
        public string VirusName { get; }
    }
}
