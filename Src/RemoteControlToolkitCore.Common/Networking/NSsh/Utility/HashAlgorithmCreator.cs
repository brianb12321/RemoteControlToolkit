using System;
using System.Security.Cryptography;
using Mono.Math;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Utility
{
    public class HashAlgorithmCreator
    {
        private MacAlgorithm _macAlgorithm;
        private BigInteger _key;
        private byte[] _hash;
        private byte[] _sessionId;
        private char _type;

        public HashAlgorithmCreator(MacAlgorithm macAlgorithm, BigInteger key, byte[] hash, byte[] sessionId, char type)
        {
            _macAlgorithm = macAlgorithm;
            _key = key;
            _hash = hash;
            _sessionId = sessionId;
            _type = type;
        }
        public HashAlgorithm CreateMac()
        {
            if (_type == 0) return null;
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

            byte[] keyData = CipherFactory.DeriveKey(_key, _hash, _sessionId, _type, keySize);

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
    }
}