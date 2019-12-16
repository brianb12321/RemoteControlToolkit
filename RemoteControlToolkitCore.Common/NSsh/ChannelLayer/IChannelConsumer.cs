using System;
using System.Security.Principal;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel;

namespace RemoteControlToolkitCore.Common.NSsh.ChannelLayer
{
    public interface IChannelConsumer : IDisposable
    {
        void Initialise();
        ChannelRequestType ChannelType { get; set; }

        IChannelProducer Channel { get; set; }

        IIdentity AuthenticatedIdentity { get; set; }
                
        string Password { get; set; }

        void Close();
    }
}
