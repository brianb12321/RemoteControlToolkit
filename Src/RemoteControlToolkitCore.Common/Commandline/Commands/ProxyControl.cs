using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using NDesk.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Proxy;
using RemoteControlToolkitCore.Common.Utilities;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [Plugin(PluginName = "proxyControl")]
    [CommandHelp("Manages the connected servers on the proxy.")]
    public class ProxyControl : RCTApplication
    {
        private IServerPool _pool;
        public override string ProcessName => "Proxy Control";

        public override CommandResponse Execute(CommandRequest args, RCTProcess currentProc, CancellationToken token)
        {
            string mode = string.Empty;
            OptionSet options = new OptionSet()
                .Add("viewServers", "Views all the servers connected to the proxy server.", v => mode = "viewServers")
                .Add("closeAll", "Closes all proxy connections and clears the proxy list.", v => mode = "closeAll")
                .Add("help|?", "Displays the help screen.", v => mode = "help");

            options.Parse(args.Arguments.Select(a => a.ToString()));
            if (mode == "help")
            {
                options.WriteOptionDescriptions(currentProc.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else if (mode == "closeAll")
            {
                _pool.Clean();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else if (mode == "viewServers")
            {
                currentProc.Out.WriteLine(_pool.GetServersInt().ShowDictionary());
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else
            {
                options.WriteOptionDescriptions(currentProc.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            _pool = kernel.GetService<IServerPool>();
        }
    }
}