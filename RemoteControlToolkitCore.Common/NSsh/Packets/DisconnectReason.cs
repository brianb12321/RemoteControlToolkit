namespace RemoteControlToolkitCore.Common.NSsh.Packets
{
    public enum DisconnectReason : uint
    {
        Unknown = 0,
        HostNotAllowedToConnect = 1,
        ProtocolError = 2,
        KeyExchangeFailed = 3,
        Reserved = 4,
        MacError = 5,
        CompressionError = 6,
        ServiceNotAvailable = 7,
        ProtocolVersionNotSupported = 8,
        HostKeyNotVerifiable = 9,
        ConnectionLost = 10,
        ByApplication = 11,
        TooManyConnections = 12,
        AuthenticationCancelledByUser = 13,
        NoMoreAuthenticationMethodsAvailable = 14,
        IllegalUserName = 15
    }
}
