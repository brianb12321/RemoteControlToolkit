using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Plugin
{
    /// <summary>
    /// Represents the main communication point between the host and plugin library.
    /// </summary>
    public abstract class PluginBase : MarshalByRefObject
    {
        public IHostApplication PluginHost { get; }

        protected PluginBase(IHostApplication host)
        {
            PluginHost = host;
        }
        public abstract void InitializePluginLibrary();
    }
}