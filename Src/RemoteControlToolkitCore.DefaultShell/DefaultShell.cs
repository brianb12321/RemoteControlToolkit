﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Crayon;
using Microsoft.Extensions.Logging;
using NDesk.Options;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Commandline.TerminalExtensions;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Scripting;
using RemoteControlToolkitCore.Common.Utilities;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;
using RemoteControlToolkitCore.DefaultShell.Parsing;
using RemoteControlToolkitCore.DefaultShell.Parsing.CommandElements;

[assembly: PluginLibrary("DefaultShell", "RCT Default Shell")]
namespace RemoteControlToolkitCore.DefaultShell
{
    [Plugin(PluginName = "shell")]
    [CommandHelp("The main entry point for executing commands.")]
    public class DefaultShell : RCTApplication
    {
        private IScriptingEngine _engine;
        private ScriptingSubsystem _scriptingSubsystem;
        private IScriptExecutionContext _scriptContext;
        private ApplicationSubsystem _appSubsystem;
        private IHostApplication _nodeApplication;
        private string _motd = "I love cookies!";
        private ILogger<DefaultShell> _logger;
        private ITerminalHandler _shellExt;
        private RctProcess _process;
        private bool _promptAnyways;
        private Dictionary<string, Func<CommandRequest, CommandResponse>> _builtInCommands;
        private List<(string art, string artist)> _bannerArts;
        public override string ProcessName => "DefaultShell";

        private void loadBannerArt()
        {
            _bannerArts = new List<(string art, string artist)>
            {
                (@"
                 _
     .,-;-;-,. /'_\
   _/_/_/_|_\_\) /
 '-<_><_><_><_>=/\
   `/_/====/_/-'\_\
    ""     ""    """.Cyan(), "Joan Stark"),
                (@"
.--.
|__| .-------.
|=.| |.-----.|
|--| || KCK ||
|  | |'-----'|
|__|~')_____('
".Cyan(), "KCK"),
                (@"
.`.     _ _
__;_ \ /,//`
--, `._) (
 '//,,,  |
      )_/
     /_|
".Cyan(), "sk")
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
                    currentProc.Out.WriteLine($"Directory '{directory}' does not exist.".Red());
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
                        currentProc.EnvironmentVariables[key] = arg2.Arguments[2].ToString();
                    }
                    else
                    {
                        currentProc.EnvironmentVariables.Add(key, arg2.Arguments[2].ToString());
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
            _shellExt = currentProc.ClientContext.GetExtension<ITerminalHandler>();
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
                    currentProc.EnvironmentVariables["?"] = executeCommand(newCommand, currentProc).Code.ToString();
                    currentProc.Out.WriteLine("\u001b]e");
                }
            }

            if (string.IsNullOrWhiteSpace(command))
            {
                StringBuilder sb = new StringBuilder();
                _shellExt?.SetTitle($"RCT Shell - {currentProc.WorkingDirectory}");
                //Draw Banner
                loadBannerArt();
                currentProc.Out.Write(drawBanner().ToString());
                if(currentProc.Identity.IsInRole("Administrator")) currentProc.Out.WriteLine("WARNING: You are logged in as a server administrator.".BrightYellow());
                token.Register(() => _engine?.Dispose());
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
                        if (!currentProc.InRedirected || _promptAnyways) currentProc.Out.Write($"{currentProc.Identity.Identity.Name.BrightGreen()}{"@".BrightGreen()}{Environment.MachineName.BrightGreen()}:{currentProc.WorkingDirectory.ToString().BrightBlue()}{" $".Blue()} ");
                    }

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
                    currentProc.EnvironmentVariables["?"] = executeCommand(newCommand, currentProc).Code.ToString();
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
        private void setupScriptingEngine(TextWriter outWriter, TextWriter errorWriter, TextReader inReader, RctProcess currentProc, CancellationToken token)
        {
            _engine.ParentProcess = currentProc;
            _engine.Token = token;
            _engine.SetIn(inReader);
            _engine.SetOut(outWriter);
            _engine.SetError(errorWriter);
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
                currentProc.EnvironmentVariables["?"] = executeCommand(command, currentProc).Code.ToString();
            }
        }

        private static void CurrentProc_ControlC(object sender, ControlCEventArgs e)
        {
            RctProcess currentProc = (RctProcess) sender;
            currentProc.Child?.InvokeControlC();
            e.CloseProcess = false;
        }

        private CommandResponse executeCommand(string command, RctProcess currentProc)
        {
            if (command.StartsWith("::"))
            {
                _process = currentProc.ClientContext.ProcessTable.Factory.Create(currentProc.ClientContext, "Scripting",
                    (proc, newToken) =>
                    {
                        _engine.ParentProcess = proc;
                        _engine.Token = newToken;
                        setupScriptingEngine(proc.Out, proc.Error, proc.In, proc, newToken);
                        _engine.ExecuteString<dynamic>(command.Substring(2), _scriptContext);
                        return new CommandResponse(CommandResponse.CODE_SUCCESS);
                    }, currentProc, currentProc.Identity);
                _process.ThreadError += (sender, e) =>
                {
                    currentProc.Error.WriteLine($"Error while running script: {e.Message}".Red());
                };
                _process.SetOut(currentProc.Out);
                _process.SetError(currentProc.Error);
                _process.SetIn(currentProc.In);
                addProcessExtensions(_process);
                _process.Start();
                _process.WaitForExit();
                return _process.ExitCode;
            }
            ILexer lexer = new Lexer();
            try
            {
                IParser parser = new Parser(_engine, _scriptContext, currentProc.EnvironmentVariables);
                var lexedItems = lexer.Lex(command);
                var parsedItems = parser.Parse(lexedItems);
                CommandRequest newRequest = new CommandRequest(parsedItems[0].Select(e => e.ToString()).ToArray());
                string newCommand = newRequest.Arguments[0].ToString();
                //Check if command is built-in.
                if (_builtInCommands.ContainsKey(newCommand))
                {
                    _process = currentProc.ClientContext.ProcessTable.Factory.Create(currentProc.ClientContext, "internalCommand",
                        (proc, delToken) =>
                            _builtInCommands[newCommand](newRequest), currentProc, currentProc.Identity);
                    _process.ThreadError += (sender, e) =>
                    {
                        currentProc.Error.WriteLine($"Error while executing built-in command: {e.Message}".Red());
                    };
                }

                //Check if command should execute external program.
                else if (newCommand.StartsWith("./"))
                {
                    string fileName = newCommand.Substring(2);
                    _process = currentProc.ClientContext.ProcessTable.Factory.CreateFromScript(
                        currentProc.ClientContext, fileName, newRequest, currentProc,
                        currentProc.Extensions.Find<IExtensionFileSystem>().GetFileSystem(), _engine,
                        currentProc.Identity);

                    _process.ThreadError += (sender, e) =>
                    {
                        currentProc.Error.WriteLine(Output.Red($"Error while running script: {e.Message}"));
                    };
                }
                else
                {
                    try
                    {
                        var application = _appSubsystem.GetApplication(newCommand);
                        _process = currentProc.ClientContext.ProcessTable.Factory.CreateOnApplication(currentProc.ClientContext, application,
                            currentProc, newRequest, currentProc.Identity);
                        _process.ThreadError += (sender, e) =>
                        {
                            currentProc.Error.WriteLine(Output.Red($"Error while executing command: {e.Message}"));
                        };
                    }
                    catch (RctProcessException)
                    {
                        currentProc.Error.WriteLine(Output.Red("No such command, script, or built-in function exists."));
                        return new CommandResponse(CommandResponse.CODE_FAILURE);
                    }
                }

                //Redirect IO
                try
                {
                    redirectIo(parser, currentProc);
                }
                catch (Exception ex)
                {
                    currentProc.Error.WriteLine(Output.Red($"Error while redirecting IO: {ex.Message}"));
                    return new CommandResponse(CommandResponse.CODE_FAILURE);
                }
                addProcessExtensions(_process);
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

        private void addProcessExtensions(RctProcess process)
        {
            process.Extensions.Add(_scriptContext);
        }
        private void redirectIo(IParser parser, RctProcess currentProc)
        {
            IFileSystem workingDirFileSystem = currentProc.Extensions.Find<IExtensionFileSystem>().GetFileSystem();
            if (parser.OutputRedirected == RedirectionMode.File)
            {
                _process.SetOut(new StreamWriter(parser.Output, parser.OutputAppendMode));
            }
            else if (parser.OutputRedirected == RedirectionMode.VFS)
            {
                StreamWriter sw;
                if (parser.OutputAppendMode)
                {
                    sw = new StreamWriter(
                        workingDirFileSystem.OpenFile(parser.Output, FileMode.Append, FileAccess.Write));
                    _process.SetOut(sw);
                    _engine.SetOut(sw);
                }
                else
                {
                    sw = new StreamWriter(workingDirFileSystem.OpenFile(parser.Output, FileMode.Create,
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
            StreamReader sr;
            if (parser.InputRedirected == RedirectionMode.File)
            {
                sr = new StreamReader(parser.Input);
                _process.SetIn(sr);
                _engine.SetIn(sr);
            }
            else if (parser.InputRedirected == RedirectionMode.VFS)
            {
                sr = new StreamReader(workingDirFileSystem.OpenFile(parser.Input, FileMode.Open, FileAccess.Read));
                _process.SetIn(sr);
                _engine.SetIn(sr);
            }
            else
            {
                _engine.SetIn(currentProc.In);
            }
        }
        public override void InitializeServices(IServiceProvider kernel)
        {
            _nodeApplication = kernel.GetService<IHostApplication>();
            _logger = kernel.GetService<ILogger<DefaultShell>>();
            _logger.LogInformation("Shell initialized.");
            _builtInCommands = new Dictionary<string, Func<CommandRequest, CommandResponse>>();
            _scriptingSubsystem = kernel.GetService<ScriptingSubsystem>();
            _engine = _scriptingSubsystem.CreateEngine();
            _scriptContext = _engine.CreateContext();
            _appSubsystem = kernel.GetService<ApplicationSubsystem>();
        }
    }
}