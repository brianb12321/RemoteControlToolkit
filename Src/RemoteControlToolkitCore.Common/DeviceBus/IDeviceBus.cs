using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.DeviceBus
{
    /// <summary>
    /// Represents a virtual bus.
    /// </summary>
    public interface IDeviceBus : IPluginSubsystem<IDeviceSelector>
    {
        bool CategoryExist(string category);
        IDeviceSelector GetDeviceSelector(string category);
        IDeviceSelector[] GetSelectorsByTag(string tag);
    }
}