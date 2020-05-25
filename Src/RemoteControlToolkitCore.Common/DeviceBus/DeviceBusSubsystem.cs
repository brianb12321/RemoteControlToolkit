using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.DeviceBus
{
    public class DeviceBusSubsystem : PluginSubsystem
    {
        private readonly ConcurrentBag<IDeviceSelector> _deviceSelectors;
        public DeviceBusSubsystem(IPluginManager manager) : base(manager)
        {
            _deviceSelectors = new ConcurrentBag<IDeviceSelector>();
        }

        public override void InitializeSubsystem()
        {
            foreach (var deviceSelector in PluginManager.ActivateAllPluginModules<DeviceBusSubsystem>()
                .Select(m => m as IDeviceSelector))
            {
                _deviceSelectors.Add(deviceSelector);
            }
        }

        public IDeviceSelector[] GetAllDeviceSelectors()
        {
            return _deviceSelectors.ToArray();
        }
        public bool CategoryExist(string category)
        {
            return _deviceSelectors.Any(v => v.Category == category);
        }

        public IDeviceSelector GetDeviceSelector(string category)
        {
            return _deviceSelectors.FirstOrDefault(v => v.Category == category);
        }

        public IDeviceSelector[] GetSelectorsByTag(string tag)
        {
            return _deviceSelectors.Where(v => v.Tag == tag).ToArray();
        }
    }
}