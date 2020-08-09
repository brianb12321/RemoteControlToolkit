namespace RemoteControlToolkitCore.Common.Networking.NSsh.Packets.Channel
{
    public enum ChannelOpenFailureReason : uint
    {
        Invalid = 0,
        AdministrativelyProhibited = 1,
        ConnectFailed = 2,
        UnknownChannelType = 3,
        ResourceShortage = 4
    }
}
