using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSsh.Common.Types;
using NSsh.Common.Packets;
using System.Threading;
using System.Security.Principal;
using System.Diagnostics;
using NSsh.Common.Utility;
using NSsh.Server.TransportLayer;
using System.IO;
using Microsoft.Extensions.Logging;
using NSsh.Server.Configuration;
using NSsh.Common.Packets.Channel;
using NSsh.Common.Packets.Channel.RequestPayloads;
using Microsoft.Extensions.DependencyInjection;

namespace NSsh.Server.ChannelLayer
{
    public class Channel : IChannel, IChannelProducer
    {
        private const string THREAD_NAME_CHANNEL_OUTGOING = "channel-outgoing";

        /// <summary>
        /// Logging support for this class.
        /// </summary>
        private readonly ILogger<Channel> _logger;

        private uint _transmitWindowSize;

        private uint _transmitMaximumPacketSize;

        private uint _receiveWindowSize;

        private uint _receiveMaximumPacketSize;

        private Queue<Packet> _incomingData = new Queue<Packet>();

        private AutoResetEvent _incomingDataArrived = new AutoResetEvent(false);

        private Queue<Packet> _outgoingData = new Queue<Packet>();

        private AutoResetEvent _outgoingDataArrived = new AutoResetEvent(false);

        private IChannelConsumer _channelConsumer;

        private ChannelRequestType _consumerType;
        private IServiceProvider _provider;

        public Channel(ILogger<Channel> logger, IServiceProvider provider)
        {
            _logger = logger;
            NSshServiceConfiguration config = provider.GetService<NSshServiceConfiguration>();
            _provider = provider;
            _receiveWindowSize = config.ReceiveWindowSize;
            _receiveMaximumPacketSize = config.ReceiveMaximumPacketSize;
        }

        ~Channel()
        {
            Dispose(false);
        }

        #region IChannel Members

        public ITransportLayerManager TransportLayerManager { get; set; }

        public uint ChannelId { get; set; }

        public void SetChannelType(ChannelType channelType)
        {
            if (channelType != ChannelType.Session)
            {
                throw new NotSupportedException("Channel type not supported: " + channelType);
            }
        }

        public void SetTransmitWindowSize(uint windowSize)
        {
            _transmitWindowSize = windowSize;
        }

        public void SetMaximumTransmitPacketSize(uint maximumPacketSize)
        {
            _transmitMaximumPacketSize = maximumPacketSize;
        }

        public void Initialise()
        {
            Thread outgoingThread = new Thread(ProcessOutgoingData);
            outgoingThread.Start();
        }

        public uint ReceiveWindowSize
        {
            get { return _receiveWindowSize; }
        }

        public uint ReceiveMaximumPacketSize
        {
            get { return _receiveMaximumPacketSize; }
        }

        public void ProcessChannelData(ChannelDataPacket dataPacket)
        {
            lock (_incomingData)
            {
                _incomingData.Enqueue(dataPacket);
                _incomingDataArrived.Set();
            }
        }

        public void ProcessChannelExtendedData(ChannelExtendedDataPacket packet)
        {
            lock (_incomingData)
            {
                _incomingData.Enqueue(packet);
                _incomingDataArrived.Set();
            }
        }

        public void EofReceived(ChannelEofPacket packet)
        {
            lock (_incomingData)
            {
                _incomingData.Enqueue(packet);
                _incomingDataArrived.Set();
            }
        }

        public void CloseReceived(ChannelClosePacket packet)
        {
            lock (_incomingData)
            {
                _incomingData.Enqueue(packet);
                _incomingDataArrived.Set();
            }

            if (Closed != null) Closed(this, EventArgs.Empty);
        }

        public void AdjustTransmitWindow(ChannelWindowAdjustPacket packet)
        {
            lock (_outgoingData)
            {
                _transmitWindowSize += packet.BytesToAdd;
            }
        }

        public void RequestReceived(ChannelRequestPacket packet)
        {
            switch (packet.RequestType)
            {
                case ChannelRequestType.PseudoTerminal:
                    // ToDo: Extract the terminal modes so they can be used by the server side class running the shell.
                    _consumerType = packet.RequestType;
                    break;

                case ChannelRequestType.Shell:
                    _consumerType = packet.RequestType;
                    _channelConsumer = _provider.GetService<IChannelConsumer>();
                    _channelConsumer.ChannelType = _consumerType;
                    TransportLayerManager.Disconnected += (s, e) => { Close(); _channelConsumer.Close(); };
                    _channelConsumer.AuthenticatedIdentity = TransportLayerManager.AuthenticatedIdentity;
                    _channelConsumer.Password = TransportLayerManager.Password;
                    _channelConsumer.Channel = this;
                    _channelConsumer.Initialise();
                    break;

                case ChannelRequestType.ExecuteCommand:
                    _consumerType = ChannelRequestType.ExecuteCommand;
                    _channelConsumer = _provider.GetService<IChannelCommandConsumer>();
                    _channelConsumer.ChannelType = _consumerType;
                    TransportLayerManager.Disconnected += (s, e) => { Close();  _channelConsumer.Close(); };
                    ((IChannelCommandConsumer)_channelConsumer).Command = ((ExecuteCommandPayload)packet.ChannelRequestPayload).Command;
                    _channelConsumer.AuthenticatedIdentity = TransportLayerManager.AuthenticatedIdentity;
                    _channelConsumer.Password = TransportLayerManager.Password;
                    _channelConsumer.Channel = this;
                    _channelConsumer.Initialise();
                    break;

                case ChannelRequestType.WindowChange:
                    // Ignore
                    _logger.LogInformation("Window change request.");
                    break;

                case ChannelRequestType.Environment:
                    // Ignore
                    _logger.LogInformation("Environment request.");
                    break;

                case ChannelRequestType.X11Forwarding:
                    // Ignore
                    _logger.LogInformation("X11Forwarding request.");
                    break;

                case ChannelRequestType.AuthenticationAgent:
                    // Ignore
                    _logger.LogInformation("AuthenticationAgent request.");
                    break;

                default:
                    TransportLayerManager.WritePacket(new ChannelFailurePacket() { RecipientChannel = ChannelId });
                    return;
            }

            if (packet.WantReply)
            {
                TransportLayerManager.WritePacket(new ChannelSuccessPacket() { RecipientChannel = ChannelId });
            }
        }
        
        public event EventHandler Closed;

        #endregion

        private void ProcessOutgoingData()
        {
            //log.Debug("ProcessOutgoingData start");

            try
            {
                Thread.CurrentThread.Name = THREAD_NAME_CHANNEL_OUTGOING;

                while (TransportLayerManager.Connected)
                {
                    Packet packet = null;

                    while (packet == null && TransportLayerManager.Connected)
                    {
                        lock (_outgoingData)
                        {
                            //log.Debug("ProcessOutgoingData queue size " + _outgoingData.Count);

                            if (_outgoingData.Count != 0)
                            {
                                packet = _outgoingData.Dequeue();
                                break;
                            }
                        }

                        //log.Debug("ProcessOutgoingData waiting");
                        _outgoingDataArrived.WaitOne(-1, true);
                       // log.Debug("ProcessOutgoingData notified");
                    }

                    if (TransportLayerManager.Connected)
                    {
                        while (_transmitWindowSize < packet.Length)
                        {
                            Thread.Sleep(50);
                        }

                        // log.Debug("ProcessOutgoingData send " + packet.PacketType);
                        TransportLayerManager.WritePacket(packet);
                        //log.Debug("ProcessOutgoingData send done");
                    }
                }
            }
            catch (IOException e)
            {
                // Probably an error writing to a disposed socket, ignore...
                _logger.LogInformation("Exception processing session: " + e.Message, e);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Exception processing session: " + e.Message, e);
            }
        }

        #region IChannelProducer Members
        
        public Packet GetIncomingPacket()
        {
            lock (this)
            {
                while (_incomingData.Count == 0 && TransportLayerManager.Connected)
                {
                    _incomingDataArrived.WaitOne(-1, true);
                }

                if (TransportLayerManager.Connected)
                {
                    Packet result = _incomingData.Dequeue();

                    TransportLayerManager.WritePacket(new ChannelWindowAdjustPacket()
                    {
                        RecipientChannel = ChannelId,
                        BytesToAdd = (uint)result.Length
                    });

                    return result;
                }
                else
                {
                    throw new TransportDisconnectException("Transport unexpectedly disconected.");
                }
            }
        }

        public void SendOutgoingPacket(Packet packet)
        {
            //log.Debug("SendOutgoingPacket " + packet.PacketType);

            lock (_outgoingData)
            {
                _outgoingData.Enqueue(packet);
                _outgoingDataArrived.Set();
            }

            //log.Debug("SendOutgoingPacket done");
        }

        public void SendData(byte[] buffer)
        {
            SendOutgoingPacket(new ChannelDataPacket() { RecipientChannel = ChannelId, Data = buffer });
        }

        public void SendErrorData(byte[] buffer)
        {
            SendOutgoingPacket(new ChannelExtendedDataPacket()
            {
                RecipientChannel = ChannelId,
                Data = buffer,
                ExtendedDataType = ExtendedDataType.StandardError
            });
        }

        public void Close()
        {
            if (TransportLayerManager.Connected)
            {
                SendOutgoingPacket(new ChannelClosePacket()
                {
                    RecipientChannel = ChannelId
                });
            }

            _outgoingDataArrived.Set();
            _incomingDataArrived.Set();

            if (Closed != null) Closed(this, EventArgs.Empty);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);   
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_channelConsumer != null) _channelConsumer.Dispose();
            }
        }

        #endregion
    }
}
