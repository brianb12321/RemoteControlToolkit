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
    public class StopCommand : ConsoleCommand
    {
        private readonly RctProcess _currentProc;

        public StopCommand(RctProcess currentProc)
        {
            IsCommand("stop", "Stops current playback");
            _currentProc = currentProc;
        }
        public override int Run(string[] remainingArguments)
        {
            _currentProc.ClientContext.GetExtension<IAudioQueue>().StopAll();
            return CommandResponse.CODE_SUCCESS;
        }
    }
}