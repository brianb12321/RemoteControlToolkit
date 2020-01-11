using System;
using System.Collections.Generic;
using System.Text;
using RemoteControlToolkitCore.Common.NSsh;

namespace RemoteControlToolkitCore.Common
{
    public interface IHostApplication : IDisposable
    {
        void DeregisterSession(ISshSession session);
        void Run(string[] args);
        NetworkSide ExecutingSide { get; }
        IAppBuilder Builder { get; }
    }
}