using System.IO;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    public class NewKeysPacket : Packet
    {
        public NewKeysPacket() : base(PacketType.NewKeys) { }

        public NewKeysPacket(SshPacketContext context) : base(context) { }

        public override byte[] GetPayloadData()
        {
            return new byte[0];
        }

        protected override void InitialisePayload(BinaryReader reader) { }
    }
}
