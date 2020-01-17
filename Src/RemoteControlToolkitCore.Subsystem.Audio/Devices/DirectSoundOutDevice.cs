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
        public DirectSoundOutDevice(Guid id)
        {
            _id = id;
        }

        public Stream OpenDevice()
        {
            throw new NotImplementedException();
        }

        public DeviceInfo GetDeviceInfo()
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
            return deviceInfo;
        }

        public IWavePlayer Init(IWaveProvider provider)
        {
            DirectSoundOut device = new DirectSoundOut(_id);
            device.Init(provider);
            return device;
        }
    }
}