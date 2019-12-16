using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSsh.Common.Types;

namespace NSsh.Server.Services
{
    public interface IKeySetupService
    {
        void EnsureSetup();

        IPublicKeyPair GetServerKeyPair(PublicKeyAlgorithm publicKeyAlgorithm);
    }
}
