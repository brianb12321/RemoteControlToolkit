using System.IO;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    /// <summary>
    /// 
    ///  byte      SSH_MSG_IGNORE
    ///  string    data
    /// </summary>
    public class IgnorePacket : Packet
    {
        public IgnorePacket() : base(PacketType.Ignore) { }

        public IgnorePacket(SshPacketContext context) : base(context) { }

        public byte[] Data { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            Data = new SshByteArray(reader).Value;
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream(Length);
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(new SshByteArray(Data).ToByteArray());

            return buffer.ToArray();
        }
    }
}
