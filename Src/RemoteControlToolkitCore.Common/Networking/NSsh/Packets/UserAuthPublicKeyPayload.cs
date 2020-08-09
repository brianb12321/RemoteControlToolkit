using System.IO;
using System.Text;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets
{
    /// <summary>
    /// 
    /// boolean   FALSE
    /// string    public key algorithm name
    /// string    public key blob
    ///
    /// or
    ///
    /// boolean   TRUE
    /// string    public key algorithm name
    /// string    public key to be used for authentication
    /// string    signature
    /// </summary>
    public class UserAuthPublicKeyPayload : IByteData
    {        
        public UserAuthPublicKeyPayload() { }

        public UserAuthPublicKeyPayload(BinaryReader reader)
        {
            bool signaturePresent = (reader.ReadByte() == 1);
            string algorithmName = new SshString(reader, Encoding.ASCII).Value;
            Algorithm = PublicKeyAlgorithmHelper.Parse(algorithmName);
            PublicKeyBlob = new SshByteArray(reader).Value;

            if (signaturePresent)
            {
                Signature = new SshByteArray(reader).Value;
            }
        }

        public PublicKeyAlgorithm Algorithm { get; set; }

        public byte[] PublicKeyBlob { get; set; }

        public byte[] Signature { get; set; }

        #region IByteData Members

        public byte[] ToByteArray()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);

            writer.Write((byte)(Signature != null ? 1 : 0));
            writer.Write(new SshString(PublicKeyAlgorithmHelper.ToString(Algorithm), Encoding.ASCII).ToByteArray());
            writer.Write(new SshByteArray(PublicKeyBlob).ToByteArray());

            if (Signature != null)
            {
                writer.Write(new SshByteArray(Signature).ToByteArray());
            }

            return buffer.ToArray();
        }

        #endregion
    }
}
