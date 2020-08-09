using System.Security.Cryptography;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Utility
{
    public interface ICipherFactory
    {
        SymmetricAlgorithm CreateCipher(EncryptionAlgorithm algorithm, BigInteger key, byte[] hash, byte[] sessionId, char ivCharacter, char keyCharacter);
    }
}
