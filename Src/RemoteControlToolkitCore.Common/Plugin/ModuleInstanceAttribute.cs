using System;

namespace RemoteControlToolkitCore.Common.Plugin
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ModuleInstanceAttribute : Attribute
    {
        /// <summary>
        /// Determines whether the plugin loader should create an instance of a plugin module when a plugin library is loaded.
        /// </summary>
        public bool TransientMode { get; set; }
    }
}
