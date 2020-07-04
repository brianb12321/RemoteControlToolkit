using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCoreServerWCF
{
    public class LoginValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            if (!(userName == "admin" && password == "password"))
            {
                throw new FaultException("Unknown username and/or password.");
            }
        }
    }
}