using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.AudioProviders
{
    [Plugin(PluginName = "WAV")]
    public class WaveProvider : PluginModule<AudioOutSubsystem>, IAudioProviderModule
    {
        public string Description => "Opens a stream as an WAV file.";
        public bool BasedOnFile => true;
        public string FileExtension => "wav";
        public string FileDescription => "WAV Files (*.wav)";

        public NetworkSide ExecutingSide => NetworkSide.Server;

        public IWaveProvider OpenAudio(Stream input, WaveFormat format)
        {
            return new WaveFileReader(input);
        }
    }
}