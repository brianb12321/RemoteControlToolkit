using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Configuration;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.NSsh.ChannelLayer.Console;
using RemoteControlToolkitCore.Common.NSsh.Configuration;
using RemoteControlToolkitCore.Common.NSsh.Packets;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel;
using RemoteControlToolkitCore.Common.NSsh.Packets.Channel.RequestPayloads;
using RemoteControlToolkitCore.Common.NSsh.TransportLayer;
using RemoteControlToolkitCore.Common.NSsh.Types;
using RemoteControlToolkitCore.Common.Utilities;

namespace RemoteControlToolkitCore.Common.NSsh.ChannelLayer
{
    public class Channel : IChannel, IChannelProducer
    {
        private const string THREAD_NAME_CHANNEL_OUTGOING = "channel-outgoing";

        /// <summary>
        /// Logging support for this class.
        /// </summary>
        private readonly ILogger<Channel> _logger;

        private PseudoTerminalPayload _terminalPayload;
        private readonly List<EnvironmentPayload> _environmentPayloads;
        public Guid ChannelGuid { get; private set; }

        private uint _transmitWindowSize;

        // ReSharper disable once NotAccessedField.Local
        private uint _transmitMaximumPacketSize;

        private readonly uint _receiveWindowSize;

        private readonly uint _receiveMaximumPacketSize;

        private readonly Queue<Packet> _incomingData = new Queue<Packet>();

        private readonly AutoResetEvent _incomingDataArrived = new AutoResetEvent(false);

        private readonly Queue<Packet> _outgoingData = new Queue<Packet>();

        private readonly AutoResetEvent _outgoingDataArrived = new AutoResetEvent(false);

        private IChannelConsumer _channelConsumer;

        private ChannelRequestType _consumerType;
        private readonly IServiceProvider _provider;

        public Channel(ILogger<Channel> logger, IServiceProvider provider)
        {
            _logger = logger;
            NSshServiceConfiguration config = provider.GetService<IWritableOptions<NSshServiceConfiguration>>().Value;
            _provider = provider;
            _receiveWindowSize = config.ReceiveWindowSize;
            _receiveMaximumPacketSize = config.ReceiveMaximumPacketSize;
            _environmentPayloads = new List<EnvironmentPayload>();
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

            Closed?.Invoke(this, EventArgs.Empty);
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

                    _consumerType = packet.RequestType;
                    _terminalPayload = (PseudoTerminalPayload) packet.ChannelRequestPayload;
                    break;

                case ChannelRequestType.Shell:
                    _consumerType = packet.RequestType;
                    _channelConsumer = _provider.GetService<IChannelConsumer>();
                    _channelConsumer.ChannelType = _consumerType;
                    _channelConsumer.InitialTerminalConfiguration = _terminalPayload;
                    _channelConsumer.InitialEnvironmentVariables.AddRange(_environmentPayloads);
                    TransportLayerManager.Disconnected += (s, e) =>
                    {
                        try
                        {
                            Close(); _channelConsumer.Close();
                        }
                        catch (RctProcessException)
                        {
                            
                        }
                    };
                    _channelConsumer.AuthenticatedIdentity = TransportLayerManager.AuthenticatedIdentity;
                    _channelConsumer.Channel = this;
                    _channelConsumer.Initialise();
                    break;

                case ChannelRequestType.ExecuteCommand:
                    _consumerType = ChannelRequestType.ExecuteCommand;
                    _channelConsumer = _provider.GetService<IChannelCommandConsumer>();
                    _channelConsumer.ChannelType = _consumerType;
                    TransportLayerManager.Disconnected += (s, e) =>
                    {
                        try
                        {
                            Close(); _channelConsumer.Close();
                        }
                        catch (RctProcessException)
                        {

                        }
                    };
                    _channelConsumer.InitialTerminalConfiguration = _terminalPayload;
                    _channelConsumer.InitialEnvironmentVariables.AddRange(_environmentPayloads);
                    ((IChannelCommandConsumer)_channelConsumer).Command = ((ExecuteCommandPayload)packet.ChannelRequestPayload).Command;
                    _channelConsumer.AuthenticatedIdentity = TransportLayerManager.AuthenticatedIdentity;
                    _channelConsumer.Channel = this;
                    _channelConsumer.Initialise();
                    break;

                case ChannelRequestType.WindowChange:
                    _logger.LogInformation("Window change request.");
                    _channelConsumer.SignalWindowChange((WindowChangePayload)packet.ChannelRequestPayload);
                    break;

                case ChannelRequestType.Environment:
                    EnvironmentPayload environmentPayload = (EnvironmentPayload) packet.ChannelRequestPayload;
                    _environmentPayloads.Add(environmentPayload);
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
                case ChannelRequestType.PuttyWinAdj:
                    packet.WantReply = false;
                    TransportLayerManager.WritePacket(new ChannelFailurePacket() {RecipientChannel = ChannelId});
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
                           // _logger.LogInformation("ProcessOutgoingData queue size " + _outgoingData.Count);

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
            SendOutgoingPacket(new ChannelDataPacket() { RecipientChannel = ChannelId, Data = buffer, Length = buffer.Length });
        }

        public void SendErrorData(byte[] buffer)
        {
            SendOutgoingPacket(new ChannelExtendedDataPacket()
            {
                RecipientChannel = ChannelId,
                Data = buffer,
                ExtendedDataType = ExtendedDataType.StandardError,
                Length = buffer.Length
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
            lock (_incomingData)
            {
                _incomingDataArrived.Set();
            }

            Closed?.Invoke(this, EventArgs.Empty);
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
