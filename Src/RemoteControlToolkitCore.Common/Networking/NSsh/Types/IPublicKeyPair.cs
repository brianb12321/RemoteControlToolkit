namespace RemoteControlToolkitCore.Common.Networking.NSsh.Types
{
    public interface IPublicKeyPair : IByteData
    {
        PublicKeyAlgorithm Algorithm { get; }

        byte[] Sign(byte[] data);

        bool Verify(byte[] data, byte[] signedData);
    }
}
