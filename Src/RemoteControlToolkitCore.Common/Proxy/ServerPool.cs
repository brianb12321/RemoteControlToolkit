using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.Networking;

namespace RemoteControlToolkitCore.Common.Proxy
{
    public class ServerPool : IServerPool
    {
        private List<IInstanceSession> _servers = new List<IInstanceSession>();
        private IInstanceSession _selectedServer;
        private ILogger<ServerPool> _logger;

        public ServerPool(ILogger<ServerPool> logger)
        {
            _logger = logger;
        }

        public IInstanceSession GetSelectedClient()
        {
            return _selectedServer;
        }

        public void AddServer(IInstanceSession client)
        {
            _servers.Add(client);
        }

        public IInstanceSession[] GetServers()
        {
            return _servers.ToArray();
        }

        public void SetSelectedClient(int id)
        {
            _selectedServer = _servers[id];
        }

        public void Clean()
        {
            for (int i = 0; i < _servers.Count; i++)
            {
                try
                {
                    _servers[i].Close();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Unable to close connection: {ex}");
                }
                finally
                {
                    _servers.RemoveAt(i);
                }
            }
        }

        public void RemoveServer(IInstanceSession server)
        {
            _servers.Remove(server);
        }

        public IReadOnlyDictionary<int, string> GetServersInt()
        {
            Dictionary<int, string> servers = new Dictionary<int, string>();
            foreach (var s in _servers)
            {
                servers.Add(_servers.IndexOf(s), s.ClientUniqueID.ToString());
            }
            return new ReadOnlyDictionary<int, string>(servers);
        }
    }
}