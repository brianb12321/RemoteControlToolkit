using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteControlToolkitCore.Common.Plugin.Devices
{
    public class DeviceSubsystem : BasePluginSubsystem<IDeviceSubsystem, IDeviceSelector>, IDeviceSubsystem
    {

        public DeviceSubsystem(IPluginLibraryLoader loader, IServiceProvider services) : base(loader, services)
        {
        }

        public IDeviceSelector GetSelector(string name)
        {
            return PluginLoader.GetAllModules<IDeviceSelector>().FirstOrDefault(m => m.DeviceName.Equals(name));
        }
    }
}