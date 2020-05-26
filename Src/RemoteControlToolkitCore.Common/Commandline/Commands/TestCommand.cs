using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [Plugin(PluginName = "test")]
    [CommandHelp("Does testy things.")]
    public class TestCommand : RCTApplication
    {
        public override string ProcessName => "Test";
        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
        {
            context.Out.Write("Do you like cookies? ");
            char answer = (char)context.In.Read();
            context.Out.WriteLine();
            if(answer == 'y') context.Out.WriteLine("Awesome!");
            else context.Out.WriteLine(":(");
            return new CommandResponse(CommandResponse.CODE_SUCCESS);
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            
        }
    }
}