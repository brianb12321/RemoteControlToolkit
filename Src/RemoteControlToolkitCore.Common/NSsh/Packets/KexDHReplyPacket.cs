using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using RemoteControlToolkitCore.Common.NSsh.Types;
using RemoteControlToolkitCore.Common.NSsh.Utility;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.NSsh.Packets
{
    /// <summary>
    /// 
    /// byte      SSH_MSG_KEXDH_REPLY
    /// string    server public host key and certificates (K_S)
    /// mpint     f
    /// string    signature of H
    /// 
    /// S generates a random number y (0 &lt; y &lt; q) and computes
    /// f = g^y mod p.  S receives e.  It computes K = e^y mod p,
    /// H = hash(V_C || V_S || I_C || I_S || K_S || e || f || K)
    /// (these elements are encoded according to their types; see below),
    /// and signature s on H with its private host key.  S sends
    /// (K_S || f || s) to C.  The signing operation may involve a
    /// second hashing operation.
    /// </summary>
    public class KexDHReplyPacket : Packet
    {
        public KexDHReplyPacket(BigInteger e, string clientVersion, string serverVersion, 
            KexInitPacket clientKexInit, KexInitPacket serverKexInit, HashAlgorithm hashAlgorithm,
            IPublicKeyPair hostKey, PublicKeyAlgorithm signingAlgorithm, ISecureRandom secureRandom) : base(PacketType.KexDHReply)
        {
            HostKeyAndCertificates = hostKey;
            SigningAlgorithm = signingAlgorithm;

            byte[] y = new byte[16];
            secureRandom.GetBytes(y);
            Y = new BigInteger(y);

            F = new BigInteger(2).ModPow(Y, SshConstants.DHPrime);

            K = new BigInteger(e).ModPow(Y, SshConstants.DHPrime);

            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write(new SshString(clientVersion).ToByteArray());
            writer.Write(new SshString(serverVersion).ToByteArray());
            writer.Write(new SshByteArray((byte)PacketType.KexInit, clientKexInit.GetPayloadData()).ToByteArray());
            writer.Write(new SshByteArray((byte)PacketType.KexInit, serverKexInit.GetPayloadData()).ToByteArray());
            writer.Write(hostKey.ToByteArray());
            writer.Write(e);
            writer.Write(F);
            writer.Write(K);
            H = hashAlgorithm.ComputeHash(buffer.ToArray());

            switch (signingAlgorithm)
            {
                case PublicKeyAlgorithm.DSA:
                    SignatureOfH = hostKey.Sign(H);
                    break;

                case PublicKeyAlgorithm.RSA:
                    throw new NotSupportedException();
            }
        }

        public byte[] H { get; set; }

        public KexDHReplyPacket(SshPacketContext context) : base(context) { }

        public BigInteger F { get; set; }

        public byte[] SignatureOfH { get; set; }

        public BigInteger K { get; set; }

        public BigInteger Y { get; set; }

        PublicKeyAlgorithm SigningAlgorithm { get; set; }

        public IPublicKeyPair HostKeyAndCertificates { get; set; }

        protected override void InitialisePayload(BinaryReader reader)
        {
            int length = (int)reader.ReadUInt32BE();
            int publicKeyLength = (int)reader.ReadUInt32BE();
            string publicKeyAlgorithmStr = Encoding.ASCII.GetString(reader.ReadBytes(publicKeyLength));
            PublicKeyAlgorithm publicKeyAlgorithm = PublicKeyAlgorithmHelper.Parse(publicKeyAlgorithmStr);

            if (publicKeyAlgorithm == PublicKeyAlgorithm.DSA) {
                DSAParameters dsaParameters = new DSAParameters();
                int pLength = (int)reader.ReadUInt32BE();
                dsaParameters.P = (new BigInteger(reader.ReadBytes(pLength))).GetBytes();
                int qLength = (int)reader.ReadUInt32BE();
                dsaParameters.Q = (new BigInteger(reader.ReadBytes(qLength))).GetBytes();
                int gLength = (int)reader.ReadUInt32BE();
                dsaParameters.G = (new BigInteger(reader.ReadBytes(gLength))).GetBytes();
                int yLength = (int)reader.ReadUInt32BE();
                dsaParameters.Y = (new BigInteger(reader.ReadBytes(yLength))).GetBytes();

                DSACryptoServiceProvider dsaProvider = new DSACryptoServiceProvider();
                dsaProvider.ImportParameters(dsaParameters);
                HostKeyAndCertificates = new DsaKeyPair(dsaProvider);
            }
            else {
                throw new NotImplementedException("Public key algorithm " + publicKeyAlgorithm + " not implemented in KexDHReplyPacket.");
            }
            
            SshByteArray fData = new SshByteArray(reader);
            F = new BigInteger(fData.Value);

            int hashLength = (int)reader.ReadUInt32BE();
            int signingAlgorithmLength = (int)reader.ReadUInt32BE();
            string signingAlgorithmStr = Encoding.ASCII.GetString(reader.ReadBytes(signingAlgorithmLength));

            PublicKeyAlgorithm signingAlgorithm = PublicKeyAlgorithmHelper.Parse(signingAlgorithmStr);

            if (signingAlgorithm == PublicKeyAlgorithm.DSA) {
                int signatureLength = (int)reader.ReadUInt32BE();
                SignatureOfH = reader.ReadBytes(signatureLength);
            }
            else {
                throw new NotImplementedException("Signature algorithm " + signingAlgorithm + " not implemented in KexDHReplyPacket.");
            }
        }

        public override byte[] GetPayloadData()
        {
            MemoryStream buffer = new MemoryStream(Length);
            BinaryWriter writer = new BinaryWriter(buffer);

            string signatureName = PublicKeyAlgorithmHelper.ToString(SigningAlgorithm);
            byte[] signatureSshName = new SshString(signatureName).ToByteArray();
            byte[] signatureBody = new SshByteArray(SignatureOfH).ToByteArray();

            writer.Write(HostKeyAndCertificates.ToByteArray());
            writer.Write(F);
            writer.Write(new SshByteArray(signatureSshName, signatureBody).ToByteArray());

            return buffer.ToArray();
        }
    }
}
