using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Commandline.Parsing.CommandElements;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Networking
{
    public class ProxyClient : IInstanceSession
    {
        public IExtensionCollection<IInstanceSession> Extensions { get; }
        public Guid ClientUniqueID { get; }
        public string Username { get; }
        public IProcessTable ProcessTable { get; }

        private RCTProcess _proxyProcess;
        private RCTProcess _commandShell;
        private ILogger<ProxyClient> _logger;
        private NetworkStream _networkStream;
        private StreamReader _sr;
        private StreamWriter _sw;
        private TerminalHandler _terminalHandler;

        public ProxyClient(TcpClient client, ILogger<ProxyClient> logger, IApplicationSubsystem appSubsystem, IInstanceExtensionProvider[] providers)
        {
            Extensions = new ExtensionCollection<IInstanceSession>(this);
            ProcessTable = new ProcessTable();
            ClientUniqueID = Guid.NewGuid();
            _networkStream = client.GetStream();
            _logger = logger;
            foreach (IInstanceExtensionProvider provider in providers)
            {
                provider.GetExtension(this);
            }
            _proxyProcess = ProcessTable.Factory.Create(this, "Proxy Client", (current, token) =>
            {
                try
                {

                    _sw = new StreamWriter(_networkStream);
                    _sr = new StreamReader(_networkStream);
                    _sw.AutoFlush = true;
                    _commandShell = ProcessTable.Factory.CreateOnApplication(this, appSubsystem.GetApplication("shell"),
                        current, new CommandRequest(new ICommandElement[] { new StringCommandElement("shell"), }));
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
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }, null);
            initializeEnvironmentVariables(_proxyProcess);
            _terminalHandler = new TerminalHandler(GetClientReader(), GetClientWriter());
            Extensions.Add(_terminalHandler);
        }
        private void initializeEnvironmentVariables(RCTProcess process)
        {
            _logger.LogInformation("Initializing environment variables.");
            process.EnvironmentVariables.Add("PROXY_MODE", "true");
            process.EnvironmentVariables.Add(".", "0");
        }

        public void Start()
        {
            _proxyProcess.Start();
        }
        public TextReader GetClientReader()
        {
            return _sr;
        }

        public TextWriter GetClientWriter()
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