using System;

namespace RemoteControlToolkitCore.Common.Plugin
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class PluginLibraryAttribute : Attribute
    {
        public Type Startup { get; private set; }
        public NetworkSide LibraryType { get; set; }
        public string FriendlyName { get; set; }
        public string Name { get; private set; }
        public string Guid { get; set; }
        public string Version { get; set; } = "1.0.0.0";
        public PluginLibraryAttribute(Type startup, string name)
        {
            Startup = startup;
            Name = name;
        }
    }
}