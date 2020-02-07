using System.ServiceModel;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public interface IExtensionFileSystem : IExtension<RCTProcess>
    {
        IFileSystem GetFileSystem();
    }

    public class ExtensionFileSystemProvder : IExtensionProvider<RCTProcess>
    {
        private readonly IFileSystemSubsystem _subsystem;
        private IExtensionFileSystem _fileSystem;

        public ExtensionFileSystemProvder(IFileSystemSubsystem subsystem)
        {
            _subsystem = subsystem;
        }
        public void GetExtension(RCTProcess context)
        {
            _fileSystem = new ExtensionFileSystem(_subsystem.GetFileSystem());
            context.Extensions.Add(_fileSystem);
        }

        public void RemoveExtension(RCTProcess context)
        {
            context.Extensions.Remove(_fileSystem);
        }
    }
}