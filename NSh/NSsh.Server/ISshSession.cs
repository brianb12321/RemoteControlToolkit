using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace NSsh.Server
{
    public interface ISshSession : IDisposable
    {
        Stream SocketStream { get; set; }

        Socket ClientSocket { get; set; }

        void Process();

        void Reject();
    }
}
