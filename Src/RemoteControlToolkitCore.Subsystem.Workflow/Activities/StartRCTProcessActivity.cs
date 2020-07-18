using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.ApplicationSystem.Factory;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Subsystem.Workflow.ActivityDesigners;

namespace RemoteControlToolkitCore.Subsystem.Workflow.Activities
{
    [Designer(typeof(StartRCTProcessActivityDesigner))]
    public sealed class StartRCTProcessActivity : CodeActivity<CommandResponse>
    {
        public InArgument<string> ProcessName { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override CommandResponse Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            string[] processName = context.GetValue(this.ProcessName).Split(' ');
            RctProcess currentProc = context.GetExtension<RctProcess>();
            ProcessFactorySubsystem subsystem =
                context.GetExtension<IServiceProvider>().GetService<ProcessFactorySubsystem>();

            RctProcess newProc = subsystem.CreateProcess("Application", currentProc,
                currentProc.ClientContext.ProcessTable);
            newProc.CommandLineName = processName[0];
            newProc.Arguments = processName.Skip(1).ToArray();

            newProc.Start();
            newProc.WaitForExit();
            newProc.Dispose();
            return newProc.ExitCode;
        }
    }
}