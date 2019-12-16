using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Security;

namespace NSsh.Server.Services
{
    public interface IPasswordAuthenticationService
    {
        IIdentity CreateIdentity(string userName, string password);
    }
}
