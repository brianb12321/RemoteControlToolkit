using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting.ScriptItems
{
    [PluginModule]
    public class DeviceFunctions : IScriptExtensionModule
    {
        public void InitializeServices(IServiceProvider kernel)
        {
            
        }

        public void ConfigureDefaultEngine(IScriptingEngine engine)
        {
            IScriptExecutionContext context = engine.CreateModule("dev");
        }
    }
}