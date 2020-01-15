using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.DeviceBus
{
    public class DeviceBus : BasePluginSubsystem<IDeviceBus, IDeviceSelector>, IDeviceBus
    {
        public DeviceBus(IPluginLibraryLoader loader, IServiceProvider services) : base(loader, services)
        {
        }

        public bool CategoryExist(string category)
        {
            return GetAllModules().Any(v => v.Category == category);
        }

        public IDeviceSelector GetDeviceSelector(string category)
        {
            return GetAllModules().FirstOrDefault(v => v.Category == category);
        }

        public IDeviceSelector[] GetSelectorsByTag(string tag)
        {
            return GetAllModules().Where(v => v.Tag == tag).ToArray();
        }
    }
}