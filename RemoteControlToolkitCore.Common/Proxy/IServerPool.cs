using System.Collections.Generic;
using RemoteControlToolkitCore.Common.Networking;

namespace RemoteControlToolkitCore.Common.Proxy
{
    public interface IServerPool
    {
        IInstanceSession GetSelectedClient();
        void AddServer(IInstanceSession client);
        IInstanceSession[] GetServers();
        void RemoveServer(IInstanceSession server);
        IReadOnlyDictionary<int, string> GetServersInt();
        void SetSelectedClient(int id);
        void Clean();
    }
}