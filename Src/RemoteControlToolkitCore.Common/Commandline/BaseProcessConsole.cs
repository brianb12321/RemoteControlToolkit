using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.ServiceModel;
using Crayon;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.ApplicationSystem;
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

        public BaseProcessConsole(ILogger<BaseProcessConsole> logger,
            ApplicationSubsystem subsystem,
            IExtensionProvider<IInstanceSession>[] providers,
            FileSystemSubsystem fileSystemSubsystem,
            IChannelProducer producer,
            PseudoTerminalPayload terminalConfig,
            List<EnvironmentPayload> environmentPayloads,
            IServiceProvider serviceProvider,
            IPrincipal identity,
            ILogger<TerminalHandler> terminalLogger)
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
            try
            {
                _shellProcess = ProcessTable.Factory.CreateOnApplication(this, subsystem.GetApplication("shell"),
                    null, new CommandRequest(new[] {"shell"}), identity);
            }
            //Load emergency shell.
            catch (Exception)
            {
                _logger.LogWarning("There was an error starting a shell, using emergency shell.");
                _shellProcess = ProcessTable.Factory.Create(this, "shell", (current, token) =>
                {
                    current.Out.WriteLine("There was a critical error loading a shell. You have been provided with an emergency shell.".Yellow());
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
                            var application = subsystem.GetApplication(tokens[0]);
                            var process = current.ClientContext.ProcessTable.Factory.CreateOnApplication(current.ClientContext, application,
                                current, new CommandRequest(tokens), current.Identity);
                            process.ThreadError += (sender, e) =>
                            {
                                current.Error.WriteLine(Output.Red($"Error while executing command: {e.Message}"));
                            };
                            process.Start();
                            process.WaitForExit();
                            current.EnvironmentVariables["?"] = process.ExitCode.ToString();
                        }
                        catch (RctProcessException)
                        {
                            current.Error.WriteLine(Output.Red("No such command, script, or built-in function exists."));
                            current.EnvironmentVariables["?"] = "-1";
                        }
                    }
                    return new CommandResponse(CommandResponse.CODE_SUCCESS);
                    
                }, null, identity);
            }
            _shellProcess.SetOut(outStream);
            _shellProcess.SetError(outStream);
            _shellProcess.SetIn(consoleInStream);
            _shellProcess.Extensions.Add(new ExtensionFileSystem(fileSystemSubsystem.GetFileSystem()));
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
