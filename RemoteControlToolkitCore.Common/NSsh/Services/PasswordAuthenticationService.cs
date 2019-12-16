using System;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;
using RemoteControlToolkitCore.Common.NSsh.Types;
using RemoteControlToolkitCore.Common.NSsh.Utility;

namespace RemoteControlToolkitCore.Common.NSsh.Services
{
    public class PasswordAuthenticationService : IPasswordAuthenticationService
    {
        #region IPasswordAuthenticationService Members

        public IIdentity CreateIdentity(string userName, string password)
        {
            SafeFileHandle token;

            string domain = Environment.MachineName;

            bool successful = Win32Native.LogonUser(
                userName,
                domain,
                password,
                LogonSessionType.Interactive,
                LogonProvider.Default,
                out token);
            
            return (successful) ? new SafeWindowsIdentity(token) : null;
        }

        #endregion        
    }
}
