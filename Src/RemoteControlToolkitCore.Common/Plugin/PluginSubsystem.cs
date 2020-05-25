using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Plugin
{
    /// <summary>
    /// The front-end interface for interacting with the plugin manager.
    /// </summary>
    public abstract class PluginSubsystem
    {
        protected IPluginManager PluginManager { get; }
        protected PluginSubsystem(IPluginManager pluginManager)
        {
            PluginManager = pluginManager;
        }

        public virtual void InitializeSubsystem()
        {

        }
    }
}