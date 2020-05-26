using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using NDesk.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Proxy;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [Plugin(PluginName = "remote")]
    [CommandHelp("Opens a remote shell to the selected server.")]
    public class RemoteCommand : RCTApplication
    {
        private IServerPool _serverPool;
        public override string ProcessName => "Remote Command";
        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
        {
            bool showHelp = false;
            string command = string.Empty;
            OptionSet options = new OptionSet
            {
                {"help|?", "Displays the help screen.", v => showHelp = true},
                {"command|c=", "Executes the specified command on the remote server.", v => command = v}
            };
            var server = options.Parse(args.Arguments.Select(a => a.ToString()));
            if (showHelp)
            {
                options.WriteOptionDescriptions(context.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            if (!server.Any()) throw new Exception("You must provide a server to remote into.");
            if (int.TryParse(server.Last(), out int id))
            {
                if (!_serverPool.GetServersInt().ContainsKey(id)) throw new Exception("The provided server id is not registered.");
                context.ControlC += (sender, e) =>
                {
                    e.CloseProcess = false;
                    ((RctProcess)sender).Child.InvokeControlC();
                };
                var selectedServer = _serverPool.GetServers()[id];
                var remoteStreamWriter = selectedServer.GetClientWriter();
                var remoteStreamReader = selectedServer.GetClientReader();
                if (string.IsNullOrEmpty(command))
                {
                    while (true)
                    {
                        context.Out.Write($"remote [{id}]> ");
                        string newCommand = context.In.ReadLine();
                        if (string.IsNullOrWhiteSpace(newCommand)) continue;
                        //Do not send exit to server
                        if (newCommand == "exit") break;
                        remoteStreamWriter.WriteLine(newCommand);
                        string data = string.Empty;
                        while ((data = remoteStreamReader.ReadLine()) != "\u001b]e")
                        {
                            context.Out.WriteLine(data);
                        }
                    }
                }
                else
                {
                    if (command != "exit")
                    {
                        remoteStreamWriter.WriteLine(command);
                        string data = string.Empty;
                        while ((data = remoteStreamReader.ReadLine()) != "\u001b]e")
                        {
                            context.Out.WriteLine(data);
                        }
                    }
                }
                

                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else throw new Exception("Invalid server id.");
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            _serverPool = kernel.GetService<IServerPool>();
        }
    }
}