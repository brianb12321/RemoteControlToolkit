using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace RemoteControlToolkitCore.Common
{
    public class AppBuilder : IAppBuilder
    {
        private IApplicationStartup startup;
        public IHostApplication Build()
        {
            ServiceCollection collection = new ServiceCollection();
            startup.ConfigureServices(collection);
            collection.AddSingleton<IHostApplication, Application>();
            var serviceProvider = collection.BuildServiceProvider();
            var hostApplication = serviceProvider.GetService<IHostApplication>();
            startup.PostConfigureServices(serviceProvider, hostApplication);
            return hostApplication;
        }

        public IAppBuilder UseStartup<TStartup>() where TStartup : IApplicationStartup
        {
            startup = (IApplicationStartup)Activator.CreateInstance(typeof(TStartup));
            return this;
        }
    }
}