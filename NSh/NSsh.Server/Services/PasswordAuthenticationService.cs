using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Runtime.InteropServices;
using NSsh.Server.Types;
using NSsh.Server.Utility;
using Microsoft.Win32.SafeHandles;

namespace NSsh.Server.Services
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
