using System;
using System.IO;
using System.IO.Pipes;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads;
using RemoteControlToolkitCore.Common.NSsh.Utility;

namespace RemoteControlToolkitCore.Common.NSsh.ChannelLayer.Console
{
    public interface IConsole
    {
        void SignalWindowChange(WindowChangePayload args);
        IChannelProducer Producer { get; } 
        BlockingMemoryStream Pipe { get; }
        void Close();
        void Start();
        bool HasClosed { get; }
        event EventHandler Closed;
    }
}
