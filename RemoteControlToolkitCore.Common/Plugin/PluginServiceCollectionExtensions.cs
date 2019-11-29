using System;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Plugin.Devices;

namespace RemoteControlToolkitCore.Common.Plugin
{
    public static class PluginServiceCollectionExtensions
    {
        public static IServiceCollection AddPluginSystem<TLoader>(this IServiceCollection services)
            where TLoader : class, IPluginLibraryLoader
        {
            services.AddSingleton<IPluginLibraryLoader, TLoader>();
            services.AddSingleton<IDeviceSubsystem, DeviceSubsystem>();
            return services;
        }
    }
}