using System;
using System.IO;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads;

namespace RemoteControlToolkitCore.Common.NSsh.ChannelLayer.Console
{
    public interface IConsole
    {
        void SignalWindowChange(WindowChangePayload args);
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
