using System.IO;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    /// <summary>
    ///
    /// byte         SSH_MSG_USERAUTH_FAILURE
    /// name-list    authentications that can continue
    /// boolean      partial success
    /// </summary>
    public class UserAuthFailurePacket : Packet
    {
        public UserAuthFailurePacket() : base(PacketType.UserAuthFailure)
        {
            RemainingAuthMethods = new NameList();
        }

        public UserAuthFailurePacket(SshPacketContext context) : base(context) { }

        public NameList RemainingAuthMethods { get; set; }

        public bool PartialSuccess { get; set; }

        protected override void InitialisePayload(BinaryReader reader) 
        {
            RemainingAuthMethods = new NameList(reader);
            PartialSuccess = (reader.ReadByte() == 1);
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream(Length);
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(RemainingAuthMethods.ToByteArray());
            writer.Write((byte)(PartialSuccess ? 1 : 0));

            return buffer.ToArray();
        }
    }
}
