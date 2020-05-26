using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Plugin
{
    /// <summary>
    /// Main class for storing and managing all registered plugins and its lifecycle.
    /// </summary>
    public interface IPluginManager
    {
        PluginLibrary LoadPluginFile(string filePath);
        PluginLibrary LoadFromAssembly(Assembly assembly);
        PluginLibrary LoadFromType(Type type);
        IPluginModule<TSubsystem> ActivatePluginModule<TSubsystem>(string name) where TSubsystem : PluginSubsystem;

        IEnumerable<IPluginModule<TSubsystem>> ActivateAllPluginModules<TSubsystem>()
            where TSubsystem : PluginSubsystem;
        PluginAttribute[] GetAllPluginModuleInformation<TType>();
        TType[] ActivateGenericTypes<TType>();
    }
}