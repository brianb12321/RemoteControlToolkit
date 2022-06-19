using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crayon;
using ManyConsole;
using NAudio.Wave;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.DeviceBus;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace RemoteControlToolkitCore.Subsystem.Audio.Commands
{
    public class PlayCommand : ConsoleCommand
    {
        private readonly AudioOutSubsystem _audioSubsystem;
        private readonly DeviceBusSubsystem _bus;
        Stream _fileStream = Stream.Null;
        IWavePlayer _player = null;
        string _pathMode = "VFS";
        string _path = string.Empty;
        private IAudioDevice _device;
        string _deviceId = "-1";
        private IAudioProviderModule _fileType;
        bool _wait = false;
        int _sampleRate = 44100;
        int _bitDepth = 16;
        int _channels = 2;
        private readonly RctProcess _currentProc;
        private CancellationToken _token;

        public PlayCommand(AudioOutSubsystem audioSubsystem, DeviceBusSubsystem bus, RctProcess currentProc, CancellationToken token)
        {
            _currentProc = currentProc;
            _token = token;
            IsCommand("play", "Plays audio from a specified source and type.");
            _audioSubsystem = audioSubsystem;
            _bus = bus;
            
            HasOption("pathMode|m=",
                "The mode for which the system will use for scanning the specified audio file. Options: VFS, PHYS, REMOTE_YOUTUBE",
                v => _pathMode = v);
            HasOption("device|d=",
                "The installed device module to use. Default: WaveOut",
                v => _device =
                    (IAudioDevice) _bus.GetSelectorsByTag("audio").First(c => c.Category == v).GetDevice(_deviceId));
            HasOption("deviceId|i=",
                "The id of the device to open. Default: -1 (WaveOut Audio Mapper)",
                v => _deviceId = v);
            HasOption("fileType|t=",
                "The file provider module to use.",
                v =>
                {
                    _fileType = _audioSubsystem.GetAudioProvider(v);
                    if (_fileType == null)
                    {
                        throw new ConsoleHelpAsException("No audio provider module found.");
                    }
                });
            HasOption("wait|w", "Waits for the audio to finish.", v => _wait = true);
            HasOption("sampleRate|r=", "When in raw format, sets the sample rate of playback. Default is 44100 hz.",
                v =>
                {
                    if (int.TryParse(v, out int result))
                    {
                        _sampleRate = result;
                    }
                    else
                    {
                        throw new ConsoleHelpAsException("Sample rate must be a number.");
                    }
                });
            HasOption("bitDepth|b=", "When in raw format, sets the bit depth of audio. Default is 16.", v =>
                {
                    if (int.TryParse(v, out int result))
                    {
                        _bitDepth = result;
                    }
                    else
                    {
                        throw new ConsoleHelpAsException("bit depth must be a number.");
                    }
                });
            HasOption("channels|c=", "When in raw format, sets the number of channels to use for playback.", v =>
            {
                if (int.TryParse(v, out int result))
                {
                    _channels = result;
                }
                else
                {
                    throw new ConsoleHelpAsException("channel must be a number.");
                }
            });
            HasOption("dev:", "Pass configuration options to the device module.",
                (k, v) => { _device.SetProperty(k, v); });
            HasOption("ftModule:", "Pass configuration options to the file type provider module.",
                (k, v) => { _fileType.ConfigurationOptions.Add(k, v); });
            HasAdditionalArguments(1, "The file path to open.");
        }
        
        public override int Run(string[] remainingArguments)
        {
            //Set defaults
            if (_device == null)
            {
                _device = (IAudioDevice)_bus.GetSelectorsByTag("audio").First(c => c.Category == "WaveOut").GetDevice(_deviceId);
            }
            if(_fileType == null) _fileType = _audioSubsystem.GetAudioProvider("MP3");
            string path = remainingArguments[0];
            try
            {
                switch (_pathMode.ToUpper())
                {
                    case "VFS":
                        IFileSystem fileSystem = _currentProc.Extensions.Find<IExtensionFileSystem>().GetFileSystem();
                        _fileStream = fileSystem.OpenFile(path, FileMode.Open, FileAccess.Read);
                        break;
                    case "PHYS":
                        _fileStream = new FileStream(Path.GetFullPath(path), FileMode.Open, FileAccess.Read);
                        break;
                    case "REMOTE_YOUTUBE":
                        _currentProc.Out.WriteLine("Not supported");
                        break;
                    default:
                        throw new ArgumentException("Path mode must be VFS or PHYS.");
                }



                _player = _device.Init(_fileType.OpenAudio(_fileStream, new WaveFormat(_sampleRate, _bitDepth, _channels)));

                _player.PlaybackStopped += (sender, e) =>
                {
                    _player.Dispose();
                    _fileStream.Dispose();
                };
                _player.Play();
                _currentProc.ClientContext.GetExtension<IAudioQueue>().Queue.Add(_player);
                if (_wait)
                {
                    _token.Register(() =>
                    {
                        _player.Stop();
                        _player.Dispose();
                        _fileStream.Dispose();
                    });
                    while (_player.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(1000);
                    }
                }

                return CommandResponse.CODE_SUCCESS;
            }
            catch
            {
                _fileStream.Dispose();
                _player?.Dispose();
                throw;
            }
        }
    }
}