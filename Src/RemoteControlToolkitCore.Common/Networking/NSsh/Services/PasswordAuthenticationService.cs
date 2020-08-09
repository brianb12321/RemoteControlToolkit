using System.Security.Principal;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Services
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
