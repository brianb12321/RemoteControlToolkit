/*
 * Copyright 2008 Luke Quinane
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Configuration
{
    /// <summary>
    /// Configuration for the SSH service.
    /// </summary>
    /// <remarks>
    /// Explanation of configuration parameters.
    /// 
    /// MaximumClientConnections: The maximum number of simultaneous socket connections the
    ///  server will accept after which any new attempts will be immediately closed with an
    ///  SSH error message.
    ///  
    /// MaximumSameClientConnections: The maximum number of simultaneous socket connections
    ///  that will be accepted for a single IP address after which an any new attempts will 
    ///  be immediately closed with an SSH error message.
    ///  
    /// VersionsExchangedTimeout: The period a client has to complete the version string 
    ///  exchange before the server will disconnect it.
    ///  
    /// AuthenticatedTimeout: The period a client has to complete the authentication
    ///  exchange before the server will disconnect it.
    ///  
    /// IdleTimeout: The period an authenticated client must have data sent to or from  
    ///  or it will be disconnected.
    /// </remarks>
    public class NSshServiceConfiguration
    {
        public NSshServiceConfiguration()
        {
            ListenEndPoints = new List<IPEndPoint>();
            MaximumClientConnections = 20;
            MaximumSameIPAddressConnections = 2;
            VersionsExchangedTimeout = TimeSpan.FromSeconds(3);
            AuthenticatedTimeout = TimeSpan.FromSeconds(60);
            IdleTimeout = TimeSpan.FromMinutes(10);
            ReceiveWindowSize = 1024 * 1024 * 4;
            ReceiveMaximumPacketSize = 1024 * 1024;
        }

        public List<IPEndPoint> ListenEndPoints { get; set; }

        public int MaximumClientConnections { get; set; }

        public int MaximumSameIPAddressConnections { get; set; }

        public TimeSpan VersionsExchangedTimeout { get; set; }

        public TimeSpan AuthenticatedTimeout { get; set; }

        public TimeSpan IdleTimeout { get; set; }

        public PublicKeyAlgorithm ServerKeyAlgorithm { get; set; }

        public DSACryptoServiceProvider ServerDsaProvider { get; set; }

        public RSACryptoServiceProvider ServerRsaProvider{ get; set; }

        public uint ReceiveWindowSize { get; set; }

        public uint ReceiveMaximumPacketSize { get; set; }

        public string UserAuthenticationBanner { get; set; }
    }
}
