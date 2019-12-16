using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using NSsh.Server.TransportLayer;
using System.Threading;
using NSsh.Common.Utility;
using NSsh.Common.Packets;
using NSsh.Server.Configuration;
using System.IO;
using Microsoft.Extensions.Logging;

namespace NSsh.Server
{
    public class SshSession : ISshSession
    {
        /// <summary>
        /// Logging support for this class.
        /// </summary>
        protected ILogger<SshSession> _logger;

        private NSshServiceConfiguration _config;

        ITransportLayerManager _transportManager;

        ISshService _sshService;

        public SshSession(ISshService service, ITransportLayerManager manager, ILogger<SshSession> logger, NSshServiceConfiguration config)
        {
            _sshService = service;
            _transportManager = manager;
            _logger = logger;
            _config = config;
        }

        ~SshSession()
        {
            Dispose(false);
        }

        #region ISshSession Members

        public Stream SocketStream { get; set; }

        private Socket _clientSocket;
        public Socket ClientSocket
        {
            get { return _clientSocket;}
            set 
            {
                _clientSocket = value;
                _transportManager.ClientSocket = value; 
            }
        }

        public void Process()
        {
            try
            {
                // Setup idle timeouts
                _transportManager.OnIdleTimeout += RemoteEndTimedOut;
                _transportManager.StartIdleTimeout(_config.VersionsExchangedTimeout);

                // Process incoming packets
                _transportManager.Process(SocketStream);
            }
            catch (IOException e)
            {
                // Probably an error writing to a disposed socket, ignore...
                _logger.LogInformation("Exception processing session: " + e.Message, e);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Exception processing session: " + e.Message, e);
                _transportManager.Disconnect(DisconnectReason.ProtocolError);
            }
            finally
            {
                // ... but ensure we are cleaned up
                SocketStream.Close();
            }

            // Prevent the service from attempting to kill this thread on shutdown
            _sshService.DeregisterSession(this);
        }

        public void Reject()
        {
            _transportManager.Reject(SocketStream);
        }

        #endregion

        /// <summary>
        /// Disconnects the remote client if the connection is idle for too long.
        /// </summary>
        private void RemoteEndTimedOut(object sender, EventArgs e)
        {
            try
            {
                _logger.LogInformation("Remote client " + _clientSocket .ToString() + " idle for too long in state " + _transportManager.State + ", disconnecting.");
                _transportManager.Disconnect(DisconnectReason.ByApplication);
            }
            catch (IOException ex)
            {
                // Probably an error writing to a disposed socket, ignore...
                _logger.LogInformation("Exception processing session: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Exception processing session: " + ex.Message, ex);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_transportManager != null) _transportManager.Dispose();
                if (SocketStream != null) SocketStream.Dispose();
            }
        }

        #endregion
    }
}
