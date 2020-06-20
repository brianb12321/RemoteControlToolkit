using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.AudioProviders
{
    [Plugin(PluginName = "MDF")]
    public class MediaFoundationProvider : PluginModule<AudioOutSubsystem>, IAudioProviderModule
    {
        public Dictionary<string, string> ConfigurationOptions { get; }

        public MediaFoundationProvider()
        {
            ConfigurationOptions = new Dictionary<string, string>();
            ConfigurationOptions.Add("RepositionInRead", true.ToString());
            ConfigurationOptions.Add("SingleReaderObject", false.ToString());
        }
        public string Description => "A general wrapper for the media foundation system.";
        public bool BasedOnFile => false;
        public string FileExtension { get; }
        public string FileDescription { get; }
        public IWaveProvider OpenAudio(Stream input, WaveFormat format)
        {
            MediaFoundationReader.MediaFoundationReaderSettings settings = new MediaFoundationReader.MediaFoundationReaderSettings();
            settings.RepositionInRead = bool.Parse(ConfigurationOptions["RepositionInRead"]);
            settings.SingleReaderObject = bool.Parse(ConfigurationOptions["SingleReaderObject"]);
            return new StreamMediaFoundationReader(input, settings);
        }
    }
}