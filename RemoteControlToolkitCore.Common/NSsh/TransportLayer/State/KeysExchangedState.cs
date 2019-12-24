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
using System.Globalization;
using System.Security.Principal;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.NSsh.Configuration;
using RemoteControlToolkitCore.Common.NSsh.Packets;
using RemoteControlToolkitCore.Common.NSsh.Packets.UserAuth;
using RemoteControlToolkitCore.Common.NSsh.Services;
using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.TransportLayer.State
{
    public class KeysExchangedState : AbstractTransportState
    {
        bool _sentAuthBanner;
        private IServiceProvider _provider;
        public KeysExchangedState(IServiceProvider provider)
        {
            _provider = provider;
        }

        public override void Process(ITransportLayerManager manager)
        {
            ProcessPacket(manager);
        }

        public override void KexInitPacket(ITransportLayerManager manager, KexInitPacket packet)
        {
            // TODO: handle re-exchanging keys
            throw new NotImplementedException("Re-keying is not implemented.");
        }

        public override void ServiceRequestPacket(ITransportLayerManager manager, ServiceRequestPacket packet)
        {
            ServiceType serviceType;

            try
            {
                serviceType = ServiceTypeAlgorithmHelper.Parse(packet.ServiceName);
            }
            catch (ArgumentException)
            {
                manager.Disconnect(DisconnectReason.ServiceNotAvailable);
                return;
            }

            if (serviceType == ServiceType.UserAuthentication)
            {
                ServiceAcceptPacket accept = new ServiceAcceptPacket
                    {
                        ServiceName = packet.ServiceName
                    };
                manager.WritePacket(accept);
            }
            else
            {
                manager.Disconnect(DisconnectReason.ServiceNotAvailable);
                return;
            }
        }

        public override void UserAuthRequestPacket(ITransportLayerManager manager, UserAuthRequestPacket packet)
        {
            NSshServiceConfiguration config = _provider.GetService<NSshServiceConfiguration>();
            
            // Send the user authentication banner if present
            if (config.UserAuthenticationBanner != null && !_sentAuthBanner)
            {
                manager.WritePacket(new UserAuthBanner()
                {
                    Message = config.UserAuthenticationBanner,
                    LanguageTag = CultureInfo.CurrentCulture.Name
                });

                _sentAuthBanner = true;
            }

            if (packet.AuthMethod == AuthenticationMethod.Password)
            {
                IPasswordAuthenticationService passwordAuthService = _provider.GetService<IPasswordAuthenticationService>();
                UserAuthPasswordPayload passwordPayload = (UserAuthPasswordPayload) packet.AuthPayload;

                IIdentity identity = passwordAuthService.CreateIdentity(packet.UserName, passwordPayload.Password);

                if (identity != null)
                {
                    manager.WritePacket(new UserAuthSuccessPacket());
                    manager.AuthenticatedIdentity = identity;
                    manager.ChangeState(TransportLayerState.Authenticated);
                    return;
                }
                else
                {
                    // Fall through to failure case
                }
            }
            else if (packet.AuthMethod == AuthenticationMethod.PublicKey)
            {
                IPublicKeyAuthenticationService publicKeyAuthService = _provider.GetService<IPublicKeyAuthenticationService>();
                UserAuthPublicKeyPayload publicKeyPayload = (UserAuthPublicKeyPayload)packet.AuthPayload;

                IIdentity identity = publicKeyAuthService.CreateIdentity(packet.UserName, publicKeyPayload);

                if (identity != null)
                {
                    manager.WritePacket(new UserAuthSuccessPacket());
                    manager.AuthenticatedIdentity = identity;
                    manager.ChangeState(TransportLayerState.Authenticated);
                    return;
                }
                else
                {
                    // Fall through to failure case
                }
            }

            // Failed to authenticate, unknown, or "none" auth method. List the available authentication methods
            UserAuthFailurePacket failure = new UserAuthFailurePacket();
            failure.RemainingAuthMethods.Names.Add(AuthenticationMethod.Password.ToString().ToLower());
            failure.RemainingAuthMethods.Names.Add(AuthenticationMethod.PublicKey.ToString().ToLower());
            manager.WritePacket(failure);            
        }
    }
}
