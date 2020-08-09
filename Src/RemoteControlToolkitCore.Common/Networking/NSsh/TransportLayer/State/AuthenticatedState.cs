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
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Networking.NSsh.ChannelLayer;
using RemoteControlToolkitCore.Common.Networking.NSsh.Packets;
using RemoteControlToolkitCore.Common.Networking.NSsh.Packets.Channel;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.TransportLayer.State
{
    public class AuthenticatedState : AbstractTransportState
    {
        Dictionary<uint, IChannel> _channels = new Dictionary<uint, IChannel>();
        private IServiceProvider _provider;

        public AuthenticatedState(IServiceProvider provider)
        {
            _provider = provider;
        }

        public override void Process(ITransportLayerManager manager)
        {
            ProcessPacket(manager);
        }

        public override void UserAuthRequestPacket(ITransportLayerManager manager, UserAuthRequestPacket packet)
        {
            // Ignore: see rfc4552 - 5.1
        }

        public override void ChannelOpenPacket(ITransportLayerManager manager, ChannelOpenPacket packet)
        {
            if (_channels.ContainsKey(packet.SenderChannel))
            {
                ChannelOpenFailurePacket failurePacket = new ChannelOpenFailurePacket
                    {
                        FailureReason = ChannelOpenFailureReason.ConnectFailed,
                        Description = "A channel already exists with number: " + packet.SenderChannel
                    };
                manager.WritePacket(failurePacket);
            }
            else if (packet.ChannelType == ChannelType.Session)
            {
                IChannel channel = _provider.GetService<IChannel>();
                channel.TransportLayerManager = manager;
                channel.ChannelId = packet.SenderChannel;
                channel.SetChannelType(packet.ChannelType);
                channel.SetTransmitWindowSize(packet.InitialWindowSize);
                channel.SetMaximumTransmitPacketSize(packet.MaximumWindowSize);
                channel.Initialise();

                _channels[packet.SenderChannel] = channel;

                ChannelOpenConfirmationPacket confirmation = new ChannelOpenConfirmationPacket
                    {
                        SenderChannel = packet.SenderChannel,
                        RecipientChannel = packet.SenderChannel,
                        MaximumPacketSize = channel.ReceiveMaximumPacketSize,
                        InitialWindowSize = channel.ReceiveWindowSize,
                    };
                manager.WritePacket(confirmation);
            }
            else
            {
                ChannelOpenFailurePacket failurePacket = new ChannelOpenFailurePacket
                    {
                        FailureReason = ChannelOpenFailureReason.UnknownChannelType,
                        Description = "Unknown channel type: " + packet.ChannelType
                    };
                manager.WritePacket(failurePacket);
            }
        }

        public override void ChannelDataPacket(ITransportLayerManager manager, ChannelDataPacket packet)
        {
            IChannel channel = _channels[packet.RecipientChannel];
            channel.ProcessChannelData(packet);
        }

        public override void ChannelExtendedDataPacket(ITransportLayerManager manager, ChannelExtendedDataPacket packet)
        {
            IChannel channel = _channels[packet.RecipientChannel];
            channel.ProcessChannelExtendedData(packet);
        }

        public override void ChannelWindowAdjustPacket(ITransportLayerManager manager, ChannelWindowAdjustPacket packet)
        {
            IChannel channel = _channels[packet.RecipientChannel];
            channel.AdjustTransmitWindow(packet);
        }

        public override void ChannelEofPacket(ITransportLayerManager manager, ChannelEofPacket packet)
        {
            IChannel channel = _channels[packet.RecipientChannel];
            channel.EofReceived(packet);
        }

        public override void ChannelClosePacket(ITransportLayerManager manager, ChannelClosePacket packet)
        {
            IChannel channel = _channels[packet.RecipientChannel];
            channel.CloseReceived(packet);
            _channels.Remove(packet.RecipientChannel);
        }

        public override void ChannelRequestPacket(ITransportLayerManager manager, ChannelRequestPacket packet)
        {
            IChannel channel = _channels[packet.RecipientChannel];
            channel.RequestReceived(packet);
        }

        public void GlobalRequestPacket(ITransportLayerManager manager,GlobalRequestPacket packet) {
            // ToDo: Work out how to process these.
        }

        public override void DisconnectPacket(ITransportLayerManager manager, DisconnectPacket packet)
        {
            // Clean up each channel
            foreach (IChannel channel in _channels.Values)
            {
                channel.Dispose();
            }
            _channels.Clear();

            base.DisconnectPacket(manager, packet);
        }

        public override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (IChannel channel in _channels.Values)
                {
                    channel.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
