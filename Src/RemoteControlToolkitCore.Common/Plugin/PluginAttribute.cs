using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Plugin
{
    /// <summary>
    /// Designates an implemented class as a plugin implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginAttribute : Attribute
    {
        /// <summary>
        /// A unique plugin name.
        /// </summary>
        public string PluginName { get; set; }
    }
}