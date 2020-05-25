using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem.FileSystems
{
    [Plugin]
    public class MemoryFileSystemModule : PluginModule<FileSystemSubsystem>, IFileSystemPluginModule
    {
        public bool AutoMount => true;

        private IFileSystem internalMount(UPath mountPoint, IReadOnlyDictionary<string, string> options)
        {
            MemoryFileSystem mfs = new MemoryFileSystem();
            mfs.AppendAllText("/README.txt", "Awesome stuff!!!!");
            return mfs;
        }

        private void internalUnMount(UPath mountPoint, MountFileSystem mfs)
        {
            mfs.Unmount(mountPoint);
        }

        public (UPath MountPoint, IFileSystem FileSystem) MountFileSystem(IReadOnlyDictionary<string, string> options)
        {
            return ("/vfs", internalMount("/vfs", options));
        }

        public void UnMount(MountFileSystem mfs)
        {
            internalUnMount("/vfs", mfs);
        }

        public void UnMount(UPath mountPoint, MountFileSystem mfs)
        {
            internalUnMount(mountPoint, mfs);
        }
    }
}