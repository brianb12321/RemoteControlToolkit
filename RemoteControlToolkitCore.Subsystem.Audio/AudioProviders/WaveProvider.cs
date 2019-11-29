using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.AudioProviders
{
    [PluginModule]
    public class WaveProvider : IAudioProviderModule
    {
        public string ProviderName => "WAV";
        public string Description => "Opens a stream as an WAV file.";
        public bool BasedOnFile => true;
        public string FileExtension => "wav";
        public string FileDescription => "WAV Files (*.wav)";

        public NetworkSide ExecutingSide => NetworkSide.Server;

        public void InitializeServices(IServiceProvider kernel)
        {
            
        }

        public IWaveProvider OpenAudio(Stream input, WaveFormat format)
        {
            return new WaveFileReader(input);
        }
    }
}