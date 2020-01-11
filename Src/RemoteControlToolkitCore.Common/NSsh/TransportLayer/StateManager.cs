using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RemoteControlToolkitCore.Common.NSsh.Services;
using RemoteControlToolkitCore.Common.NSsh.TransportLayer.State;
using RemoteControlToolkitCore.Common.NSsh.Utility;

namespace RemoteControlToolkitCore.Common.NSsh.TransportLayer
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