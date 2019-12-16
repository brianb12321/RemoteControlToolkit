/*
 * Copyright 2004-2008 Luke Quinane and Daniel Frampton
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
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSsh.Server.Configuration;
using NSsh.Common.Utility;
using NSsh.Common;
using NSsh.Server.Services;
using Microsoft.Win32;

namespace NSsh.Server
{
    /// <summary>
    /// Provides a SSH server service.
    /// </summary>
    [CoverageExclude("Hard to test :(")]
    public class NSshService : ISshService
    {
        /// <summary>
        /// Logging support for this class.
        /// </summary>
        private readonly ILogger<NSshService> _logger;

        private readonly IServiceProvider _provider;

        /// <summary>
        /// The configuration for this service.
        /// </summary>
        private NSshServiceConfiguration config;

        public NSshService(IServiceProvider provider)
        {
            _provider = provider;
            config = _provider.GetService<NSshServiceConfiguration>();
            _provider.GetService<IKeySetupService>().EnsureSetup();
            _logger = _provider.GetService<ILogger<NSshService>>();
        }

        public void Start()
        {
            _shutdown = false;
            handleConnections(new IPEndPoint(IPAddress.Any,22));
        }

        public void Stop()
        {
            _shutdown = true;

            lock (this)
            {
                // Kill each of the listening sockets
                foreach (TcpListener listener in _listenSockets)
                {
                    listener.Stop();
                }
                _listenSockets.Clear();
            }

            // wait for sessions threads to finish up
            if (_sessions.Count > 0)
            {
                Thread.Sleep(1000);
            }

            lock (this)
            {
                // non-graceful shutdown of smtp sessions
                foreach (Thread thread in _sessions.Values)
                {
                    thread.Abort();
                }
                _sessions.Clear();
            }
        }

        private bool _shutdown;

        private long _connectionsReceived;

        /// <summary>
        /// A list of sockets listening for new connections.
        /// </summary>
        private List<TcpListener> _listenSockets = new List<TcpListener>();

        /// <summary>
        /// A list of the current sessions.
        /// </summary>
        private Dictionary<ISshSession, Thread> _sessions = new Dictionary<ISshSession, Thread>();

        private void handleConnections(object endPointObject)
        {
            TcpListener socket = null;
            IPEndPoint endPoint = (IPEndPoint)endPointObject;

            try
            {
                socket = new TcpListener(endPoint);
                socket.Start();

                lock (this)
                {
                    // Register this socket
                    _listenSockets.Add(socket);
                }

                while (true)
                {
                    Socket client = socket.AcceptSocket();

                    Interlocked.Increment(ref _connectionsReceived);

                    _logger.LogInformation("Connection from " + client.RemoteEndPoint + ". connection count=" + _sessions.Count + ".");

                    lock (this)
                    {
                        // Create a new session and thread to handle this connection
                        ISshSession session = _provider.GetService<ISshSession>();
                        session.ClientSocket = client;
                        session.SocketStream = new NetworkStream(client);

                        int sameIPAddressCount =
                           (from sess in _sessions.Keys
                            where ((IPEndPoint)sess.ClientSocket.RemoteEndPoint).Address.ToString() == ((IPEndPoint)client.RemoteEndPoint).Address.ToString()
                            select sess).Count();

                        if (_sessions.Count >= config.MaximumClientConnections)
                        {
                            _logger.LogWarning("Rejecting connection from " + client.RemoteEndPoint + " due to maximum client connections of " 
                                + config.MaximumClientConnections + " limit being reached.");
                            
                            Thread rejectSessionThread = new Thread(session.Reject);
                            rejectSessionThread.Start();
                        }
                        else if (sameIPAddressCount >= config.MaximumSameIPAddressConnections)
                        {
                            _logger.LogWarning("Rejecting connection from " + client.RemoteEndPoint + " due to maximum client connections for "
                              + " same IP address of " +  config.MaximumSameIPAddressConnections + " limit being reached.");

                            Thread rejectSessionThread = new Thread(session.Reject);
                            rejectSessionThread.Start();
                        }
                        else
                        {
                            Thread sessionThread = new Thread(session.Process);
                            RegisterSession(session, sessionThread);
                            sessionThread.Start();
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                // Ignore interrupted exception during shutdown
                if (!_shutdown)
                {
                    _logger.LogWarning(e.Message, e);
                }
            }
            finally
            {
                lock (this)
                {
                    // De-register this socket
                    _listenSockets.Remove(socket);
                }
            }
        }

        /// <summary>
        /// Registers the session thread with the service so that it can be kill if
        /// required.
        /// </summary>
        /// <param name="sessionThread">The session thread to register.</param>
        public void RegisterSession(ISshSession session, Thread sessionThread)
        {
            lock (this)
            {
                _sessions.Add(session, sessionThread);
            }
        }

        /// <summary>
        /// Deregisters the session thread with the service. The session thread will not
        /// attempt to kill this thread in a shutdown.
        /// </summary>
        /// <param name="sessionThread">The session thread to deregister.</param>
        public void DeregisterSession(ISshSession session)
        {
            lock (this)
            {
                _sessions.Remove(session);
            }
        }
    }
}
