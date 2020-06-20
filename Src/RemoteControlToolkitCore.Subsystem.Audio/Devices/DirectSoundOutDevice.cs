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
    public class DirectSoundOutDevice : IAudioDevice
    {
        private readonly Guid _id;
        private DeviceInfo _info;

        public float Volume
        {
            get => (float)_info.Data["Volume"];
            set => _info.Data["Volume"] = value;
        }
        public DirectSoundOutDevice(Guid id)
        {
            _id = id;
            _info = setupDevice();
        }

        private DeviceInfo setupDevice()
        {
            DirectSoundDeviceInfo info = DirectSoundOut.Devices.FirstOrDefault(d => d.Guid == _id);
            if (info == null)
            {
                throw new ArgumentException("The specified guid does not exist.");
            }
            DeviceInfo deviceInfo = new DeviceInfo(info.ModuleName, info.Guid.ToString());
            deviceInfo.Data.Add("Guid", info.Guid.ToString());
            deviceInfo.Data.Add("Description", info.Description);
            deviceInfo.Data.Add("ModuleName", info.ModuleName);
            deviceInfo.Data.Add("Volume", 1f);
            return deviceInfo;
        }

        public Stream OpenDevice()
        {
            throw new NotImplementedException();
        }

        public DeviceInfo GetDeviceInfo()
        {
            return _info;
        }

        public TType Query<TType>(string key)
        {
            return (TType) _info.Data[key];
        }

        public void SetProperty(string propertyName, object value)
        {
            
        }

        public IWavePlayer Init(IWaveProvider provider)
        {
            DirectSoundOut device = new DirectSoundOut(_id);
            device.Volume = (float)_info.Data["Volume"];
            device.Init(provider);
            return device;
        }
    }
}