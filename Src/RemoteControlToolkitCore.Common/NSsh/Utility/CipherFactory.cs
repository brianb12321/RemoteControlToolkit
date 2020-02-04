using System;
using System.IO;
using System.Security.Cryptography;
using RemoteControlToolkitCore.Common.NSsh.Types;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.NSsh.Utility
{
    public class CipherFactory : ICipherFactory
    {
        #region ICipherFactory Members

        public SymmetricAlgorithm CreateCipher(EncryptionAlgorithm algorithm, BigInteger key, byte[] hash,
            byte[] sessionId, char ivCharacter, char keyCharacter)
        {
            // Key size in bytes (not bits!)
            int keySize;

            if (algorithm == EncryptionAlgorithm.TripleDesCbc)
            {
                keySize = 24;
            }
            else if (algorithm == EncryptionAlgorithm.BlowfishCbc || algorithm == EncryptionAlgorithm.Aes128Cbc)
            {
                keySize = 16;
            }
            else
            {
                throw new ArgumentException("Invalid algorithm: " + algorithm, "algorithm");
            }

            byte[] ivData = DeriveKey(key, hash, sessionId, ivCharacter, keySize);
            byte[] keyData = DeriveKey(key, hash, sessionId, keyCharacter, keySize);

            SymmetricAlgorithm result = null; 

            switch (algorithm)
            {
                case EncryptionAlgorithm.Aes128Cbc:
                    result = Rijndael.Create();
                    break;

                case EncryptionAlgorithm.TripleDesCbc:
                    result = TripleDES.Create();
                    break;

                default:
                    throw new ArgumentException("Unsupported algorithm: " + algorithm, "algorithm");
            }

            result.Mode = CipherMode.CBC;
            result.BlockSize = keySize * 8;
            result.IV = ivData;
            result.Key = keyData;
            result.Padding = PaddingMode.None;

            return result;
        }

        #endregion

        public static byte[] DeriveKey(BigInteger key, byte[] hash, byte[] sessionId, char c, int length)
        {
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();

            MemoryStream keyBuffer = new MemoryStream();
            BinaryWriter keyWriter = new BinaryWriter(keyBuffer);

            MemoryStream hashBuffer = new MemoryStream();
            BinaryWriter hashWriter = new BinaryWriter(hashBuffer);

            hashWriter.Write(key);
            hashWriter.Write(hash);
            hashWriter.Write((byte)c);
            hashWriter.Write(sessionId);
            
            while (keyBuffer.Length < length)
            {
                byte[] keyInput;
                keyInput = sha1.ComputeHash(hashBuffer.ToArray());
                int writeLength = Math.Min(length, keyInput.Length);
                keyWriter.Write(keyInput, 0, writeLength);

                if (keyBuffer.Length < length)
                {
                    hashBuffer = new MemoryStream();
                    hashWriter = new BinaryWriter(hashBuffer);
                    hashWriter.Write(key);
                    hashWriter.Write(hash);
                    hashWriter.Write(keyBuffer.ToArray());
                }
            }

            return keyBuffer.ToArray();
        }
    }
}
