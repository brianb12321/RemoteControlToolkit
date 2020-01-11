using RemoteControlToolkitCore.Common.Plugin;
using Zio;
using Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public interface IFileSystemSubsystem : IPluginSubsystem<IFileSystemPluginModule>
    {
        IFileSystem GetFileSystem();
    }
}