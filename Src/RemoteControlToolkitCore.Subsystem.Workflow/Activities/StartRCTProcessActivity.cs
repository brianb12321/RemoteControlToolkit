using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Subsystem.Workflow.ActivityDesigners;

namespace RemoteControlToolkitCore.Subsystem.Workflow.Activities
{
    [Designer(typeof(StartRCTProcessActivityDesigner))]
    public sealed class StartRCTProcessActivity : CodeActivity<CommandResponse>
    {
        public InArgument<CommandRequest> Request { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override CommandResponse Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            CommandRequest request = context.GetValue(this.Request);
            RCTProcess currentProc = context.GetExtension<RCTProcess>();
            IApplicationSubsystem subsystem =
                context.GetExtension<IServiceProvider>().GetService<IApplicationSubsystem>();

            RCTProcess newProc = currentProc.ClientContext.ProcessTable.Factory.CreateOnApplication(
                currentProc.ClientContext, subsystem.GetApplication(request.Arguments[0].ToString()), currentProc,
                request, currentProc.Identity);

            newProc.Start();
            newProc.WaitForExit();
            newProc.Dispose();
            return newProc.ExitCode;
        }
    }
}