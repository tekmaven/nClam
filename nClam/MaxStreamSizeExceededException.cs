using nClam.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace nClam
{
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
            : base(String.Format(Resources.MaxStreamSizeExceededFormat, maxStreamSize))
        {
            
        }

        protected MaxStreamSizeExceededException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
