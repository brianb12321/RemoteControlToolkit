using System;
using System.Runtime.Serialization;

namespace RemoteControlToolkitCore.Common.Plugin
{
    [Serializable]
    public class InvalidPluginLibraryException : Exception
    {
        public InvalidPluginLibraryException()
        {
        }

        public InvalidPluginLibraryException(string message) : base(message)
        {
        }

        public InvalidPluginLibraryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidPluginLibraryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}