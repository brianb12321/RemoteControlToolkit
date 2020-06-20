using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Vorbis;
using NAudio.Wave;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.AudioProviders
{
    [Plugin(PluginName = "OGG")]
    public class OggProvider : PluginModule<AudioOutSubsystem>, IAudioProviderModule
    {
        public Dictionary<string, string> ConfigurationOptions { get; }

        public OggProvider()
        {
            ConfigurationOptions = new Dictionary<string, string>();
        }
        public string Description => "Uses Vorbis to decode and play ogg files.";
        public bool BasedOnFile => true;
        public string FileExtension => "ogg";
        public string FileDescription => "OGG Files (*.ogg)";
        public NetworkSide ExecutingSide => NetworkSide.Server | NetworkSide.Proxy;

        public IWaveProvider OpenAudio(Stream input, WaveFormat format)
        {
            return new VorbisWaveReader(input);
        }
    }
}