using System.IO;

namespace RemoteControlToolkitCore.Common.NSsh.Packets
{
    /// <summary>
    ///       
    /// byte      SSH_MSG_USERAUTH_SUCCESS
    /// </summary>
    public class UserAuthSuccessPacket : Packet
    {
        public UserAuthSuccessPacket() : base(PacketType.UserAuthSuccess) { }

        public UserAuthSuccessPacket(SshPacketContext context) : base(context) { }
        
        protected override void InitialisePayload(BinaryReader reader) { }

        public override byte[] GetPayloadData()
        {
            return new byte[0];
        }
    }
}
