using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Packets
{
    /// <summary>
    /// 
    ///  boolean   FALSE
    ///  string    plaintext password in ISO-10646 UTF-8 encoding [RFC3629]
    /// </summary>
    public class UserAuthPasswordPayload : IByteData
    {        
        public UserAuthPasswordPayload() { }

        public UserAuthPasswordPayload(BinaryReader reader)
        {
            byte unused = reader.ReadByte();
            Password = new SshString(reader, Encoding.UTF8).Value;
        }

        public string Password { get; set; }

        #region IByteData Members

        public byte[] ToByteArray()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write((byte) 0);
            writer.Write(new SshString(Password, Encoding.UTF8).ToByteArray());

            return buffer.ToArray();
        }

        #endregion
    }
}
