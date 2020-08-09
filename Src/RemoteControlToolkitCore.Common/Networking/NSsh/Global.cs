using System;
using System.IO;
using System.Security;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.Networking.NSsh
{
    public delegate void MethodInvoker();
    public delegate void MethodInvoker<T>(T argument);

    public static class Extensions
    {
        public static void Write(this Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

        public static uint ToUInt32BE(this UInt32 value)
        {
            uint litteEndian = value;
            uint bigEndian = 0;

            bigEndian |= (byte)litteEndian;

            for (int i = 0; i < 3; i++)
            {
                bigEndian <<= 8;
                litteEndian >>= 8;
                bigEndian |= (byte)litteEndian;
            }

            return bigEndian;
        }

        public static void WriteBE(this BinaryWriter writer, uint value)
        {
            writer.Write(value.ToUInt32BE());
        }

        public static void Write(this BinaryWriter writer, BigInteger value)
        {
            byte[] data = value.GetBytes();
            int length = data.Length;

            if (data[0] >= 0x80)
            {
                length++;
                writer.WriteBE((uint)length);
                writer.Write((byte)0);
            }
            else
            {
                writer.WriteBE((uint)length);
            }

            writer.Write(data);
        }

        public static uint ReadUInt32BE(this BinaryReader reader)
        {
            return reader.ReadUInt32().ToUInt32BE();
        }

        public static bool IsNullOrBlank(this string s) 
        {
            if (s == null || s.Trim().Length == 0) {
                return true;
            }

            return false;
        }

        public static void Append(this SecureString secureString, string value)
        {
            foreach (char c in value)
            {
                secureString.AppendChar(c);
            }
        }
    }
}
