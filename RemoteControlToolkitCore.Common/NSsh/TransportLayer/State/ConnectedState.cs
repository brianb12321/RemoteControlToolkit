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
 * This work has been adapted from the Ganymed SSH-2 Java client.
 */

using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace RemoteControlToolkitCore.Common.NSsh.TransportLayer.State
{
    public class ConnectedState : AbstractTransportState
    {
        private const string CRLF = "\r\n";

        private ILogger<ConnectedState> _logger;

        public ConnectedState(ILogger<ConnectedState> logger)
        {
            _logger = logger;
        }

        public override void Process(ITransportLayerManager manager)
        {
            // Send our version string
            manager.Write(Encoding.ASCII.GetBytes(VersionString + CRLF));

            // Read the client's version string
            string clientVersion = manager.ReadLine();
            _logger.LogInformation("Client version=" + clientVersion + ".");
            if (!clientVersion.StartsWith("SSH-2.0-") && !clientVersion.StartsWith("SSH-1.99-"))
            {
                throw new ArgumentException("Invalid version string. Only SSH version 2.0 is supported.");
            }

            // Change to the versions exchanged state
            manager.ClientVersion = clientVersion;
            manager.ChangeState(TransportLayerState.VersionsExchanged);
        }

        /// <summary>
        /// The version string to send to the remote client.
        /// </summary>
        public const string VersionString = "SSH-2.0-NSsh";
    }
}
