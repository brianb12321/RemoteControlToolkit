using System.Security.Principal;
using RemoteControlToolkitCore.Common.NSsh.Packets;

namespace RemoteControlToolkitCore.Common.NSsh.Services
{
    public interface IPublicKeyAuthenticationService
    {
        IIdentity CreateIdentity(string userName, UserAuthPublicKeyPayload publicKeyPayload);
    }
}
