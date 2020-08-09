using System;
using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    /// <summary>
    /// 
    /// byte      SSH_MSG_USERAUTH_REQUEST
    /// string    user name in ISO-10646 UTF-8 encoding [RFC3629]
    /// string    service name in US-ASCII
    /// string    method name in US-ASCII
    /// ....      method specific fields
    /// </summary>
    public class UserAuthRequestPacket : Packet
    {
        public UserAuthRequestPacket() : base(PacketType.UserAuthRequest) { }

        public UserAuthRequestPacket(SshPacketContext context) : base(context) { }

        public string UserName { get; set; }

        public string ServiceName { get; set; }

        public AuthenticationMethod AuthMethod { get; set; }

        public IByteData AuthPayload { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            UserName = new SshString(reader, Encoding.UTF8).Value;
            ServiceName = new SshString(reader, Encoding.ASCII).Value;
            string authMethod = new SshString(reader, Encoding.ASCII).Value;

            AuthMethod = (AuthenticationMethod) Enum.Parse(typeof(AuthenticationMethod), authMethod, true);

            switch (AuthMethod)
            {
                case AuthenticationMethod.Password:
                    AuthPayload = new UserAuthPasswordPayload(reader);
                    break;

                case AuthenticationMethod.PublicKey:
                    AuthPayload = new UserAuthPublicKeyPayload(reader);
                    break;

                case AuthenticationMethod.None:
                    // No extra payload
                    break;

                default:
                    throw new NotSupportedException("Auth method not supported.");
            }
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream(Length);
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(new SshString(UserName, Encoding.UTF8).ToByteArray());
            writer.Write(new SshString(ServiceName, Encoding.ASCII).ToByteArray());
            writer.Write(new SshString(AuthMethod.ToString().ToLower(), Encoding.ASCII).ToByteArray());

            if (AuthPayload != null)
            {
                writer.Write(AuthPayload.ToByteArray());
            }

            return buffer.ToArray();
        }

        public override string ToString()
        {
            return base.ToString() + " "  + ServiceName + " " + AuthMethod;
        }
    }
}
