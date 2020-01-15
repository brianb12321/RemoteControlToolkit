using System.Collections.Generic;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio
{
    public interface IAudioOutSubsystem : IPluginSubsystem<IAudioProviderModule>
    {
        IAudioProviderModule GetAudioProvider(string name);
        IAudioProviderModule[] GetAllAudioProviders();
    }
}