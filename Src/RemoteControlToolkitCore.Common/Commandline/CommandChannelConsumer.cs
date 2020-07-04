using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.ApplicationSystem.Factory;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer.Console;
using RemoteControlToolkitCore.Common.NSsh.Services;
using RemoteControlToolkitCore.Common.Utilities;
using RemoteControlToolkitCore.Common.VirtualFileSystem;

namespace RemoteControlToolkitCore.Common.Commandline
{
    public class CommandChannelConsumer : BaseConsoleChannelConsumer, IChannelCommandConsumer
    {
        private readonly ILogger<BaseProcessConsole> _logger;
        private readonly ILogger<TerminalHandler> _terminalLogger;
        private readonly ProcessFactorySubsystem _subsystem;
        private readonly IExtensionProvider<IInstanceSession>[] _providers;
        private readonly FileSystemSubsystem _fileSystemSubsystem;
        private readonly ITerminalHandlerFactory _terminalHandlerFactory;
        public CommandChannelConsumer(ILogger<BaseConsoleChannelConsumer> logger, ILogger<TerminalHandler> terminalLogger, IImpersonationProvider provider, FileSystemSubsystem fileSystemSubsystem, ILogger<BaseProcessConsole> consoleLogger, ProcessFactorySubsystem subsystem, IServiceProvider serviceProvider) : base(logger)
        {
            _fileSystemSubsystem = fileSystemSubsystem;
            _logger = consoleLogger;
            _terminalLogger = terminalLogger;
            _subsystem = subsystem;
            _providers = serviceProvider.GetServices<IExtensionProvider<IInstanceSession>>().ToArray();
            _terminalHandlerFactory = serviceProvider.GetService<ITerminalHandlerFactory>();
        }
        protected override IConsole CreateConsole()
        {
            var principal = new ClaimsPrincipal(AuthenticatedIdentity);
            ClaimsIdentity identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
            principal.AddIdentity(identity);
            return new BaseProcessConsole(_logger, _subsystem, _providers, _fileSystemSubsystem, Channel, InitialTerminalConfiguration, InitialEnvironmentVariables, principal, _terminalLogger, _terminalHandlerFactory);
        }

        public string Command { get; set; }
    }
}
