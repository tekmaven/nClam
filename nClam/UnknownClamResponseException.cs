using System;

namespace nClam
{
    public class UnknownClamResponseException : Exception
    {
        public UnknownClamResponseException(string response) : base($"Unable to parse the server response: {response}")
        {
        }
    }
}