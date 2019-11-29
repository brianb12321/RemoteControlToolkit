using RemoteControlToolkitCore.Common.Networking;
using Zio;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public class ExtensionFileSystem : IExtensionFileSystem
    {
        public IFileSystem FileSystem { get; set; }

        public ExtensionFileSystem(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }
        public void Attach(IInstanceSession owner)
        {
        }

        public void Detach(IInstanceSession owner)
        {
            
        }
    }
}
