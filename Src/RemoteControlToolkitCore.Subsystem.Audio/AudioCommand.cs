using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using AudioSwitcher.AudioApi.CoreAudio;
using Crayon;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using NDesk.Options;
using NReco.VideoConverter;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.DeviceBus;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Utilities;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

[assembly: PluginLibrary("AudioSubsystem", "Audio Subsystem")]
namespace RemoteControlToolkitCore.Subsystem.Audio
{
    [Plugin(PluginName = "audio")]
    [CommandHelp("Manages the audio subsystem.")]
    public class AudioCommand : RCTApplication
    {
        private AudioOutSubsystem _audioSubystem;
        private DeviceBusSubsystem _bus;

        public override string ProcessName => "Audio Command";

        public override CommandResponse Execute(CommandRequest args, RctProcess currentProc, CancellationToken token)
        {
            Stream fileStream = Stream.Null;
            IWavePlayer player = null;
            string mode = "play";
            string pathMode = "VFS";
            string path = string.Empty;
            IAudioDevice device = (IAudioDevice)_bus.GetSelectorsByTag("audio").First(v => v.Category == "WaveOut").GetDevice("-1");
            string deviceId = "-1";
            IAudioProviderModule fileType = _audioSubystem.GetAudioProvider("MP3");
            bool wait = false;
            int sampleRate = 44100;
            int bitDepth = 16;
            int channels = 2;
            int volume = 0;
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
                    v => device = (IAudioDevice)_bus.GetSelectorsByTag("audio").First(c => c.Category == v).GetDevice(deviceId))
                .Add("deviceId|i=",
                    "The id of the device to open. Default: -1 (WaveOut Audio Mapper)",
                    v => deviceId = v)
                .Add("fileType|t=",
                    "The file provider module to use.",
                    v =>
                    {
                        fileType = _audioSubystem.GetAudioProvider(v);
                        if (fileType == null)
                        {
                            throw new ArgumentException("No audio provider module found.");
                        }
                    })
                .Add("stop", "Stops all audio playback.", v => mode = "stop")
                .Add("pause", "Pauses all audio playback.", v => mode = "pause")
                .Add("resume", "Resume all paused audio playback.", v => mode = "resume")
                .Add("setVolume=", "Sets the {VOLUME} of the current audio device.", v =>
                {
                    mode = "volume";
                    volume = int.Parse(v);
                })
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
                .Add("dev:", "Pass configuration options to the device module.", (k, v) =>
                {
                    device.SetProperty(k, v);
                })
                .Add("ftModule:", "Pass configuration options to the file type provider module.", (k, v) =>
                {
                    fileType.ConfigurationOptions.Add(k, v);
                })
                .Add("help|?",
                    "Displays the help screen.",
                    v => showHelp = true);

            options.Parse(args.Arguments.Select(a => a.ToString()));
            if (showHelp)
            {
                currentProc.Out.WriteLine("audio [OPTIONS] [DEVICE_OPTIONS] [PROVIDER_OPTIONS]\r\n");
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
                IDeviceSelector[] deviceInfo = _bus.GetSelectorsByTag("audio");
                foreach (IDeviceSelector info in deviceInfo)
                {
                    currentProc.Out.WriteLine(info.Category);
                    currentProc.Out.WriteLine("=========================================================");
                    currentProc.Out.WriteLine(info.GetDevicesInfo().ToDictionary(k => k.FileName).ShowDictionary(v => v.Name));
                }
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else if (mode == "showAllProviders")
            {
                StringBuilder sb = new StringBuilder();
                IAudioProviderModule[] providers = _audioSubystem.GetAllAudioProviders();
                int max = providers.Max(p => p.GetPluginAttribute().PluginName.Length) + 5;
                currentProc.Out.WriteLine("Installed Audio providers.");
                currentProc.Out.WriteLine("================================================");
                foreach (var provider in providers)
                {
                    sb.Append(provider.GetPluginAttribute().PluginName.PadRight(max)).AppendLine(provider.Description);
                }
                currentProc.Out.WriteLine(sb.ToString());
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else if (mode == "pause")
            {
                foreach (var audio in currentProc.ClientContext.GetExtension<IAudioQueue>().Queue)
                {
                    audio.Pause();
                }
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else if (mode == "resume")
            {
                foreach (var audio in currentProc.ClientContext.GetExtension<IAudioQueue>().Queue)
                {
                    audio.Play();
                }
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else if (mode == "volume")
            {
                CoreAudioDevice defaultPlaybackDevice;
                if(deviceId != "-1") defaultPlaybackDevice = new CoreAudioController().GetDevice(new Guid(deviceId));
                else defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
                defaultPlaybackDevice.Volume = volume;
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            try
            {

                switch (pathMode.ToUpper())
                {
                    case "VFS":
                        IFileSystem fileSystem = currentProc.Extensions.Find<IExtensionFileSystem>().GetFileSystem();
                        fileStream = fileSystem.OpenFile(path, FileMode.Open, FileAccess.Read);
                        break;
                    case "PHYS":
                        fileStream = new FileStream(Path.GetFullPath(path), FileMode.Open, FileAccess.Read);
                        break;
                    case "REMOTE_YOUTUBE":
                        try
                        {
                            currentProc.Out.WriteLine("Loading Youtube client.".BrightCyan());
                            var client = new YoutubeClient();
                            var videoTitle = client.Videos.GetAsync(path).GetAwaiter().GetResult().Title;
                            currentProc.Out.WriteLine($"Found Youtube video: \"{videoTitle}\" Retrieving video manifest.".BrightGreen());
                            var videoManifest = client.Videos.Streams.GetManifestAsync(new VideoId(path)).GetAwaiter()
                                .GetResult();
                            currentProc.Out.WriteLine("Attempting to get audio only stream.".BrightCyan());
                            var audioStreamInfo = videoManifest.GetAudio().FirstOrDefault();
                            if (audioStreamInfo == null)
                            {
                                currentProc.Out.WriteLine("Unable to retrieve audio only stream. Attempting to retrieve muxed stream.".BrightYellow());
                                audioStreamInfo = videoManifest.GetMuxed().FirstOrDefault();
                            }
                            // Download video
                            currentProc.Out.WriteLine($"Downloading Video ({audioStreamInfo.Container}) please wait ... ".BrightCyan());

                            //using (var progress = new ProgressBar())
                            var stream = client.Videos.Streams.GetAsync(audioStreamInfo).GetAwaiter().GetResult();
                            currentProc.Out.WriteLine("Loading FFMPeg converter...".BrightCyan());
                            var convert = new FFMpegConverter();
                            MemoryStream audioStream = new MemoryStream();
                            var task = convert.ConvertLiveMedia(stream, audioStreamInfo.Container.Name, audioStream, "mp3", new ConvertSettings());
                            convert.LogReceived += (sender, eventArgs) =>
                            {
                                //Display progress bar.
                                if (eventArgs.Data.StartsWith("size=") && !currentProc.OutRedirected)
                                {
                                    var handler = currentProc.ClientContext.GetExtension<ITerminalHandler>();
                                    handler.ClearRow();
                                    currentProc.Out.Write($"LOG: {eventArgs.Data}");
                                    handler.MoveCursorLeft(9999999);
                                }
                                else
                                {
                                    currentProc.Out.WriteLine($"LOG: {eventArgs.Data}");
                                }
                            };
                            task.Start();
                            task.Wait();
                            currentProc.Out.WriteLine();
                            audioStream.Seek(0, SeekOrigin.Begin);
                            fileStream = audioStream;
                            currentProc.Out.WriteLine($"Loaded file with audio: \"{videoTitle}\"".BrightGreen());
                        }
                        catch (AggregateException e)
                        {
                            currentProc.Out.WriteLine(Output.Red("Error downloading youtube video: "));
                            currentProc.Out.WriteLine();
                            foreach (Exception inner in e.InnerExceptions)
                            {
                                currentProc.Out.WriteLine(Output.Red($"Exception: {inner.Message}"));
                            }
                            throw;
                        }
                        break;
                    default:
                        throw new ArgumentException("Path mode must be VFS or PHYS.");
                }

                

                player = device.Init(fileType.OpenAudio(fileStream, new WaveFormat(sampleRate, bitDepth, channels)));

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
            _audioSubystem = kernel.GetService<AudioOutSubsystem>();
            _bus = kernel.GetService<DeviceBusSubsystem>();
        }
    }
}