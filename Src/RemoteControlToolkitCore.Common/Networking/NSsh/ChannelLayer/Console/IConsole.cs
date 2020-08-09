using System;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Networking.NSsh.Packets.Channel.RequestPayloads;
using RemoteControlToolkitCore.Common.Networking.NSsh.Utility;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.ChannelLayer.Console
{
    public interface IConsole
    {
        ITerminalHandler TerminalHandler { get; }
        void SignalWindowChange(WindowChangePayload args);
        IChannelProducer Producer { get; } 
        BlockingMemoryStream Pipe { get; }
        void Close();
        void Start();
        void CancellationRequested();
        bool HasClosed { get; }
        event EventHandler Closed;
    }
}
