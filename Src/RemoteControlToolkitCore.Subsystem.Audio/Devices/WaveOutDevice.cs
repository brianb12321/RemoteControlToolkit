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
        private DeviceInfo _info;

        public float Volume
        {
            get => (float)_info.Data["Volume"];
            set => _info.Data["Volume"] = value;
        }
        public WaveOutDevice(int id)
        {
            _id = id;
            _info = setupDevice();
        }
        public Stream OpenDevice()
        {
            throw new NotImplementedException();
        }

        private DeviceInfo setupDevice()
        {
            WaveOutCapabilities cap = WaveOut.GetCapabilities(_id);
            DeviceInfo info = new DeviceInfo(cap.ProductName, _id.ToString());
            info.Data.Add("ProductName", cap.ProductName);
            info.Data.Add("Channels", cap.Channels.ToString());
            info.Data.Add("Volume", 1f);
            return info;
        }
        public DeviceInfo GetDeviceInfo()
        {
            return _info;
        }

        public TType Query<TType>(string key)
        {
            return (TType)_info.Data[key];
        }

        public void SetProperty(string propertyName, object value)
        {
            
        }

        public IWavePlayer Init(IWaveProvider provider)
        {
            WaveOutEvent device = new WaveOutEvent {DeviceNumber = _id};
            device.Volume = Volume;
            device.Init(provider);
            return device;
        }
    }
}