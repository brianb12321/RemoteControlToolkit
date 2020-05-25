using System;

namespace RemoteControlToolkitCore.Common.Plugin
{
    [Serializable]
    public class PluginSearchException : Exception
    {
        public PluginSearchException() { }
        public PluginSearchException(string message) : base(message) { }
        public PluginSearchException(string message, Exception inner) : base(message, inner) { }
        protected PluginSearchException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}