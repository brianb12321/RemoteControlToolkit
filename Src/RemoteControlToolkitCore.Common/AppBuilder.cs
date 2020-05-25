using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.NSsh.Configuration;
using RemoteControlToolkitCore.Common.NSsh.Services;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Proxy;

namespace RemoteControlToolkitCore.Common
{
    public class AppBuilder : IAppBuilder
    {
        private List<IApplicationStartup> _startups;
        private IServiceCollection _services;
        private IPluginManager _pluginManager;
        private ILoggerFactory _loggerFactory;
        public NetworkSide ExecutingSide => NetworkSide.Server;

        public AppBuilder()
        {
            _startups = new List<IApplicationStartup>();
            _services = new ServiceCollection();
        }

        public IHostApplication Build()
        {
            _startups.ForEach(s => s.ConfigureServices(_services, this));
            _services.AddSingleton<IHostApplication, Application>((provider) =>
            {
                return new Application(provider.GetService<ILogger<Application>>(),
                    provider.GetService<ILogger<ProxyNetworkInstance>>(),
                    provider.GetService<ILogger<ProxyClient>>(),
                    provider, provider.GetService<IServerPool>(), ExecutingSide, this, 
                    provider.GetService<IKeySetupService>(), 
                    _pluginManager,
                    provider.GetService<NSshServiceConfiguration>());
            });
            
            var serviceProvider = _services.BuildServiceProvider();
            var hostApplication = serviceProvider.GetService<IHostApplication>();
            _startups.ForEach(s => s.PostConfigureServices(serviceProvider, hostApplication));
            _services.BuildServiceProvider();
            return hostApplication;
        }

        public IAppBuilder UsePluginManager<TPluginManagerImpl>() where TPluginManagerImpl : IPluginManager
        {
            _pluginManager = (IPluginManager)Activator.CreateInstance(typeof(TPluginManagerImpl));
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
                    catch (PluginLoadException e)
                    {
                        //Create temporary logger.
                        ILogger<AppBuilder> logger = _loggerFactory.CreateLogger<AppBuilder>();
                        logger.LogWarning($"Unable to load plugin file: {e.Message}");
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