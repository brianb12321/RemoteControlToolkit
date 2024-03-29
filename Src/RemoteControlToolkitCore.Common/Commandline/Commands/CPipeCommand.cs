﻿using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Crayon.Output;
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
        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
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

            options.Parse(args.Arguments);
            if (mode == "help")
            {
                options.WriteOptionDescriptions(context.Out);
            }
            else if (mode == "createNamed")
            {
                context.Out.WriteLine($"Creating named pipe with name \"{name}\'");
                var (position, stream) = _pipeService.OpenNamedPipe(PipeDirection.InOut, name);
                context.Out.WriteLine(Green($"Pipe successfully created at position {position}!"));
                stream.WaitForConnectionAsync(token);
            }
            else if (mode == "createAnonymous")
            {
                context.Out.WriteLine("Creating anonymous pipe.");
                var (position, stream) = _pipeService.OpenAnonymousPipe(PipeDirection.In);
                context.Out.WriteLine(Green($"Pipe successfully created with handle \"{stream.GetClientHandleAsString()}\", position {position}!"));
            }
            else if (mode == "closeAnonymous")
            {
                context.Out.WriteLine("Closing anonymous pipe.");
                _pipeService.CloseAnonymousServerPipe(int.Parse(name));
                context.Out.WriteLine(Green($"Successfully closed anonymous pipe {name}."));
            }
            else if (mode == "closeNamed")
            {
                context.Out.WriteLine("Closing named pipe.");
                _pipeService.CloseNamedServerPipe(int.Parse(name));
                context.Out.WriteLine(Green($"Successfully closed named pipe {name}."));
            }
            else if (mode == "closeClientAnonymous")
            {
                context.Out.WriteLine("Closing client anonymous pipe.");
                _pipeService.DisconnectAnonymousClientPipe(int.Parse(name));
                context.Out.WriteLine(Green($"Successfully closed anonymous client pipe {name}."));
            }
            else if (mode == "closeClientNamed")
            {
                context.Out.WriteLine("Closing client named pipe.");
                _pipeService.DisconnectNamedClientPipe(int.Parse(name));
                context.Out.WriteLine(Green($"Successfully closed named client pipe {name}."));
            }
            else if (mode == "connectAnonymous")
            {
                context.Out.WriteLine("Connecting to anonymous pipe.");
                var (position, _) = _pipeService.ConnectToPipe(name, direction);
                context.Out.WriteLine(Green($"Successfully connected to anonymous pipe. Pipe index = {position}!"));
            }
            else if (mode == "connectNamed")
            {
                context.Out.WriteLine("Connecting to named pipe.");
                var (position, _) = _pipeService.ConnectToNamedPipe(name, pipeName, direction, timeout);
                context.Out.WriteLine(Green($"Successfully connected to named pipe. Pipe Index = {position}"));
            }
            return new CommandResponse(CommandResponse.CODE_SUCCESS);
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            _pipeService = kernel.GetService<IPipeService>();
        }
    }
}
