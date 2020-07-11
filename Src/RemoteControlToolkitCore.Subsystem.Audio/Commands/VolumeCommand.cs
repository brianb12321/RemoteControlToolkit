using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioSwitcher.AudioApi.CoreAudio;
using ManyConsole;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;

namespace RemoteControlToolkitCore.Subsystem.Audio.Commands
{
    public class VolumeCommand : ConsoleCommand
    {
        private Guid _guid;
        public VolumeCommand()
        {
            IsCommand("setVolume", "Sets the volume for the system.");
            HasAdditionalArguments(1, "The volume to set to.");
            HasOption("guid|g=", "The device guid to change.", v =>
            {
                if (Guid.TryParse(v, out Guid result))
                {
                    _guid = result;
                }
                else throw new ConsoleHelpAsException("Invalid guid.");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            
            CoreAudioDevice defaultPlaybackDevice;
            if (_guid != Guid.Empty) defaultPlaybackDevice = new CoreAudioController().GetDevice(_guid);
            else defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
            defaultPlaybackDevice.Volume = int.Parse(remainingArguments[0]);
            return CommandResponse.CODE_SUCCESS;
        }
    }
}