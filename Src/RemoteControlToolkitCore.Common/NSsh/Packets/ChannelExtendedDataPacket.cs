using System.IO;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Packets
{
    /// <summary>
    ///
    /// byte      SSH_MSG_CHANNEL_EXTENDED_DATA
    /// uint32    recipient channel
    /// uint32    data_type_code
    /// string    data
    /// </summary>
    public class ChannelExtendedDataPacket : Packet
    {
        public ChannelExtendedDataPacket() : base(PacketType.ChannelExtendedData) { }

        public ChannelExtendedDataPacket(SshPacketContext context) : base(context) { }

        public uint RecipientChannel { get; set; }

        public ExtendedDataType ExtendedDataType { get; set; }
        
        public byte[] Data { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            RecipientChannel = reader.ReadUInt32BE();
            ExtendedDataType = (ExtendedDataType)reader.ReadUInt32BE();
            Data = new SshByteArray(reader).Value;
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.WriteBE(RecipientChannel);
            writer.WriteBE((uint)ExtendedDataType);
            writer.Write(new SshByteArray(Data).ToByteArray());

            return buffer.ToArray();
        }
    }
}
