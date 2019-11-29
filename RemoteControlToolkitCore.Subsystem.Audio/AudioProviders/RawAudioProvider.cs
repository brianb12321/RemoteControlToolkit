using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.AudioProviders
{
    [PluginModule]
    public class RawAudioProvider : IAudioProviderModule
    {
        public string ProviderName => "RAW";
        public string Description => "Reads raw PCM data from a data source.";
        public bool BasedOnFile => false;
        public string FileExtension { get; }
        public string FileDescription { get; }
        public NetworkSide ExecutingSide => NetworkSide.Server | NetworkSide.Proxy;

        public void InitializeServices(IServiceProvider kernel)
        {

        }

        public IWaveProvider OpenAudio(Stream input, WaveFormat format)
        {
            return new RawSourceWaveStream(input, format);
        }
    }
}