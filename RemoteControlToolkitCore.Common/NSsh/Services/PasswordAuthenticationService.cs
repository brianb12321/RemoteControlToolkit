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
            //Basic authentication.
            if (userName == "admin" && password == "password")
            {
                return new GenericIdentity(userName);
            }
            else return null;
        }

        #endregion
    }
}
