using System.IO;
using System.Text;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Types
{
    public class SshString : IByteData
    {
        public SshString(string value) : this(value, Encoding.ASCII) { }

        public SshString(string value, Encoding encoding)
        {
            Value = value;
            Encoding = encoding;
        }

        public SshString(BinaryReader reader) : this(reader, Encoding.ASCII) { }

        public SshString(BinaryReader reader, Encoding encoding)
        {
            int length = (int) reader.ReadUInt32BE();
            byte[] data = reader.ReadBytes(length);
            
            Encoding = encoding;
            Value = Encoding.GetString(data);
        }

        public string Value { get; set; }

        public Encoding Encoding { get; set; }

        public byte[] ToByteArray()
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memoryStream);

            if (Value == null)
            {
                writer.Write((uint)0);
            }
            else
            {
                byte[] data = Encoding.GetBytes(Value);
                writer.WriteBE((uint) data.Length);
                writer.Write(data);
            }

            return memoryStream.ToArray();
        }
    }
}
