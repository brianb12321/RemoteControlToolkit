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
using RemoteControlToolkitCore.Common.Plugin;
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
        private NetworkStream _networkStream;
        private readonly RCTProcess _proxyProcess;
        private StreamReader _sr;
        private StreamWriter _sw;

        public ProxyNetworkInstance(TcpClient client, ILogger<ProxyNetworkInstance> logger, IApplicationSubsystem appSubsystem, IServerPool pool)
        {
            Extensions = new ExtensionCollection<IInstanceSession>(this);
            _pool = pool;
            ProcessTable = new ProcessTable();
            _client = client;
            _proxyProcess = ProcessTable.Factory.Create(this, "Proxy Client", (current, token) =>
            {
                try
                {
                    _networkStream = _client.GetStream();
                    _sw = new StreamWriter(_networkStream);
                    _sr = new StreamReader(_networkStream);
                    while (true)
                    {
                        Task.Delay(-1).Wait();
                    }
                }
                finally
                {
                    _sw.Close();
                    _sr.Close();
                    //_sslStream.Close();
                    _networkStream.Close();
                    _pool.RemoveServer(this);
                }
                return new CommandResponse(CommandResponse.CODE_SUCCESS);
            }, null);
        }

        public StreamReader GetClientReader()
        {
            return _sr;
        }

        public StreamWriter GetClientWriter()
        {
            return _sw;
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
    }
}