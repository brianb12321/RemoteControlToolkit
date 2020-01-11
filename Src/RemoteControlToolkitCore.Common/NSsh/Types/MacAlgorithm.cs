using System;

namespace RemoteControlToolkitCore.Common.NSsh.Types
{
    public enum MacAlgorithm
    {
        None,

        HmacSha1
    }

    public static class MacAlgorithmHelper
    {
        public const string None = "none";

        public const string HmacSha1 = "hmac-sha1";

        public static MacAlgorithm Parse(string value)
        {
            switch (value)
            {
                case None:
                    return MacAlgorithm.None;

                case HmacSha1:
                    return MacAlgorithm.HmacSha1;

                default:
                    throw new ArgumentException("Invalid MAC alogrithm: " + value, "value");
            }
        }

        public static string ToString(MacAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case MacAlgorithm.None:
                    return None;

                case MacAlgorithm.HmacSha1:
                    return HmacSha1;

                default:
                    throw new ArgumentException("Invalid MAC alogrithm: " + algorithm, "algorithm");
            }
        }
    }
}
