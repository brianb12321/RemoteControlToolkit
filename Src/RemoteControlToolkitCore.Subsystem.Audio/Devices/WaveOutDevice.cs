using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using RemoteControlToolkitCore.Common.DeviceBus;

namespace RemoteControlToolkitCore.Subsystem.Audio.Devices
{
    public class WaveOutDevice : IAudioDevice
    {
        private readonly int _id;

        public WaveOutDevice(int id)
        {
            _id = id;
        }
        public Stream OpenDevice()
        {
            throw new NotImplementedException();
        }

        public DeviceInfo GetDeviceInfo()
        {
            WaveOutCapabilities cap = WaveOut.GetCapabilities(_id);
            DeviceInfo info = new DeviceInfo(cap.ProductName, _id.ToString());
            info.Data.Add("ProductName", cap.ProductName);
            info.Data.Add("Channels", cap.Channels.ToString());
            return info;
        }

        public IWavePlayer Init(IWaveProvider provider)
        {
            WaveOutEvent device = new WaveOutEvent {DeviceNumber = _id};
            device.Init(provider);
            return device;
        }
    }
}