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
using NDesk.Options;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Proxy;

[assembly: PluginLibrary(null, "CommonPlugin", FriendlyName = "Common Plugin", LibraryType = NetworkSide.Proxy | NetworkSide.Server)]
namespace RemoteControlToolkitCore.Common
{
    public class Application : IHostApplication
    {
        private TcpListener _listener;
        private TcpListener _proxyListener;
        private ILogger<Application> _logger;
        private List<NetworkInstance> _clients;
        private IServerPool _proxyClients;
        private ILogger<NetworkInstance> _instanceLogger;
        private ILogger<ProxyNetworkInstance> _proxyLogger;
        private IServiceProvider _provider;
        private Thread _clientThread;
        private Thread _proxyThread;
        private bool proxyMode = false;
        private string proxyAddress = string.Empty;
        private int proxyPort = 8080;

        public Application(ILogger<Application> logger, ILogger<ProxyNetworkInstance> proxyLogger, ILogger<NetworkInstance> instanceLogger, IServiceProvider provider, IServerPool pool)
        {
            _logger = logger;
            _clients = new List<NetworkInstance>();
            _instanceLogger = instanceLogger;
            _proxyLogger = proxyLogger;
            _provider = provider;
            _proxyClients = pool;
        }
        public void Run(string[] args)
        {
            OptionSet options = new OptionSet()
                .Add("p|proxy", "Connect to a proxy server.", v => proxyMode = true)
                .Add("proxyAddress|a=", "The address to connect to.", v => proxyAddress = v)
                .Add("proxyPort|o=", "The port to connect to.", v => proxyPort = int.Parse(v));

            options.Parse(args);
            if (proxyMode)
            {
                
            }
            else
            {
                _clientThread = new Thread(() =>
                {
                    _listener = new TcpListener(IPAddress.Any, 8081);
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
                });
                _proxyThread = new Thread(() =>
                {
                    _proxyListener = new TcpListener(IPAddress.Any, 8080);
                    _logger.LogInformation("Starting proxy listener.");
                    _proxyListener.Start();
                    while (true)
                    {
                        TcpClient client = _proxyListener.AcceptTcpClient();
                        _logger.LogInformation("A proxy client established a connection.");
                        ProxyNetworkInstance instance = new ProxyNetworkInstance(client, _proxyLogger, (IApplicationSubsystem)_provider.GetService<IPluginSubsystem<IApplication>>(), _proxyClients);
                        _proxyClients.AddServer(instance);
                    }
                });
                _proxyThread.Start();
                _clientThread.Start();
                _clientThread.Join();
            }
        }

        public NetworkSide ExecutingSide { get; } = NetworkSide.Server;

        public void Dispose()
        {
            
        }
    }
}