using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Scripting.Utils;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio.FileSystems;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public class FileSystemSubsystem : PluginSubsystem
    {
        private IFileSystem _fileSystem;
        private readonly IServiceProvider _provider;
        public FileSystemSubsystem(IPluginManager pluginManager, IServiceProvider provider) : base(pluginManager)
        {
            _provider = provider;

        }

        public override void InitializeSubsystem()
        {
            base.InitializeSubsystem();
            MountFileSystem mfs = new MountFileSystem(new MemoryFileSystem());
            foreach (var fileSystem in PluginManager.ActivateAllPluginModules<FileSystemSubsystem>().Select(m => m as IFileSystemPluginModule))
            {
                if (fileSystem.AutoMount)
                {
                    fileSystem.InitializeServices(_provider);
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