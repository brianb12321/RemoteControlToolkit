using System;

namespace RemoteControlToolkitCore.Common.Plugin
{
    /// <summary>
    /// Represents a logical subsystem in an application. Each subsystem manages different types of <see cref="IPluginModule"/> in the system.
    /// All subsystems will use the <see cref="IPluginLibraryLoader"/> to retrieve specific modules.
    /// A subsystem can be called when a client uses a the routeSubsystem function.
    /// </summary>
    public interface IPluginSubsystem<out TModuleType> where TModuleType : IPluginModule
    {
        TModuleType[] GetAllModules();
        Type[] GetModuleTypes();
        void Init();
    }
}