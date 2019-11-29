using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Crayon;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using NDesk.Options;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Utilities;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using VideoLibrary;
using Zio;

[assembly: PluginLibrary(null, "AudioSubsystem", FriendlyName = "Audio Subsystem", LibraryType = NetworkSide.Proxy | NetworkSide.Server)]
namespace RemoteControlToolkitCore.Subsystem.Audio
{
    [PluginModule(Name = "audio", ExecutingSide = NetworkSide.Server)]
    [CommandHelp("Manages the audio subsystem.")]
    public class AudioCommand : RCTApplication
    {
        private IAudioOutSubsystem _audioSubystem;

        public override string ProcessName => "Audio Command";

        public override CommandResponse Execute(CommandRequest args, RCTProcess currentProc, CancellationToken token)
        {
            Stream fileStream = Stream.Null;
            IWavePlayer player = null;
            string mode = "play";
            string pathMode = "VFS";
            string path = string.Empty;
            string device = "WaveOut";
            string deviceId = "-1";
            string fileType = "WAV";
            bool wait = false;
            int sampleRate = 44100;
            int bitDepth = 16;
            int channels = 2;
            bool showHelp = false;
            OptionSet options = new OptionSet()
                .Add("pathMode|m=",
                    "The mode for which the system will use for scanning the specified audio file. Options: VFS, PHYS, REMOTE_YOUTUBE",
                    v => pathMode = v)
                .Add("path|p=",
                    "The path to the audio file.",
                    v => path = v)
                .Add("device|d=",
                    "The installed device module to use. Default: WaveOut",
                    v => device = v)
                .Add("deviceId|i=",
                    "The id of the device to open. Default: -1 (WaveOut Audio Mapper)",
                    v => deviceId = v)
                .Add("fileType|t=",
                    "The file provider module to use.",
                    v => fileType = v)
                .Add("stop", "Stops all audio playback.", v => mode = "stop")
                .Add("wait|w", "Waits for the audio to finish.", v => wait = true)
                .Add("showAllDevices", "Displays all the registered audio devices.", v => mode = "showAllDevices")
                .Add("showAllProviders", "Shows all the audio providers.", v => mode = "showAllProviders")
                .Add("sampleRate|r=", "When in raw format, sets the sample rate of playback. Default is 44100 hz.", v =>
                {
                    if (int.TryParse(v, out int result))
                    {
                        sampleRate = result;
                    }
                    else
                    {
                        currentProc.Error.WriteLine(Output.Red("Sample rate must be a number."));
                    }
                })
                .Add("bitDepth|b=", "When in raw format, sets the bit depth of audio. Default is 16.", v =>
                {
                    if (int.TryParse(v, out int result))
                    {
                        bitDepth = result;
                    }
                    else
                    {
                        currentProc.Error.WriteLine(Output.Red("bit depth must be a number."));
                    }
                })
                .Add("channels|c=", "When in raw format, sets the number of channels to use for playback.", v =>
                {
                    if (int.TryParse(v, out int result))
                    {
                        channels = result;
                    }
                    else
                    {
                        currentProc.Error.WriteLine(Output.Red("channel must be a number."));
                    }
                })
                .Add("help|?",
                    "Displays the help screen.",
                    v => showHelp = true);

            options.Parse(args.Arguments.Select(a => a.ToString()));
            if (showHelp)
            {
                options.WriteOptionDescriptions(currentProc.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS); ;
            }
            else if (mode == "stop")
            {
                currentProc.ClientContext.GetExtension<IAudioQueue>().StopAll();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else if (mode == "showAllDevices")
            {
                IAudioOutDeviceModule[] modules = _audioSubystem.GetAllModules();
                foreach (IAudioOutDeviceModule module in modules)
                {
                    currentProc.Out.WriteLine(module.DeviceName);
                    currentProc.Out.WriteLine("=========================================================");
                    currentProc.Out.WriteLine(module.GetDevices().ShowDictionary());
                }
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else if (mode == "showAllProviders")
            {
                StringBuilder sb = new StringBuilder();
                IAudioProviderModule[] providers = _audioSubystem.GetAllAudioProviders();
                int max = providers.Max(p => p.ProviderName.Length) + 5;
                currentProc.Out.WriteLine("Installed Audio providers.");
                currentProc.Out.WriteLine("================================================");
                foreach (var provider in providers)
                {
                    sb.Append(provider.ProviderName.PadRight(max)).AppendLine(provider.Description);
                }
                currentProc.Out.WriteLine(sb.ToString());
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            try
            {

                IAudioOutDeviceModule module = _audioSubystem.GetAudioDeviceType(device);
                switch (pathMode.ToUpper())
                {
                    case "VFS":
                        IFileSystem fileSystem = currentProc.ClientContext.GetExtension<IExtensionFileSystem>().FileSystem;
                        fileStream = fileSystem.OpenFile(path, FileMode.Open, FileAccess.Read);
                        break;
                    case "PHYS":
                        fileStream = new FileStream(Path.GetFullPath(path), FileMode.Open, FileAccess.Read);
                        break;
                    case "REMOTE_YOUTUBE":
                        var youtube = YouTube.Default;
                        var vid = youtube.GetVideo(path);

                        fileStream = new MemoryStream(vid.GetBytes(), false);
                        currentProc.Out.WriteLine($"Loaded file with audio {vid.AudioFormat.ToString()}");
                        break;
                    default:
                        throw new ArgumentException("Path mode must be VFS or PHYS.");
                }

                IAudioProviderModule provider = _audioSubystem.GetAudioProvider(fileType);
                if (provider == null)
                {
                    throw new ArgumentException("No audio provider module found.");
                }

                player = module.OpenDeviceForPlayback(provider.OpenAudio(fileStream, new WaveFormat(sampleRate, bitDepth, channels)), deviceId);

                player.PlaybackStopped += (sender, e) =>
                {
                    player.Dispose();
                    fileStream.Dispose();
                };
                player.Play();
                currentProc.ClientContext.GetExtension<IAudioQueue>().Queue.Add(player);
                if (wait)
                {
                    token.Register(() =>
                    {
                        player.Stop();
                        player.Dispose();
                        fileStream.Dispose();
                    });
                    while (player.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(1000);
                    }
                }
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            catch
            {
                fileStream.Dispose();
                player?.Dispose();
                throw;
            }
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            _audioSubystem = (IAudioOutSubsystem)kernel.GetService<IPluginSubsystem<IAudioOutDeviceModule>>();
        }
    }
}