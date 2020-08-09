using System.IO;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    /// <summary>
    ///       
    /// byte      SSH_MSG_REQUEST_FAILURE
    /// </summary>
    public class GlobalRequestFailurePacket : Packet
    {
        public GlobalRequestFailurePacket() : base(PacketType.GlobalRequestFailure) { }

        public GlobalRequestFailurePacket(SshPacketContext context) : base(context) { }
        
        protected override void InitialisePayload(BinaryReader reader) { }

        public override byte[] GetPayloadData()
        {
            return new byte[0];
        }
    }
}
