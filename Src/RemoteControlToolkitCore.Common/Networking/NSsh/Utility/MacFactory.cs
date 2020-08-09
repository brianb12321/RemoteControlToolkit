using RemoteControlToolkitCore.Common.Networking.NSsh.Types;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Utility
{
    public class MacFactory : IMacFactory
    {
        #region IMacFactory Members

        public HashAlgorithmCreator Initialize(MacAlgorithm macAlgorithm, BigInteger key, byte[] hash, byte[] sessionId, char type)
        {
            return new HashAlgorithmCreator(macAlgorithm, key, hash, sessionId, type);
        }

        #endregion
    }
}
