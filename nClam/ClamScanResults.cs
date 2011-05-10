namespace nClam
{
    public enum ClamScanResults
    {
        /// <summary>
        /// Indicates the value is not set.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Indicates the scan was successful and no viruses were found.
        /// </summary>
        Clean,

        /// <summary>
        /// Indicates the scan was successful and one or more viruses were found.
        /// </summary>
        VirusDetected,

        /// <summary>
        /// Indicates the scan was unsuccessful.
        /// </summary>
        Error
    }
}