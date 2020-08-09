using System.IO;
using Mono.Math;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;
using RemoteControlToolkitCore.Common.Networking.NSsh.Utility;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    /// <summary>
    /// 
    /// byte      SSH_MSG_KEXDH_INIT
    /// mpint     e
    /// 
    /// C generates a random number x (1 &lt; x &lt; q) and computes
    ///  e = g^x mod p.  C sends e to S.
    /// </summary>
    public class KexDHInitPacket : Packet
    {
        public KexDHInitPacket(ISecureRandom secureRandom) : base(PacketType.KexDHInit)
        {
            byte[] x = new byte[16];
            secureRandom.GetBytes(x);
            X = new BigInteger(x);

            E = new BigInteger(2).ModPow(X, SshConstants.DHPrime);
        }

        public BigInteger X { get; set; }

        public BigInteger E { get; set; }

        public KexDHInitPacket(SshPacketContext context) : base(context) { }

        protected override void InitialisePayload(BinaryReader reader)
        {
            uint length = reader.ReadUInt32BE();
            byte[] data = reader.ReadBytes((int)length);

            E = new BigInteger(data);
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream(Length);
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(E.GetBytes());

            return buffer.ToArray();
        }
    }
}
