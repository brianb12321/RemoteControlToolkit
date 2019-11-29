using System.IO;
using NAudio.Wave;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio
{
    public interface IAudioProviderModule : IPluginModule
    {
        string ProviderName { get; }
        string Description { get; }
        bool BasedOnFile { get; }
        string FileExtension { get; }
        string FileDescription { get; }
        IWaveProvider OpenAudio(Stream input, WaveFormat format);
    }
}