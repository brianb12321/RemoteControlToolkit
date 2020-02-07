using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public class ExtensionFileSystem : IExtensionFileSystem
    {
        private RCTProcess _owner;
        private IFileSystem _fileSystem;
        public ExtensionFileSystem(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        public void Attach(RCTProcess owner)
        {
            _owner = owner;
        }

        public void Detach(RCTProcess owner)
        {
            
        }

        public IFileSystem GetFileSystem()
        {
            return new FileSystemHelper(_fileSystem, _owner.WorkingDirectory);
        }
    }
}