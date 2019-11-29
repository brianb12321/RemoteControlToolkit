using System.Collections.Generic;
using NAudio.Wave;
using RemoteControlToolkitCore.Common.Networking;

namespace RemoteControlToolkitCore.Subsystem.Audio
{
    public class AudioQueue : IAudioQueue
    {
        public AudioQueue()
        {
            Queue = new List<IWavePlayer>();
        }
        public void Attach(IInstanceSession owner)
        {
            
        }

        public void Detach(IInstanceSession owner)
        {
            Queue.Clear();
        }

        public List<IWavePlayer> Queue { get; }
        public void StopAll()
        {
            foreach (var wavePlayer in Queue)
            {
                wavePlayer.Stop();
            }
            Queue.Clear();
        }
    }
}