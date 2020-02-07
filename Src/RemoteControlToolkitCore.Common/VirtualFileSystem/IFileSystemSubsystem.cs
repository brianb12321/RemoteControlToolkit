using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public interface IFileSystemSubsystem : IPluginSubsystem<IFileSystemPluginModule>
    {
        IFileSystem GetFileSystem();
    }
}