namespace RemoteControlToolkitCore.Common.NSsh.Types
{
    public class TransportLayerParameters
    {
        public PublicKeyAlgorithm HostKeyVerification { get; set; }

        public EncryptionAlgorithm ClientToServerEncryption { get; set; }
        public EncryptionAlgorithm ServerToClientEncryption { get; set; }

        public MacAlgorithm ClientToServerMac { get; set; }
        public MacAlgorithm ServerToClientMac { get; set; }

        public CompressionAlgorithm ClientToServerCompression { get; set; }
        public CompressionAlgorithm ServerToClientCompression { get; set; }
    }
}
