using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using static Crayon.Output;
using Microsoft.Extensions.Logging;
using NDesk.Options;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.ApplicationSystem.Factory;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Commandline.TerminalExtensions;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Utilities;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;
using RemoteControlToolkitCore.DefaultShell.Parsing;

[assembly: PluginLibrary("DefaultShell", "RCT Default Shell")]
namespace RemoteControlToolkitCore.DefaultShell
{
    [Plugin(PluginName = "shell")]
    [CommandHelp("The main entry point for executing commands.")]
    public class DefaultShell : RCTApplication
    {
        private ProcessFactorySubsystem _processFactory;
        private IHostApplication _nodeApplication;
        private IPipeService _pipeService;
        private string _motd = "I love cookies!";
        private ILogger<DefaultShell> _logger;
        private ITerminalHandler _shellExt;
        private List<RctProcess> _processes;
        private bool _promptAnyways;
        private Dictionary<string, Func<CommandRequest, CommandResponse>> _builtInCommands;
        private List<(string art, string artist)> _bannerArts;
        public override string ProcessName => "DefaultShell";

        private void loadBannerArt()
        {
            _bannerArts = new List<(string art, string artist)>
            {
                (Cyan(@"
                 _
     .,-;-;-,. /'_\
   _/_/_/_|_\_\) /
 '-<_><_><_><_>=/\
   `/_/====/_/-'\_\
    ""     ""    """), "Joan Stark"),
                (Cyan(@"
.--.
|__| .-------.
|=.| |.-----.|
|--| || KCK ||
|  | |'-----'|
|__|~')_____('
"), "KCK"),
                (Cyan(@"
.`.     _ _
__;_ \ /,//`
--, `._) (
 '//,,,  |
      )_/
     /_|
"), "sk")
            };
        }
        private void setupInternalCommands(RctProcess currentProc)
        {
            _builtInCommands.Add("cls", (args2) =>
            {
                _shellExt.Clear();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
            _builtInCommands.Add("cd", arg2 =>
            {
                var workingDirFileSystem = currentProc.Extensions.Find<IExtensionFileSystem>().GetFileSystem();
                UPath directory = new UPath(arg2.Arguments[1].ToString());
                if (workingDirFileSystem.DirectoryExists(directory))
                {
                    currentProc.WorkingDirectory = (directory.IsAbsolute) ? directory : UPath.Combine(currentProc.WorkingDirectory, directory);
                    _shellExt?.SetTitle($"RCT Shell - {currentProc.WorkingDirectory}");
                    return new CommandResponse(CommandResponse.CODE_SUCCESS);
                }
                else
                {
                    currentProc.Out.WriteLine(Red($"Directory '{directory}' does not exist."));
                    return new CommandResponse(CommandResponse.CODE_FAILURE);
                }
            });
            _builtInCommands.Add("pwd", arg2 =>
            {
                currentProc.Out.WriteLine(currentProc.WorkingDirectory);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
            _builtInCommands.Add("exit", arg2 =>
            {
                currentProc.Close();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
            _builtInCommands.Add("clearHistory", arg2 =>
            {
                var history =_shellExt.Extensions.Find<ITerminalHistory>();
                history?.History?.Clear();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
            _builtInCommands.Add("title", arg2 =>
            {
                _shellExt?.SetTitle(arg2.Arguments[1].ToString());
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
                        currentProc.EnvironmentVariables.AddVariable(key, arg2.Arguments[2].ToString());
                    }
                    else
                    {
                        currentProc.EnvironmentVariables.AddVariable(key, arg2.Arguments[2].ToString());
                    }
                }

                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
            _builtInCommands.Add("shellCommands", arg2 =>
            {
                foreach (string command in _builtInCommands.Keys)
                {
                    currentProc.Out.WriteLine(command);
                }
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
            _builtInCommands.Add("bell", arg2 =>
            {
                _shellExt.Bell();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
            _builtInCommands.Add("banner", arg2 =>
            {
                currentProc.Out.WriteLine(drawBanner());
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            });
        }
        public override CommandResponse Execute(CommandRequest args, RctProcess currentProc, CancellationToken token)
        {
            bool printNewLine = false;
            _logger.LogDebug("Getting terminal handler.");
            _shellExt = currentProc.ClientContext.GetExtension<ITerminalHandler>();
            _logger.LogDebug("Attaching control-c handler.");
            currentProc.ControlC += CurrentProc_ControlC;
            setupInternalCommands(currentProc);
            string command = string.Empty;
            bool showHelp = false;
            OptionSet options = new OptionSet()
                .Add("command|c=", "The command to execute.", v => command = v)
                .Add("help|?", "Displays the help screen.", v => showHelp = true)
                .Add("newLine|n", "Prints a new-line when finished reading from StdIn.", v => printNewLine = true)
                .Add("motd|m=", "Sets the shell's motto of the day.", v => _motd = v)
                .Add("promptAnyways|p", "Displays the prompt even when StdIn is redirected.", v => _promptAnyways = true);

            
            options.Parse(args.Arguments.Select(a => a.ToString()));
            if (showHelp)
            {
                currentProc.Out.WriteLine("Usage: shell [-c=<COMMAND>] [-n] [-m=<MOTD>] [-p]");
                currentProc.Out.WriteLine("The default shell experience for Remote Control Toolkit Core.");
                currentProc.Out.WriteLine();
                currentProc.Out.WriteLine("Options:");
                options.WriteOptionDescriptions(currentProc.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }

            if (currentProc.EnvironmentVariables["PROXY_MODE"] == "true")
            {
                while (true)
                {
                    string newCommand = currentProc.In.ReadLine();
                    if (string.IsNullOrWhiteSpace(newCommand))
                    {
                        currentProc.Out.WriteLine("\u001b]e");
                        continue;
                    }
                    currentProc.EnvironmentVariables.AddVariable("?", executeCommand(newCommand, currentProc).Code.ToString());
                    currentProc.Out.WriteLine("\u001b]e");
                }
            }

            if (string.IsNullOrWhiteSpace(command))
            {
                StringBuilder sb = new StringBuilder();
                _shellExt?.SetTitle($"RCT Shell - {currentProc.WorkingDirectory}");
                //Draw Banner
                _logger.LogDebug("Drawing banner.");
                loadBannerArt();
                currentProc.Out.Write(drawBanner().ToString());
                if(currentProc.Identity.IsInRole("Administrator")) currentProc.Out.WriteLine(Bright.Yellow("WARNING: You are logged in as a server administrator."));
                while (!token.IsCancellationRequested)
                {
                    sb.Clear();
                    token.ThrowIfCancellationRequested();
                    if(_nodeApplication.ExecutingSide == NetworkSide.Proxy)
                    {
                        if(!currentProc.InRedirected || _promptAnyways) currentProc.Out.Write($"[proxy {Environment.MachineName}]> ");
                    }
                    else
                    {
                        if (!currentProc.InRedirected || _promptAnyways) currentProc.Out.Write($"{Bright.Green(currentProc.Identity.Identity.Name)}{Bright.Green("@")}{Bright.Green(Environment.MachineName)}:{Bright.Blue(currentProc.WorkingDirectory.ToString())}{Blue(" $")} ");
                    }
                    _logger.LogDebug("Beginning readline.");
                    string newCommand = currentProc.In.ReadLine();
                    if (printNewLine) currentProc.Out.WriteLine();
                    if (newCommand == null) break;
                    if (string.IsNullOrWhiteSpace(newCommand)) continue;
                    if (newCommand.StartsWith("`"))
                    {
                        handleMultipleCommands(currentProc, sb);
                        if (printNewLine) currentProc.Out.WriteLine();
                        continue;
                    }
                    currentProc.EnvironmentVariables.AddVariable("?", executeCommand(newCommand, currentProc).Code.ToString());
                    _processes.Clear();
                }
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else return executeCommand(command, currentProc);
        }

        (string art, string artist) getBannerArt()
        {
            Random rand = new Random();
            return _bannerArts[rand.Next(_bannerArts.Count)];
        }
        StringBuilder drawBanner()
        {
            StringBuilder bannerBuilder = new StringBuilder();
            var (art, artist) = getBannerArt();
            bannerBuilder.AppendLine(art);
            bannerBuilder.AppendLine($"Art by {artist}");
            bannerBuilder.AppendLine();
            bannerBuilder.AppendLine("┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓");
            bannerBuilder.AppendLine("┃  Welcome to RCT shell! For a list of commands, enter help   ┃");
            bannerBuilder.AppendLine("┃                                                             ┃");
            bannerBuilder.AppendLine("┃  Visit my Github page at https://github.com/brianb12321     ┃");
            bannerBuilder.AppendLine("┃  For executing a script, start a command with ./            ┃");
            bannerBuilder.AppendLine("┃  For inlining a script as an argument, use {script}         ┃");
            bannerBuilder.AppendLine("┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛");
            bannerBuilder.AppendLine();
            if (!string.IsNullOrWhiteSpace(_motd))
            {
                bannerBuilder.AppendLine(_motd);
                bannerBuilder.AppendLine();
            }
            return bannerBuilder;
        }
        private void handleMultipleCommands(RctProcess currentProc, StringBuilder sb)
        {
            List<string> commands = new List<string>();
            while (true)
            {
                sb.Clear();
                if (!currentProc.InRedirected || _promptAnyways) currentProc.Out.Write("> ");
                var batchCommand = currentProc.In.ReadLine();
                if (string.IsNullOrWhiteSpace(batchCommand)) continue;
                if (batchCommand.Contains("`"))
                {
                    break;
                }
                commands.Add(batchCommand);
            }

            foreach (var command in commands)
            {
                currentProc.EnvironmentVariables.AddVariable("?", executeCommand(command, currentProc).Code.ToString());
            }
        }

        private static void CurrentProc_ControlC(object sender, ControlCEventArgs e)
        {
            RctProcess currentProc = (RctProcess) sender;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < currentProc.Children.Count; i++)
            {
                currentProc.Children[i].InvokeControlC();
            }
            e.CloseProcess = false;
        }

        private CommandResponse executeCommand(string command, RctProcess currentProc)
        {
            if (command.StartsWith("::"))
            {
                //TODO: Add scripting
            }
            ILexer lexer = new Lexer();
            try
            {
                var lexedItems = lexer.Lex(command).ToList();
                //var parsedItems = parser.Parse(lexedItems);
                var parsedItems = splitToken(lexedItems.ToArray(), TokenType.Semicolon);
                CommandResponse exitCode = new CommandResponse(CommandResponse.CODE_SUCCESS);

                for (int i = 0; i < parsedItems.Length; i++)
                {
                    var pipedItems = splitToken(parsedItems[i].ToArray(), TokenType.Pipe);
                    //We need to create a pipe. We are going to use the pipe service to create the piping. Usually this will be a Microsoft anonymous pipe.
                    for (int p = 0; p < pipedItems.Length; p++)
                    {
                        string[] arguments = pipedItems[p].Select(e => e.ToString()).ToArray();
                        string newCommand = arguments[0];
                        //Check if command is built-in.
                        if (_builtInCommands.ContainsKey(newCommand))
                        {
                            _processes.Add(currentProc.ClientContext.ProcessTable.CreateProcessBuilder()
                                .SetProcessName(name => $"internalCommand {name}")
                                .SetParent(currentProc)
                                .SetAction((args, proc, delToken) =>
                                    _builtInCommands[newCommand](args))
                                .Build());

                            _processes[p].ThreadError += (sender, e) =>
                            {
                                currentProc.Error.WriteLine(
                                    Red($"Error while executing built-in command: {e.Message}"));
                            };
                            _processes[p].CommandLineName = newCommand;
                            _processes[p].Arguments = arguments.Skip(1).ToArray();
                        }

                        //Check if command should execute external program.
                        else if (newCommand.StartsWith("./"))
                        {
                            string fileName = newCommand.Substring(2);
                            if (string.IsNullOrWhiteSpace(fileName)) return new CommandResponse(CommandResponse.CODE_FAILURE);
                            _processes.Add(_processFactory.CreateProcess("Scripting", currentProc,
                                currentProc.ClientContext.ProcessTable));
                            _processes[p].CommandLineName = fileName;
                            _processes[p].Arguments = arguments.Skip(1).ToArray();

                            _processes[p].ThreadError += (sender, e) =>
                            {
                                currentProc.Error.WriteLine(Red($"Error while running script: {e.Message}"));
                            };
                        }
                        else
                        {
                            _processes.Add(_processFactory.CreateProcess("Application", currentProc,
                                currentProc.ClientContext.ProcessTable));

                            _processes[p].CommandLineName = newCommand;
                            _processes[p].Arguments = arguments.Skip(1).ToArray();
                            _processes[p].ThreadError += (sender, e) =>
                            {
                                if (e is RctProcessException)
                                {
                                    currentProc.Error.WriteLine(
                                        Red("No such command, script, or built-in function exists."));
                                }
                                else
                                {
                                    currentProc.Error.WriteLine(
                                        Red($"Error while executing command: {e.Message}"));
                                }

                            };
                        }

                        //Redirect IO
                        try
                        {
                            //Seek next item. If exists, we can setup the pipe.
                            if (p <= pipedItems.Length - 1)
                            {
                                if (p > 0)
                                {
                                    if (_processes[p - 1].EnvironmentVariables.ContainsKey("PIPE"))
                                    {
                                        connectPipeToProcesses(_processes[p - 1], _processes[p]);
                                    }
                                }
                                _processes[p].EnvironmentVariables.AddVariableLocal("PIPE", "true");
                            }

                            //redirectIo(parser, currentProc);
                        }
                        catch (Exception ex)
                        {
                            currentProc.Error.WriteLine(Red($"Error while redirecting IO: {ex.Message}"));
                            return new CommandResponse(CommandResponse.CODE_FAILURE);
                        }
                    }
                    //Execute processes
                    foreach (var process in _processes)
                    {
                        process.Start();
                    }
                    //Unless otherwise specified, run the last command synchronously.
                    _processes[_processes.Count - 1].WaitForExit();

                    _processes.Clear();
                }
                return exitCode;
            }
            catch (ParserException e)
            {
                currentProc.Error.WriteLine(Red($"Error while parsing command-line: {e.Message}"));
                return new CommandResponse(CommandResponse.CODE_FAILURE);
            }
            catch (CommandElementException e)
            {
                currentProc.Error.WriteLine(Red($"Error while expanding command element: {e.Message}"));
                return new CommandResponse(CommandResponse.CODE_FAILURE);
            }
        }

        private void connectPipeToProcesses(RctProcess processA, RctProcess processB)
        {
            //Pre conditions
            if (processA == null || processB == null) return;
            AnonymousPipeServerStream server = _pipeService.OpenAnonymousPipe(PipeDirection.Out).stream;
            (int position, AnonymousPipeClientStream stream) client =
                _pipeService.ConnectToPipe(server.GetClientHandleAsString(), PipeDirection.In);
            processA.SetOut(server);
            processA.SetError(server);
            processB.SetIn(client.stream);
        }

        private List<CommandToken>[] splitToken(CommandToken[] tokens, TokenType type)
        {
            List<List<CommandToken>> splittedTokens = new List<List<CommandToken>>();
            for (int i = 0; i < tokens.Length; i++)
            {
                List<CommandToken> subTokens = new List<CommandToken>();
                for (int j = i; j < tokens.Length; j++)
                {
                    if (tokens[j].Type == type) break;
                    else subTokens.Add(tokens[j]);
                    i++;
                }
                splittedTokens.Add(subTokens);
            }

            return splittedTokens.ToArray();
        }

        //private void redirectIo(IParser parser, RctProcess currentProc)
        //{
        //    IFileSystem workingDirFileSystem = currentProc.Extensions.Find<IExtensionFileSystem>().GetFileSystem();
        //    if (parser.OutputRedirected == RedirectionMode.File)
        //    {
        //        _processes.SetOut(new StreamWriter(parser.Output, parser.OutputAppendMode));
        //    }
        //    else if (parser.OutputRedirected == RedirectionMode.VFS)
        //    {
        //        StreamWriter sw;
        //        if (parser.OutputAppendMode)
        //        {
        //            sw = new StreamWriter(
        //                workingDirFileSystem.OpenFile(parser.Output, FileMode.Append, FileAccess.Write));
        //            _processes.SetOut(sw);
        //            _engine.SetOut(sw);
        //        }
        //        else
        //        {
        //            sw = new StreamWriter(workingDirFileSystem.OpenFile(parser.Output, FileMode.Create,
        //                FileAccess.Write));
        //            _processes.SetOut(sw);
        //            _engine.SetOut(sw);
        //        }
        //    }
        //    else
        //    {
        //        _engine.SetOut(currentProc.Out);
        //        _engine.SetError(currentProc.Error);
        //    }
        //    StreamReader sr;
        //    if (parser.InputRedirected == RedirectionMode.File)
        //    {
        //        sr = new StreamReader(parser.Input);
        //        _processes.SetIn(sr);
        //        _engine.SetIn(sr);
        //    }
        //    else if (parser.InputRedirected == RedirectionMode.VFS)
        //    {
        //        sr = new StreamReader(workingDirFileSystem.OpenFile(parser.Input, FileMode.Open, FileAccess.Read));
        //        _processes.SetIn(sr);
        //        _engine.SetIn(sr);
        //    }
        //    else
        //    {
        //        _engine.SetIn(currentProc.In);
        //    }
        //}
        public override void InitializeServices(IServiceProvider kernel)
        {
            _nodeApplication = kernel.GetService<IHostApplication>();
            _processFactory = kernel.GetService<ProcessFactorySubsystem>();
            _logger = kernel.GetService<ILogger<DefaultShell>>();
            _pipeService = kernel.GetService<IPipeService>();
            _logger.LogInformation("Shell initialized.");
            _builtInCommands = new Dictionary<string, Func<CommandRequest, CommandResponse>>();
            _processes = new List<RctProcess>();
        }
    }
}