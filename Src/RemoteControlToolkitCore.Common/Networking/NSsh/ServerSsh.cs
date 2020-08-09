using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NDesk.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Configuration;
using RemoteControlToolkitCore.Common.Networking.NSsh.Configuration;
using RemoteControlToolkitCore.Common.Networking.NSsh.Services;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Networking.NSsh
{
    [Plugin(PluginName = "server-ssh")]
    public class ServerSsh : RCTApplication
    {
        private ILogger<ServerSsh> _logger;
        private ILogger<ProxyNetworkInstance> _proxyLogger;
        private IServiceProvider _provider;
        private Thread _clientThread;
        private bool _proxyMode;
        private string _proxyAddress = string.Empty;
        private int _proxyPort = 8080;
        private NSshServiceConfiguration _config;
        private bool _shutdown;
        private readonly List<TcpListener> _listenSockets = new List<TcpListener>();
        private readonly Dictionary<ISshSession, Thread> _sessions = new Dictionary<ISshSession, Thread>();
        private long _connectionsReceived;
        public override string ProcessName => "Server SSH";
        public override void InitializeServices(IServiceProvider provider)
        {
            _config = provider.GetService<IWritableOptions<NSshServiceConfiguration>>().Value;
            _logger = provider.GetService<ILogger<ServerSsh>>();
            _proxyLogger = provider.GetService<ILogger<ProxyNetworkInstance>>();
            provider.GetService<IKeySetupService>().EnsureSetup();
            _provider = provider;
        }

        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
        {
            bool displayHelp = false;
            context.EventBus.Subscribe(new Action<UnRegisterSessionEvent>(e => UnRegisterSession(e.Session)));
            _shutdown = false;
            OptionSet options = new OptionSet()
                .Add("p|proxy", "Connect to a proxy server.", v => _proxyMode = true)
                .Add("proxyAddress|a=", "The address to connect to.", v => _proxyAddress = v)
                .Add("proxyPort|o=", "The port to connect to.", v => _proxyPort = int.Parse(v))
                .Add("help|?", "Displays the help screen.", v => displayHelp = true);

            options.Parse(args.Arguments);
            if (displayHelp)
            {
                options.WriteOptionDescriptions(context.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
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
                    handleConnections(_config.ListenEndPoints[0], token);
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
                    return new CommandResponse(CommandResponse.CODE_SUCCESS);
                }
                catch (ThreadStateException ex)
                {
                    _logger.LogError($"Unable to start client thread: {ex.Message}");
                    return new CommandResponse(CommandResponse.CODE_FAILURE);
                }
            }
            return new CommandResponse(CommandResponse.CODE_SUCCESS);
        }

        private void handleConnections(object endPointObject, CancellationToken token)
        {
            TcpListener socket = null;
            var address = (NSshServiceConfiguration.IPSetting)endPointObject;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(address.IPAddress), address.Port);

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

                while (!token.IsCancellationRequested)
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

        public override void Dispose()
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