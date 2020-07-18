using System;
using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.ApplicationSystem.Factory;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Utilities;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using RemoteControlToolkitCoreLibraryWCF;

namespace RemoteControlToolkitCoreServerWCF
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true,
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        UseSynchronizationContext = false)]
    public class RCTService : IRCTService, IInstanceSession
    {
        public IExtensionCollection<IInstanceSession> Extensions { get; }
        public Guid ClientUniqueID { get; }
        public string Username { get; }
        public IProcessTable ProcessTable { get; }
        private readonly ITerminalHandlerFactory _factory;
        private IRCTServiceCallback _callback;
        private ITerminalHandler _terminalHandler;
        private RctProcess _shellProcess;
        private readonly FileSystemSubsystem _fileSystemSubsystem;
        private readonly ProcessFactorySubsystem _processFactory;

        public RCTService(IServiceProvider provider)
        {
            Extensions = new ExtensionCollection<IInstanceSession>(this);
            ClientUniqueID = Guid.NewGuid();
            Username = "Bob";
            ProcessTable = new ProcessTable();
            _factory = provider.GetService<ITerminalHandlerFactory>();
            _processFactory = provider.GetService<ProcessFactorySubsystem>();
            _fileSystemSubsystem = provider.GetService<FileSystemSubsystem>();
            foreach (IExtensionProvider<IInstanceSession> extensionProvider in provider.GetServices<IExtensionProvider<IInstanceSession>>())
            {
                extensionProvider.GetExtension(this);
            }
        }
        public void StartShell()
        {
            _callback = OperationContext.Current.GetCallbackChannel<IRCTServiceCallback>();
            var dimensions = _callback.GetTerminalDimensions();
            _terminalHandler = _factory.CreateNewTerminalHandler("WCF", terminalRows: dimensions.rows,
                terminalColumns: dimensions.columns, additionalArguments: new object[]{_callback});

            //Configure security
            var identity = new GenericIdentity("bob");
            var principal = new ClaimsPrincipal(identity);
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
            principal.AddIdentity(identity);

            _shellProcess =
                _processFactory.GetProcessBuilder("Application", null, ProcessTable)
                    .SetSecurityPrincipal(principal)
                    .SetInstanceSession(this)
                    .Build();
            _shellProcess.CommandLineName = "shell";

            var outStream = new WCFStream(_callback);
            _shellProcess.SetOut(outStream);
            _shellProcess.SetError(outStream);
            _shellProcess.SetIn(new ConsoleTextReader(_terminalHandler), outStream);
            _shellProcess.Extensions.Add(new ExtensionFileSystem(_fileSystemSubsystem.GetFileSystem()));
            _shellProcess.EnvironmentVariables.AddVariable("PROXY_MODE", "false");
            _shellProcess.EnvironmentVariables.AddVariable("?", "0");
            _shellProcess.EnvironmentVariables.AddVariable("TERM", "WCF");
            _shellProcess.EnvironmentVariables.AddVariable("WORKINGDIR", "/");
            Extensions.Add(_terminalHandler);
            _shellProcess.Start();
            _shellProcess.WaitForExit();
        }

        public void SendControlC()
        {
            _shellProcess.InvokeControlC();
        }


        public StreamReader GetClientReader()
        {
            throw new NotImplementedException();
        }

        public Stream OpenNetworkStream()
        {
            throw new NotImplementedException();
        }

        public StreamWriter GetClientWriter()
        {
            throw new NotImplementedException();
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
            OperationContext.Current.Channel.Close();
        }
    }
}