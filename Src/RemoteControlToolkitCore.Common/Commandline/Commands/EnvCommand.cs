using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Crayon.Output;
using NDesk.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [Plugin(PluginName = "env")]
    [CommandHelp("Controls the environment the application is loaded in.")]
    public class EnvCommand : RCTApplication
    {
        public override string ProcessName => "Environment Control";
        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
        {
            OptionSet options = new OptionSet();

            options.Add("commandLine", "Gets the command line for this process.",
                v => context.Out.WriteLine(Environment.CommandLine));
            options.Add("currentDirectory", "Gets or sets the fully qualified path of the current working directory.",
                v => context.Out.WriteLine(Environment.CurrentDirectory));
            options.Add("currentManagedThreadId", "Gets a unique identifier for the current managed thread.",
                v => context.Out.WriteLine(Environment.CurrentManagedThreadId));
            options.Add("processorCount", "Gets the number of processors on the current machine.",
                v => context.Out.WriteLine(Environment.ProcessorCount));
            options.Add("stackTrace", "Gets current stack trace information.",
                v => context.Out.WriteLine(Environment.StackTrace));
            options.Add("tickCount", "Gets the number of milliseconds elapsed since the system started.",
                v => context.Out.WriteLine(Environment.TickCount));
            options.Add("username", "Gets the user name of the person who is associated with the current thread.",
                v => context.Out.WriteLine(Environment.UserName));
            options.Add("exit=", "Terminates this process and returns an {EXIT} code to the operating system.",
                v => Environment.Exit(int.Parse(v)));
            options.Add("failFast=",
                "Immediately terminates a process after writing a message to the Windows Application event log, and then includes the message in error reporting to Microsoft.",
                Environment.FailFast);
            options.Add("getEnvironmentVariables:",
                "Retrieves all environment variable names and their values from the specified {LOCATION}.",
                v =>
                {
                    EnvironmentVariableTarget target = EnvironmentVariableTarget.Process;
                    if (v != null)
                        target = (EnvironmentVariableTarget) Enum.Parse(typeof(EnvironmentVariableTarget), v, true);
                    foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables(target))
                    {
                        context.Out.WriteLine($"{variable.Key}: {variable.Value}");
                    }
                });
            options.Add("getEnvironmentVariableProcess=",
                "Replaces the name of each environment variable embedded in the specified string with the string equivalent of the value of the variable, then returns the resulting string from the process scope.",
                v => context.Out.WriteLine(Environment.GetEnvironmentVariable(v, EnvironmentVariableTarget.Process)));
            options.Add("getEnvironmentVariableMachine=",
                "Replaces the name of each environment variable embedded in the specified string with the string equivalent of the value of the variable, then returns the resulting string from the machine scope.",
                v => context.Out.WriteLine(Environment.GetEnvironmentVariable(v, EnvironmentVariableTarget.Machine)));
            options.Add("getEnvironmentVariableUser=",
                "Replaces the name of each environment variable embedded in the specified string with the string equivalent of the value of the variable, then returns the resulting string from the user scope.",
                v => context.Out.WriteLine(Environment.GetEnvironmentVariable(v, EnvironmentVariableTarget.User)));
            options.Add("help|?", "Display the help screen.", v => options.WriteOptionDescriptions(context.Out));
            try
            {
                options.Parse(args.Arguments);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            catch (Exception e)
            {
                context.Out.WriteLine(Red($"An error occurred while performing an environment action: {e.Message}"));
                return new CommandResponse(CommandResponse.CODE_FAILURE);
            }
        }
    }
}