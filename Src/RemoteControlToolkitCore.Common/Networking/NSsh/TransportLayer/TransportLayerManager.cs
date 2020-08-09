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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.Configuration;
using RemoteControlToolkitCore.Common.Networking.NSsh.Configuration;
using RemoteControlToolkitCore.Common.Networking.NSsh.Packets;
using RemoteControlToolkitCore.Common.Networking.NSsh.TransportLayer.State;
using RemoteControlToolkitCore.Common.Networking.NSsh.Types;
using RemoteControlToolkitCore.Common.Networking.NSsh.Utility;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.TransportLayer
{
    public class TransportLayerManager : ITransportLayerManager
    {
        private const int VERSION_STRING_MAX_LENGTH = 255;

        /// <summary>
        /// Logging support for this class.
        /// </summary>
        private ILogger<TransportLayerManager> _logger;

        private AbstractTransportState _state;
        public AbstractTransportState State
        {
            get { return _state; }
        }

        private Stream _stream;

        private Timer _idleTimeout;

        private EndPoint _clientEndPoint;

        private IPacketFactory _packetFactory;
        private ISecureRandom _random;
        private IMacFactory _macFactory;
        private NSshServiceConfiguration _config;
        private StateManager _stateManager;

        public TransportLayerManager(ILogger<TransportLayerManager> logger, IPacketFactory factory, ISecureRandom random, IWritableOptions<NSshServiceConfiguration> config, StateManager stateManager, IMacFactory macFactory)
        {
            _logger = logger;
            _stateManager = stateManager;
            CommunicationLock = new object();
            _packetFactory = factory;
            _random = random;
            _config = config.Value;
            _macFactory = macFactory;
        }

        ~TransportLayerManager()
        {
            Dispose(false);
        }

        private void IdleTimeoutCallback(object state)
        {
            if (Connected && OnIdleTimeout != null)
            {
                OnIdleTimeout(this, EventArgs.Empty);
            }
        }

        #region ITransportLayerManager Members

        public bool Connected { get; private set; }

        public event EventHandler OnIdleTimeout;

        public event EventHandler Disconnected;

        public void StartIdleTimeout(TimeSpan idleTimeout)
        {
            // create a new timer to timeout idle connections
            _idleTimeout = new Timer(
                new TimerCallback(IdleTimeoutCallback),
                null,
                idleTimeout,
                TimeSpan.FromMilliseconds(-1));
        }

        public void Process(Stream stream)
        {
            Connected = true;

            _stream = stream;

            _clientEndPoint = ClientSocket.RemoteEndPoint;

            ChangeState(TransportLayerState.Connected);

            while (Connected)
            {
                _state.Process(this);
            }
        }

        public void Reject(Stream stream)
        {
            Connected = true;

            _stream = stream;

            _clientEndPoint = ClientSocket.RemoteEndPoint;

            ChangeState(TransportLayerState.Connected);

            Disconnect(DisconnectReason.TooManyConnections);
        }

        public void Disconnect(DisconnectReason reason)
        {
            _logger.LogInformation("Disconnecting " + _clientEndPoint + " with " + reason + ".");

            DisconnectPacket disconnect = new DisconnectPacket();
            disconnect.DisconnectReason = reason;

            // TODO: Get proper description and language tag
            disconnect.Description = string.Empty;
            disconnect.LanguageTag = string.Empty;

            WritePacket(disconnect);

            Disconnect();
        }

        public void DisconnectReceived(DisconnectPacket packet)
        {
            _logger.LogInformation("Disconnect received from " + _clientEndPoint + ".");

            Disconnect();
        }

        private void Disconnect()
        {
            Connected = false;

            if (Disconnected != null)
            {
                Disconnected(this, new EventArgs());
            }

            if (_idleTimeout != null)
            {
                _idleTimeout.Dispose();
                _idleTimeout = null;
            }

            if (_stream != null)
            {
                _stream.Close();
            }

            try
            {
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
            }
            catch (Exception excp)
            {
                _logger.LogInformation("Exception closing client socket. " + excp.Message);
            }
        }

        /// <summary>
        /// Writes data directly to the network stream. This is only used to exchange the SSH version 
        /// strings which are the first packets in the intialisation sequence. 
        /// </summary>
        /// <param name="data">The data to write to the stream.</param>
        public void Write(byte[] data)
        {
            try
            {
                _stream.Write(data);
                _stream.Flush();
            }
            catch (Exception excp)
            {
                StreamExceptionHandler(excp);
            }
        }

        /// <summary>
        /// This method is used to only read from the network stream up until and including the location 
        /// of the first CR LF sequence. The method is only used to read the version string from the remote
        /// end which will always be the first packet in the exchange. RFC 4253 chapter 
        /// "4.2. Protocol Version Exchange" specifies that the version strings must be ASCII encoded.
        /// </summary>
        /// <returns>An ASCII representation of the bytes read NOT including the CR LF.</returns>
        public string ReadLine()
        {
            try
            {
                bool eol = false;
                byte[] buffer = new byte[VERSION_STRING_MAX_LENGTH];
                int index = 0;
                while (!eol && index < VERSION_STRING_MAX_LENGTH)
                {
                    buffer[index] = (byte)_stream.ReadByte();
                    // Detect the end of line.
                    if (index > 0 && buffer[index - 1] == 0x0d && buffer[index] == 0x0a)
                    {
                        return Encoding.ASCII.GetString(buffer, 0, index - 1);
                    }
                    else
                    {
                        index++;
                    }
                }

                throw new ApplicationException("The end of line was not detected on the version string.");
            }
            catch (Exception excp)
            {
                StreamExceptionHandler(excp);
                return null;
            }
        }

        public void WritePacket(Packet packet)
        {
            try
            {
                _stream.Write(packet.ToByteArray(_transmitTransform, TransmitMac?.CreateMac(), TransmitSequenceNumber++, _random));
                _stream.Flush();
            }
            catch (Exception excp)
            {
                StreamExceptionHandler(excp);
            }
        }

        public Packet ReadPacket()
        {
            try
            {
                Packet result;
                result = _packetFactory.ReadFrom(_stream, _receiveTransform, ReceiveMac?.CreateMac(), ReceiveSequenceNumber++);
                //log.Debug("READ PACKET: " + result);

                if (AuthenticatedIdentity != null && Connected)
                {
                    _idleTimeout.Change(_config.IdleTimeout, TimeSpan.FromMilliseconds(-1));
                }

                return result;
            }
            catch (Exception excp)
            {
                StreamExceptionHandler(excp);
                return null;
            }
        }

        private void StreamExceptionHandler(Exception excp)
        {
            if (excp is ObjectDisposedException)
            {
                _logger.LogInformation($"Stream was disconnected with ObjectDisposedException: {excp.ToString()}");
                if (Connected)
                {
                    Disconnect();
                }
            }
            else if (excp is EndOfStreamException)
            {
                _logger.LogInformation("Stream was disconnected by EndOfStreamException.");
                if (Connected)
                {
                    Disconnect();
                }
                return;
            }
            else if (excp is IOException)
            {
                _logger.LogInformation("Stream was disconnected by SocketException.");
                if (Connected)
                {
                    Disconnect();
                }
                return;
            }

            throw excp;
        }

        public Socket ClientSocket { get; set; }

        public string ClientVersion { get; set; }

        public string ServerVersion { get; set; }

        public void ChangeState(TransportLayerState newState)
        {
            _logger.LogInformation("Changing state to " + newState + ".");

            if (newState == TransportLayerState.VersionsExchanged)
            {
                TimeSpan idleTimeout = _config.AuthenticatedTimeout;
                _idleTimeout.Change(idleTimeout, TimeSpan.FromMilliseconds(-1));
            }
            else if (newState == TransportLayerState.Authenticated)
            {
                TimeSpan idleTimeout = _config.IdleTimeout;
                _idleTimeout.Change(idleTimeout, TimeSpan.FromMilliseconds(-1));
            }

            string stateKey = newState.ToString();
            _state = _stateManager.States[stateKey]();
        }

        public TransportLayerParameters Parameters { get; set; }

        public KexInitPacket ClientKexInit { get; set; }
        public KexInitPacket ServerKexInit { get; set; }

        public BigInteger E { get; set; }

        public BigInteger X { get; set; }

        public BigInteger Key { get; set; }

        public byte[] Hash { get; set; }

        public byte[] SessionId { get; set; }

        public object CommunicationLock { get; set; }

        public uint TransmitSequenceNumber { get; set; }

        public uint ReceiveSequenceNumber { get; set; }

        private ICryptoTransform _transmitTransform;

        private SymmetricAlgorithm _transmitCipher;

        public SymmetricAlgorithm TransmitCipher
        {
            get { return _transmitCipher; }
            set
            {
                _transmitCipher = value;
                _transmitTransform = _transmitCipher.CreateEncryptor();
            }
        }

        private ICryptoTransform _receiveTransform;

        private SymmetricAlgorithm _receiveCipher;

        public SymmetricAlgorithm ReceiveCipher
        {
            get { return _receiveCipher; }
            set
            {
                _receiveCipher = value;
                _receiveTransform = _receiveCipher.CreateDecryptor();
            }
        }

        public HashAlgorithmCreator TransmitMac { get; set; }

        public HashAlgorithmCreator ReceiveMac { get; set; }

        public IIdentity AuthenticatedIdentity { get; set; }

        public string Password { get; set; }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_state != null) _state.Dispose();
            }
        }

        #endregion
    }
}
