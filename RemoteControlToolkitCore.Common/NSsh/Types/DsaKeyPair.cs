using System.IO;
using System.Security.Cryptography;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.NSsh.Types
{
    public class DsaKeyPair : IPublicKeyPair
    {
        public DsaKeyPair(DSACryptoServiceProvider dsaProvider)
        {
            _dsa = dsaProvider;
        }

        private DSACryptoServiceProvider _dsa;

        public PublicKeyAlgorithm Algorithm { get { return PublicKeyAlgorithm.DSA; } }

        public byte[] ToByteArray()
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memoryStream);

            writer.Write(new SshString(PublicKeyAlgorithmHelper.ToString(Algorithm)).ToByteArray());

            DSAParameters dsaParameters = _dsa.ExportParameters(false);
            writer.Write(new BigInteger(dsaParameters.P));
            writer.Write(new BigInteger(dsaParameters.Q));
            writer.Write(new BigInteger(dsaParameters.G));
            writer.Write(new BigInteger(dsaParameters.Y));

            return new SshByteArray(memoryStream.ToArray()).ToByteArray();
        }

        public byte[] Sign(byte[] data)
        {
            return _dsa.SignData(data);
        }

        public bool Verify(byte[] data, byte[] signedData) {
            return _dsa.VerifyData(data, signedData); 
        }
    }
}
