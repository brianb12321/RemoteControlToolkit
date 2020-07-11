using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManyConsole;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.DeviceBus;
using RemoteControlToolkitCore.Common.Utilities;

namespace RemoteControlToolkitCore.Subsystem.Audio.Commands
{
    public class ShowAllDeviceCommand : ConsoleCommand
    {
        private readonly RctProcess _currentProc;
        private readonly DeviceBusSubsystem _subsystem;

        public ShowAllDeviceCommand(RctProcess currentProc, DeviceBusSubsystem subsystem)
        {
            IsCommand("showAllDevices", "Displays all registered audio devices.");
            _currentProc = currentProc;
            _subsystem = subsystem;
        }
        public override int Run(string[] remainingArguments)
        {
            IDeviceSelector[] deviceInfo = _subsystem.GetSelectorsByTag("audio");
            foreach (IDeviceSelector info in deviceInfo)
            {
                _currentProc.Out.WriteLine(info.Category);
                _currentProc.Out.WriteLine("=========================================================");
                _currentProc.Out.WriteLine(info.GetDevicesInfo().ToDictionary(k => k.FileName).ShowDictionary(v => v.Name));
            }
            return CommandResponse.CODE_SUCCESS;
        }
    }
}