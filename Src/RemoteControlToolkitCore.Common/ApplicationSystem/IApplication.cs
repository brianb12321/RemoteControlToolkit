using System;
using System.ServiceModel;
using System.Threading;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public interface IApplication : IPluginModule, IDisposable
    {
        string ProcessName { get; }
        CommandResponse Execute(CommandRequest args, RCTProcess currentProcess, CancellationToken token);
    }
}