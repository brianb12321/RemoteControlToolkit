using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Plugin;
using Zio;
using Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public static class VFSServiceCollectionExtensions
    {
        public static IServiceCollection AddVFS(this IServiceCollection services)
        {
            return services.AddSingleton<IFileSystemSubsystem, FileSystemSubsystem>();
        }
    }
}