﻿using System;
using System.Collections.Generic;
using System.IO;
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

namespace RemoteControlToolkitCore.Common.Commandline
{
    public class BaseProcessConsole : IConsole, IInstanceSession
    {
        /// <summary>
        /// Logging support for this class.
        /// </summary>
        private ILogger<BaseProcessConsole> _logger;

        private ILogger<ChannelTextReader> _channelLogger;

        public IProcessTable ProcessTable { get; }
        public Guid ClientUniqueID { get; }
        public string Username { get; }
        public IExtensionCollection<IInstanceSession> Extensions { get; }
        private RCTProcess _shellProcess;
        private TerminalHandler _terminalHandler;

        public BaseProcessConsole(ILogger<BaseProcessConsole> logger,
            IApplicationSubsystem subsystem,
            IInstanceExtensionProvider[] providers,
            IChannelProducer producer,
            ILogger<ChannelTextReader> channelLogger,
            PseudoTerminalPayload terminalConfig,
            List<EnvironmentPayload> environmentPayloads)
        {
            Producer = producer;
            _channelLogger = channelLogger;
            Extensions = new ExtensionCollection<IInstanceSession>(this);
            _logger = logger;
            ProcessTable = new ProcessTable();
            foreach (IInstanceExtensionProvider provider in providers)
            {
                provider.GetExtension(this);
            }

            var outStream = GetClientWriter();
            var inStream = GetClientReader();
            _terminalHandler = new TerminalHandler(inStream, outStream, terminalConfig);
            var consoleInStream = new ConsoleTextReader(_terminalHandler, inStream, outStream);
            _shellProcess = ProcessTable.Factory.CreateOnApplication(this, subsystem.GetApplication("shell"),
                null, new CommandRequest(new ICommandElement[] {new StringCommandElement("shell") }));
            _shellProcess.SetOut(outStream);
            _shellProcess.SetError(outStream);
            _shellProcess.SetIn(consoleInStream);
            initializeEnvironmentVariables(_shellProcess, environmentPayloads);
            Extensions.Add(_terminalHandler);
        }

        private void initializeEnvironmentVariables(RCTProcess process, List<EnvironmentPayload> environmentPayloads)
        {
            _logger.LogInformation("Initializing environment variables.");
            process.EnvironmentVariables.Add("PROXY_MODE", "false");
            process.EnvironmentVariables.Add(".", "0");
            foreach (EnvironmentPayload payload in environmentPayloads)
            {
                process.EnvironmentVariables.Add(payload.VariableName, payload.VariableValue);
            }
        }

        #region IConsole Members

        public void SignalWindowChange(WindowChangePayload args)
        {
            _terminalHandler.TerminalColumns = args.TerminalWidth;
            _terminalHandler.TerminalRows = args.TerminalHeight;
        }

        public IChannelProducer Producer { get; private set; }

        public virtual TextWriter StandardInput
        {
            get { return TextWriter.Null; }
        }

        public virtual TextReader StandardOutput
        {
            get { return TextReader.Null; }
        }

        public virtual TextReader StandardError
        {
            get { return TextReader.Null; }
        }

        public TextReader GetClientReader()
        {
            return new ChannelTextReader(Producer, _channelLogger, this);
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
            _shellProcess.Dispose();
            Close();
        }

        public bool HasClosed
        {
            get => _shellProcess.Running;
        }

        public event EventHandler Closed;

        #endregion
    }
}
