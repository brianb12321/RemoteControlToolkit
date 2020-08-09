using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets.UserAuth
{
    /// <summary>
    ///
    /// byte      SSH_MSG_USERAUTH_PK_OK
    /// string    public key algorithm name from the request
    /// string    public key blob from the request
    /// </summary>
    public class UserAuthPublicKeyOk : Packet
    {
        public UserAuthPublicKeyOk() : base(PacketType.UserAuthPublicKeyOk) { }

        public UserAuthPublicKeyOk(SshPacketContext context) : base(context) { }

        public PublicKeyAlgorithm Algorithm { get; set; }
        public byte[] PublicKeyBlob { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            string algorithm = new SshString(reader, Encoding.ASCII).Value;
            Algorithm = PublicKeyAlgorithmHelper.Parse(algorithm);
            PublicKeyBlob = new SshByteArray(reader).Value;
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream(Length);
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(new SshString(PublicKeyAlgorithmHelper.ToString(Algorithm), Encoding.ASCII).ToByteArray());
            writer.Write(new SshByteArray(PublicKeyBlob).ToByteArray());

            return buffer.ToArray();
        }
    }
}
