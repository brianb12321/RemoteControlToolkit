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
using System.Linq;
using System.Net;
using System.Xml;
using System.Collections.Generic;
using NSsh.Common.Types;
using System.Security.Cryptography;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace NSsh.Server.Configuration
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
    ///  or it will be disconencted.
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

        public List<IPEndPoint> ListenEndPoints { get; private set; }

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

        public static NSshServiceConfiguration LoadFromFile(string file)
        {
            NSshServiceConfiguration configuration = new NSshServiceConfiguration();
            XDocument configurationXml = XDocument.Load(file);

            var endPoints = from endPoint in configurationXml.Descendants("EndPoint")
                            select new
                            {
                                Port = Int32.Parse(endPoint.Attribute("Port").Value),
                                IPAddress = IPAddress.Parse(endPoint.Attribute("IPAddress").Value)
                            };

            foreach (var endPoint in endPoints)
            {
                configuration.ListenEndPoints.Add(new IPEndPoint(endPoint.IPAddress, endPoint.Port));
            }

            return configuration;
        }

        public static void SaveToFile(string file, NSshServiceConfiguration configuration)
        {
            using (XmlWriter writer = new XmlTextWriter(file, Encoding.UTF8))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("NSsh");
                writer.WriteStartElement("Configuration");

                writer.WriteStartElement("ListenEndPoints");
                foreach (IPEndPoint endpoint in configuration.ListenEndPoints)
                {
                    writer.WriteStartElement("EndPoint");
                    writer.WriteAttributeString("IPAddress", endpoint.Address.ToString());
                    writer.WriteAttributeString("Port", endpoint.Port.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
    }
}
