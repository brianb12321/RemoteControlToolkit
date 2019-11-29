using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common;

[assembly: PluginLibrary(null, "CommonPlugin", FriendlyName = "Common Plugin", LibraryType = NetworkSide.Proxy | NetworkSide.Server)]
namespace RemoteControlToolkitCore.Common
{
    public class Application : IHostApplication
    {
        private TcpListener _listener;
        private ILogger<Application> _logger;
        private List<NetworkInstance> _clients;
        private ILogger<NetworkInstance> _instanceLogger;
        private IServiceProvider _provider;

        public Application(ILogger<Application> logger, ILogger<NetworkInstance> instanceLogger, IServiceProvider provider)
        {
            _logger = logger;
            _clients = new List<NetworkInstance>();
            _instanceLogger = instanceLogger;
            _provider = provider;
        }
        public void Run()
        {
            _listener = new TcpListener(IPAddress.Any, 8080);
            _logger.LogInformation("Starting listener.");
            _listener.Start();
            while (true)
            {
                _logger.LogInformation("Waiting for client connection.");
                TcpClient client = _listener.AcceptTcpClient();
                _logger.LogInformation("A client established a connection.");
                NetworkInstance instance = new NetworkInstance(client, _instanceLogger, (IApplicationSubsystem)_provider.GetService<IPluginSubsystem<IApplication>>(), _provider.GetServices<IInstanceExtensionProvider>().ToArray());
                _clients.Add(instance);
                instance.Start();
            }
        }

        public NetworkSide ExecutingSide { get; } = NetworkSide.Server;

        public void Dispose()
        {
            
        }
    }
}