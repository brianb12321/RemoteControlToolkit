using System.Collections.Generic;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public interface IFileSystemPluginModule : IPluginModule<FileSystemSubsystem>
    {
        bool AutoMount { get; }
        (UPath MountPoint, IFileSystem FileSystem) MountFileSystem(IReadOnlyDictionary<string, string> options);
        void UnMount(MountFileSystem mfs);
        void UnMount(UPath mountPoint, MountFileSystem mfs);
    }
}