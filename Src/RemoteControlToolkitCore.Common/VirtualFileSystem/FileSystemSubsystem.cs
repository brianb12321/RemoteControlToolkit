using System;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Plugin;
using Zio;
using Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public class FileSystemSubsystem : BasePluginSubsystem<IFileSystemSubsystem, IFileSystemPluginModule>, IFileSystemSubsystem
    {
        private IFileSystem _fileSystem;
        public FileSystemSubsystem(IPluginLibraryLoader loader, IServiceProvider services) : base(loader, services)
        {
            
        }

        public override void Init()
        {
            base.Init();
            MountFileSystem mfs = new MountFileSystem(new MemoryFileSystem());
            foreach (var fileSystem in GetAllModules())
            {
                if (fileSystem.AutoMount)
                {
                    var fs = fileSystem.MountFileSystem(null);
                    mfs.Mount(fs.MountPoint, fs.FileSystem);
                }
            }

            _fileSystem = mfs;
        }

        public IFileSystem GetFileSystem()
        {
            return _fileSystem;
        }
    }
}