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
    [PluginModule]
    public class WaveOutDeviceSelector : IDeviceSelector
    {
        public string Category => "WaveOut";
        public string Tag => "audio";
        public NetworkSide ExecutingSide => NetworkSide.Server | NetworkSide.Proxy;

        public void InitializeServices(IServiceProvider kernel)
        {
            
        }

        DeviceInfo getInfo(int id)
        {
            WaveOutCapabilities cap = WaveOut.GetCapabilities(id);
            DeviceInfo info = new DeviceInfo(cap.ProductName, id.ToString());
            info.Data.Add("ProductName", cap.ProductName);
            info.Data.Add("Channels", cap.Channels.ToString());
            return info;
        }

        public IDevice GetDevice(string name)
        {
            int id = int.Parse(name);
            return new WaveOutDevice(id, getInfo(id));
        }

        IDevice[] IDeviceSelector.GetDevices()
        {
            List<IDevice> devices = new List<IDevice>();
            devices.Add(new WaveOutDevice(-1, getInfo(-1)));
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                devices.Add(new WaveOutDevice(i, getInfo(i)));
            }

            return devices.ToArray();
        }

        public DeviceInfo GetDeviceInfo(string name)
        {
            return getInfo(int.Parse(name));
        }

        public DeviceInfo[] GetDevicesInfo()
        {
            List<DeviceInfo> deviceInfo = new List<DeviceInfo>();
            deviceInfo.Add(getInfo(-1));
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                deviceInfo.Add(getInfo(i));
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