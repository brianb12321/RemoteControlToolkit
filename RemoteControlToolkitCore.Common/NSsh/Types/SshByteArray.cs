using System.Collections.Generic;
using System.IO;

namespace RemoteControlToolkitCore.Common.NSsh.Types
{
    public class SshByteArray
    {
        public SshByteArray(byte[] value)
        {
            Value = value;
        }

        public SshByteArray(byte value1, byte[] value2)
        {
            List<byte> buffer = new List<byte>();
            buffer.Add(value1);
            buffer.AddRange(value2);
            Value = buffer.ToArray();
        }

        public SshByteArray(byte[] value1, byte[] value2)
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(value1);
            buffer.AddRange(value2);
            Value = buffer.ToArray();
        }

        public SshByteArray(BinaryReader reader)
        {
            int length = (int) reader.ReadUInt32BE();
            Value = reader.ReadBytes(length);
        }

        public byte[] Value { get; set; }

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
                writer.WriteBE((uint)Value.Length);
                writer.Write(Value);
            }

            return memoryStream.ToArray();
        }
    }
}
