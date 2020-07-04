using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.NSsh.Configuration;
using RemoteControlToolkitCore.Common.NSsh.Services;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common
{
    public class AppBuilder : IAppBuilder
    {
        private readonly List<IApplicationStartup> _startups;
        private readonly IServiceCollection _services;
        protected IPluginManager _pluginManager;
        private ILoggerFactory _loggerFactory;
        public NetworkSide ExecutingSide => NetworkSide.Server;

        public AppBuilder()
        {
            _startups = new List<IApplicationStartup>();
            _services = new ServiceCollection();
        }

        protected virtual IHostApplication InjectHostApplication(IServiceProvider provider)
        {
            return new Application(provider.GetService<ILogger<Application>>(),
                provider.GetService<ILogger<ProxyNetworkInstance>>(),
                provider, ExecutingSide, this,
                provider.GetService<IKeySetupService>(),
                _pluginManager,
                provider.GetService<NSshServiceConfiguration>());
        }
        public IHostApplication Build()
        {
            _startups.ForEach(s => s.ConfigureServices(_services));
            _services.AddSingleton(InjectHostApplication);
            var serviceProvider = _services.BuildServiceProvider();
            var hostApplication = serviceProvider.GetService<IHostApplication>();
            _startups.ForEach(s => s.PostConfigureServices(serviceProvider, hostApplication));
            _services.BuildServiceProvider();
            return hostApplication;
        }

        public IAppBuilder UsePluginManager<TPluginManagerImpl>() where TPluginManagerImpl : IPluginManager
        {
            ConstructorInfo constructor =
                typeof(TPluginManagerImpl).GetConstructor(new Type[] {typeof(ILogger<TPluginManagerImpl>)});
            //Check if plugin manager requires a logger.
            if (constructor != null)
            {
                _pluginManager = (IPluginManager) Activator.CreateInstance(typeof(TPluginManagerImpl), _loggerFactory.CreateLogger<TPluginManagerImpl>());
            }
            else
            {
                _pluginManager = (IPluginManager) Activator.CreateInstance(typeof(TPluginManagerImpl));
            }

            return this;
        }

        public IAppBuilder ConfigureLogging(Action<ILoggingBuilder> factory)
        {
            _loggerFactory = LoggerFactory.Create(factory);
            //Add the logging factory to DI.
            _services.TryAdd(ServiceDescriptor.Singleton(_loggerFactory));
            _services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
            //Add all the other crap into the container.
            _services.AddLogging(factory);
            return this;
        }

        public IAppBuilder LoadFromPluginsFolder()
        {
            //Load from Plugins folder.
            if (Directory.Exists("Plugins"))
            {
                foreach (string assemblyPath in Directory.GetFiles("Plugins", "*.dll", SearchOption.AllDirectories))
                {
                    try
                    {
                        _pluginManager.LoadPluginFile(assemblyPath);
                    }
                    catch
                    {
                        //Ignore
                    }
                }
            }
            //Activate all IApplicationStartup classes.
            foreach (var startup in _pluginManager.ActivateGenericTypes<IApplicationStartup>())
            {
                if (!_startups.Contains(startup))
                {
                    _startups.Add(startup);
                }
            }
            return this;
        }

        public IAppBuilder AddStartup<TStartup>() where TStartup : IApplicationStartup
        {
            var startup = (IApplicationStartup) Activator.CreateInstance(typeof(TStartup));
            if (!_startups.Contains(startup))
            {
                _startups.Add(startup);
            }
            return this;
        }
    }
}