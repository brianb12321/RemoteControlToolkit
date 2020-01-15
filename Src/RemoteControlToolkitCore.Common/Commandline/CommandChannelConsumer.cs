using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer.Console;
using RemoteControlToolkitCore.Common.NSsh.Services;
using RemoteControlToolkitCore.Common.Utilities;
using RemoteControlToolkitCore.Common.VirtualFileSystem;

namespace RemoteControlToolkitCore.Common.Commandline
{
    public class CommandChannelConsumer : BaseConsoleChannelConsumer, IChannelCommandConsumer
    {
        private readonly IImpersonationProvider _provider;
        private ILogger<BaseProcessConsole> _logger;
        private IApplicationSubsystem _subsystem;
        private IExtensionProvider<IInstanceSession>[] _providers;
        private IFileSystemSubsystem _fileSystemSubsystem;
        private IServiceProvider _serviceProvider;
        public CommandChannelConsumer(ILogger<BaseConsoleChannelConsumer> logger, IImpersonationProvider provider, IFileSystemSubsystem fileSystemSubsystem, ILogger<BaseProcessConsole> consoleLogger, IApplicationSubsystem subsystem, IServiceProvider serviceProvider) : base(logger)
        {
            _fileSystemSubsystem = fileSystemSubsystem;
            _provider = provider;
            _logger = consoleLogger;
            _subsystem = subsystem;
            _provider = provider;
            _serviceProvider = serviceProvider;
            _providers = serviceProvider.GetServices<IExtensionProvider<IInstanceSession>>().ToArray();
        }
        protected override IConsole CreateConsole()
        {
            var principal = new ClaimsPrincipal(AuthenticatedIdentity);
            ClaimsIdentity identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
            principal.AddIdentity(identity);
            return new BaseProcessConsole(_logger, _subsystem, _providers, _fileSystemSubsystem, Channel, InitialTerminalConfiguration, InitialEnvironmentVariables, _serviceProvider, principal);
        }

        public string Command { get; set; }
    }
}
