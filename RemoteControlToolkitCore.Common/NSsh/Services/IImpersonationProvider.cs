using System;
using System.Diagnostics;
using System.Security.Principal;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Services
{
    public interface IImpersonationProvider : IDisposable
    {
        ProcessDetails CreateProcess(IIdentity identity, string password, ProcessStartInfo startInfo);
    }
}
