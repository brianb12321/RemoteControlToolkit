using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Vorbis;
using NAudio.Wave;
using NVorbis;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Subsystem.Audio;

namespace RemoteControlToolkitLibrary.Subsystem.Audio.AudioProviders
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