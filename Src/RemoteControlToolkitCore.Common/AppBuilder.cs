using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
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
                    provider.GetService<NSshServiceConfiguration>());
            });
            
            var serviceProvider = _services.BuildServiceProvider();
            var hostApplication = serviceProvider.GetService<IHostApplication>();
            _startups.ForEach(s => s.PostConfigureServices(serviceProvider, hostApplication));
            _services.BuildServiceProvider();
            return hostApplication;
        }

        public IAppBuilder AddStartup<TStartup>() where TStartup : IApplicationStartup
        {
            _startups.Add((IApplicationStartup)Activator.CreateInstance(typeof(TStartup)));
            return this;
        }
        
        public IAppBuilder ScanForAppStartup(string folder)
        {
            if (Directory.Exists(folder))
            {
                foreach (string file in Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories))
                {
                    Assembly assembly = Assembly.LoadFrom(file);
                    var startupAttrib = assembly.GetCustomAttribute<ApplicationStartupAttribute>();
                    if (startupAttrib != null)
                    {
                        _startups.Add((IApplicationStartup)Activator.CreateInstance(startupAttrib.Startup));
                    }
                }
            }

            return this;
        }
    }
}