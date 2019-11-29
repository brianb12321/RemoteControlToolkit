using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RemoteControlToolkitCore.Common.Plugin
{
    public class DefaultPluginLoader : IPluginLibraryLoader
    {
        List<PluginLibrary> _libs = new List<PluginLibrary>();
        ILogger<DefaultPluginLoader> _logger;
        public DefaultPluginLoader(ILogger<DefaultPluginLoader> logger)
        {
            _logger = logger;
        }

        public int Count => _libs.Count;

        public event EventHandler<PluginLibrary> LibraryAdded;

        public Type[] GetModuleTypes<TModuleType>() where TModuleType : IPluginModule
        {
            List<Type> result = new List<Type>();
            foreach (PluginLibrary libs in _libs)
            {
                foreach (Type t in libs.GetAssembly().GetTypes().Where(t => t.IsClass && typeof(TModuleType).IsAssignableFrom(t)))
                {
                    result.Add(t);
                }
            }
            return result.ToArray();
        }

        public void UnloadModule<TModule>(TModule module) where TModule : IPluginModule
        {
            bool moduleDeleted = false;
            foreach (var library in _libs)
            {
                if (library.UnloadModule(module))
                {
                    moduleDeleted = true;
                }
            }

            if (!moduleDeleted)
            {
                throw new ArgumentException("The specified module cannot be unloaded: The module does not exist.");
            }
        }

        public PluginLibrary[] GetAllLibraries()
        {
            return _libs.ToArray();
        }

        public TModuleType[] GetAllModules<TModuleType>() where TModuleType : IPluginModule
        {
            List<TModuleType> result = new List<TModuleType>();
            foreach (PluginLibrary libs in _libs)
            {
                foreach (IPluginModule module in libs.Modules)
                {
                    if (module is TModuleType)
                    {
                        result.Add((TModuleType)module);
                    }
                }
            }
            return result.ToArray();
        }

        public PluginLibrary LoadFromAssembly(Assembly assembly, NetworkSide side)
        {
            try
            {
                PluginLibrary loadedLib = PluginLibrary.LoadFromAssembly(assembly, side);
                LibraryAdded?.Invoke(this, loadedLib);
                _libs.Add(loadedLib);
                return loadedLib;
            }
            catch (InvalidPluginLibraryException ex)
            {
                _logger.LogWarning($"An error occured loading plugin library \"{assembly.GetName().FullName}\": {ex.Message}");
                return null;
            }
        }

        public PluginLibrary[] LoadFromFolder(string folder, NetworkSide side)
        {
            List<PluginLibrary> _libs = new List<PluginLibrary>();
            if (Directory.Exists(folder))
            {
                foreach (string file in Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories))
                {
                    _logger.LogInformation($"Found extension library \"{Path.GetFileName(file)}\"");
                    _libs.Add(LoadFromAssembly(Assembly.LoadFrom(file), side));
                }
                return _libs.ToArray();
            }
            else
            {
                _logger.LogWarning($"{folder} does not exist.");
                return null;
            }
        }

        public void RunPostInit()
        {
            _libs.ForEach(l => l.RunPostInit());
        }
    }
}