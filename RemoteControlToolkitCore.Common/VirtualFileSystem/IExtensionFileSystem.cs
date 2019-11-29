using System.ServiceModel;
using RemoteControlToolkitCore.Common.Networking;
using Zio;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public interface IExtensionFileSystem : IExtension<IInstanceSession>
    {
        IFileSystem FileSystem { get; }
    }
}