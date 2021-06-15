namespace nClam
{
    using System;

    /// <summary>
    /// Signifies that the maximum stream size for the INSTREAM command has been exceeded.
    /// </summary>
    [Serializable]
    public class MaxStreamSizeExceededException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaxStreamSizeExceededException"/> class.
        /// </summary>
        /// <param name="maxStreamSize">Maximum size of the stream.</param>
        public MaxStreamSizeExceededException(long maxStreamSize)
             : base($"The maximum stream size of {maxStreamSize} bytes has been exceeded.")
        {

        }
    }
}