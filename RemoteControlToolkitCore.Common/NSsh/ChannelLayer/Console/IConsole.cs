using System;
using System.IO;

namespace RemoteControlToolkitCore.Common.NSsh.ChannelLayer.Console
{
    public interface IConsole
    {
        IChannelProducer Producer { get; }
        TextWriter StandardInput { get; }
        TextReader StandardOutput { get; }
        TextReader StandardError { get; }
        void Close();
        void Start();
        bool HasClosed { get; }
        event EventHandler Closed;
    }
}
