using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using NSsh.Common.Packets.UserAuth;
using NSsh.Common.Packets;

namespace NSsh.Server.Services
{
    public interface IPublicKeyAuthenticationService
    {
        IIdentity CreateIdentity(string userName, UserAuthPublicKeyPayload publicKeyPayload);
    }
}
