using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem.FileSystems
{
    [PluginModule]
    public class PhysicalFileSystemModule : IFileSystemPluginModule
    {
        public bool AutoMount => true;
        public void InitializeServices(IServiceProvider kernel)
        {

        }

        private IFileSystem internalMount(UPath mountPoint, IReadOnlyDictionary<string, string> options)
        {
            return new PhysicalFileSystem();
        }

        private void internalUnMount(UPath mountPoint, MountFileSystem mfs)
        {
            mfs.Unmount(mountPoint);
        }

        public (UPath MountPoint, IFileSystem FileSystem) MountFileSystem(IReadOnlyDictionary<string, string> options)
        {
            return ("/phys", internalMount("/phys", options));
        }

        public void UnMount(MountFileSystem mfs)
        {
            internalUnMount("/phys", mfs);
        }

        public void UnMount(UPath mountPoint, MountFileSystem mfs)
        {
            internalUnMount(mountPoint, mfs);
        }
    }
}