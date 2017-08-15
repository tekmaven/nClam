using System;

namespace nClam
{
    /// <summary>
    /// Signifies that the maximum stream size for the INSTREAM command has been exceeded.
    /// </summary>

#if NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6
#else
    [Serializable]
#endif
    public class MaxStreamSizeExceededException : Exception
    {
       /// <summary>
       /// Initializes a new instance of the <see cref="MaxStreamSizeExceededException"/> class.
       /// </summary>
       /// <param name="maxStreamSize">Maximum size of the stream.</param>
       public MaxStreamSizeExceededException(long maxStreamSize)
            : base(String.Format("The maximum stream size of {0} bytes has been exceeded.", maxStreamSize))
       {

       }
    }
}