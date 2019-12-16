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
using System.Linq;
using System.Reflection;
using RemoteControlToolkitCore.Common.NSsh.Packets;

namespace RemoteControlToolkitCore.Common.NSsh.TransportLayer.State
{
    public abstract class AbstractTransportState : IDisposable
    {
        ~AbstractTransportState()
        {
            Dispose(false);
        }

        public abstract void Process(ITransportLayerManager manager);

        public void ProcessPacket(ITransportLayerManager manager)
        {
            Packet packet = manager.ReadPacket();

            if (manager.Connected && packet != null)
            {
                MethodInfo method = (from m in GetType().GetMethods()
                                     where m.Name == packet.PacketType.ToString() + "Packet"
                                     select m).FirstOrDefault();

                try
                {
                    method.Invoke(this, new object[] { manager, packet });
                }
                catch (TargetInvocationException e)
                {
                    Exception innerOrException = e.InnerException ?? e;
                    throw innerOrException;
                }
            }
        }

        public virtual void IgnorePacket(ITransportLayerManager manager, IgnorePacket packet)
        {
            // Nop
        }

        public virtual void DisconnectPacket(ITransportLayerManager manager, DisconnectPacket packet)
        {
            manager.DisconnectReceived(packet);
        }

        public virtual void KexInitPacket(ITransportLayerManager manager, KexInitPacket packet)
        {
            throw new InvalidOperationException("KEX INIT packet not allowed in this state.");
        }

        public virtual void KexDHInitPacket(ITransportLayerManager manager, KexDHInitPacket packet)
        {
            throw new InvalidOperationException("KEX DH INIT packet not allowed in this state.");
        }

        public virtual void NewKeysPacket(ITransportLayerManager manager, NewKeysPacket packet)
        {
            throw new InvalidOperationException("NEW KEYS packet not allowed in this state.");
        }

        public virtual void ServiceRequestPacket(ITransportLayerManager manager, ServiceRequestPacket packet)
        {
            throw new InvalidOperationException("SERVICE REQUEST packet not allowed in this state.");
        }

        public virtual void UserAuthRequestPacket(ITransportLayerManager manager, UserAuthRequestPacket packet)
        {
            throw new InvalidOperationException("USER AUTH REQUEST packet not allowed in this state.");
        }

        public virtual void ChannelOpenPacket(ITransportLayerManager manager, ChannelOpenPacket packet)
        {
            throw new InvalidOperationException("CHANNEL OPEN packet not allowed in this state.");
        }

        public virtual void ChannelDataPacket(ITransportLayerManager manager, ChannelDataPacket packet)
        {
            throw new InvalidOperationException("CHANNEL DATA packet not allowed in this state.");
        }

        public virtual void ChannelExtendedDataPacket(ITransportLayerManager manager, ChannelExtendedDataPacket packet)
        {
            throw new InvalidOperationException("CHANNEL EXTENDED DATA packet not allowed in this state.");
        }

        public virtual void ChannelWindowAdjustPacket(ITransportLayerManager manager, ChannelWindowAdjustPacket packet)
        {
            throw new InvalidOperationException("CHANNEL WINDOW ADJUST packet not allowed in this state.");
        }

        public virtual void ChannelEofPacket(ITransportLayerManager manager, ChannelEofPacket packet)
        {
            throw new InvalidOperationException("CHANNEL EOF packet not allowed in this state.");
        }

        public virtual void ChannelClosePacket(ITransportLayerManager manager, ChannelClosePacket packet)
        {
            throw new InvalidOperationException("CHANNEL CLOSE packet not allowed in this state.");
        }

        public virtual void ChannelRequestPacket(ITransportLayerManager manager, ChannelRequestPacket packet)
        {
            throw new InvalidOperationException("CHANNEL REQUEST packet not allowed in this state.");
        }

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        public virtual void Dispose(bool disposing)
        {
        }

        #endregion
    }
}
