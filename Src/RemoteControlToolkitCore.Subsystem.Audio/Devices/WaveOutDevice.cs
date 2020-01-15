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
        private readonly WaveOutEvent _device;
        private readonly DeviceInfo _info;

        public WaveOutDevice(int id, DeviceInfo info)
        {
            _device = new WaveOutEvent();
            _device.DeviceNumber = id;
        }
        public Stream OpenDevice()
        {
            throw new NotImplementedException();
        }

        public DeviceInfo GetDeviceInfo()
        {
            return _info;
        }

        public IWavePlayer Init(IWaveProvider provider)
        {
            _device.Init(provider);
            return _device;
        }
    }
}