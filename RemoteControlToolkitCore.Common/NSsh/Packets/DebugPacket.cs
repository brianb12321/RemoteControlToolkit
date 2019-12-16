using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Packets
{
    /// <summary>
    /// 
    /// byte      SSH_MSG_DEBUG
    /// boolean   always_display
    /// string    message in ISO-10646 UTF-8 encoding [RFC3629]
    /// string    language tag [RFC3066]
    /// </summary>
    public class DebugPacket : Packet
    {
        public DebugPacket() : base(PacketType.Debug) { }

        public DebugPacket(SshPacketContext context) : base(context) { }

        public bool AlwaysDisplay { get; set; }

        public string Message { get; set; }

        public string LanguageTag { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            AlwaysDisplay = (reader.ReadByte() == 1);
            Message = new SshString(reader, Encoding.UTF8).Value;
            LanguageTag = new SshString(reader).Value;
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream(Length);
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write((byte)((AlwaysDisplay) ? 1 : 0));
            writer.Write(new SshString(Message, Encoding.UTF8).ToByteArray());
            writer.Write(new SshString(LanguageTag).ToByteArray());

            return buffer.ToArray();
        }
    }
}
