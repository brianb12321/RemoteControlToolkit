using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
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
        private readonly ChannelStreamWriter _channelStreamWriter;

        public BaseProcessConsole(ILogger<BaseProcessConsole> logger,
            ProcessFactorySubsystem subsystem,
            IExtensionProvider<IInstanceSession>[] providers,
            FileSystemSubsystem fileSystemSubsystem,
            IChannelProducer producer,
            PseudoTerminalPayload terminalConfig,
            List<EnvironmentPayload> environmentPayloads,
            IPrincipal identity,
            ILogger<TerminalHandler> terminalLogger,
            ITerminalHandlerFactory terminalFactory)
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
            var outStream = GetClientWriter().BaseStream;
            TextReader consoleInStream;
            if (terminalConfig != null)
            {
                TerminalHandler = terminalFactory.CreateNewTerminalHandler(terminalConfig?.TerminalType ?? "vt100",
                    Pipe,
                    outStream,
                    terminalConfig.TerminalHeight,
                    terminalConfig.TerminalWidth);
                consoleInStream = new ConsoleTextReader(TerminalHandler);
            }
            else
            {
                _logger.LogWarning("A pseudo terminal was not allocated. Some interactive commands may not work properly.");
                consoleInStream = new StreamReader(Pipe, new UTF8Encoding(false, false), false, 1, true);
            }
            
            try
            {
                _shellProcess = subsystem.GetProcessBuilder("Application", null, ProcessTable)
                    .SetSecurityPrincipal(identity)
                    .SetInstanceSession(this)
                    .Build();
                _shellProcess.CommandLineName = "shell";
            }
            //Load emergency shell.
            catch (Exception)
            {
                _logger.LogWarning("There was an error starting a shell, using emergency shell.");
                _shellProcess = ProcessTable.CreateProcessBuilder()
                    .SetProcessName(name => "Emergency Shell")
                    .SetInstanceSession(this)
                    .SetSecurityPrincipal(identity)
                    .SetAction((args, current, token) =>
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
                                var process = subsystem.CreateProcess("Application", current, ProcessTable);
                                process.CommandLineName = tokens[0];
                                process.Arguments = tokens.Skip(1).ToArray();
                                process.ThreadError += (sender, e) =>
                                {
                                    current.Error.WriteLine(
                                        Output.Red($"Error while executing command: {e.Message}"));
                                };
                                process.Start();
                                process.WaitForExit();
                                current.EnvironmentVariables.AddVariable("?", process.ExitCode.ToString());
                            }
                            catch (RctProcessException)
                            {
                                current.Error.WriteLine(
                                    Output.Red("No such command, script, or built-in function exists."));
                                current.EnvironmentVariables.AddVariable("?", "-1");
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
            if(TerminalHandler != null) Extensions.Add(TerminalHandler);
        }

        private void initializeEnvironmentVariables(RctProcess process, List<EnvironmentPayload> environmentPayloads)
        {
            _logger.LogInformation("Initializing environment variables.");
            process.EnvironmentVariables.AddVariable("PROXY_MODE", "false");
            process.EnvironmentVariables.AddVariable("?", "0");
            if(TerminalHandler != null) process.EnvironmentVariables.AddVariable("TERM", TerminalHandler.TerminalName);
            process.EnvironmentVariables.AddVariable("WORKINGDIR", "/");
            foreach (EnvironmentPayload payload in environmentPayloads)
            {
                process.EnvironmentVariables.AddVariable(payload.VariableName, payload.VariableValue);
            }
        }

        #region IConsole Members

        public void SignalWindowChange(WindowChangePayload args)
        {
            TerminalHandler.TerminalColumns = args.TerminalWidth;
            TerminalHandler.TerminalRows = args.TerminalHeight;
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
           return new StreamWriter(_channelStreamWriter) {AutoFlush = true};
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
