using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Packets
{
    /// <summary>
    ///
    /// byte      SSH_MSG_SERVICE_REQUEST
    /// string    service name
    /// </summary>
    public class ServiceRequestPacket : Packet
    {
        public ServiceRequestPacket() : base(PacketType.ServiceRequest) { }

        public ServiceRequestPacket(SshPacketContext context) : base(context) { }

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

        public override string ToString()
        {
            return base.ToString() + " " + ServiceName;
        }
    }
}
