using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.Devices
{
    [PluginModule]
    public class WaveOutDevice : IAudioOutDeviceModule
    {
        public string DeviceName => "WaveOut";
        public NetworkSide ExecutingSide => NetworkSide.Server | NetworkSide.Proxy;

        public void InitializeServices(IServiceProvider kernel)
        {
            
        }

        public IWavePlayer OpenDeviceForPlayback(IWaveProvider audio, string device)
        {
            WaveOutEvent dev = new WaveOutEvent();
            dev.DeviceNumber = int.Parse(device);
            dev.Init(audio);
            return dev;
        }

        public IReadOnlyDictionary<string, string> GetDeviceData(string device)
        {
            Dictionary<string, string> _details = new Dictionary<string, string>();
            WaveOutCapabilities cap = WaveOut.GetCapabilities(int.Parse(device));
            _details.Add("ProductName", cap.ProductName);
            _details.Add("Channels", cap.Channels.ToString());
            return new ReadOnlyDictionary<string, string>(_details);
        }

        public void SetDeviceData(string deviceId, string propertyName, string value)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<string, string> GetDevices()
        {
            Dictionary<string, string> _devices = new Dictionary<string, string>();
            _devices.Add("-1", WaveOut.GetCapabilities(-1).ProductName);
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                WaveOutCapabilities cap = WaveOut.GetCapabilities(i);
                _devices.Add(i.ToString(), cap.ProductName);
            }
            return new ReadOnlyDictionary<string, string>(_devices);
        }
    }
}