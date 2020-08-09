using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NDesk.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.ApplicationSystem.Factory;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Utilities;
using RemoteControlToolkitCore.Common.VirtualFileSystem;

namespace RemoteControlToolkitCore.Common.Networking.Raw
{
    [Plugin(PluginName = "server-raw")]
    public class ServerRawCommand : RCTApplication
    {
        public override string ProcessName => "Server Raw";
        private ProcessFactorySubsystem _factory;
        private IHostApplication _application;
        private ITerminalHandlerFactory _terminalHandlerFactory;
        private ILogger<ServerRawCommand> _logger;
        private ILogger<Connection> _connectionLogger;
        private FileSystemSubsystem _fileSystemSubsystem;
        private IExtensionProvider<IInstanceSession>[] _extensions;

        public override void InitializeServices(IServiceProvider provider)
        {
            _application = provider.GetService<IHostApplication>();
            _logger = provider.GetService<ILogger<ServerRawCommand>>();
            _factory = provider.GetService<ProcessFactorySubsystem>();
            _connectionLogger = provider.GetService<ILogger<Connection>>();
            _terminalHandlerFactory = provider.GetService<ITerminalHandlerFactory>();
            _extensions = provider.GetServices<IExtensionProvider<IInstanceSession>>().ToArray();
            _fileSystemSubsystem = provider.GetService<FileSystemSubsystem>();
        }

        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
        {
            int port = 8080;
            bool displayHelp = false;
            OptionSet options = new OptionSet()
                .Add("help|?", "Display the help screen.", v => displayHelp = true)
                .Add("port|p=", "Specify an alternative port for listening. (Default: 8080)", v => port = int.Parse(v));

            options.Parse(args.Arguments);
            if (displayHelp)
            {
                options.WriteOptionDescriptions(context.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            List<Connection> connections = new List<Connection>();
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            _logger.LogInformation("Opening TCP listener.");
            token.Register(() => listener.Stop());
            listener.Start();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Connection connection = new Connection(client.GetStream(), _terminalHandlerFactory, _factory, _connectionLogger, _extensions, _fileSystemSubsystem);
                    connections.Add(connection);
                    connection.Thread.Start();
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.Interrupted) throw;
                }
            }
            return new CommandResponse(CommandResponse.CODE_SUCCESS);

        }
        private class Connection : IInstanceSession
        {
            public Thread Thread { get; }
            private NetworkStream NetworkStream { get; }
            public IExtensionCollection<IInstanceSession> Extensions { get; }
            private ITerminalHandler _handler;
            public IProcessTable ProcessTable { get; }
            public Guid ClientUniqueID { get; }
            public string Username { get; }


            public Connection(NetworkStream stream,
                ITerminalHandlerFactory factory,
                ProcessFactorySubsystem processFactory,
                ILogger<Connection> logger,
                IExtensionProvider<IInstanceSession>[] extensions,
                FileSystemSubsystem fileSystemSubsystem)
            {
                NetworkStream = stream;
                ProcessTable = new ProcessTable();
                Extensions = new ExtensionCollection<IInstanceSession>(this);
                Thread = new Thread(() =>
                {
                    _handler =
                        factory.CreateNewTerminalHandler("xterm", stream, stream);
                    var identity = new GenericIdentity("bob");
                    var principal = new ClaimsPrincipal(identity);
                    identity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
                    principal.AddIdentity(identity);
                    RctProcess shellProcess = processFactory.GetProcessBuilder("Application", null, ProcessTable)
                        .SetInstanceSession(this)
                        .SetSecurityPrincipal(principal)
                        .Build();

                    shellProcess.CommandLineName = "shell";
                    shellProcess.SetOut(stream);
                    shellProcess.SetError(stream);
                    shellProcess.SetIn(new ConsoleTextReader(_handler), stream);
                    Extensions.Add(_handler);
                    shellProcess.EnvironmentVariables.AddVariable("PROXY_MODE", "false");
                    shellProcess.EnvironmentVariables.AddVariable("?", "0");
                    shellProcess.EnvironmentVariables.AddVariable("TERM", _handler.TerminalName);
                    shellProcess.EnvironmentVariables.AddVariable("WORKINGDIR", "/");
                    shellProcess.Extensions.Add(new ExtensionFileSystem(fileSystemSubsystem.GetFileSystem()));
                    shellProcess.ThreadError += (sender, e) =>
                        logger.LogError($"Thread [{Thread.ManagedThreadId}] encountered an unhandled error: {e.Message}");
                    shellProcess.Start();
                    shellProcess.WaitForExit();
                    Close();
                });
                foreach (IExtensionProvider<IInstanceSession> extensionProvider in extensions)
                {
                    extensionProvider.GetExtension(this);
                }
            }

            public StreamReader GetClientReader()
            {
                return new StreamReader(NetworkStream, Encoding.UTF8, false, 1, true);
            }

            public Stream OpenNetworkStream()
            {
                return NetworkStream;
            }

            public StreamWriter GetClientWriter()
            {
                return new StreamWriter(NetworkStream, Encoding.UTF8, 1, true)
                {
                    AutoFlush = true
                };
            }

            public T GetExtension<T>() where T : IExtension<IInstanceSession>
            {
                return Extensions.Find<T>();
            }

            public void AddExtension<T>(T extension) where T : IExtension<IInstanceSession>
            {
                Extensions.Add(extension);
            }

            public void Close()
            {
                NetworkStream.Close();
            }
        }
    }
}