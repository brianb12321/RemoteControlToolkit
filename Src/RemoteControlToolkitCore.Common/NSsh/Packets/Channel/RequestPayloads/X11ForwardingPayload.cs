using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads
{
    /// <summary>
    /// 
    /// boolean   single connection
    /// string    x11 authentication protocol
    /// string    x11 authentication cookie
    /// uint32    x11 screen number
    /// </summary>
    public class X11ForwardingPayload : IByteData
    {
        public X11ForwardingPayload() { }

        public X11ForwardingPayload(BinaryReader reader)
        {
            SingleConnection = (reader.ReadByte() == 1);
            AuthenticationProtocol = new SshString(reader, Encoding.ASCII).Value;
            AuthenticationCookie = new SshString(reader, Encoding.ASCII).Value;
            ScreenNumber = reader.ReadUInt32BE();
        }

        public bool SingleConnection { get; set; }
        public string AuthenticationProtocol { get; set; }
        public string AuthenticationCookie { get; set; }
        public uint ScreenNumber { get; set; }

        #region IByteData Members

        public byte[] ToByteArray()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write((byte)(SingleConnection ? 1 : 0));
            writer.Write(new SshString(AuthenticationProtocol, Encoding.ASCII).ToByteArray());
            writer.Write(new SshString(AuthenticationCookie, Encoding.ASCII).ToByteArray());
            writer.WriteBE(ScreenNumber);

            return buffer.ToArray();
        }

        #endregion
    }
}
