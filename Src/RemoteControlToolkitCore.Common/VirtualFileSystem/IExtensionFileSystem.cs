using System.ServiceModel;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Networking;
using Zio;

namespace RemoteControlToolkitCore.Common.VirtualFileSystem
{
    public interface IExtensionFileSystem : IExtension<RCTProcess>
    {
        IFileSystem GetFileSystem();
    }
}