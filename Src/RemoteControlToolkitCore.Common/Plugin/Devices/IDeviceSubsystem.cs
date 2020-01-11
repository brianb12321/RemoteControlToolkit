namespace RemoteControlToolkitCore.Common.Plugin.Devices
{
    public interface IDeviceSubsystem : IPluginSubsystem<IDeviceSelector>
    {
        IDeviceSelector GetSelector(string name);
    }
}