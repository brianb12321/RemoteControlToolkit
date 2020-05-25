using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Crayon;
using Microsoft.Extensions.DependencyInjection;
using NDesk.Options;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Commandline.Attributes;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Workflow
{
    [Plugin(PluginName = "wws")]
    [CommandHelp("Manage and execute Windows Workflow activities.")]
    public class WorkflowCommand : RCTApplication
    {
        private WorkflowSubsystem _subsystem;
        private IServiceProvider _provider;
        public override string ProcessName => "Windows Workflow Service";
        public override CommandResponse Execute(CommandRequest args, RCTProcess context, CancellationToken token)
        {
            AutoResetEvent blocker = new AutoResetEvent(false);
            string mode = "help";
            string activity = string.Empty;
            string argument = string.Empty;
            OptionSet options = new OptionSet()
                .Add("execute=", "Execute an installed activity.", v =>
                {
                    mode = "execute";
                    activity = v;
                })
                .Add("displayActivities", "Displays all the installed activities.", v => mode = "displayActivities")
                .Add("argument|e=", "Arguments to pass into the activity.", v => argument = v)
                .Add("help|?", "Display the help screen.", v => mode = "help");
            options.Parse(args.Arguments.Select(a => a.ToString()));
            if (mode == "help")
            {
                options.WriteOptionDescriptions(context.Out);
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else if (mode == "displayActivities")
            {
                foreach (var activityName in _subsystem.GetAllActivityNames())
                {
                    context.Out.WriteLine(activityName);
                }

                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else if (mode == "execute")
            {
                var activityModule = _subsystem.GetActivity(activity);
                var instance = activityModule.ExecuteActivity(argument, context);
                WorkflowApplication app = new WorkflowApplication(instance);
                app.Extensions.Add(context);
                app.Extensions.Add(_provider);
                app.OnUnhandledException = e =>
                {
                    // Display the unhandled exception.
                    context.Error.WriteLine(Output.Red(
                        $"A workflow error occurred ({e.InstanceId}): {e.UnhandledException.Message}\r\n\r\nExceptionSource: {e.ExceptionSource.DisplayName} - {e.ExceptionSourceInstanceId}"));

                    // Instruct the runtime to terminate the workflow.
                    return UnhandledExceptionAction.Abort;

                    // Other choices are UnhandledExceptionAction.Abort and 
                    // UnhandledExceptionAction.Cancel
                };
                app.Aborted = e => blocker.Set();
                app.Completed = e => blocker.Set();
                token.Register(() => app.Cancel());
                app.Run();
                blocker.WaitOne();
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
            else
            {
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }
        }

        public override void InitializeServices(IServiceProvider kernel)
        {
            _subsystem = kernel.GetService<WorkflowSubsystem>();
            _provider = kernel;
        }
    }
}