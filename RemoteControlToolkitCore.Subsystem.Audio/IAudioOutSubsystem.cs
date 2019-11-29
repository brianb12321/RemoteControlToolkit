using System.Collections.Generic;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio
{
    public interface IAudioOutSubsystem : IPluginSubsystem<IAudioOutDeviceModule>
    {
        IReadOnlyDictionary<string, string> GetAudioDevices(string name);
        IAudioOutDeviceModule GetAudioDeviceType(string name);
        IAudioProviderModule GetAudioProvider(string name);
        IAudioProviderModule[] GetAllAudioProviders();
        string BuildBrowserFilter();
    }
}