using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using RemoteControlToolkitCore.Common.DeviceBus;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.Devices
{
    [Plugin]
    public class DirectSoundOutDeviceSelector : PluginModule<DeviceBusSubsystem>, IDeviceSelector
    {
        public string Category => "directSoundOut";
        public string Tag => "audio";

        public IDevice GetDevice(string name)
        {
            Guid id = Guid.Parse(name);
            return new DirectSoundOutDevice(id);
        }

        IDevice[] IDeviceSelector.GetDevices()
        {
            List<IDevice> devices = new List<IDevice>();
            foreach (var device in DirectSoundOut.Devices)
            {
                devices.Add(new DirectSoundOutDevice(device.Guid));
            }

            return devices.ToArray();
        }

        public DeviceInfo GetDeviceInfo(string name)
        {
            return new DirectSoundOutDevice(Guid.Parse(name)).GetDeviceInfo();
        }

        public DeviceInfo[] GetDevicesInfo()
        {
            List<DeviceInfo> info = new List<DeviceInfo>();
            foreach (var device in DirectSoundOut.Devices)
            {
                info.Add(new DirectSoundOutDevice(device.Guid).GetDeviceInfo());
            }

            return info.ToArray();
        }

        public bool DeviceConnected(string name)
        {
            return DirectSoundOut.Devices.Any(d => d.Guid == Guid.Parse(name));
        }
    }
}