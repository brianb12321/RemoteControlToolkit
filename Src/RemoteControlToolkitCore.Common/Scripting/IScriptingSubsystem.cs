using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting
{
    public interface IScriptingSubsystem : IPluginSubsystem<IScriptExtensionModule>
    {
        IScriptingEngine CreateEngine();
    }
}