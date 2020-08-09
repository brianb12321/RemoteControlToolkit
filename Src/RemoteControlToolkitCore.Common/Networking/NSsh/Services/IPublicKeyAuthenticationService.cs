using System.Security.Principal;
using RemoteControlToolkitCore.Common.Networking.NSsh.Packets;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Services
{
    public interface IPublicKeyAuthenticationService
    {
        IIdentity CreateIdentity(string userName, UserAuthPublicKeyPayload publicKeyPayload);
    }
}
