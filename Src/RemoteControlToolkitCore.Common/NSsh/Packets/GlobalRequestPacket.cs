using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Packets
{
    /// <summary>
    /// 
    ///  byte      SSH_MSG_GLOBAL_REQUEST
    ///  string    request name in US-ASCII only
    ///  boolean   want reply
    ///  ....      request-specific data follows
    ///  
    /// </summary>
    public class GlobalRequestPacket : Packet
    {
        public GlobalRequestPacket() : base(PacketType.GlobalRequest) { }

        public GlobalRequestPacket(SshPacketContext context) : base(context) { }

        public string RequestName { get; set; }

        public bool WantReply { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            int length = (int)reader.ReadUInt32BE();
            RequestName = Encoding.ASCII.GetString(reader.ReadBytes(length));
            WantReply = reader.ReadBoolean();
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream(Length);
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(new SshString(RequestName).ToByteArray());
            writer.Write((byte)(WantReply ? 1 : 0));

            return buffer.ToArray();
        }
    }
}
