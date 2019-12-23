using System;
using System.Collections.Generic;
using System.Security.Principal;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer.Console;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads;

namespace RemoteControlToolkitCore.Common.NSsh.ChannelLayer
{
    public interface IChannelConsumer : IDisposable
    {
        PseudoTerminalPayload InitialTerminalConfiguration { get; set; }
        List<EnvironmentPayload> InitialEnvironmentVariables { get; }
        void Initialise();
        void SignalWindowChange(WindowChangePayload args);
        ChannelRequestType ChannelType { get; set; }

        IChannelProducer Channel { get; set; }

        IIdentity AuthenticatedIdentity { get; set; }

        void Close();
    }
}
