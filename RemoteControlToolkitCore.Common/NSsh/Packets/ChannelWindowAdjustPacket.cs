using System.IO;

namespace RemoteControlToolkitCore.Common.NSsh.Packets
{
    /// <summary>
    ///
    /// byte      SSH_MSG_CHANNEL_WINDOW_ADJUST
    /// uint32    recipient channel
    /// uint32    bytes to add
    /// </summary>
    public class ChannelWindowAdjustPacket : Packet
    {
        public ChannelWindowAdjustPacket() : base(PacketType.ChannelWindowAdjust) { }

        public ChannelWindowAdjustPacket(SshPacketContext context) : base(context) { }

        public uint RecipientChannel { get; set; }

        public uint BytesToAdd { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            RecipientChannel = reader.ReadUInt32BE();
            BytesToAdd = reader.ReadUInt32BE();
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.WriteBE(RecipientChannel);
            writer.WriteBE(BytesToAdd);

            return buffer.ToArray();
        }
    }
}
