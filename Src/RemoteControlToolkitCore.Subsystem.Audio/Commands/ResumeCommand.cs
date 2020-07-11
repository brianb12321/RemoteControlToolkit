using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManyConsole;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;

namespace RemoteControlToolkitCore.Subsystem.Audio.Commands
{
    public class ResumeCommand : ConsoleCommand
    {
        private readonly RctProcess _currentProc;

        public ResumeCommand(RctProcess currentProc)
        {
            IsCommand("resume", "Resumes current playback");
            _currentProc = currentProc;
        }
        public override int Run(string[] remainingArguments)
        {
            foreach (var audio in _currentProc.ClientContext.GetExtension<IAudioQueue>().Queue)
            {
                audio.Play();
            }
            return CommandResponse.CODE_SUCCESS;
        }
    }
}