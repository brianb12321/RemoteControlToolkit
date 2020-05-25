using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public static class VFSServiceCollectionExtensions
    {
        public static IServiceCollection AddVFS(this IServiceCollection services)
        {
            services.AddSingleton(provider => new FileSystemSubsystem(provider.GetRequiredService<IHostApplication>().PluginManager));
            return services.AddTransient<IExtensionProvider<RCTProcess>, ExtensionFileSystemProvder>();
        }
    }
}