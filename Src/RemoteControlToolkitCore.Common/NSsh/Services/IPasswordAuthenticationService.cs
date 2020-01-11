using System.Security.Principal;

namespace RemoteControlToolkitCore.Common.NSsh.Services
{
    public interface IPasswordAuthenticationService
    {
        IIdentity CreateIdentity(string userName, string password);
    }
}
