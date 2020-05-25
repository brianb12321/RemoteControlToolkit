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
        public string DisplayName { get; set; }
        public string UniqueName { get; set; }

        public PluginLibraryAttribute(string uniqueName, string displayName)
        {
            UniqueName = uniqueName;
            DisplayName = displayName;
        }
    }
}