using System;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Types
{
    public enum PublicKeyAlgorithm
    {
        DSA,

        RSA
    }

    public static class PublicKeyAlgorithmHelper
    {
        public const string DSA = "ssh-dss";

        public const string RSA = "ssh-rsa";

        public static PublicKeyAlgorithm Parse(string value)
        {
            switch (value)
            {
                case DSA:
                    return PublicKeyAlgorithm.DSA;

                case RSA:
                    return PublicKeyAlgorithm.RSA;

                default:
                    throw new ArgumentException("Invalid public key alogrithm: " + value, "value");
            }
        }

        public static string ToString(PublicKeyAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case PublicKeyAlgorithm.DSA:
                    return DSA;

                case PublicKeyAlgorithm.RSA:
                    return RSA;

                default:
                    throw new ArgumentException("Invalid public key alogrithm: " + algorithm, "algorithm");
            }
        }
    }
}
