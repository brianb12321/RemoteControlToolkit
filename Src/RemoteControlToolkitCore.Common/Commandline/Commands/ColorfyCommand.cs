using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crayon;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    /// <summary>
    /// Takes input from StdIn and outputs it in a different color.
    /// </summary>
    [Plugin(PluginName = "colorfy")]
    [CommandHelp("Adds color to inputted text.")]
    public class ColorfyCommand : RCTApplication
    {
        public override string ProcessName => "Colorfy";
        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
        {
            context.Out.Write(context.In.ReadToEnd().Green());
            return new CommandResponse(CommandResponse.CODE_SUCCESS);
        }
    }
}