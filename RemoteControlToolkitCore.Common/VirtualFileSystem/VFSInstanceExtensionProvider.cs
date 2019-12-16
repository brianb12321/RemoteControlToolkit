using System;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;
using Zio;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public class VFSInstanceExtensionProvider : IInstanceExtensionProvider
    {
        private IServiceProvider _services;

        public VFSInstanceExtensionProvider(IServiceProvider services)
        {
            _services = services;
        }
        public void GetExtension(IInstanceSession context)
        {
            IFileSystemSubsystem subsystem = _services.GetService<IFileSystemSubsystem>();
            var fileSystem = new ExtensionFileSystem(subsystem.NewFileSystem());
            fileSystem.FileSystem.WriteAllText("/vfs/README.txt", "This is a test");
            context.Extensions.Add(fileSystem);
        }

        public void RemoveExtension(IInstanceSession context)
        {
            context.GetExtension<IExtensionFileSystem>().FileSystem.Dispose();
        }
    }
}