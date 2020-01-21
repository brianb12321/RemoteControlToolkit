using System;
using System.Reflection;
using Community.CsharpSqlite;
using Microsoft.Extensions.DependencyInjection;

namespace RemoteControlToolkitCore.Common.Plugin
{
    public interface IPluginLibraryLoader
    {
        event EventHandler<PluginLibrary> LibraryAdded;
        int Count { get; }
        PluginLibrary LoadFromAssembly(Assembly assembly, NetworkSide side);
        PluginLibrary[] LoadFromFolder(string folder, NetworkSide side);
        PluginModuleAttribute GetModuleAttributeByName<TModuleType>(string name) where TModuleType : class, IPluginModule;
        PluginModuleAttribute[] GetAllModuleAttribute<TModuleType>() where TModuleType : class, IPluginModule;
        TModuleType ActivateModuleByName<TModuleType>(string name) where TModuleType : class, IPluginModule;
        TModuleType[] ActivateAll<TModuleType>() where TModuleType : class, IPluginModule;
        bool HasPluginModule<TModuleType>(string name) where TModuleType : class, IPluginModule;
        TModuleType[] GetAllModules<TModuleType>() where TModuleType : class, IPluginModule;
        Type[] GetModuleTypes<TModuleType>() where TModuleType : class, IPluginModule;
        Type GetModuleTypeByName<TModuleType>(string name) where TModuleType : class, IPluginModule;
        void UnloadModule<TModule>(TModule module) where TModule : class, IPluginModule;
        PluginLibrary[] GetAllLibraries();
    }
}