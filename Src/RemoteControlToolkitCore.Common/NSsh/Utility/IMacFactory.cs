using System.Security.Cryptography;
using RemoteControlToolkitCore.Common.NSsh.Types;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.NSsh.Utility
{
    public interface IMacFactory
    {
        HashAlgorithmCreator Initialize(MacAlgorithm macAlgorithm, BigInteger key, byte[] hash, byte[] sessionId, char type);
    }
}
