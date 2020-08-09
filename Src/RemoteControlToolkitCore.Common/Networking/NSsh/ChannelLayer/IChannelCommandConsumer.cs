namespace RemoteControlToolkitCore.Common.Networking.NSsh.ChannelLayer
{
    public interface IChannelCommandConsumer : IChannelConsumer
    {
        string Command { get; set; }
    }
}
