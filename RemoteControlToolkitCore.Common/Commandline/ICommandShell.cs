using System.ServiceModel;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Networking;

namespace RemoteControlToolkitCore.Common.Commandline
{
    public interface ICommandShell : IApplication, IExtension<IInstanceSession>
    {
    }
}