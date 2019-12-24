using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NDesk.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Utilities;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [PluginModule(Name = "eps", ExecutingSide = NetworkSide.Server | NetworkSide.Proxy)]
    [CommandHelp("Manages all external processes on the server.")]
    public class EpsCommand : RCTApplication
    {
        public override string ProcessName => "External Process";
        public override CommandResponse Execute(CommandRequest args, RCTProcess context, CancellationToken token)
        {
            string mode = "showAll";
            string name = string.Empty;
            string processArgs = string.Empty;
            bool useShellExecute = true;
            bool redirectStandardIn = false;
            bool redirectStandardOut = false;
            bool redirectStandardError = false;
            bool runAsAdministrator = false;
            bool waitForExit = false;
            bool runAsRCTProcess = false;
            bool createNoWindow = false;
            OptionSet options = new OptionSet()
                .Add("showAll", "Displays all the running processes on the server.", v => mode = "showAll")
                .Add("execute=", "Starts a new process.", v =>
                {
                    mode = "execute";
                    name = v;
                })
                .Add("debugMode", "Enters the server into OS debug mode.", v => mode = "debugMode")
                .Add("exitDebugMode", "Exits the server from OS debug mode.", v => mode = "exitDebugMode")
                .Add("arguments|a=", "The arguments to send to a process.", v => processArgs = v)
                .Add("shellExecute|s", "Executes a new process using the OS shell.", v => useShellExecute = true)
                .Add("createNoWindow|n", "Do not create a window when executing a process from the OS shell.", v => createNoWindow = true)
                .Add("redirectStdIn|i", "Redirects standard to RCT standard in. ShellExecute must be false.", v => redirectStandardIn = true)
                .Add("redirectStdOut|o", "Redirects standard out to RCT standard out. ShellExecute must be false.", v => redirectStandardOut = true)
                .Add("redirectStdError|e", "Redirects standard error to RCT standard error. ShellExecute must be false.", v => redirectStandardError = true)
                .Add("runAsAdmin|r", "Runs the process as an administrator.", v => runAsAdministrator = true)
                .Add("runAsRCTProcess|p", "Creates a new RCT process and register it into the connection table.", v => runAsRCTProcess = true)
                .Add("waitForExit|w", "Waits for the process to exit.", v => waitForExit = true)
                .Add("kill=", "Kills the specified PID.", v =>
                {
                    mode = "kill";
                    name = v;
                })
                .Add("help|?", "Displays the help screen.", v => mode = "help");

            options.Parse(args.Arguments.Select(a => a.ToString()));
            if (mode == "showAll")
            {
                var processes = Process.GetProcesses();
                context.Out.WriteLine(processes.ToDictionary(p => p.Id).ShowDictionary(p => p.ProcessName));
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else if (mode == "execute")
            {
                Process p = new Process();
                p.StartInfo.FileName = name;
                p.StartInfo.Arguments = processArgs;
                p.StartInfo.UseShellExecute = useShellExecute;
                p.StartInfo.RedirectStandardError = redirectStandardError;
                p.StartInfo.RedirectStandardOutput = redirectStandardOut;
                p.StartInfo.RedirectStandardInput = redirectStandardIn;
                if (redirectStandardOut) p.OutputDataReceived += (sender, e) => context.Out.WriteLine(e.Data);
                if (redirectStandardError) p.ErrorDataReceived += (sender, e) => context.Out.WriteLine(e.Data);
                if (runAsAdministrator) p.StartInfo.Verb = "runas";
                if (useShellExecute) p.StartInfo.CreateNoWindow = createNoWindow;
                if (runAsRCTProcess)
                {
                    RCTProcess proc = context.ClientContext.ProcessTable.Factory.Create(context.ClientContext, $"Ext - {name}",
                        (current, cancellationToken) =>
                        {
                            string text = string.Empty;
                            if (redirectStandardIn)
                            {
                                text = current.In.ReadToEnd();
                            }
                            p.Start();
                            if(redirectStandardIn) p.StandardInput.WriteLine(text);
                            if(redirectStandardOut) p.BeginOutputReadLine();
                            if(redirectStandardError) p.BeginErrorReadLine();
                            if (waitForExit)
                            {
                                token.Register(() => p.Kill());
                                p.WaitForExit();
                            }
                            return new CommandResponse((p.HasExited) ? p.ExitCode : 0);
                        }, context);
                    proc.Start();
                    proc.WaitForExit();
                    return proc.ExitCode;
                }
                else
                {
                    string text = string.Empty;
                    if (redirectStandardIn)
                    {
                        text = context.In.ReadToEnd();
                    }
                    p.Start();
                    if (redirectStandardIn) p.StandardInput.WriteLine(text);
                    if (redirectStandardOut) p.BeginOutputReadLine();
                    if (redirectStandardError) p.BeginErrorReadLine();
                    if (waitForExit)
                    {
                        token.Register(() => p.Kill());
                        p.WaitForExit();
                    }
                    return new CommandResponse((p.HasExited) ? p.ExitCode : 0);
                }
            }
            else if (mode == "debugMode")
            {
                Process.EnterDebugMode();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else if (mode == "exitDebugMode")
            {
                Process.LeaveDebugMode();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else if (mode == "kill")
            {
                int pid = int.Parse(name);
                Process.GetProcessById(pid).Kill();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else
            {
                options.WriteOptionDescriptions(context.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            
        }
    }
}