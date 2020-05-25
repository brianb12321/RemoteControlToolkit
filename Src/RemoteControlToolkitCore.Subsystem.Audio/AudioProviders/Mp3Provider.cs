using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.AudioProviders
{
    [Plugin(PluginName = "MP3")]
    public class Mp3Provider : PluginModule<AudioOutSubsystem>, IAudioProviderModule
    {
        public string Description => "Opens a stream as an MP3 file.";
        public bool BasedOnFile => true;
        public string FileExtension => "mp3";
        public string FileDescription => "MP3 File (*.mp3)";
        public NetworkSide ExecutingSide => NetworkSide.Server | NetworkSide.Proxy;

        public IWaveProvider OpenAudio(Stream input, WaveFormat format)
        {
            return new Mp3FileReader(input);
        }
    }
}