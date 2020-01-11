using System;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Workflow
{
    public class WorkflowSubsystem : BasePluginSubsystem<IWorkflowSubsystem, IWorkflowPluginModule>, IWorkflowSubsystem
    {
        public WorkflowSubsystem(IPluginLibraryLoader loader, IServiceProvider services) : base(loader, services)
        {
        }

        public IWorkflowPluginModule GetActivity(string activityName)
        {
            var activity = PluginLoader.ActivateModuleByName<IWorkflowPluginModule>(activityName);
            if (activity == null) throw new Exception($"The activity \"{activityName}\" does not exist.");
            return activity;
        }

        public PluginModuleAttribute[] GetInstalledActivityAttributes()
        {
            return PluginLoader.GetAllModuleAttribute<IWorkflowPluginModule>();
        }
    }
}