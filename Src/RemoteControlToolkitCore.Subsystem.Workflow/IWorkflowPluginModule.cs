using System.Activities;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Workflow
{
    public interface IWorkflowPluginModule : IPluginModule<WorkflowSubsystem>
    {
        Activity ExecuteActivity(string args, RctProcess currentProc);
    }
}