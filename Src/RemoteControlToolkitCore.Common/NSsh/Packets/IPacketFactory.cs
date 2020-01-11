using System.IO;
using System.Security.Cryptography;

namespace RemoteControlToolkitCore.Common.NSsh.Packets
{
    public interface IPacketFactory
    {
        Packet ReadFrom(Stream stream, ICryptoTransform receiveCipher, HashAlgorithm receiveMac, uint sequenceNumber);
    }
}
