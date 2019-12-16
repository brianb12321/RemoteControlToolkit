using System.Security.Cryptography;
using RemoteControlToolkitCore.Common.NSsh.Types;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.NSsh.Utility
{
    public interface ICipherFactory
    {
        SymmetricAlgorithm CreateCipher(EncryptionAlgorithm algorithm, BigInteger key, byte[] hash, byte[] sessionId, char ivCharacter, char keyCharacter);
    }
}
