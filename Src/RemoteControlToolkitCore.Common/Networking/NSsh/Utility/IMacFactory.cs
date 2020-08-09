using RemoteControlToolkitCore.Common.Networking.NSsh.Types;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Utility
{
    public interface IMacFactory
    {
        HashAlgorithmCreator Initialize(MacAlgorithm macAlgorithm, BigInteger key, byte[] hash, byte[] sessionId, char type);
    }
}
