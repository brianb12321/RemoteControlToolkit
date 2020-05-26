using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public class ExtensionFileSystem : IExtensionFileSystem
    {
        private RctProcess _owner;
        private readonly IFileSystem _fileSystem;
        public ExtensionFileSystem(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        public void Attach(RctProcess owner)
        {
            _owner = owner;
        }

        public void Detach(RctProcess owner)
        {
            
        }

        public IFileSystem GetFileSystem()
        {
            return new FileSystemHelper(_fileSystem, _owner.WorkingDirectory);
        }
    }
}