using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting
{
    public static class ScriptingServiceCollectionExtensions
    {
        public static IServiceCollection AddScriptingEngine<TImpl>(this IServiceCollection services)
            where TImpl : class, IScriptingEngine
        {
            services.AddSingleton<IScriptingEngine, TImpl>();
            services.AddTransient<IInstanceExtensionProvider, ScriptingEngineExtensionProvider>();
            return services;
        }
    }
}