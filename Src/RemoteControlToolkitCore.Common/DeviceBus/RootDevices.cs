using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.DeviceBus
{
    [PluginModule]
    public class RootDevices : IDeviceSelector
    {
        public string Category => "root";
        public string Tag => "utilities";
        private Dictionary<string, (DeviceInfo info, Func<IDevice> function)> _devices;

        public RootDevices()
        {
            _devices = new Dictionary<string, (DeviceInfo info, Func<IDevice> function)>();
            _devices.Add("null", (new DeviceInfo("Null Device", "null"), () => new InlineDevice(() => Stream.Null)));
            _devices.Add("consoleOut", (new DeviceInfo("Console Out Device", "consoleOut"), () => new InlineDevice(() => Console.OpenStandardOutput())));
            _devices.Add("consoleError", (new DeviceInfo("Console Out Device", "consoleError"), () => new InlineDevice(() => Console.OpenStandardError())));
            _devices.Add("consoleIn", (new DeviceInfo("Console In Device", "consoleIn"), () => new InlineDevice(() => Console.OpenStandardInput())));
        }
        public void InitializeServices(IServiceProvider kernel)
        {
            
        }

        public IDevice GetDevice(string name)
        {
            return _devices[name].function();
        }

        public IDevice[] GetDevices()
        {
            return _devices.Values.Select(v => v.function()).ToArray();
        }

        public DeviceInfo GetDeviceInfo(string name)
        {
            return _devices[name].info;
        }

        public DeviceInfo[] GetDevicesInfo()
        {
            return _devices.Values.Select(v => v.info).ToArray();
        }

        public bool DeviceConnected(string name)
        {
            return _devices.ContainsKey(name);
        }
    }
}