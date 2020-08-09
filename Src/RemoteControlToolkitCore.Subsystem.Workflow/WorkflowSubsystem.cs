using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Workflow
{
    public class WorkflowSubsystem : PluginSubsystem
    {
        public WorkflowSubsystem(IPluginManager pluginManager, IServiceProvider services) : base(pluginManager)
        {
        }

        public string[] GetAllActivityNames()
        {
            return PluginManager.GetAllPluginModuleInformation<IWorkflowPluginModule>()
                .Select(i => i.PluginName)
                .ToArray();
        }
        public IWorkflowPluginModule GetActivity(string activityName)
        {
            var activity = (IWorkflowPluginModule)PluginManager.ActivatePluginModule<WorkflowSubsystem>(activityName);
            if (activity == null) throw new Exception($"The activity \"{activityName}\" does not exist.");
            return activity;
        }
    }
}