using System.IO;
using System.Security.Cryptography;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    public class SshPacketContext
    {
        public SshPacketContext() { }

        public SshPacketContext(BinaryReader reader)
        {
            Stream = reader.BaseStream;
            Reader = reader;

            PacketLength = (int) reader.ReadUInt32BE();
            PaddingLength = reader.ReadByte();
            PacketType = (PacketType)reader.ReadByte();
        }

        public int PacketLength { get; set; }

        public byte PaddingLength { get; set; }

        public PacketType PacketType { get; set; }

        public ICryptoTransform ReceiveCipher { get; set; }

        public HashAlgorithm ReceiveMac { get; set; }

        public uint MacSequenceNumber { get; set; }
        
        public Stream Stream { get; set; }

        public BinaryReader Reader { get; set; }
    }
}
