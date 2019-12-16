using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Commandline.Parsing.CommandElements;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Proxy;

namespace RemoteControlToolkitCore.Common.Networking
{
    public class ProxyNetworkInstance : IInstanceSession
    {
        public IExtensionCollection<IInstanceSession> Extensions { get; }
        private readonly ILogger<ProxyNetworkInstance> _logger;
        public Guid ClientUniqueID { get; }
        public string Username { get; }
        private readonly IServerPool _pool;
        private readonly TcpClient _client;
        private NetworkStream _networkStream;
        private readonly RCTProcess _proxyProcess;
        private StreamReader _sr;
        private StreamWriter _sw;
        private RCTProcess _commandShell;

        public ProxyNetworkInstance(TcpClient client, ILogger<ProxyNetworkInstance> logger, IServerPool pool)
        {
            _logger = logger;
            ClientUniqueID = Guid.NewGuid();
            Extensions = new ExtensionCollection<IInstanceSession>(this);
            _pool = pool;
            ProcessTable = new ProcessTable();
            _client = client;
            _networkStream = _client.GetStream();
            _sr = new StreamReader(_networkStream);
            _sw = new StreamWriter(_networkStream);
            _sw.AutoFlush = true;
        }

        public TextReader GetClientReader()
        {
            return _sr;
        }

        public TextWriter GetClientWriter()
        {
            return _sw;
        }

        public void Start()
        {
            _proxyProcess.Start();
        }
        public IProcessTable ProcessTable { get; }
        public T GetExtension<T>() where T : IExtension<IInstanceSession>
        {
            return Extensions.Find<T>();
        }

        public void AddExtension<T>(T extension) where T : IExtension<IInstanceSession>
        {
            Extensions.Add(extension);
        }

        public void Close()
        {
            _sw.Close();
            _sr.Close();
            //_sslStream.Close();
            _networkStream.Close();
            _pool.RemoveServer(this);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Stream SocketStream { get; set; }
        public Socket ClientSocket { get; set; }
        public void Process()
        {
            throw new NotImplementedException();
        }

        public void Reject()
        {
            throw new NotImplementedException();
        }
    }
}