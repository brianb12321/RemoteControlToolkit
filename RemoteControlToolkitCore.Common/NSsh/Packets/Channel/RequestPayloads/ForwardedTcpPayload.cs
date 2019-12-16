using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads
{
    /// <summary>
    /// 
    /// string    address that was connected
    /// uint32    port that was connected
    /// string    originator IP address
    /// uint32    originator port
    /// </summary>
    public class ForwardedTcpPayload : IByteData
    {
        public ForwardedTcpPayload() { }

        public ForwardedTcpPayload(BinaryReader reader)
        {
            Address = new SshString(reader, Encoding.ASCII).Value;
            Port = reader.ReadUInt32BE();
            OriginIPAddress = new SshString(reader, Encoding.ASCII).Value;
            OriginPort = reader.ReadUInt32BE();
        }

        public string Address { get; set; }

        public uint Port { get; set; }

        public string OriginIPAddress{ get; set; }

        public uint OriginPort { get; set; }

        #region IByteData Members

        public byte[] ToByteArray()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(new SshString(Address, Encoding.ASCII).ToByteArray());
            writer.WriteBE(Port);
            writer.Write(new SshString(OriginIPAddress, Encoding.ASCII).ToByteArray());
            writer.WriteBE(OriginPort);

            return buffer.ToArray();
        }

        #endregion
    }
}
