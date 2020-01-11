using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Audio
{
    public class AudioQueueExtensionProvider : IInstanceExtensionProvider
    {
        public void GetExtension(IInstanceSession context)
        {
            context.Extensions.Add(new AudioQueue());
        }

        public void RemoveExtension(IInstanceSession context)
        {
            
        }
    }
}