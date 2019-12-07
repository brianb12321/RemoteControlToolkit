using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using Crayon;
using IronPython.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NDesk.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Commandline.Parsing;
using RemoteControlToolkitCore.Common.Commandline.Parsing.CommandElements;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Scripting;
using RemoteControlToolkitCore.Common.Utilities;
using RemoteControlToolkitCore.Common.VirtualFileSystem;

namespace RemoteControlToolkitCore.Common.Commandline
{
    [PluginModule(Name = "shell", ExecutingSide = NetworkSide.Proxy | NetworkSide.Server)]
    [CommandHelp("The main entry point for executing commands.")]
    public class DefaultShell : RCTApplication
    {
        private IScriptingEngine _engine;
        private IApplicationSubsystem _appSubsystem;
        private IHostApplication _nodeApplication;
        private IServiceProvider _services;
        private ILogger<DefaultShell> _logger;
        private ITerminalHandler _shellExt;
        private RCTProcess _process;
        private Dictionary<string, Func<CommandRequest, CommandResponse>> _builtInCommands;
        public override string ProcessName => "DefaultShell";

        public override CommandResponse Execute(CommandRequest args, RCTProcess currentProc, CancellationToken token)
        {
            _shellExt = currentProc.Extensions.Find<ITerminalHandler>();
            currentProc.ControlC += CurrentProc_ControlC;
            _builtInCommands.Add("cls", (args2) =>
            {
                _shellExt.Clear();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
            _builtInCommands.Add("exit", arg2 =>
            {
                currentProc.Close();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
            _builtInCommands.Add("clearHistory", arg2 =>
            {
                _shellExt.History.Clear();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
            _builtInCommands.Add("title", arg2 =>
            {
                currentProc.Out.Write($"\u001b]2;{arg2.Arguments[1].ToString()}\007");
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
            _builtInCommands.Add("set", arg2 =>
            {
                if (arg2.Arguments.Count() <= 1)
                {
                    currentProc.Out.WriteLine(currentProc.EnvironmentVariables.ShowDictionary());
                }
                else
                {
                    string key = arg2.Arguments[1].ToString();
                    if (currentProc.EnvironmentVariables.ContainsKey(key))
                    {
                        currentProc.EnvironmentVariables[key] = arg2.Arguments[2].ToString();
                    }
                    else
                    {
                        currentProc.EnvironmentVariables.Add(key, arg2.Arguments[2].ToString());
                    }
                }

                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
            _builtInCommands.Add("bell", arg2 =>
            {
                _shellExt.Bell();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
            string command = string.Empty;
            bool showHelp = false;
            OptionSet options = new OptionSet()
                .Add("command|c=", "The command to execute.", v => command = v)
                .Add("help|?", "Displays the help screen.", v => showHelp = true);

            options.Parse(args.Arguments.Select(a => a.ToString()));
            if (showHelp)
            {
                options.WriteOptionDescriptions(currentProc.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }

            if (string.IsNullOrWhiteSpace(command))
            {
                StringBuilder sb = new StringBuilder();
                currentProc.Out.WriteLine("Welcome to RCT shell! For help, enter help");
                currentProc.Out.WriteLine();
                while (!token.IsCancellationRequested)
                {
                    sb.Clear();
                    token.ThrowIfCancellationRequested();
                    if(_nodeApplication.ExecutingSide == NetworkSide.Proxy)
                    {
                        currentProc.Out.Write($"[proxy {Environment.MachineName}]> ");
                    }
                    else
                    {
                        currentProc.Out.Write($"[{Environment.MachineName}]> ");
                    }

                    string newCommand = _shellExt.ReadLine();
                    if (newCommand.StartsWith("`"))
                    {
                        handleMultipleCommands(token, currentProc, sb);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(newCommand)) continue;
                    _shellExt.History.Add(newCommand);
                    currentProc.EnvironmentVariables["."] = executeCommand(newCommand, currentProc, token).Code.ToString();
                }
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else return executeCommand(command, currentProc, token);
        }

        private void handleMultipleCommands(CancellationToken token, RCTProcess currentProc, StringBuilder sb)
        {
            List<string> _commands = new List<string>();
            while (true)
            {
                sb.Clear();
                currentProc.Out.Write("> ");
                string batchCommand = _shellExt.ReadLine();
                if (string.IsNullOrWhiteSpace(batchCommand)) continue;
                if (batchCommand.Contains("`"))
                {
                    break;
                }
                _shellExt.History.Add(batchCommand);
                _commands.Add(batchCommand);
            }

            foreach (var command in _commands)
            {
                currentProc.EnvironmentVariables["."] = executeCommand(command, currentProc, token).Code.ToString();
            }
        }

        private void CurrentProc_ControlC(object sender, ControlCEventArgs e)
        {
            RCTProcess currentProc = (RCTProcess) sender;
            currentProc.Child?.InvokeControlC();
            e.CloseProcess = false;
        }

        public CommandResponse executeCommand(string command, RCTProcess currentProc, CancellationToken token)
        {
            var context = currentProc.ClientContext.GetExtension<IScriptExecutionContext>();
            ILexer lexer = new Lexer();
            IReadOnlyList<IReadOnlyList<ICommandElement>> parsedItems = null;
            try
            {
                IParser parser = new Parser(_engine, context, currentProc.EnvironmentVariables);
                var lexedItems = lexer.Lex(command);
                parsedItems = parser.Parse(lexedItems);
                CommandRequest newRequest = new CommandRequest(parsedItems[0].ToArray());
                string newCommand = newRequest.Arguments[0].ToString();
                //Check if command is built-in.
                if (_builtInCommands.ContainsKey(newCommand))
                {
                    _process = currentProc.ClientContext.ProcessTable.Factory.Create(currentProc.ClientContext, "internalCommand",
                        (proc, delToken) =>
                            _builtInCommands[newCommand](newRequest), currentProc);
                }
                //Check if command should execute external program.
                else if (newCommand.StartsWith("./"))
                {
                    _process = currentProc.ClientContext.ProcessTable.Factory.CreateOnExternalProcess(currentProc.ClientContext, newRequest,
                        currentProc);
                }
                else
                {
                    try
                    {
                        var application = _appSubsystem.GetApplication(newCommand);
                        _process = currentProc.ClientContext.ProcessTable.Factory.CreateOnApplication(currentProc.ClientContext, application,
                            currentProc, newRequest);
                    }
                    catch (RctProcessException ex)
                    {
                        currentProc.Error.WriteLine(Output.Red("No such command, script, or built-in function exists."));
                        return new CommandResponse(CommandResponse.CODE_FAILURE);
                    }
                }

                _process.ThreadError += (sender, e) =>
                {
                    currentProc.Error.WriteLine(Output.Red($"Error while executing command: {e.Message}"));
                };
                //Redirect IO.
                try
                {
                    var _fileSystem = currentProc.ClientContext.GetExtension<IExtensionFileSystem>().FileSystem;
                    if (parser.OutputRedirected == RedirectionMode.File)
                    {
                        _process.SetOut(new StreamWriter(parser.Output, parser.OutputAppendMode));
                    }
                    else if (parser.OutputRedirected == RedirectionMode.VFS)
                    {
                        StreamWriter sw = StreamWriter.Null;
                        if (parser.OutputAppendMode)
                        {
                            sw = new StreamWriter(
                                _fileSystem.OpenFile(parser.Output, FileMode.Append, FileAccess.Write));
                            _process.SetOut(sw);
                            _engine.SetOut(sw);
                        }
                        else
                        {
                            sw = new StreamWriter(_fileSystem.OpenFile(parser.Output, FileMode.Create,
                                FileAccess.Write));
                            _process.SetOut(sw);
                            _engine.SetOut(sw);
                        }
                    }
                    else
                    {
                        _engine.SetOut(currentProc.Out);
                        _engine.SetError(currentProc.Error);
                    }
                    StreamReader sr = StreamReader.Null;
                    if (parser.InputRedirected == RedirectionMode.File)
                    {
                        sr = new StreamReader(parser.Input);
                        _process.SetIn(sr);
                        _engine.SetIn(sr);
                    }
                    else if (parser.InputRedirected == RedirectionMode.VFS)
                    {
                        sr = new StreamReader(_fileSystem.OpenFile(parser.Input, FileMode.Open, FileAccess.Read));
                        _process.SetIn(sr);
                        _engine.SetIn(sr);
                    }
                    else
                    {
                        _engine.SetIn(currentProc.In);
                    }
                }
                catch (Exception ex)
                {
                    currentProc.Error.WriteLine(Output.Red($"Error while redirecting IO: {ex.Message}"));
                    return new CommandResponse(CommandResponse.CODE_FAILURE);
                }
                //Configure dispose options
                if (parser.ErrorRedirected == RedirectionMode.None) _process.DisposeError = false;
                if (parser.OutputRedirected == RedirectionMode.None) _process.DisposeOut = false;
                if (parser.InputRedirected == RedirectionMode.None) _process.DisposeIn = false;
                _process.Start();
                _process.WaitForExit();
                return _process.ExitCode;
            }
            catch (ParserException e)
            {
                currentProc.Error.WriteLine(Output.Red($"Error while parsing command-line: {e.Message}"));
                return new CommandResponse(CommandResponse.CODE_FAILURE);
            }
            catch (CommandElementException e)
            {
                currentProc.Error.WriteLine(Output.Red($"Error while expanding command element: {e.Message}"));
                return new CommandResponse(CommandResponse.CODE_FAILURE);
            }
        }
        public override void InitializeServices(IServiceProvider kernel)
        {
            _nodeApplication = kernel.GetService<IHostApplication>();
            _logger = kernel.GetService<ILogger<DefaultShell>>();
            _logger.LogInformation("Shell initialized.");
            _builtInCommands = new Dictionary<string, Func<CommandRequest, CommandResponse>>();
            _engine = kernel.GetService<IScriptingEngine>();
            _appSubsystem = (IApplicationSubsystem)kernel.GetService<IPluginSubsystem<IApplication>>();
            _services = kernel;
        }

        public static RCTProcess CreateShellWithParent(string command, RCTProcess parent, IApplicationSubsystem subsystem)
        {
            var request = new CommandRequest(new ICommandElement[]
            {
                new CommandNameCommandElement("shell"),
                new StringCommandElement("-c"),
                new StringCommandElement(command)
            });
            RCTProcess shellProcess = parent.ClientContext.ProcessTable.Factory.CreateOnApplication(parent.ClientContext,
                subsystem.GetApplication("shell"), parent, request);
            return shellProcess;
        }
        public static RCTProcess CreateShell(string command, IInstanceSession session, IApplicationSubsystem subsystem)
        {
            var request = new CommandRequest(new ICommandElement[]
            {
                new CommandNameCommandElement("shell"),
                new StringCommandElement("-c"),
                new StringCommandElement(command)
            });
            RCTProcess shellProcess = session.ProcessTable.Factory.CreateOnApplication(session,
                subsystem.GetApplication("shell"), null, request);
            return shellProcess;
        }
    }
}