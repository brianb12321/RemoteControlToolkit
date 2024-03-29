﻿using System;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Types
{
    public class VerificationException : Exception
    {
        public VerificationException() : base() { }

        public VerificationException(string message) : base(message) { }

        public VerificationException(string message, Exception inner) : base(message, inner) { }
    }
}
