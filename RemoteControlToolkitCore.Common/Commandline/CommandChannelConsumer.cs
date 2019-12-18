using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer.Console;
using RemoteControlToolkitCore.Common.NSsh.Services;
using RemoteControlToolkitCore.Common.Utilities;

namespace RemoteControlToolkitCore.Common.Commandline
{
    public class CommandChannelConsumer : BaseConsoleChannelConsumer, IChannelCommandConsumer
    {
        private readonly IImpersonationProvider _provider;
        private ILogger<BaseProcessConsole> _logger;
        private IApplicationSubsystem _subsystem;
        private IInstanceExtensionProvider[] _providers;
        public CommandChannelConsumer(ILogger<BaseConsoleChannelConsumer> logger, IImpersonationProvider provider, ILogger<BaseProcessConsole> consoleLogger, IApplicationSubsystem subsystem, IServiceProvider serviceProvider) : base(logger)
        {
            _provider = provider;
            _logger = consoleLogger;
            _subsystem = subsystem;
            _provider = provider;
            _providers = serviceProvider.GetServices<IInstanceExtensionProvider>().ToArray();
        }
        protected override IConsole CreateConsole()
        {
            return new BaseProcessConsole(_logger, _subsystem, _providers, Channel, InitialTerminalConfiguration, InitialEnvironmentVariables);
        }

        public string Command { get; set; }
    }
}
