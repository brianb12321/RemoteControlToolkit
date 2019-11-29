using System;

namespace RemoteControlToolkitCore.Common.Commandline
{
    public class CommandElementException : Exception
    {
        public CommandElementException(Exception baseException) : base(baseException.Message, baseException)
        {
        }
    }
}