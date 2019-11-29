using System;

namespace RemoteControlToolkitCore.Common.Plugin
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginModuleAttribute : Attribute
    {
        public NetworkSide ExecutingSide { get; set; } = NetworkSide.Server | NetworkSide.Proxy;
        public string Name { get; set; }
    }
}