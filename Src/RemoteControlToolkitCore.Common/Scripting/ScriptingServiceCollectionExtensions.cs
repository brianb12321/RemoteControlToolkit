using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting
{
    public static class ScriptingServiceCollectionExtensions
    {
        public static IServiceCollection AddScriptingEngine<TSubsystem>(this IServiceCollection services)
            where TSubsystem : class, IScriptingSubsystem
        {
            services.AddSingleton<IScriptingSubsystem, TSubsystem>();
            return services;
        }
    }
}