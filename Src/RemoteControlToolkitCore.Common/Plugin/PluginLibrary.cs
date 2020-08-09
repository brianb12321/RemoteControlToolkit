using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RemoteControlToolkitCore.Common.Plugin
{
    /// <summary>
    /// Represents the currently loaded plugin library assembly.
    /// </summary>
    public class PluginLibrary
    {
        private Assembly _pluginAssembly;
        private readonly IPluginManager _parentPluginManager;
        private PluginLibraryAttribute _pluginLibraryAttribute;
        public string DisplayName => _pluginLibraryAttribute.DisplayName;
        public string UniqueName => _pluginLibraryAttribute.UniqueName;

        public PluginLibrary(string pluginAssembly, IPluginManager parentPluginManager)
        {
            //Convert path to absolute.
            string absolutePath = Path.GetFullPath(pluginAssembly);

            //Check for illegal arguments.
            if (string.IsNullOrWhiteSpace(pluginAssembly))
                throw new ArgumentException("An assembly must be provided to load a new plugin library.",
                    nameof(pluginAssembly));

            //Load assembly
            Assembly assembly = Assembly.LoadFrom(absolutePath);
            if (checkIfAssemblyIsValid(assembly))
            {
                loadAssembly(assembly);
                _parentPluginManager = parentPluginManager;
            }
            else
            {
                throw new PluginLoadException($"Plugin file \"{assembly.GetName().Name}\" does not have PluginLibraryAttribute.");
            }
        }

        private bool checkIfAssemblyIsValid(Assembly assembly)
        {
            var attribute = getPluginLibraryAttribute(assembly);
            return attribute != null;
        }
        private void loadAssembly(Assembly assembly)
        {
            //Get plugin attribute
            var attribute = getPluginLibraryAttribute(assembly);
            //Attribute exists
            if (attribute != null)
            {
                _pluginAssembly = assembly;
                _pluginLibraryAttribute = attribute;
            }
            else
            {
                throw new PluginLoadException($"Plugin file \"{assembly.GetName().Name}\" does not have PluginLibraryAttribute.");
            }
        }

        public PluginLibrary(Assembly assembly, IPluginManager parentPluginManager)
        {
            loadAssembly(assembly);
            _parentPluginManager = parentPluginManager;
        }

        private PluginLibraryAttribute getPluginLibraryAttribute(Assembly assembly)
        {
            PluginLibraryAttribute attribute = assembly.GetCustomAttribute<PluginLibraryAttribute>();
            return attribute;
        }
        /// <summary>
        /// Gets all plugin attributes from the library.
        /// </summary>
        /// <returns>All enumerated plugin attributes.</returns>
        public IEnumerable<PluginAttribute> GetPluginInformation()
        {
            return _pluginAssembly.GetCustomAttributes<PluginAttribute>();
        }
        /// <summary>
        /// Gets a plugin attribute based on the module's unique name. The first name matched will be returned.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>The associated plugin attribute.</returns>
        public PluginAttribute GetPluginAttributeByName(string name)
        {
            return GetPluginInformation().FirstOrDefault(i => i.PluginName == name);
        }

        /// <summary>
        /// Gets all plugin attributes associated with the specified module type.
        /// </summary>
        /// <typeparam name="TType">The module type to look for associated plugin attributes.</typeparam>
        /// <returns>All the plugin attributes found.</returns>
        public IEnumerable<PluginAttribute> GetPluginAttributes<TType>()
        {
            List<PluginAttribute> attributes = new List<PluginAttribute>();
            foreach (Type type in _pluginAssembly.GetTypes())
            {
                if (typeof(TType).IsInterface)
                {
                    if (typeof(TType).IsAssignableFrom(type))
                    {
                        PluginAttribute currentAttribute = type.GetCustomAttribute<PluginAttribute>();
                        if(currentAttribute != null) attributes.Add(currentAttribute);
                    }
                }
            }

            return attributes;
        }
        /// <summary>
        /// Returns all type information associated with the name of the module being searched for. All types will be returned.
        /// </summary>
        /// <param name="moduleName">The name of the unique module to search for.</param>
        /// <returns>All types associated with the unique name.</returns>
        public IEnumerable<Type> ModuleExists(string moduleName)
        {
            foreach (Type type in _pluginAssembly.GetTypes())
            {
                PluginAttribute attribute = type.GetCustomAttribute<PluginAttribute>();
                if (attribute != null && attribute.PluginName == moduleName) yield return type;
            }
        }
        /// <summary>
        /// Activates all <see cref="TType"/> that have a <see cref="PluginAttribute"/>
        /// </summary>
        /// <typeparam name="TType">The type to activate.</typeparam>
        /// <returns>The activated types.</returns>
        public IEnumerable<TType> ActivateGenericTypes<TType>()
        {
            List<TType> foundTypes = new List<TType>();
            try
            {
                foreach (Type t in _pluginAssembly.GetTypes())
                {
                    //Check if TType is interface.
                    if (typeof(TType).IsInterface)
                    {
                        if (typeof(TType).IsAssignableFrom(t) &&
                            t.GetCustomAttribute<PluginAttribute>() != null)
                            foundTypes.Add((TType)Activator.CreateInstance(t));
                    }
                }
            }
            catch (ReflectionTypeLoadException e)
            {
                throw new PluginSearchException(buildReflectionError($"Plugin activation failed for assembly \"{_pluginAssembly.GetName().FullName}\":", e), e);
            }

            return foundTypes;
        }

        public Type[] GetTypeByType<TType>()
        {
            List<Type> foundTypes = new List<Type>();
            foreach (Type t in _pluginAssembly.GetTypes())
            {
                //Check if TType is interface.
                if (typeof(TType).IsInterface)
                {
                    if(typeof(TType).IsAssignableFrom(t) &&
                       t.GetCustomAttribute<PluginAttribute>() != null)
                    foundTypes.Add(t);
                }
            }

            return foundTypes.ToArray();
        }

        private string buildReflectionError(string message, ReflectionTypeLoadException ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{message}\n");
            foreach (Exception inner in ex.LoaderExceptions)
            {
                sb.AppendLine(inner.Message);
            }

            return sb.ToString();
        }
        /// <summary>
        /// Activates all plugin modules associated with <see cref="TSubsystem"/>
        /// </summary>
        /// <typeparam name="TSubsystem">The subsystem to search for associated plugin modules.</typeparam>
        /// <returns>All activated plugin modules.</returns>
        public IEnumerable<IPluginModule<TSubsystem>> ActivateAllPluginModules<TSubsystem>() where TSubsystem : PluginSubsystem
        {
            //TODO: This may not work.
            return ActivateGenericTypes<IPluginModule<TSubsystem>>();
        }

        /// <summary>
        /// Activates a specific plugin module based on its unique name.
        /// </summary>
        /// <typeparam name="TSubsystem">The subsystem to which the plugin module is associated with.</typeparam>
        /// <param name="name">The name of the module to activate.</param>
        /// <returns>The resulting activated module.</returns>
        public IPluginModule<TSubsystem> ActivatePluginModule<TSubsystem>(string name)
            where TSubsystem : PluginSubsystem
        {
            //Iterate through all types that matches name.
            Type type = ModuleExists(name).FirstOrDefault();
            if (type != null)
            {
                PluginAttribute attribute = type.GetCustomAttribute<PluginAttribute>();
                IPluginModule<TSubsystem> pluginModule = (IPluginModule<TSubsystem>)Activator.CreateInstance(type);
                return pluginModule;
            }
            //If no matches found.
            throw new PluginSearchException(
                $"Plugin module \"{name}\" could not be activated: does not exist in library \"{UniqueName}\"");
        }
    }
}