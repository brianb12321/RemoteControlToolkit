using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlToolkitCore.Common.Plugin
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ApplicationStartupAttribute : Attribute
    {
        public Type Startup { get; }

        public ApplicationStartupAttribute(Type startup)
        {
            Startup = startup;
        }
    }
}