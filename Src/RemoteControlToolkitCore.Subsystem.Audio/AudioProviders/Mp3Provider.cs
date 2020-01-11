using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.AudioProviders
{
    [PluginModule]
    public class Mp3Provider : IAudioProviderModule
    {
        public string ProviderName => "MP3";
        public string Description => "Opens a stream as an MP3 file.";
        public bool BasedOnFile => true;
        public string FileExtension => "mp3";
        public string FileDescription => "MP3 File (*.mp3)";
        public NetworkSide ExecutingSide => NetworkSide.Server | NetworkSide.Proxy;

        public void InitializeServices(IServiceProvider kernel)
        {
            
        }

        public IWaveProvider OpenAudio(Stream input, WaveFormat format)
        {
            return new Mp3FileReader(input);
        }
    }
}