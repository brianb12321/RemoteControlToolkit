using System.IO;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Packets
{
    /// <summary>
    ///
    /// byte      SSH_MSG_CHANNEL_DATA
    /// uint32    recipient channel
    /// string    data
    /// </summary>
    public class ChannelDataPacket : Packet
    {
        public ChannelDataPacket() : base(PacketType.ChannelData) { }

        public ChannelDataPacket(SshPacketContext context) : base(context) { }

        public uint RecipientChannel { get; set; }

        public byte[] Data { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            RecipientChannel = reader.ReadUInt32BE();
            Data = new SshByteArray(reader).Value;
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.WriteBE(RecipientChannel);
            writer.Write(new SshByteArray(Data).ToByteArray());

            return buffer.ToArray();
        }
    }
}
