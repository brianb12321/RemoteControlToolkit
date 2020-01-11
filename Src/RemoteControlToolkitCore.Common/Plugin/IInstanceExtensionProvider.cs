using RemoteControlToolkitCore.Common.Networking;

namespace RemoteControlToolkitCore.Common.Plugin
{
    public interface IInstanceExtensionProvider
    {
        void GetExtension(IInstanceSession context);
        void RemoveExtension(IInstanceSession context);
    }
}