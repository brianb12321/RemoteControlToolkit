using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Packets.UserAuth
{
    /// <summary>
    ///
    /// byte      SSH_MSG_USERAUTH_BANNER
    /// string    message in ISO-10646 UTF-8 encoding [RFC3629]
    /// string    language tag [RFC3066]
    /// </summary>
    public class UserAuthBanner : Packet
    {
        public UserAuthBanner() : base(PacketType.UserAuthBanner) { }

        public UserAuthBanner(SshPacketContext context) : base(context) { }

        public string Message { get; set; }
        public string LanguageTag { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            Message = new SshString(reader, Encoding.UTF8).Value;
            LanguageTag = new SshString(reader).Value;
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream(Length);
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(new SshString(Message, Encoding.UTF8).ToByteArray());
            writer.Write(new SshString(LanguageTag).ToByteArray());

            return buffer.ToArray();
        }
    }
}
