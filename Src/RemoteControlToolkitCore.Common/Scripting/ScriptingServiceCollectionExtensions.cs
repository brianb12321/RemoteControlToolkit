using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting
{
    public static class ScriptingServiceCollectionExtensions
    {
        public static IServiceCollection AddScriptingEngine(this IServiceCollection services)
        {
            services.AddSingleton(provider => new ScriptingSubsystem(provider.GetRequiredService<IHostApplication>().PluginManager, provider));
            return services;
        }
    }
}