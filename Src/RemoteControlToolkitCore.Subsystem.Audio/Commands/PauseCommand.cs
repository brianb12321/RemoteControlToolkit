using ManyConsole;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;

namespace RemoteControlToolkitCore.Subsystem.Audio.Commands
{
    public class PauseCommand : ConsoleCommand
    {
        private readonly RctProcess _currentProc;

        public PauseCommand(RctProcess currentProc)
        {
            IsCommand("pause", "Pauses current playback.");
            _currentProc = currentProc;
        }
        public override int Run(string[] remainingArguments)
        {
            foreach (var audio in _currentProc.ClientContext.GetExtension<IAudioQueue>().Queue)
            {
                audio.Pause();
            }

            return CommandResponse.CODE_SUCCESS;
        }
    }
}