﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSsh.Common.Utility;
using NSsh.Server.Configuration;
using NSsh.Common.Types;
using System.IO;
using System.Security.Cryptography;

namespace NSsh.Server.Services
{
    public class KeySetupService : IKeySetupService
    {
        CspParameters dsaCspParameters;

        CspParameters rsaCspParameters;

        DSACryptoServiceProvider dsaProvider;
        RSACryptoServiceProvider rsaProvider;
        private NSshServiceConfiguration _config;

        public KeySetupService(NSshServiceConfiguration config)
        {
            _config = config;
            dsaCspParameters = new CspParameters(13);
            dsaCspParameters.KeyContainerName = "NSshServer";

            rsaCspParameters = new CspParameters(1);
            rsaCspParameters.KeyContainerName = "NSshServer";
        }

        #region IKeySetupService Members

        public void EnsureSetup()
        {

            dsaProvider = new DSACryptoServiceProvider(1024, dsaCspParameters);
            rsaProvider = new RSACryptoServiceProvider(2048, rsaCspParameters);

            _config.ServerDsaProvider = dsaProvider;
            _config.ServerRsaProvider = rsaProvider;
        }

        public IPublicKeyPair GetServerKeyPair(PublicKeyAlgorithm publicKeyAlgorithm)
        {

            switch (publicKeyAlgorithm)
            {
                case PublicKeyAlgorithm.DSA:
                    return new DsaKeyPair(_config.ServerDsaProvider);

                case PublicKeyAlgorithm.RSA:
                    throw new NotImplementedException();

                default:
                    throw new NotSupportedException("Unsupported alogrithm: " + publicKeyAlgorithm);
            }
        }

        #endregion
    }
}