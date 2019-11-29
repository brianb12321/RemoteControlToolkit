using System.Collections.Generic;
using RemoteControlToolkitCore.Common.Plugin;
using Zio;
using Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public interface IFileSystemPluginModule : IPluginModule
    {
        bool AutoMount { get; }
        (UPath MountPoint, IFileSystem FileSystem) MountFileSystem(IReadOnlyDictionary<string, string> options);
        void UnMount(MountFileSystem mfs);
        void UnMount(UPath mountPoint, MountFileSystem mfs);
    }
}