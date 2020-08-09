using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    /// <summary>
    ///
    /// byte      SSH_MSG_SERVICE_ACCEPT
    /// string    service name
    /// </summary>
    public class ServiceAcceptPacket : Packet
    {
        public ServiceAcceptPacket() : base(PacketType.ServiceAccept) { }

        public ServiceAcceptPacket(SshPacketContext context) : base(context) { }

        public string ServiceName { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            ServiceName = new SshString(reader, Encoding.ASCII).Value;
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream(Length);
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(new SshString(ServiceName, Encoding.ASCII).ToByteArray());

            return buffer.ToArray();
        }
    }
}
