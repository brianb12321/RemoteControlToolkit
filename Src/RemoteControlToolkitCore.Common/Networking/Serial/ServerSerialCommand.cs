using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
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
using RemoteControlToolkitCore.Common.Networking.Raw;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Utilities;
using RemoteControlToolkitCore.Common.VirtualFileSystem;

namespace RemoteControlToolkitCore.Common.Networking.Serial
{
    [Plugin(PluginName = "server-serial")]
    public class ServerSerialCommand : RCTApplication
    {
        public override string ProcessName => "Server Serial";
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
            string port = "COM1";
            int baudRate = 9600;
            Parity parity = Parity.None;
            int dataBits = 8;
            StopBits stopBits = StopBits.One;
            Handshake handshake = Handshake.XOnXOff;
            bool displayHelp = false;
            OptionSet options = new OptionSet()
                .Add("help|?", "Display the help screen.", v => displayHelp = true)
                .Add("baudRate|b=", "The baud rate for the serial line. Default: 9600", v => baudRate = int.Parse(v))
                .Add("port|p=", "Specify an alternative COM port for listening. (Default: COM1)", v => port = v)
                .Add("parity|a=", "The parity to open the serial port to. Default: None", v => parity = (Parity)Enum.Parse(typeof(Parity), v, true))
                .Add("dataBits|d=", "The data bit to set. Default: 8", v => dataBits = int.Parse(v))
                .Add("stopBits|s=", "The stop bit to set to. Default: 1", v => stopBits = (StopBits)Enum.Parse(typeof(StopBits), v, true))
                .Add("handshake|h=", "The handshake protocol to use.", v => handshake = (Handshake)Enum.Parse(typeof(Handshake), v, true));

            options.Parse(args.Arguments);
            if (displayHelp)
            {
                options.WriteOptionDescriptions(context.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            SerialPort listener = new SerialPort(port, baudRate, parity, dataBits, stopBits);
            listener.Handshake = handshake;
            _logger.LogInformation($"Opening serial line for {port}.");
            listener.Open();
            Connection connection = new Connection(listener.BaseStream, listener.BaudRate, _terminalHandlerFactory, _factory, _connectionLogger, _extensions, _fileSystemSubsystem);
            token.Register(() => connection.Close());
            connection.Thread.Start();
            connection.Thread.Join();
            return new CommandResponse(CommandResponse.CODE_SUCCESS);

        }
        private class Connection : IInstanceSession
        {
            private RctProcess _shellProcess;
            public Thread Thread { get; }
            private Stream SerialStream { get; }
            public IExtensionCollection<IInstanceSession> Extensions { get; }
            private ITerminalHandler _handler;
            public IProcessTable ProcessTable { get; }
            public Guid ClientUniqueID { get; }
            public string Username { get; }


            public Connection(Stream stream,
                int baudRate,
                ITerminalHandlerFactory factory,
                ProcessFactorySubsystem processFactory,
                ILogger<Connection> logger,
                IExtensionProvider<IInstanceSession>[] extensions,
                FileSystemSubsystem fileSystemSubsystem)
            {
                SerialStream = stream;
                ProcessTable = new ProcessTable();
                Extensions = new ExtensionCollection<IInstanceSession>(this);
                Thread = new Thread(() =>
                {
                    _handler =
                        factory.CreateNewTerminalHandler("xterm", stream, stream, baudRate: baudRate);
                    var identity = new GenericIdentity("bob");
                    var principal = new ClaimsPrincipal(identity);
                    identity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
                    principal.AddIdentity(identity);
                    _shellProcess = processFactory.GetProcessBuilder("Application", null, ProcessTable)
                        .SetInstanceSession(this)
                        .SetSecurityPrincipal(principal)
                        .Build();

                    _shellProcess.CommandLineName = "shell";
                    _shellProcess.SetOut(stream);
                    _shellProcess.SetError(stream);
                    _shellProcess.SetIn(new ConsoleTextReader(_handler), stream);
                    Extensions.Add(_handler);
                    _shellProcess.EnvironmentVariables.AddVariable("PROXY_MODE", "false");
                    _shellProcess.EnvironmentVariables.AddVariable("?", "0");
                    _shellProcess.EnvironmentVariables.AddVariable("TERM", _handler.TerminalName);
                    _shellProcess.EnvironmentVariables.AddVariable("WORKINGDIR", "/");
                    _shellProcess.Extensions.Add(new ExtensionFileSystem(fileSystemSubsystem.GetFileSystem()));
                    _shellProcess.ThreadError += (sender, e) =>
                        logger.LogError($"Thread [{Thread.ManagedThreadId}] encountered an unhandled error: {e.Message}");
                    _shellProcess.Start();
                    _shellProcess.WaitForExit();
                    Close();
                });
                foreach (IExtensionProvider<IInstanceSession> extensionProvider in extensions)
                {
                    extensionProvider.GetExtension(this);
                }
            }

            public StreamReader GetClientReader()
            {
                return new StreamReader(SerialStream, Encoding.UTF8, false, 1, true);
            }

            public Stream OpenNetworkStream()
            {
                return SerialStream;
            }

            public StreamWriter GetClientWriter()
            {
                return new StreamWriter(SerialStream, Encoding.UTF8, 1, true)
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
                _shellProcess.Close();
                SerialStream.Close();
            }
        }
    }
}