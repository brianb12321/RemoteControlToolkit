using System;

namespace RemoteControlToolkitCore.Common.NSsh.Types
{
    public enum EncryptionAlgorithm
    {
        None,

        TripleDesCbc,

        BlowfishCbc,

        Aes128Cbc
    }

    public static class EncryptionAlgorithmHelper
    {
        public const string None = "none";

        public const string TripleDesCbc = "3des-cbc";

        public const string BlowfishCbc = "blowfish-cbc";

        public const string Aes128Cbc = "aes128-cbc";

        public static EncryptionAlgorithm Parse(string value)
        {
            switch (value)
            {
                case None:
                    return EncryptionAlgorithm.None;

                case TripleDesCbc:
                    return EncryptionAlgorithm.TripleDesCbc;

                case BlowfishCbc:
                    return EncryptionAlgorithm.BlowfishCbc;

                case Aes128Cbc:
                    return EncryptionAlgorithm.Aes128Cbc;

                default:
                    throw new ArgumentException("Invalid encryption alogrithm: " + value, "value");
            }
        }

        public static string ToString(EncryptionAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case EncryptionAlgorithm.None:
                    return None;

                case EncryptionAlgorithm.TripleDesCbc:
                    return TripleDesCbc;

                case EncryptionAlgorithm.BlowfishCbc:
                    return BlowfishCbc;

                case EncryptionAlgorithm.Aes128Cbc:
                    return Aes128Cbc;

                default:
                    throw new ArgumentException("Invalid encryption alogrithm: " + algorithm, "algorithm");
            }
        }
    }
}
