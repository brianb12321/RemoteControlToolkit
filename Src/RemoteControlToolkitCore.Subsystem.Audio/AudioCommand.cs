using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using AudioSwitcher.AudioApi.CoreAudio;
using Crayon;
using ManyConsole;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using Mono.Options;
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
using RemoteControlToolkitCore.Subsystem.Audio.Commands;
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
        private AudioOutSubsystem _audioSubsystem;
        private DeviceBusSubsystem _bus;

        public override string ProcessName => "Audio Command";

        public override CommandResponse Execute(CommandRequest args, RctProcess currentProc, CancellationToken token)
        {
            return new CommandResponse(ConsoleCommandDispatcher.DispatchCommand(new List<ConsoleCommand>
            {
                new PlayCommand(_audioSubsystem, _bus, currentProc, token),
                new StopCommand(currentProc),
                new PauseCommand(currentProc),
                new ResumeCommand(currentProc),
                new VolumeCommand(),
                new ShowAllDeviceCommand(currentProc, _bus),
                new ShowAllProvidersCommand(currentProc, _audioSubsystem)
            }, args.Arguments.Skip(1).ToArray(), currentProc.Out, true));
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            _audioSubsystem = kernel.GetService<AudioOutSubsystem>();
            _bus = kernel.GetService<DeviceBusSubsystem>();
        }
    }
}