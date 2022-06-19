using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common
{
    public interface IAppBuilder
    {
        NetworkSide ExecutingSide { get; }
        IHostApplication Build();
        IAppBuilder UsePluginManager<TPluginManagerImpl>() where TPluginManagerImpl : IPluginManager;
        IAppBuilder ConfigureLogging(Action<ILoggingBuilder> factory);
        IAppBuilder LoadFromPluginsFolder();
        IAppBuilder AddConfiguration(Action<IConfigurationBuilder> configure);
        IAppBuilder AddStartup<TStartup>() where TStartup : IApplicationStartup;
    }
}