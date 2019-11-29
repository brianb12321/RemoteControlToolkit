using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.Devices
{
    [PluginModule]
    public class DirectSoundOutDevice : IAudioOutDeviceModule
    {
        public string DeviceName => "DirectSoundOut";
        public NetworkSide ExecutingSide => NetworkSide.Server | NetworkSide.Proxy;

        public void InitializeServices(IServiceProvider kernel)
        {
            
        }

        
        public IWavePlayer OpenDeviceForPlayback(IWaveProvider audio, string deviceGuid)
        {
            DirectSoundOut device = new DirectSoundOut(Guid.Parse(deviceGuid));
            device.Init(audio);
            return device;
        }

        public IReadOnlyDictionary<string, string> GetDeviceData(string device)
        {
            Dictionary<string, string> details = new Dictionary<string, string>();
            Guid guid = Guid.Parse(device);
            DirectSoundDeviceInfo info = DirectSoundOut.Devices.FirstOrDefault(v => v.Guid == guid);
            if (info == null)
            {
                throw new ArgumentException("The specified guid does not exist.");
            }
            details.Add("Guid", info.Guid.ToString());
            details.Add("Description", info.Description);
            details.Add("ModuleName", info.ModuleName);
            return new ReadOnlyDictionary<string, string>(details);
        }

        public void SetDeviceData(string deviceId, string propertyName, string value)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<string, string> GetDevices()
        {
            Dictionary<string, string> _devices = new Dictionary<string, string>();
            foreach (var dev in DirectSoundOut.Devices)
            {
               _devices.Add(dev.Guid.ToString(), dev.Description);
            }
            return new ReadOnlyDictionary<string, string>(_devices);
        }
    }
}