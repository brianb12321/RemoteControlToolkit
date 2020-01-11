using System.Collections.Generic;

namespace RemoteControlToolkitCore.Common.Plugin.Devices
{
    public interface IDeviceSelector : IPluginModule
    {
        string DeviceName { get; }
        IReadOnlyDictionary<string, string> GetDeviceData(string device);
        void SetDeviceData(string deviceId, string propertyName, string value);
        IReadOnlyDictionary<string, string> GetDevices();
    }
}