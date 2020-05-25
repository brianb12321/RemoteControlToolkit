using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Networking;

namespace RemoteControlToolkitCore.Common.Plugin
{
    /// <summary>
    /// Code that extends the system. Used in conjunction with a plugin subsystem.
    /// </summary>
    public interface IPluginModule<TSubsystem> where TSubsystem : PluginSubsystem
    {
        TSubsystem ParentSubsystem { get; set; }

        PluginAttribute GetPluginAttribute();
        void InitializeServices(IServiceProvider provider);
    }
}