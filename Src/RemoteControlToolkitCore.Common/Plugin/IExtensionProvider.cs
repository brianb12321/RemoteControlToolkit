using System.ServiceModel;
using RemoteControlToolkitCore.Common.Networking;

namespace RemoteControlToolkitCore.Common.Plugin
{
    public interface IExtensionProvider<in TExtension> where TExtension : IExtensibleObject<TExtension>
    {
        void GetExtension(TExtension context);
        void RemoveExtension(TExtension context);
    }
}