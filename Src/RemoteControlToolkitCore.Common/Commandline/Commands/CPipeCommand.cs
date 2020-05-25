using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crayon;
using Microsoft.Extensions.DependencyInjection;
using NDesk.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [Plugin(PluginName = "cpipe")]
    [CommandHelp("Creates and manages operating system pipes.")]
    public class CPipeCommand : RCTApplication
    {
        private IPipeService _pipeService;
        public override string ProcessName => "CPipe";
        public override CommandResponse Execute(CommandRequest args, RCTProcess context, CancellationToken token)
        {
            string mode = "help";
            string name = string.Empty;
            string pipeName = "RCTPipe";
            int timeout = 30000;
            PipeDirection direction = PipeDirection.InOut;
            OptionSet options = new OptionSet()
                .Add("createNamed=", "Creates a new named operating system pipe.", v =>
                {
                    mode = "createNamed";
                    name = v;
                })
                .Add("createAnonymous", "Creates an anonymous operating system pipe.", v =>
                {
                    mode = "createAnonymous";
                })
                .Add("closeNamed=", "Closes the named pipe using its index number.", v =>
                {
                    mode = "closeNamed";
                    name = v;
                })
                .Add("closeAnonymous=", "Closes an anonymous pipe using its index number.", v =>
                {
                    mode = "closeAnonymous";
                    name = v;
                })
                .Add("closeClientAnonymous=", "Closes a client anonymous pipe using its index number.", v =>
                {
                    mode = "closeClientAnonymous";
                    name = v;
                })
                .Add("closeClientNamed=", "Closes a client named pipe using its index number.", v =>
                {
                    mode = "closeClientNamed";
                    name = v;
                })
                .Add("connectAnonymous=", "Connect to an anonymous pipe with a pipe handle.", v =>
                {
                    mode = "connectAnonymous";
                    name = v;
                })
                .Add("connectNamed=", "Connect to a named pipe with the specified host.", v =>
                {
                    mode = "connectNamed";
                    name = v;
                })
                .Add("pipeName=", "The name of the server pipe to connect to when connecting to a named pipe.", v => pipeName = v)
                .Add("timeout|t=", "The timeout in milliseconds to wait for a named pipe connection.", v => timeout = int.Parse(v))
                .Add("direction|d=", "The direction of pipe communication. Can be In, Out, InOut", v =>
                {
                    direction = (PipeDirection)Enum.Parse(typeof(PipeDirection), v, true);
                })
                .Add("help|?", "Displays the help screen.", v => mode = "help");

            options.Parse(args.Arguments.Select(a => a.ToString()));
            if (mode == "help")
            {
                options.WriteOptionDescriptions(context.Out);
            }
            else if (mode == "createNamed")
            {
                context.Out.WriteLine($"Creating named pipe with name \"{name}\'");
                var pipe = _pipeService.OpenNamedPipe(PipeDirection.InOut, name);
                context.Out.WriteLine($"Pipe successfully created at position {pipe.position}!".BrightGreen());
                pipe.stream.WaitForConnectionAsync();
            }
            else if (mode == "createAnonymous")
            {
                context.Out.WriteLine("Creating anonymous pipe.");
                var pipe = _pipeService.OpenAnonymousPipe(PipeDirection.In);
                context.Out.WriteLine($"Pipe successfully created with handle \"{pipe.stream.GetClientHandleAsString()}\", position {pipe.position}!".BrightGreen());
            }
            else if (mode == "closeAnonymous")
            {
                context.Out.WriteLine("Closing anonymous pipe.");
                _pipeService.CloseAnonymousServerPipe(int.Parse(name));
                context.Out.WriteLine($"Successfully closed anonymous pipe {name}.".BrightGreen());
            }
            else if (mode == "closeNamed")
            {
                context.Out.WriteLine("Closing named pipe.");
                _pipeService.CloseNamedServerPipe(int.Parse(name));
                context.Out.WriteLine($"Successfully closed named pipe {name}.".BrightGreen());
            }
            else if (mode == "closeClientAnonymous")
            {
                context.Out.WriteLine("Closing client anonymous pipe.");
                _pipeService.DisconnectAnonymousClientPipe(int.Parse(name));
                context.Out.WriteLine($"Successfully closed anonymous client pipe {name}.".BrightGreen());
            }
            else if (mode == "closeClientNamed")
            {
                context.Out.WriteLine("Closing client named pipe.");
                _pipeService.DisconnectNamedClientPipe(int.Parse(name));
                context.Out.WriteLine($"Successfully closed named client pipe {name}.".BrightGreen());
            }
            else if (mode == "connectAnonymous")
            {
                context.Out.WriteLine("Connecting to anonymous pipe.");
                var pipe = _pipeService.ConnectToPipe(name, direction);
                context.Out.WriteLine($"Successfully connected to anonymous pipe. Pipe index = {pipe.position}!".BrightGreen());
            }
            else if (mode == "connectNamed")
            {
                context.Out.WriteLine("Connecting to named pipe.");
                var pipe = _pipeService.ConnectToNamedPipe(name, pipeName, direction, timeout);
                context.Out.WriteLine($"Successfully connected to named pipe. Pipe Index = {pipe.position}".BrightGreen());
            }
            return new CommandResponse(CommandResponse.CODE_SUCCESS);
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            _pipeService = kernel.GetService<IPipeService>();
        }
    }
}
