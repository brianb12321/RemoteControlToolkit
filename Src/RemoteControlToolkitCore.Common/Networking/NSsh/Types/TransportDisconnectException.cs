using System;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Types
{
    public class TransportDisconnectException : Exception
    {
        public TransportDisconnectException() : base() { }

        public TransportDisconnectException(string message) : base(message) { }

        public TransportDisconnectException(string message, Exception inner) : base(message, inner) { }
    }
}