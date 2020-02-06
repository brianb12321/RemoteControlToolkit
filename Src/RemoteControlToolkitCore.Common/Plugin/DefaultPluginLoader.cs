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
        private IHostApplication _application;
        private IServiceProvider _provider;
        public DefaultPluginLoader(ILogger<DefaultPluginLoader> logger, IHostApplication application, IServiceProvider provider)
        {
            _logger = logger;
            _application = application;
            _provider = provider;
        }

        public int Count => _libs.Count;

        public event EventHandler<PluginLibrary> LibraryAdded;

        public Type[] GetModuleTypes<TModuleType>() where TModuleType : class, IPluginModule
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

        public Type GetModuleTypeByName<TModuleType>(string name) where TModuleType : class, IPluginModule
        {
            return GetModuleTypes<TModuleType>()
                .FirstOrDefault(t => t.GetCustomAttribute<PluginModuleAttribute>() != null && t.GetCustomAttribute<PluginModuleAttribute>().Name == name && t.GetCustomAttribute<PluginModuleAttribute>().ExecutingSide.HasFlag(_application.ExecutingSide));
        }

        public void UnloadModule<TModule>(TModule module) where TModule : class, IPluginModule
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

        public PluginModuleAttribute GetModuleAttributeByName<TModuleType>(string name) where TModuleType : class, IPluginModule
        {
            var type = GetModuleTypes<TModuleType>().FirstOrDefault(app =>
                app.GetCustomAttribute<PluginModuleAttribute>() != null
                && app.GetCustomAttribute<PluginModuleAttribute>().Name == name
                && app.GetCustomAttribute<PluginModuleAttribute>().ExecutingSide.HasFlag(_application.ExecutingSide));
            if (type != null) return type.GetCustomAttribute<PluginModuleAttribute>();
            else return null;
        }

        public PluginModuleAttribute[] GetAllModuleAttribute<TModuleType>() where TModuleType : class, IPluginModule
        {
            return GetModuleTypes<TModuleType>()
                .Select(t => t.GetCustomAttribute<PluginModuleAttribute>())
                .Where(a => a.ExecutingSide.HasFlag(_application.ExecutingSide))
                .ToArray();
        }
        public TModuleType[] ActivateAll<TModuleType>() where TModuleType : class, IPluginModule
        {
            List<TModuleType> _modules = new List<TModuleType>();
            Type[] pluginTypes = GetModuleTypes<TModuleType>();
            foreach (Type type in pluginTypes)
            {
                var module = (TModuleType) Activator.CreateInstance(type);
                module.InitializeServices(_provider);
                _modules.Add(module);
            }

            return _modules.ToArray();
        }
        public TModuleType ActivateModuleByName<TModuleType>(string name) where TModuleType : class, IPluginModule
        {
            bool pluginExists = HasPluginModule<TModuleType>(name);
            Type pluginType = GetModuleTypeByName<TModuleType>(name);
            //Check if an external IApplication object can execute the command.
            if (!pluginExists)
            {
                return null;
            }

            var app = (TModuleType)Activator.CreateInstance(pluginType);
            app.InitializeServices(_provider);
            return app;
        }

        public bool HasPluginModule<TModuleType>(string name) where TModuleType : class, IPluginModule
        {
            return GetModuleAttributeByName<TModuleType>(name) != null;
        }

        public TModuleType[] GetAllModules<TModuleType>() where TModuleType : class, IPluginModule
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
                if (_libs.Count(l => l.GetAssembly() == assembly) == 0)
                {
                    PluginLibrary loadedLib = PluginLibrary.LoadFromAssembly(assembly, side);
                    LibraryAdded?.Invoke(this, loadedLib);
                    _libs.Add(loadedLib);
                    return loadedLib;
                }
                else
                {
                    _logger.LogInformation($"Assembly: \"{assembly.GetName().Name}\" already loaded.");
                }
                return null;
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
                try
                {
                    foreach (string file in Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories))
                    {
                        _logger.LogInformation($"Found extension library \"{Path.GetFileName(file)}\"");
                        _libs.Add(LoadFromAssembly(Assembly.LoadFrom(file), side));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error while enumerating directory: {ex.Message}");
                }
                return _libs.ToArray();
            }
            else
            {
                _logger.LogWarning($"{folder} folder does not exist.");
                return null;
            }
        }
    }
}