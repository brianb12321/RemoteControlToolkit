using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [Plugin(PluginName = "echo")]
    [CommandHelp("Writes the provided arguments into standard out.")]
    public class EchoCommand : RCTApplication
    {
        public override string ProcessName => "Echo Command";

        public override CommandResponse Execute(CommandRequest args, RctProcess currentProc, CancellationToken token)
        {
            //Check if StdIn has data.
            if (currentProc.In.Peek() != -1)
            {
                currentProc.Out.WriteLine(currentProc.In.ReadToEnd());
            }
            else
            {
                if (args.Arguments.Length > 1)
                {
                    for (int i = 1; i < args.Arguments.Length; i++)
                    {
                        currentProc.Out.WriteLine(args.Arguments[i]);
                    }
                }
            }
            return new CommandResponse(CommandResponse.CODE_SUCCESS);
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            
        }
    }
}