using System;
using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.Networking.NSsh.Packets.Channel.RequestPayloads;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    /// <summary>
    ///
    /// byte      SSH_MSG_CHANNEL_OPEN
    /// string    channel type in US-ASCII only
    /// uint32    sender channel
    /// uint32    initial window size
    /// uint32    maximum packet size
    /// ....      channel type specific data follows
    /// </summary>
    public class ChannelOpenPacket : Packet
    {
        public ChannelOpenPacket() : base(PacketType.ChannelOpen) { }

        public ChannelOpenPacket(SshPacketContext context) : base(context) { }

        public ChannelType ChannelType { get; set; }

        public uint SenderChannel { get; set; }

        public uint InitialWindowSize { get; set; }

        public uint MaximumWindowSize { get; set; }

        public IByteData ChannelPayload { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            string channelType = new SshString(reader, Encoding.ASCII).Value;
            SenderChannel = reader.ReadUInt32BE();
            InitialWindowSize = reader.ReadUInt32BE();
            MaximumWindowSize = reader.ReadUInt32BE();

            ChannelType = ChannelTypeHelper.Parse(channelType);

            switch (ChannelType)
            {
                case ChannelType.Session:
                    // No extra data
                    break;

                case ChannelType.DirectTcp:
                    ChannelPayload = new DirectTcpPayload(reader);
                    break;

                case ChannelType.ForwardedTcp:
                    ChannelPayload = new ForwardedTcpPayload(reader);
                    break;

                default:
                    throw new NotSupportedException("Unknown channel type.");
            }
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(new SshString(ChannelTypeHelper.ToString(ChannelType), Encoding.ASCII).ToByteArray());
            writer.WriteBE(SenderChannel);
            writer.WriteBE(InitialWindowSize);
            writer.WriteBE(MaximumWindowSize);

            if (ChannelPayload != null)
            {
                writer.Write(ChannelPayload.ToByteArray());
            }

            return buffer.ToArray();
        }
    }
}
