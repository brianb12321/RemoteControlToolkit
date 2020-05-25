using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.DeviceBus;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.Devices
{
    [Plugin]
    public class WaveOutDeviceSelector : PluginModule<DeviceBusSubsystem>, IDeviceSelector
    {
        public string Category => "WaveOut";
        public string Tag => "audio";
        public NetworkSide ExecutingSide => NetworkSide.Server | NetworkSide.Proxy;

        public IDevice GetDevice(string name)
        {
            int id = int.Parse(name);
            return new WaveOutDevice(id);
        }

        IDevice[] IDeviceSelector.GetDevices()
        {
            List<IDevice> devices = new List<IDevice>();
            devices.Add(new WaveOutDevice(-1));
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                devices.Add(new WaveOutDevice(i));
            }

            return devices.ToArray();
        }

        public DeviceInfo GetDeviceInfo(string name)
        {
            return new WaveOutDevice(int.Parse(name)).GetDeviceInfo();
        }

        public DeviceInfo[] GetDevicesInfo()
        {
            List<DeviceInfo> deviceInfo = new List<DeviceInfo>();
            deviceInfo.Add(new WaveOutDevice(-1).GetDeviceInfo());
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                deviceInfo.Add(new WaveOutDevice(i).GetDeviceInfo());
            }

            return deviceInfo.ToArray();
        }

        public bool DeviceConnected(string name)
        {
            int id = int.Parse(name);
            return id == -1 || (id < WaveOut.DeviceCount && id > -1);
        }
    }
}