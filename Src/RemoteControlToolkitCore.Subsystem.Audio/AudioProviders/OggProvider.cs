﻿using System;
using System.IO;
using NAudio.Vorbis;
using NAudio.Wave;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio.AudioProviders
{
    [PluginModule]
    public class OggProvider : IAudioProviderModule
    {
        public string ProviderName => "OGG";
        public string Description => "Uses Vorbis to decode and play ogg files.";
        public bool BasedOnFile => true;
        public string FileExtension => "ogg";
        public string FileDescription => "OGG Files (*.ogg)";
        public NetworkSide ExecutingSide => NetworkSide.Server | NetworkSide.Proxy;

        public void InitializeServices(IServiceProvider kernel)
        {
            
        }
        public IWaveProvider OpenAudio(Stream input, WaveFormat format)
        {
            return new VorbisWaveReader(input);
        }
    }
}