using System.Activities;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Workflow
{
    [ModuleInstance(TransientMode = true)]
    public interface IWorkflowPluginModule : IPluginModule
    {
        Activity ExecuteActivity(string args, RCTProcess currentProc);
    }
}