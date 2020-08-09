using System.IO;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    /// <summary>
    /// 
    ///  byte      SSH_MSG_UNIMPLEMENTED
    ///  uint32    packet sequence number of rejected message
    /// </summary>
    public class UnimplementedPacket : Packet
    {
        public UnimplementedPacket() : base(PacketType.Unimplemented) { }

        public UnimplementedPacket(SshPacketContext context) : base(context) { }

        public uint SequenceNumber { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            SequenceNumber = reader.ReadUInt32BE();
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream(Length);
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(SequenceNumber);

            return buffer.ToArray();
        }
    }
}
