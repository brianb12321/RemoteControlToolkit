using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;
using Microsoft.Extensions.DependencyInjection;
using NDesk.Options;
using RemoteControlToolkitCore.Common.NSsh;
using RemoteControlToolkitCore.Common.NSsh.Configuration;
using RemoteControlToolkitCore.Common.NSsh.Services;

[assembly: PluginLibrary("CommonPlugin","Common Plugin")]
namespace RemoteControlToolkitCore.Common
{
    public class Application : IHostApplication
    {
        private readonly ILogger<Application> _logger;
        private readonly ILogger<ProxyNetworkInstance> _proxyLogger;
        private readonly IServiceProvider _provider;
        private Thread _clientThread;
        private bool _proxyMode;
        private string _proxyAddress = string.Empty;
        private int _proxyPort = 8080;
        private readonly NSshServiceConfiguration _config;
        private bool _shutdown;

        public NetworkSide ExecutingSide { get; }
        public IAppBuilder Builder { get; }
        public IPluginManager PluginManager { get; }
        private long _connectionsReceived;
        private readonly List<TcpListener> _listenSockets = new List<TcpListener>();
        private readonly Dictionary<ISshSession, Thread> _sessions = new Dictionary<ISshSession, Thread>();

        public Application(ILogger<Application> logger,
            ILogger<ProxyNetworkInstance> proxyLogger,
            IServiceProvider provider,
            NetworkSide side,
            IAppBuilder builder,
            IKeySetupService keySetup,
            IPluginManager pluginManager,
            NSshServiceConfiguration config)
        {
            _logger = logger;
            _proxyLogger = proxyLogger;
            _provider = provider;
            ExecutingSide = side;
            Builder = builder;
            _config = config;
            PluginManager = pluginManager;
            keySetup.EnsureSetup();
        }
        public void Run(string[] args)
        {
            _shutdown = false;
            OptionSet options = new OptionSet()
                .Add("p|proxy", "Connect to a proxy server.", v => _proxyMode = true)
                .Add("proxyAddress|a=", "The address to connect to.", v => _proxyAddress = v)
                .Add("proxyPort|o=", "The port to connect to.", v => _proxyPort = int.Parse(v));

            options.Parse(args);
            if (_proxyMode)
            {
                TcpClient client = new TcpClient();
                client.Connect(_proxyAddress, _proxyPort);
                ProxyClient instance = new ProxyClient(client, _provider);
                _proxyLogger.LogInformation("Connected to proxy server.");
                instance.Start();
            }
            else
            {
                _clientThread = new Thread(() =>
                {
                    handleConnections(_config.ListenEndPoints[0]);
                });
                //_proxyThread = new Thread(() =>
                //{
                //    _proxyListener = new TcpListener(IPAddress.Any, 8080);
                //    _logger.LogInformation("Starting proxy listener.");
                //    _proxyListener.Start();
                //    while (true)
                //    {
                //        TcpClient client = _proxyListener.AcceptTcpClient();
                //        _logger.LogInformation("A proxy client established a connection.");
                //        ProxyNetworkInstance instance = new ProxyNetworkInstance(client, _provider);
                //        _proxyClients.AddServer(instance);
                //    }
                //});
                //_proxyThread.Start();
                try
                {
                    _clientThread.Start();
                    _clientThread.Join();
                }
                catch(ThreadStateException ex)
                {
                    _logger.LogError($"Unable to start client thread: {ex.Message}");
                }
            }
        }

        private void handleConnections(object endPointObject)
        {
            TcpListener socket = null;
            IPEndPoint endPoint = (IPEndPoint)endPointObject;

            try
            {
                socket = new TcpListener(endPoint);
                socket.Start();
                _logger.LogInformation("Server started.");
                lock (this)
                {
                    // Register this socket
                    _listenSockets.Add(socket);
                }

                while (true)
                {
                    _logger.LogInformation("Accepting new connections.");
                    Socket client = socket.AcceptSocket();

                    Interlocked.Increment(ref _connectionsReceived);

                    _logger.LogInformation("Connection from " + client.RemoteEndPoint + ". connection count=" + _sessions.Count + ".");

                    lock (this)
                    {
                        // Create a new session and thread to handle this connection
                        ISshSession session = _provider.GetService<ISshSession>();
                        session.ClientSocket = client;
                        session.SocketStream = new NetworkStream(client);

                        int sameIpAddressCount =
                           (from sess in _sessions.Keys
                            where ((IPEndPoint)sess.ClientSocket.RemoteEndPoint).Address.ToString() == ((IPEndPoint)client.RemoteEndPoint).Address.ToString()
                            select sess).Count();

                        if (_sessions.Count >= _config.MaximumClientConnections)
                        {
                            _logger.LogWarning("Rejecting connection from " + client.RemoteEndPoint + " due to maximum client connections of "
                                + _config.MaximumClientConnections + " limit being reached.");

                            Thread rejectSessionThread = new Thread(session.Reject);
                            rejectSessionThread.Start();
                        }
                        else if (sameIpAddressCount >= _config.MaximumSameIPAddressConnections)
                        {
                            _logger.LogWarning("Rejecting connection from " + client.RemoteEndPoint + " due to maximum client connections for "
                              + " same IP address of " + _config.MaximumSameIPAddressConnections + " limit being reached.");

                            Thread rejectSessionThread = new Thread(session.Reject);
                            rejectSessionThread.Start();
                        }
                        else
                        {
                            Thread sessionThread = new Thread(session.Process);
                            RegisterSession(session, sessionThread);
                            sessionThread.Start();
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                // Ignore interrupted exception during shutdown
                if (!_shutdown)
                {
                    _logger.LogWarning(e.Message, e);
                }
            }
            finally
            {
                lock (this)
                {
                    // De-register this socket
                    _listenSockets.Remove(socket);
                }
            }
        }

        public void RegisterSession(ISshSession session, Thread sessionThread)
        {
            lock (this)
            {
                _sessions.Add(session, sessionThread);
            }
        }

        public void UnRegisterSession(ISshSession session)
        {
            lock (this)
            {
                _sessions.Remove(session);
            }
        }

        public void Dispose()
        {
            _shutdown = true;

            lock (this)
            {
                // Kill each of the listening sockets
                foreach (TcpListener listener in _listenSockets)
                {
                    listener.Stop();
                }
                _listenSockets.Clear();
            }

            // wait for sessions threads to finish up
            if (_sessions.Count > 0)
            {
                Thread.Sleep(1000);
            }

            lock (this)
            {
                // non-graceful shutdown of smtp sessions
                foreach (Thread thread in _sessions.Values)
                {
                    thread.Abort();
                }
                _sessions.Clear();
            }
        }
    }
}