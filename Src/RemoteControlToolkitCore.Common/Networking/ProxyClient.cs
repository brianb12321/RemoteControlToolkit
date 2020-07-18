using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.ApplicationSystem.Factory;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Networking
{
    public class ProxyClient : IInstanceSession
    {
        public IExtensionCollection<IInstanceSession> Extensions { get; }
        public Guid ClientUniqueID { get; }
        public string Username { get; }
        public IProcessTable ProcessTable { get; }

        private readonly RctProcess _proxyProcess;
        private RctProcess _commandShell;
        private readonly ILogger<ProxyClient> _logger;
        private readonly NetworkStream _networkStream;
        private StreamReader _sr;
        private StreamWriter _sw;

        public ProxyClient(TcpClient client, IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<ProxyClient>>();
            ApplicationSubsystem appSubsystem = serviceProvider.GetService<ApplicationSubsystem>();
            IExtensionProvider<IInstanceSession>[] providers =
                serviceProvider.GetService<IExtensionProvider<IInstanceSession>[]>();
            Extensions = new ExtensionCollection<IInstanceSession>(this);
            ProcessTable = new ProcessTable();
            ClientUniqueID = Guid.NewGuid();
            _networkStream = client.GetStream();
            foreach (IExtensionProvider<IInstanceSession> provider in providers)
            {
                provider.GetExtension(this);
            }

            _proxyProcess = ProcessTable.CreateProcessBuilder()
                .SetProcessName(name => "Proxy Client")
                .SetInstanceSession(this)
                .SetSecurityPrincipal(new ClaimsPrincipal(new GenericIdentity("bob")))
                .SetAction((args, current, token) =>
                {
                    try
                    {
                        _sw = new StreamWriter(_networkStream);
                        _sr = new StreamReader(_networkStream);
                        _sw.AutoFlush = true;
                        _commandShell = serviceProvider.GetService<ProcessFactorySubsystem>().CreateProcess("Application", current, ProcessTable);
                        _commandShell.CommandLineName = "shell";
                        _commandShell.SetOut(GetClientWriter());
                        _commandShell.SetIn(GetClientReader());
                        _commandShell.SetError(GetClientWriter());
                        _commandShell.Start();
                        _commandShell.WaitForExit();
                        return new CommandResponse(CommandResponse.CODE_SUCCESS);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "A communication error occurred. The connection will be terminated.");
                        return new CommandResponse(CommandResponse.CODE_FAILURE);
                    }
                    finally
                    {
                        Close();
                    }
                })
                .Build();
            initializeEnvironmentVariables(_proxyProcess);
        }
        private void initializeEnvironmentVariables(RctProcess process)
        {
            _logger.LogInformation("Initializing environment variables.");
            process.EnvironmentVariables.AddVariable("PROXY_MODE", "true");
            process.EnvironmentVariables.AddVariable(".", "0");
        }

        public void Start()
        {
            _proxyProcess.Start();
        }
        public StreamReader GetClientReader()
        {
            return _sr;
        }

        public Stream OpenNetworkStream()
        {
            return _sr.BaseStream;
        }

        public StreamWriter GetClientWriter()
        {
            return _sw;
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
            _sw.Close();
            _sr.Close();
            //_sslStream.Close();
            _networkStream.Close();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Stream SocketStream { get; set; }
        public Socket ClientSocket { get; set; }
        public void Process()
        {
            throw new NotImplementedException();
        }

        public void Reject()
        {
            throw new NotImplementedException();
        }
    }
}