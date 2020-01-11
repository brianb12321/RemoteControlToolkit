using System;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public interface IApplicationSubsystem : IPluginSubsystem<IApplication>, IProcessTable
    {
        IApplication GetApplication(string name);
        Type GetApplicationType(string name);
        PluginModuleAttribute[] GetAllInstalledApplications();
        bool ApplicationExists(string name);
    }
}