using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Plugin
{
    /// <summary>
    /// Exceptions that occurs during plugin initialization into the system.
    /// </summary>
    [Serializable]
    public class PluginLoadException : Exception
    {
        public PluginLoadException()
        {
        }

        public PluginLoadException(string message) : base(message)
        {
        }

        public PluginLoadException(string message, Exception inner) : base(message, inner)
        {
        }

        protected PluginLoadException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}