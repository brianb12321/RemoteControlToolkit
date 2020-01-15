using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Parsing.CommandElements;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer.Console;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads;
using RemoteControlToolkitCore.Common.NSsh.Utility;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Utilities;
using RemoteControlToolkitCore.Common.VirtualFileSystem;

namespace RemoteControlToolkitCore.Common.Commandline
{
    public class BaseProcessConsole : IConsole, IInstanceSession
    {
        /// <summary>
        /// Logging support for this class.
        /// </summary>
        private readonly ILogger<BaseProcessConsole> _logger;

        public IProcessTable ProcessTable { get; }
        public Guid ClientUniqueID { get; }
        public string Username { get; }
        public ITerminalHandler TerminalHandler { get; }
        public IExtensionCollection<IInstanceSession> Extensions { get; }
        private readonly RCTProcess _shellProcess;
        private readonly TerminalHandler _terminalHandler;

        public BaseProcessConsole(ILogger<BaseProcessConsole> logger,
            IApplicationSubsystem subsystem,
            IExtensionProvider<IInstanceSession>[] providers,
            IFileSystemSubsystem fileSystemSubsystem,
            IChannelProducer producer,
            PseudoTerminalPayload terminalConfig,
            List<EnvironmentPayload> environmentPayloads,
            IServiceProvider serviceProvider,
            IPrincipal identity)
        {
            ClientUniqueID = Guid.NewGuid();
            Pipe = new BlockingMemoryStream();
            Producer = producer;
            Extensions = new ExtensionCollection<IInstanceSession>(this);
            _logger = logger;
            ProcessTable = new ProcessTable(serviceProvider);
            foreach (IExtensionProvider<IInstanceSession> provider in providers)
            {
                provider.GetExtension(this);
            }
            var outStream = GetClientWriter();
            _terminalHandler = new TerminalHandler(Pipe, outStream, terminalConfig);
            TerminalHandler = _terminalHandler;
            var consoleInStream = new ConsoleTextReader(_terminalHandler);
            _shellProcess = ProcessTable.Factory.CreateOnApplication(this, subsystem.GetApplication("shell"),
                null, new CommandRequest(new ICommandElement[] {new StringCommandElement("shell") }), identity);
            _shellProcess.SetOut(outStream);
            _shellProcess.SetError(outStream);
            _shellProcess.SetIn(consoleInStream);
            _shellProcess.Extensions.Add(new ExtensionFileSystem(fileSystemSubsystem.GetFileSystem()));
            initializeEnvironmentVariables(_shellProcess, environmentPayloads);
            Extensions.Add(_terminalHandler);
        }

        private void initializeEnvironmentVariables(RCTProcess process, List<EnvironmentPayload> environmentPayloads)
        {
            _logger.LogInformation("Initializing environment variables.");
            process.EnvironmentVariables.Add("PROXY_MODE", "false");
            process.EnvironmentVariables.Add("?", "0");
            process.EnvironmentVariables.Add("TERM", _terminalHandler.TerminalName);
            process.EnvironmentVariables.Add("WORKINGDIR", "/");
            foreach (EnvironmentPayload payload in environmentPayloads)
            {
                if (process.EnvironmentVariables.ContainsKey(payload.VariableName))
                {
                    process.EnvironmentVariables[payload.VariableName] = payload.VariableValue;
                }
                else
                {
                    process.EnvironmentVariables.Add(payload.VariableName, payload.VariableValue);
                }
            }
        }

        #region IConsole Members

        public void SignalWindowChange(WindowChangePayload args)
        {
            _terminalHandler.TerminalColumns = args.TerminalWidth;
            _terminalHandler.TerminalRows = args.TerminalHeight;
        }

        public IChannelProducer Producer { get; private set; }
        public BlockingMemoryStream Pipe { get; private set; }

        public StreamReader GetClientReader()
        {
            return new StreamReader(Pipe);
        }

        public TextWriter GetClientWriter()
        { 
            return new ChannelTextWriter(Producer);
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
            Closed?.Invoke(this, EventArgs.Empty);
        }

        public void Start()
        {
            _shellProcess.Start();
            _shellProcess.WaitForExit();
            Close();
        }

        public void CancellationRequested()
        {
            _shellProcess.InvokeControlC();
        }

        public bool HasClosed => _shellProcess.Disposed;

        public event EventHandler Closed;

        #endregion
    }
}
