using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteControlToolkitCore.Common
{
    public interface IHostApplication : IDisposable
    {
        void Run();
        NetworkSide ExecutingSide { get; }
    }
}