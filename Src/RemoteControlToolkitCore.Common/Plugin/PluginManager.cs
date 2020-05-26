using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RemoteControlToolkitCore.Common.Plugin
{
    //Not thread-safe. You really shouldn't create or destroy plugins on multiple threads.
    public class PluginManager : IPluginManager
    {
        private readonly Dictionary<string, PluginLibrary> _loadedLibraries;
        private readonly ILogger<PluginManager> _logger;
        public PluginManager(ILogger<PluginManager> logger)
        {
            _logger = logger;
            _loadedLibraries = new Dictionary<string, PluginLibrary>();
        }
        public PluginLibrary LoadPluginFile(string filePath)
        {
            try
            {
                _logger.LogInformation($"Loading plugin file from \"{Path.GetFullPath(filePath)}\"");
                PluginLibrary library = new PluginLibrary(filePath, this);
                _logger.LogInformation($"Plugin file \"{library.DisplayName}\" successfully loaded.");
                _loadedLibraries.Add(library.UniqueName, library);
                return library;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Unable to load plugin file: {e.Message}");
                throw;
            }
        }

        public PluginLibrary LoadFromAssembly(Assembly assembly)
        {
            try
            {
                PluginLibrary library = new PluginLibrary(assembly, this);
                _loadedLibraries.Add(library.UniqueName, library);
                return library;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Unable to load plugin assembly: {e.Message}");
                throw;
            }
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