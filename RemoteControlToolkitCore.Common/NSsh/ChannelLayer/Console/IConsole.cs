using System;
using System.IO;
using System.IO.Pipes;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads;

namespace RemoteControlToolkitCore.Common.NSsh.ChannelLayer.Console
{
    public interface IConsole
    {
        void SignalWindowChange(WindowChangePayload args);
        IChannelProducer Producer { get; } 
        AnonymousPipeServerStream Pipe { get; }
        void Close();
        void Start();
        bool HasClosed { get; }
        event EventHandler Closed;
    }
}
