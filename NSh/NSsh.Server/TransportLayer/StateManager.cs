using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSsh.Common.Utility;
using NSsh.Server.Services;
using NSsh.Server.TransportLayer.State;

namespace NSsh.Server.TransportLayer
{
    public class StateManager
    {
        public Dictionary<string, Func<AbstractTransportState>> States { get; }

        public StateManager(IServiceProvider provider, ILogger<ConnectedState> connectedLogger)
        {
            States = new Dictionary<string, Func<AbstractTransportState>>();
            States.Add(TransportLayerState.Authenticated.ToString(), () => new AuthenticatedState(provider));
            States.Add(TransportLayerState.Connected.ToString(), () => new ConnectedState(connectedLogger));
            States.Add(TransportLayerState.KeysExchanged.ToString(), () => new KeysExchangedState(provider));
            States.Add(TransportLayerState.VersionsExchanged.ToString(), () => new VersionsExchangedState(provider.GetService<ICipherFactory>(), provider.GetService<IKeySetupService>(), provider.GetService<ISecureRandom>(), provider.GetService<IMacFactory>()));
        }
    }
}