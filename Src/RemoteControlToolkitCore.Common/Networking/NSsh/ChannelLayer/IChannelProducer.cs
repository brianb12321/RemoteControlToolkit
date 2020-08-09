using RemoteControlToolkitCore.Common.Networking.NSsh.Packets;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.ChannelLayer
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
