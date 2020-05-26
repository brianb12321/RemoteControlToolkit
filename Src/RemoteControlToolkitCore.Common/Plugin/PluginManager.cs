using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Plugin
{
    //Not thread-safe. You really shouldn't create or destroy plugins on multiple threads.
    public class PluginManager : IPluginManager
    {
        private readonly Dictionary<string, PluginLibrary> _loadedLibraries;
        public PluginManager()
        {
            _loadedLibraries = new Dictionary<string, PluginLibrary>();
        }
        public PluginLibrary LoadPluginFile(string filePath)
        {
            PluginLibrary library = new PluginLibrary(filePath, this);
            _loadedLibraries.Add(library.UniqueName, library);
            return library;
        }

        public PluginLibrary LoadFromAssembly(Assembly assembly)
        {
            PluginLibrary library = new PluginLibrary(assembly, this);
            _loadedLibraries.Add(library.UniqueName, library);
            return library;
        }

        public PluginLibrary LoadFromType(Type type)
        {
            return LoadFromAssembly(Assembly.GetAssembly(type));
        }

        public IPluginModule<TSubsystem> ActivatePluginModule<TSubsystem>(string name) where TSubsystem : PluginSubsystem
        {
            foreach (PluginLibrary library in _loadedLibraries.Values)
            {
                try
                {
                    return library.ActivatePluginModule<TSubsystem>(name);
                }
                catch (PluginSearchException)
                {
                }
            }
            //No plugin to be activated.
            return null;
        }

        public IEnumerable<IPluginModule<TSubsystem>> ActivateAllPluginModules<TSubsystem>() where TSubsystem : PluginSubsystem
        {
            List<IPluginModule<TSubsystem>> foundModules = new List<IPluginModule<TSubsystem>>();
            foreach (PluginLibrary library in _loadedLibraries.Values)
            {
                try
                {
                    foundModules.AddRange(library.ActivateAllPluginModules<TSubsystem>());
                }
                catch (PluginSearchException)
                {
                }
            }

            return foundModules;
        }

        public PluginAttribute[] GetAllPluginModuleInformation<TType>()
        {
            List<PluginAttribute> _attributes = new List<PluginAttribute>();
            foreach (PluginLibrary library in _loadedLibraries.Values)
            {
                _attributes.AddRange(library.GetPluginAttributes<TType>());
            }

            return _attributes.ToArray();
        }

        public TType[] ActivateGenericTypes<TType>()
        {
            List<TType> types = new List<TType>();
            foreach (PluginLibrary library in _loadedLibraries.Values)
            {
                types.AddRange(library.ActivateGenericTypes<TType>());
            }

            return types.ToArray();
        }
    }
}