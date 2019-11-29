using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.AudioProviders
{
    [PluginModule]
    public class MediaFoundationProvider : IAudioProviderModule
    {
        public void InitializeServices(IServiceProvider kernel)
        {
            
        }

        public string ProviderName => "MDF";
        public string Description => "A general wrapper for the media foundation system.";
        public bool BasedOnFile => false;
        public string FileExtension { get; }
        public string FileDescription { get; }
        public IWaveProvider OpenAudio(Stream input, WaveFormat format)
        {
            return new StreamMediaFoundationReader(input);
        }
    }
}