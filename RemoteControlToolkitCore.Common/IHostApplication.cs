using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteControlToolkitCore.Common
{
    public interface IHostApplication : IDisposable
    {
        void Run(string[] args);
        NetworkSide ExecutingSide { get; }
    }
}