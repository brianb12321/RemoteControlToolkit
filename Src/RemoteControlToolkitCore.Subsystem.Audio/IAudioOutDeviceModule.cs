using NAudio.Wave;
using RemoteControlToolkitCore.Common.Plugin.Devices;

namespace RemoteControlToolkitCore.Subsystem.Audio
{
    public interface IAudioOutDeviceModule : IDeviceSelector
    {
        IWavePlayer OpenDeviceForPlayback(IWaveProvider audio, string device);
    }
}