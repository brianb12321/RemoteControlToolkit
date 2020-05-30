using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Plugin
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class PluginLibraryAttribute : Attribute
    {
        public string DisplayName { get; }
        public string UniqueName { get; }

        public PluginLibraryAttribute(string uniqueName, string displayName)
        {
            UniqueName = uniqueName;
            DisplayName = displayName;
        }
    }
}