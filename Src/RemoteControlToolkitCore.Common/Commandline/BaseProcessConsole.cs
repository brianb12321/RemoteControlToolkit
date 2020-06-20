using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.ServiceModel;
using Crayon;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.ApplicationSystem.Factory;
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
        private readonly RctProcess _shellProcess;
        private readonly TerminalHandler _terminalHandler;
        private readonly ChannelStreamWriter _channelStreamWriter;

        public BaseProcessConsole(ILogger<BaseProcessConsole> logger,
            ProcessFactorySubsystem subsystem,
            IExtensionProvider<IInstanceSession>[] providers,
            FileSystemSubsystem fileSystemSubsystem,
            IChannelProducer producer,
            PseudoTerminalPayload terminalConfig,
            List<EnvironmentPayload> environmentPayloads,
            IPrincipal identity,
            ILogger<TerminalHandler> terminalLogger)
        {
            ClientUniqueID = Guid.NewGuid();
            Pipe = new BlockingMemoryStream();
            Producer = producer;
            _channelStreamWriter = new ChannelStreamWriter(Producer);
            Extensions = new ExtensionCollection<IInstanceSession>(this);
            _logger = logger;
            ProcessTable = new ProcessTable();
            foreach (IExtensionProvider<IInstanceSession> provider in providers)
            {
                provider.GetExtension(this);
            }
            var outStream = GetClientWriter();
            _terminalHandler = new TerminalHandler(Pipe, outStream, terminalConfig);
            TerminalHandler = _terminalHandler;
            var consoleInStream = new ConsoleTextReader(_terminalHandler);
            try
            {
                _shellProcess = subsystem.GetProcessBuilder("Application", new CommandRequest(new[] {"shell"}), null, ProcessTable)
                    .SetSecurityPrincipal(identity)
                    .SetInstanceSession(this)
                    .Build();
            }
            //Load emergency shell.
            catch (Exception)
            {
                _logger.LogWarning("There was an error starting a shell, using emergency shell.");
                _shellProcess = ProcessTable.CreateProcessBuilder()
                    .SetProcessName("Emergency Shell")
                    .SetInstanceSession(this)
                    .SetSecurityPrincipal(identity)
                    .SetAction((current, token) =>
                    {
                        current.Out.WriteLine(
                            "There was a critical error loading a shell. You have been provided with an emergency shell."
                                .Yellow());
                        current.Out.WriteLine("No extensions will be loaded into any child processes.");
                        current.Out.WriteLine("You may request any application by typing its name.");
                        current.Out.WriteLine();
                        while (!token.IsCancellationRequested)
                        {
                            current.Out.Write("> ");
                            string command = current.In.ReadLine();
                            if (string.IsNullOrWhiteSpace(command)) continue;
                            string[] tokens = command.Split(' ');
                            try
                            {
                                var process = subsystem.CreateProcess("Application",
                                    new CommandRequest(tokens), current, ProcessTable);
                                    process.ThreadError += (sender, e) =>
                                {
                                    current.Error.WriteLine(
                                        Output.Red($"Error while executing command: {e.Message}"));
                                };
                                process.Start();
                                process.WaitForExit();
                                current.EnvironmentVariables["?"] = process.ExitCode.ToString();
                            }
                            catch (RctProcessException)
                            {
                                current.Error.WriteLine(
                                    Output.Red("No such command, script, or built-in function exists."));
                                current.EnvironmentVariables["?"] = "-1";
                            }
                        }

                        return new CommandResponse(CommandResponse.CODE_SUCCESS);
                    })
                    .Build();
            }
            _shellProcess.SetOut(outStream);
            _shellProcess.SetError(outStream);
            _shellProcess.SetIn(consoleInStream, Pipe);
            _shellProcess.Extensions.Add(new ExtensionFileSystem(fileSystemSubsystem.GetFileSystem()));
            _shellProcess.ThreadError += (sender, e) =>
                _logger.LogError($"A critical error occurred while running the shell: {e.Message}");
            initializeEnvironmentVariables(_shellProcess, environmentPayloads);
            Extensions.Add(_terminalHandler);
        }

        private void initializeEnvironmentVariables(RctProcess process, List<EnvironmentPayload> environmentPayloads)
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

        public Stream OpenNetworkStream()
        {
            return _channelStreamWriter;
        }

        public StreamWriter GetClientWriter()
        {
           return new ChannelTextWriter(_channelStreamWriter);
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
            try
            {
                _shellProcess.Close();
                Pipe.Close();
                Closed?.Invoke(this, EventArgs.Empty);
            }
            catch (RctProcessException)
            {
            }
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
