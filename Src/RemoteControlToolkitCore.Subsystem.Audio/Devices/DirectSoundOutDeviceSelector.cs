using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using RemoteControlToolkitCore.Common.DeviceBus;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.Devices
{
    [PluginModule]
    public class DirectSoundOutDeviceSelector : IDeviceSelector
    {
        public string Category => "directSoundOut";
        public string Tag => "audio";

        public void InitializeServices(IServiceProvider kernel)
        {
            
        }

        private DeviceInfo getInfo(Guid id)
        {
            DirectSoundDeviceInfo info = DirectSoundOut.Devices.FirstOrDefault(v => v.Guid == id);
            if (info == null)
            {
                throw new ArgumentException("The specified guid does not exist.");
            }
            DeviceInfo deviceInfo = new DeviceInfo(info.ModuleName, info.Guid.ToString());
            deviceInfo.Data.Add("Guid", info.Guid.ToString());
            deviceInfo.Data.Add("Description", info.Description);
            deviceInfo.Data.Add("ModuleName", info.ModuleName);
            return deviceInfo;
        }

        public IDevice GetDevice(string name)
        {
            Guid id = Guid.Parse(name);
            return new DirectSoundOutDevice(id, getInfo(id));
        }

        IDevice[] IDeviceSelector.GetDevices()
        {
            List<IDevice> devices = new List<IDevice>();
            foreach (var device in DirectSoundOut.Devices)
            {
                devices.Add(new DirectSoundOutDevice(device.Guid, getInfo(device.Guid)));
            }

            return devices.ToArray();
        }

        public DeviceInfo GetDeviceInfo(string name)
        {
            return getInfo(Guid.Parse(name));
        }

        public DeviceInfo[] GetDevicesInfo()
        {
            List<DeviceInfo> info = new List<DeviceInfo>();
            foreach (var device in DirectSoundOut.Devices)
            {
                info.Add(getInfo(device.Guid));
            }

            return info.ToArray();
        }

        public bool DeviceConnected(string name)
        {
            return DirectSoundOut.Devices.Any(d => d.Guid == Guid.Parse(name));
        }
    }
}