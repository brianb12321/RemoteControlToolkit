using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.IO;
using NSsh.Common.Packets;
using NSsh.Common.Packets.Channel;

namespace NSsh.Server.ChannelLayer
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
