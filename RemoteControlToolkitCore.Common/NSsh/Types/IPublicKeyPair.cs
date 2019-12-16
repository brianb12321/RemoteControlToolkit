namespace RemoteControlToolkitCore.Common.NSsh.Types
{
    public interface IPublicKeyPair : IByteData
    {
        PublicKeyAlgorithm Algorithm { get; }

        byte[] Sign(byte[] data);

        bool Verify(byte[] data, byte[] signedData);
    }
}
