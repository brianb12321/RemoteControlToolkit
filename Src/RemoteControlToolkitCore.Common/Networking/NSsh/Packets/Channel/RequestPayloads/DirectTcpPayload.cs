using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets.Channel.RequestPayloads
{
    /// <summary>
    /// 
    /// string    host to connect
    /// uint32    port to connect
    /// string    originator IP address
    /// uint32    originator port
    /// </summary>
    public class DirectTcpPayload : IByteData
    {
        public DirectTcpPayload() { }

        public DirectTcpPayload(BinaryReader reader)
        {
            Host = new SshString(reader, Encoding.ASCII).Value;
            Port = reader.ReadUInt32BE();
            OriginIPAddress = new SshString(reader, Encoding.ASCII).Value;
            OriginPort = reader.ReadUInt32BE();
        }

        public string Host { get; set; }

        public uint Port { get; set; }

        public string OriginIPAddress{ get; set; }

        public uint OriginPort { get; set; }

        #region IByteData Members

        public byte[] ToByteArray()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(new SshString(Host, Encoding.ASCII).ToByteArray());
            writer.WriteBE(Port);
            writer.Write(new SshString(OriginIPAddress, Encoding.ASCII).ToByteArray());
            writer.WriteBE(OriginPort);

            return buffer.ToArray();
        }

        #endregion
    }
}
