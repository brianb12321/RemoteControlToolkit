using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsWorkflow.Activities;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Subsystem.Workflow;

namespace WindowsWorkflow.ActivityModules
{
    [Plugin(PluginName = "TestActivity")]
    public class TestActivityModule : PluginModule<WorkflowSubsystem>, IWorkflowPluginModule
    {

        public Activity ExecuteActivity(string arg, RctProcess currentProc)
        {
            return new TestActivity();
        }
    }
}