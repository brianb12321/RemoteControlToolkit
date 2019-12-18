using System.Security.Cryptography;
using RemoteControlToolkitCore.Common.NSsh.Types;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.NSsh.Utility
{
    public interface IMacFactory
    {
        void Initialize(MacAlgorithm macAlgorithm, BigInteger key, byte[] hash, byte[] sessionId);
        HashAlgorithm CreateMac(char c);
    }
}
