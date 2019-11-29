using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace RemoteControlToolkitCore.Common.Plugin
{
    public class PluginLibrary
    {
        public string Name { get; private set; }
        public string FriendlyName { get; private set; }
        public NetworkSide LibraryType { get; private set; }
        public Version Version { get; private set; }
        public Guid UniqueID { get; private set; }
        public IPluginModule[] Modules { get; private set; }
        private ILibraryStartup _startup;
        private Assembly _assembly;
        private PluginLibrary(string name, string friendlyName, Version version, Guid id, NetworkSide type, IPluginModule[] modules, ILibraryStartup startup, Assembly assembly)
        {
            Name = name;
            FriendlyName = friendlyName;
            Version = version;
            UniqueID = id;
            LibraryType = type;
            Modules = modules;
            _startup = startup;
            _assembly = assembly;
        }
        public void RunPostInit()
        {
            _startup?.PostInit();
        }

        public Assembly GetAssembly()
        {
            return _assembly;
        }

        public bool UnloadModule<TModule>(TModule module) where TModule : IPluginModule
        {
            var modules = Modules.ToList();
            if (modules.Contains(module))
            {
                modules.Remove(module);
                Modules = modules.ToArray();
                return true;
            }
            else
            {
                return false;
            }
        }
        public static PluginLibrary LoadFromAssembly(Assembly a, NetworkSide side)
        {
            PluginLibraryAttribute attrib = a.GetCustomAttribute<PluginLibraryAttribute>();
            if (attrib != null)
            {
                if (!attrib.LibraryType.HasFlag(side))
                {
                    throw new InvalidPluginLibraryException($"Assembly: \"{a.GetName().FullName}\" could not be loaded: Extension \"{attrib.FriendlyName}\" requires a network side of {attrib.LibraryType.ToString()}");
                }
                ILibraryStartup startup = null;
                if (attrib.Startup != null)
                {
                    startup = (ILibraryStartup)Activator.CreateInstance(attrib.Startup);
                }
                List<IPluginModule> _modules = new List<IPluginModule>();
                foreach (IPluginModule module in a.GetTypes().Where(
                    t => t.IsClass && !t.IsAbstract
                                   && typeof(IPluginModule).IsAssignableFrom(t)
                                   && t.GetCustomAttribute<PluginModuleAttribute>() != null
                                   && (t.GetCustomAttribute<ModuleInstanceAttribute>() == null || t.GetCustomAttribute<ModuleInstanceAttribute>().TransientMode != true))
                    .Select(t => (IPluginModule)Activator.CreateInstance(t)))
                {
                    if (module.GetType().GetCustomAttribute<PluginModuleAttribute>().ExecutingSide.HasFlag(side))
                        _modules.Add(module);
                }
                return new PluginLibrary(attrib.Name, attrib.FriendlyName, a.GetName().Version, Guid.TryParse(attrib.Guid, out Guid value) ? value : Guid.NewGuid(), attrib.LibraryType, _modules.ToArray(), startup, a);
            }
            else
            {
                throw new InvalidPluginLibraryException($"Assembly: \"{a.GetName().FullName}\" could not be loaded: PluginLibraryAttribute missing.");
            }
        }
    }
}