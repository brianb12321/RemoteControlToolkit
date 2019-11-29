using System;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Plugin;
using Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public class FileSystemSubsystem : BasePluginSubsystem<IFileSystemSubsystem, IFileSystemPluginModule>, IFileSystemSubsystem
    {
        public FileSystemSubsystem(IPluginLibraryLoader loader, IServiceProvider services) : base(loader, services)
        {
        }

        public MountFileSystem NewFileSystem()
        {
            MountFileSystem mfs = new MountFileSystem(new MemoryFileSystem());
            foreach (var fileSystem in GetAllModules())
            {
                if (fileSystem.AutoMount)
                {
                    var fs = fileSystem.MountFileSystem(null);
                    mfs.Mount(fs.MountPoint, fs.FileSystem);
                }
            }

            return mfs;
        }
    }
}