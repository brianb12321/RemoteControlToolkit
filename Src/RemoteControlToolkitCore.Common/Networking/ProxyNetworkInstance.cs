using System;
using System.IO;
using System.Net.Sockets;
using System.ServiceModel;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Proxy;

namespace RemoteControlToolkitCore.Common.Networking
{
    public class ProxyNetworkInstance : IInstanceSession
    {
        public IExtensionCollection<IInstanceSession> Extensions { get; }
        public Guid ClientUniqueID { get; }
        public string Username { get; }
        private readonly IServerPool _pool;
        private readonly TcpClient _client;
        private readonly NetworkStream _networkStream;
        private readonly RctProcess _proxyProcess;
        private readonly StreamReader _sr;
        private readonly StreamWriter _sw;

        public ProxyNetworkInstance(TcpClient client, IServerPool pool)
        {
            _proxyProcess = null;
            _networkStream = null;
            _pool = pool;
            ClientUniqueID = Guid.NewGuid();
            Extensions = new ExtensionCollection<IInstanceSession>(this);
            ProcessTable = new ProcessTable();
            _client = client;
            _networkStream = _client.GetStream();
            _sr = new StreamReader(_networkStream);
            _sw = new StreamWriter(_networkStream) {AutoFlush = true};
        }

        public StreamReader GetClientReader()
        {
            return _sr;
        }

        public Stream OpenNetworkStream()
        {
            return _networkStream;
        }

        public StreamWriter GetClientWriter()
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