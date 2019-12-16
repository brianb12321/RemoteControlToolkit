using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RemoteControlToolkitCore.Common.Plugin
{
    public abstract class BasePluginSubsystem<TSystem, TModule> : IPluginSubsystem<TModule>
         where TSystem : IPluginSubsystem<TModule>
         where TModule : class, IPluginModule
    {

        protected IPluginLibraryLoader PluginLoader { get; private set; }
        private readonly IServiceProvider _services;
        private ILogger<BasePluginSubsystem<TSystem, TModule>> _logger;
        protected BasePluginSubsystem(IPluginLibraryLoader loader, IServiceProvider services)
        {
            PluginLoader = loader;
            _services = services;
            _logger = services.GetService<ILogger<BasePluginSubsystem<TSystem, TModule>>>();
        }
        public TModule[] GetAllModules()
        {
            return PluginLoader.GetAllModules<TModule>();
        }

        public Type[] GetModuleTypes()
        {
            return PluginLoader.GetModuleTypes<TModule>();
        }

        public virtual void Init()
        {
            _logger.LogInformation("Initializing plugins.");
            PluginLoader.GetAllModules<TModule>().ToList().ForEach(m => m.InitializeServices(_services));
        }
    }
}