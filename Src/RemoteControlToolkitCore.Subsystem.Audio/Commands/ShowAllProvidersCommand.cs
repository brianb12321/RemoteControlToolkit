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
    public class ShowAllProvidersCommand : ConsoleCommand
    {
        private readonly RctProcess _currentProc;
        private readonly AudioOutSubsystem _subsystem;

        public ShowAllProvidersCommand(RctProcess currentProc, AudioOutSubsystem subsystem)
        {
            IsCommand("showAllProviders", "Shows all audio providers.");
            _currentProc = currentProc;
            _subsystem = subsystem;
        }
        public override int Run(string[] remainingArguments)
        {
            StringBuilder sb = new StringBuilder();
            IAudioProviderModule[] providers = _subsystem.GetAllAudioProviders();
            int max = providers.Max(p => p.GetPluginAttribute().PluginName.Length) + 5;
            _currentProc.Out.WriteLine("Installed Audio providers.");
            _currentProc.Out.WriteLine("================================================");
            foreach (var provider in providers)
            {
                sb.Append(provider.GetPluginAttribute().PluginName.PadRight(max)).AppendLine(provider.Description);
            }
            _currentProc.Out.WriteLine(sb.ToString());
            return CommandResponse.CODE_SUCCESS;
        }
    }
}