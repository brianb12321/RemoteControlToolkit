using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting.ScriptItems
{
    [Plugin]
    public class DeviceFunctions : PluginModule<ScriptingSubsystem>, IScriptExtensionModule
    {
        public void ConfigureDefaultEngine(IScriptingEngine engine)
        {
            IScriptExecutionContext context = engine.CreateModule("dev");
        }
    }
}