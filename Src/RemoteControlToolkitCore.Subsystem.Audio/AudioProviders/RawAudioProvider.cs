using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.AudioProviders
{
    [Plugin(PluginName = "RAW")]
    public class RawAudioProvider : PluginModule<AudioOutSubsystem>, IAudioProviderModule
    {
        public Dictionary<string, string> ConfigurationOptions { get; }

        public RawAudioProvider()
        {
            ConfigurationOptions = new Dictionary<string, string>();
        }
        public string Description => "Reads raw PCM data from a data source.";
        public bool BasedOnFile => false;
        public string FileExtension { get; }
        public string FileDescription { get; }
        public NetworkSide ExecutingSide => NetworkSide.Server | NetworkSide.Proxy;

        public IWaveProvider OpenAudio(Stream input, WaveFormat format)
        {
            return new RawSourceWaveStream(input, format);
        }
    }
}