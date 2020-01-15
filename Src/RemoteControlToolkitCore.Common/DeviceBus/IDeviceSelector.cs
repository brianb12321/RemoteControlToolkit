using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.DeviceBus
{

    public interface IDeviceSelector : IPluginModule
    {
        string Category { get; }
        string Tag { get; }
        IDevice GetDevice(string name);
        IDevice[] GetDevices();
        DeviceInfo GetDeviceInfo(string name);
        DeviceInfo[] GetDevicesInfo();
        bool DeviceConnected(string name);
    }
}