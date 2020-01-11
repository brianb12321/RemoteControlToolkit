namespace RemoteControlToolkitCore.Common.NSsh.ChannelLayer
{
    public interface IChannelCommandConsumer : IChannelConsumer
    {
        string Command { get; set; }
    }
}
