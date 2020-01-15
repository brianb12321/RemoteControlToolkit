using System;
using Microsoft.Extensions.DependencyInjection;

namespace RemoteControlToolkitCore.Common.Plugin
{
    public static class PluginServiceCollectionExtensions
    {
        public static IServiceCollection AddPluginSystem<TLoader>(this IServiceCollection services)
            where TLoader : class, IPluginLibraryLoader
        {
            services.AddSingleton<IPluginLibraryLoader, TLoader>();
            return services;
        }
    }
}