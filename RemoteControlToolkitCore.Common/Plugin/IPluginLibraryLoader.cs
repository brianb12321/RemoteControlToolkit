using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace RemoteControlToolkitCore.Common.Plugin
{
    public interface IPluginLibraryLoader
    {
        event EventHandler<PluginLibrary> LibraryAdded;
        int Count { get; }
        PluginLibrary LoadFromAssembly(Assembly assembly, NetworkSide side);
        PluginLibrary[] LoadFromFolder(string folder, NetworkSide side);
        TModuleType[] GetAllModules<TModuleType>() where TModuleType : IPluginModule;
        Type[] GetModuleTypes<TModuleType>() where TModuleType : IPluginModule;
        void UnloadModule<TModule>(TModule module) where TModule : IPluginModule;
        PluginLibrary[] GetAllLibraries();
        void RunPostInit();
    }
}