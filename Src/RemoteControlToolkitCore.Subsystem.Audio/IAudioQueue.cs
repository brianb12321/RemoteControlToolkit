using System.Collections.Generic;
using System.ServiceModel;
using NAudio.Wave;
using RemoteControlToolkitCore.Common.Networking;

namespace RemoteControlToolkitCore.Subsystem.Audio
{
    /// <summary>
    /// Represents all audio devices being played for each client.
    /// </summary>
    public interface IAudioQueue : IExtension<IInstanceSession>
    {
        List<IWavePlayer> Queue { get; }
        void StopAll();
    }
}