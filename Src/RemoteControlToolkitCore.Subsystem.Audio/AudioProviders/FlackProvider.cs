using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Flac;
using NAudio.Wave;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.AudioProviders
{
    [Plugin(PluginName = "FLAC")]
    public class FlackProvider : PluginModule<AudioOutSubsystem>, IAudioProviderModule
    {
        public string Description => "Reads and decodes a FLAC file";
        public bool BasedOnFile => true;
        public string FileExtension => "flac";
        public string FileDescription => "FLAC File (*.flac)";
        public IWaveProvider OpenAudio(Stream input, WaveFormat format)
        {
            return new FlacReader(input);
        }
    }
}