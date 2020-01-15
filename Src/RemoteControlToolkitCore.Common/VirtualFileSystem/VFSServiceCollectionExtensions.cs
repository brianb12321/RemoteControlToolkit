using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Plugin;
using Zio;
using Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public static class VFSServiceCollectionExtensions
    {
        public static IServiceCollection AddVFS(this IServiceCollection services)
        {
            services.AddSingleton<IFileSystemSubsystem, FileSystemSubsystem>();
            return services.AddTransient<IExtensionProvider<RCTProcess>, ExtensionFileSystemProvder>();
        }
    }
}