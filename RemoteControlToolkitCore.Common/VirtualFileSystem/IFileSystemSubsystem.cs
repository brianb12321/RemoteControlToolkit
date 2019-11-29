using RemoteControlToolkitCore.Common.Plugin;
using Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public interface IFileSystemSubsystem : IPluginSubsystem<IFileSystemPluginModule>
    {
        MountFileSystem NewFileSystem();
    }
}