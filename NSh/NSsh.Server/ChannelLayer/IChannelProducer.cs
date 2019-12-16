using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSsh.Common.Packets;

namespace NSsh.Server.ChannelLayer
{
    // TODO: this is not a very good name, rename to a better one
    public interface IChannelProducer
    {
        Packet GetIncomingPacket();

        void SendData(byte[] buffer);

        void SendErrorData(byte[] buffer);

        void Close();
    }
}
