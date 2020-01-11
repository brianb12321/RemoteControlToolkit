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
    [PluginModule(Name = "TestActivity", ExecutingSide = NetworkSide.Server)]
    public class TestActivityModule : IWorkflowPluginModule
    {
        public void InitializeServices(IServiceProvider kernel)
        {
            
        }

        public Activity ExecuteActivity(string arg, RCTProcess currentProc)
        {
            return new TestActivity();
        }
    }
}