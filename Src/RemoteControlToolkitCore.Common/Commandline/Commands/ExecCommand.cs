using System;
using System.Linq;
using System.Threading;
using Crayon;
using IronPython.Modules;
using Microsoft.Extensions.DependencyInjection;
using NDesk.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.ApplicationSystem.Factory;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;

namespace RemoteControlToolkitCore.Common.Commandline.Commands
{
    [Plugin(PluginName = "exec")]
    [CommandHelp("Executes a program using the process factory.")]
    public class ExecCommand : RCTApplication
    {
        private ProcessFactorySubsystem _processFactorySubsystem;
        public override void InitializeServices(IServiceProvider provider)
        {
            _processFactorySubsystem = provider.GetService<ProcessFactorySubsystem>();
        }

        public override string ProcessName => "Exec Command";
        public override CommandResponse Execute(CommandRequest args, RctProcess context, CancellationToken token)
        {
            bool showHelp = false;
            string module = "Application";
            OptionSet options = new OptionSet()
                .Add("showHelp|?", "Displays the help screen.", v => showHelp = true)
                .Add("module|m=", "The process factory module to use. Default is Application", v => module = v);

            var fileAndArguments = options.Parse(args.Arguments).Skip(1);
            if (showHelp)
            {
                options.WriteOptionDescriptions(context.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else
            {
                var process = _processFactorySubsystem.CreateProcess(module,
                    new CommandRequest(fileAndArguments.ToArray()), context, context.ClientContext.ProcessTable);
                process.ThreadError += (sender, e) =>
                    context.Error.WriteLine($"Error while running process: {e.Message}".Red());
                process.SetIn(context.In);
                process.SetOut(context.Out);
                process.SetError(context.Error);
                process.Start();
                process.WaitForExit();
                return process.ExitCode;
            }
        }
    }
}