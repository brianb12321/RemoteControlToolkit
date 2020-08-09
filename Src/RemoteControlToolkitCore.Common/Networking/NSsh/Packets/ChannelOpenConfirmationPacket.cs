using System.IO;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    /// <summary>
    ///
    /// byte      SSH_MSG_CHANNEL_OPEN_CONFIRMATION
    /// uint32    recipient channel
    /// uint32    sender channel
    /// uint32    initial window size
    /// uint32    maximum packet size
    /// ....      channel type specific data follows
    /// </summary>
    public class ChannelOpenConfirmationPacket : Packet
    {
        public ChannelOpenConfirmationPacket() : base(PacketType.ChannelOpenConfirmation) { }

        public ChannelOpenConfirmationPacket(SshPacketContext context) : base(context) { }

        public uint RecipientChannel { get; set; }

        public uint SenderChannel { get; set; }

        public uint InitialWindowSize { get; set; }

        public uint MaximumPacketSize { get; set; }

        public IByteData ChannelPayload { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            RecipientChannel = reader.ReadUInt32BE();
            SenderChannel = reader.ReadUInt32BE();
            InitialWindowSize = reader.ReadUInt32BE();
            MaximumPacketSize = reader.ReadUInt32BE();
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);
                        
            writer.WriteBE(RecipientChannel);
            writer.WriteBE(SenderChannel);
            writer.WriteBE(InitialWindowSize);
            writer.WriteBE(MaximumPacketSize);

            if (ChannelPayload != null)
            {
                writer.Write(ChannelPayload.ToByteArray());
            }

            return buffer.ToArray();
        }
    }
}
