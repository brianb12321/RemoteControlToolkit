using System;
using System.IO;
using System.Net.Sockets;

namespace RemoteControlToolkitCore.Common.NSsh
{
    public interface ISshSession : IDisposable
    {
        Stream SocketStream { get; set; }

        Socket ClientSocket { get; set; }

        void Process();

        void Reject();
    }
}
