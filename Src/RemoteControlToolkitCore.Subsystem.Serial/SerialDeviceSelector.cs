using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.DeviceBus;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Serial
{
    [PluginModule]
    public class SerialDeviceSelector : IDeviceSelector
    {
        public string Category => "serial";
        public string Tag => "IO";

        public void InitializeServices(IServiceProvider kernel)
        {
            
        }

        private DeviceInfo getInfo(string name)
        {
            DeviceInfo info = new DeviceInfo(name, name);
            info.Data.Add("BaudRate", 9600);
            info.Data.Add("Parity", Parity.None);
            return info;
        }
        public IDevice GetDevice(string name)
        {
            return new RCTSerialDevice(getInfo(name));
        }

        public IDevice[] GetDevices()
        {
            List<IDevice> devices = new List<IDevice>();
            foreach (string name in SerialPort.GetPortNames())
            {
                devices.Add(new RCTSerialDevice(getInfo(name)));
            }

            return devices.ToArray();
        }

        public DeviceInfo GetDeviceInfo(string name)
        {
            return getInfo(name);
        }

        public DeviceInfo[] GetDevicesInfo()
        {
            List<DeviceInfo> devices = new List<DeviceInfo>();
            foreach (string name in SerialPort.GetPortNames())
            {
                devices.Add(getInfo(name));
            }

            return devices.ToArray();
        }

        public bool DeviceConnected(string name)
        {
            return SerialPort.GetPortNames().Any(v => name == v);
        }
    }
}