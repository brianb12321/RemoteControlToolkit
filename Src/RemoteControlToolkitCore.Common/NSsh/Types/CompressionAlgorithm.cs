using System;

namespace RemoteControlToolkitCore.Common.NSsh.Types
{
    public enum CompressionAlgorithm
    {
        None,

        Zlib
    }

    public static class CompressionAlgorithmHelper
    {
        public const string None = "none";

        public const string Zlib = "zlib";

        public static CompressionAlgorithm Parse(string value)
        {
            switch (value)
            {
                case None:
                    return CompressionAlgorithm.None;

                case Zlib:
                    return CompressionAlgorithm.Zlib;

                default:
                    throw new ArgumentException("Invalid compression alogrithm: " + value, "value");
            }
        }

        public static string ToString(CompressionAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case CompressionAlgorithm.None:
                    return None;

                case CompressionAlgorithm.Zlib:
                    return Zlib;

                default:
                    throw new ArgumentException("Invalid compression alogrithm: " + algorithm, "algorithm");
            }
        }
    }
}
