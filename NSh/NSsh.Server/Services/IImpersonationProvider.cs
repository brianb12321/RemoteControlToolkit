using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Diagnostics;
using System.IO;
using NSsh.Server.Types;

namespace NSsh.Server.Services
{
    public interface IImpersonationProvider : IDisposable
    {
        ProcessDetails CreateProcess(IIdentity identity, string password, ProcessStartInfo startInfo);
    }
}
