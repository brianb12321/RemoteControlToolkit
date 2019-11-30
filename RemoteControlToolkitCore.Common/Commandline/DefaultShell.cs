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
    public class DefaultShell : RCTApplication, IShellExtensions
    {
        private List<string> _history;
        private IScriptingEngine _engine;
        private IApplicationSubsystem _appSubsystem;
        private IHostApplication _nodeApplication;
        private IServiceProvider _services;
        private ILogger<DefaultShell> _logger;
        private RCTProcess _process;
        private Dictionary<string, Func<CommandRequest, CommandResponse>> _builtInCommands;
        public override string ProcessName => "DefaultShell";

        public override CommandResponse Execute(CommandRequest args, RCTProcess currentProc, CancellationToken token)
        {
            initializeEnvironmentVariables(currentProc);
            currentProc.ControlC += CurrentProc_ControlC;
            _builtInCommands.Add("cls", (args2) =>
            {
                currentProc.Out.WriteLine("\u001b[2J\u001b[;H\u001b[0m");
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
            _builtInCommands.Add("exit", arg2 =>
            {
                currentProc.Close();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
            _builtInCommands.Add("clearHistory", arg2 =>
            {
                _history.Clear();
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
                _history = new List<string>();
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

                    string newCommand = ReadLine(currentProc, sb, _history);
                    if (string.IsNullOrWhiteSpace(newCommand)) continue;
                    _history.Add(newCommand);
                    var result = executeCommand(newCommand, currentProc, token);
                }
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else return executeCommand(command, currentProc, token);
        }

        private void initializeEnvironmentVariables(RCTProcess process)
        {
            _logger.LogInformation("Initializing environment variables.");
            process.EnvironmentVariables.Add("TERMINAL_ROWS", "36");
            process.EnvironmentVariables.Add("TERMINAL_COLUMNS", "130");
        }


        public string ReadLine(RCTProcess process, StringBuilder sb, List<string> history)
        {
            TextReader tr = process.In;
            TextWriter tw = process.Out;
            int col = int.Parse(process.EnvironmentVariables["TERMINAL_COLUMNS"]);
            int row = int.Parse(process.EnvironmentVariables["TERMINAL_ROWS"]);
            int historyPosition = history.Count;
            int originalCol = int.Parse(getCursorPosition(tw, tr).column);
            int originalRow = int.Parse(getCursorPosition(tw, tr).row);
            string c;
            int cursorPosition = 0;
            //Read from the terminal
            while ((c = char.ConvertFromUtf32(tr.Read())) != "\n" && c != "\r")
            {
                //Check conditions
                switch (c)
                {
                    //Handle backspace
                    case "\u007f":
                        if (sb.Length > 0)
                        {
                            sb.Remove(cursorPosition - 1, 1);
                            cursorPosition--;
                        }

                        break;
                    case "\u001b":
                        char[] chars = new char[4];
                        tr.Read(chars, 0, chars.Length);
                        string charString = new string(chars).Replace("\0", string.Empty);
                        switch (charString)
                        {
                            //Cursor left
                            case "[D":
                                if (cursorPosition > 0)
                                {
                                    cursorPosition = Math.Max(0, cursorPosition - 1);
                                }

                                break;
                            //Cursor Right
                            case "[C":
                                cursorPosition = Math.Min(sb.Length, cursorPosition + 1);
                                break;
                            //Up Arrow
                            case "[A":
                                if (history.Count > 0 && historyPosition > 0)
                                {
                                    historyPosition--;
                                    sb.Clear();
                                    cursorPosition = 0;
                                    string historyCommand = _history[historyPosition];
                                    sb.Append(historyCommand);
                                    cursorPosition = historyCommand.Length;
                                }

                                break;
                            //Down Arrow
                            case "[B":
                                if (history.Count > 0 && historyPosition < _history.Count - 1)
                                {
                                    historyPosition++;
                                    sb.Clear();
                                    cursorPosition = 0;
                                    string historyCommand = history[historyPosition];
                                    sb.Append(historyCommand);
                                    cursorPosition = historyCommand.Length;
                                }

                                break;
                            //Home
                            case "[1~":
                                cursorPosition = 0;
                                break;
                            //End
                            case "[4~":
                                cursorPosition = sb.Length;
                                break;
                            //F1
                            case "[11~":
                                sb.Clear();
                                cursorPosition = 0;
                                break;
                        }

                        break;
                    default:
                        sb.Insert(cursorPosition, c);
                        cursorPosition++;
                        break;
                }

                int realStringLength = sb.Length + originalCol;
                int realCursorPosition = (cursorPosition + originalCol);
                int rowsToMove = realStringLength / col;
                int cursorRowsToMove = realCursorPosition / col;
                int cellsToMove = (realCursorPosition % col) - 1;
                //The cursor position is a multiple of the column
                if (cellsToMove == -1)
                {
                    cursorRowsToMove--;
                    cellsToMove = col;
                }
                //Restore saved cursor position.
                tw.Write($"\u001b[{originalRow};{originalCol}H");
                //Clear lines
                if (rowsToMove > 0)
                {
                    for (int i = 0; i <= rowsToMove; i++)
                    {
                        tw.Write("\u001b[0K");
                        tw.Write("\u001b[B");
                        tw.Write("\u001b[10000000000D");
                    }
                }
                else
                {
                    tw.Write("\u001b[0K");
                }

                tw.Write($"\u001b[{originalRow};{originalCol}H");
                //Print data
                tw.Write(sb.ToString());
                tw.Write($"\u001b[{originalRow};{originalCol}H");
                //Reposition cursor
                if (cursorPosition > 0)
                {
                    if (realCursorPosition > col)
                    {
                        for (int i = 0; i < cursorRowsToMove; i++)
                        {
                            tw.Write("\u001b[E");

                        }

                        if (cellsToMove > 0)
                        {
                            tw.Write($"\u001b[{cellsToMove}C");
                        }
                    }
                    else
                    {
                        tw.Write("\u001b[" + cursorPosition + "C");
                    }
                }
            }

            tw.WriteLine();
            return sb.ToString();
        }

        private (string row, string column) getCursorPosition(TextWriter tw, TextReader tr)
        {
            //Send code for cursor position.
            tw.Write("\u001b[6n");
            char[] buffer = new char[8];
            tr.Read(buffer, 0, buffer.Length);
            string newString = new string(buffer);
            //Get rid of \0
            newString = newString.Replace("\0", string.Empty);
            //Get rid of ANSI escape code.
            newString = newString.Replace("\u001b", string.Empty);
            //Get rid of brackets
            newString = newString.Replace("[", string.Empty);
            //Split between the semicolon and R.
            string[] position = newString.Split(';', 'R');
            return (position[0], position[1]);
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
                IParser parser = new Parser(_engine, context);
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
                        _process.SetOut(new StreamWriter(parser.Output, parser.OutputAppendMode));
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
                        _process.DisposeIO = false;
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

        public void Attach(RCTProcess owner)
        {
            
        }

        public void Detach(RCTProcess owner)
        {
            
        }
    }
}