using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public static class VFSServiceCollectionExtensions
    {
        public static IServiceCollection AddVFS(this IServiceCollection services)
        {
            services.AddSingleton<IInstanceExtensionProvider, VFSInstanceExtensionProvider>();
            return services.AddSingleton<IFileSystemSubsystem, FileSystemSubsystem>();
        }
    }
}