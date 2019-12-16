using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Workflow
{
    /// <summary>
    /// Acts as an subsystem for the Windows Workflow system.
    /// </summary>
    public interface IWorkflowSubsystem : IPluginSubsystem<IWorkflowPluginModule>
    {
        IWorkflowPluginModule GetActivity(string activityName);
        PluginModuleAttribute[] GetInstalledActivityAttributes();
    }
}