using System;
using System.Security.Cryptography;
using RemoteControlToolkitCore.Common.NSsh.Types;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.NSsh.Utility
{
    public class MacFactory : IMacFactory
    {
        private MacAlgorithm _macAlgorithm;
        private BigInteger _key;
        private byte[] _hash;
        private byte[] _sessionId;
        #region IMacFactory Members

        public void Initialize(MacAlgorithm macAlgorithm, BigInteger key, byte[] hash, byte[] sessionId)
        {
            _macAlgorithm = macAlgorithm;
            _key = key;
            _hash = hash;
            _sessionId = sessionId;
        }

        public HashAlgorithm CreateMac(char c)
        {
            if (c == 0) return null;
            // Key size in bytes (not bits!)
            int keySize;

            if (_macAlgorithm == MacAlgorithm.HmacSha1)
            {
                keySize = 20;
            }
            else
            {
                throw new ArgumentException("Invalid algorithm: " + _macAlgorithm, "algorithm");
            }

            byte[] keyData = CipherFactory.DeriveKey(_key, _hash, _sessionId, c, keySize);

            HashAlgorithm result = null;

            switch (_macAlgorithm)
            {
                case MacAlgorithm.HmacSha1:
                    result = new HMACSHA1(keyData);
                    break;

                default:
                    throw new ArgumentException("Unsupported algorithm: " + _macAlgorithm, "algorithm");
            }

            return result;
        }

        #endregion
    }
}
