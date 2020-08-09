using System;
using System.Diagnostics;
using System.Security.Principal;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Services
{
    public interface IImpersonationProvider : IDisposable
    {
        ProcessDetails CreateProcess(IIdentity identity, string password, ProcessStartInfo startInfo);
    }
}
