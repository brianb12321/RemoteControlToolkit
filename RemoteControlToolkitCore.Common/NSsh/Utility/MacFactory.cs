using System;
using System.Security.Cryptography;
using RemoteControlToolkitCore.Common.NSsh.Types;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.NSsh.Utility
{
    public class MacFactory : IMacFactory
    {
        #region IMacFactory Members

        public HashAlgorithm CreateMac(MacAlgorithm macAlgorithm, BigInteger key, byte[] hash, byte[] sessionId, char c)
        {
            // Key size in bytes (not bits!)
            int keySize;

            if (macAlgorithm == MacAlgorithm.HmacSha1)
            {
                keySize = 20;
            }
            else
            {
                throw new ArgumentException("Invalid algorithm: " + macAlgorithm, "algorithm");
            }

            byte[] keyData = CipherFactory.DeriveKey(key, hash, sessionId, c, keySize);

            HashAlgorithm result = null;

            switch (macAlgorithm)
            {
                case MacAlgorithm.HmacSha1:
                    result = new HMACSHA1(keyData);
                    break;

                default:
                    throw new ArgumentException("Unsupported algorithm: " + macAlgorithm, "algorithm");
            }

            return result;
        }

        #endregion
    }
}
