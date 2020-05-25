using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Commandline
{
    public static class CommandLineSubsystemExtensions
    {
        public static IServiceCollection AddCommandLine(this IServiceCollection services)
        {
            return services.AddSingleton(provider =>
                new ApplicationSubsystem(provider.GetRequiredService<IHostApplication>().PluginManager, provider));
        }
    }
}