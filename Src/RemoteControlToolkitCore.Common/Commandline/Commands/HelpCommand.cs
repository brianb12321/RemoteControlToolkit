using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Utilities;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [PluginModule(Name = "help", ExecutingSide = NetworkSide.Server | NetworkSide.Proxy)]
    [CommandHelp("Shows all the installed applications.")]
    public class HelpCommand : RCTApplication
    {
        private IApplicationSubsystem _appSubsystem;
        private IHostApplication _nodeApplication;

        public override string ProcessName => "Help Command";

        public override CommandResponse Execute(CommandRequest args, RCTProcess currentProc, CancellationToken token)
        {
            Dictionary<string, string> _helpItems = new Dictionary<string, string>();
            var serverData = Assembly.GetEntryAssembly()?.GetName();
            currentProc.Out.WriteLine($"{serverData.Name} [Version: {serverData.Version}]");
            currentProc.Out.WriteLine();
            foreach (var apps in _appSubsystem.GetModuleTypes())
            {
                if (apps.GetCustomAttribute<CommandHelpAttribute>() != null)
                {
                    var helpClass = apps.GetCustomAttribute<CommandHelpAttribute>();
                    var moduleName = apps.GetCustomAttribute<PluginModuleAttribute>();
                    if (moduleName.ExecutingSide.HasFlag(_nodeApplication.ExecutingSide) && !_helpItems.ContainsKey(moduleName.Name))
                    {
                        _helpItems.Add(moduleName.Name, helpClass.Help);
                    }
                }
            }
            currentProc.Out.WriteLine(_helpItems.ShowDictionary());
            return new CommandResponse(CommandResponse.CODE_SUCCESS);
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            _appSubsystem = kernel.GetService<IApplicationSubsystem>();
            _nodeApplication = kernel.GetService<IHostApplication>();
        }
    }
}