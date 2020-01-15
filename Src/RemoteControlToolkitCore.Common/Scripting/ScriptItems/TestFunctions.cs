using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting.ScriptItems
{
    [PluginModule]
    public class TestFunctions : IScriptExtensionModule
    {
        public void InitializeServices(IServiceProvider kernel)
        {
            
        }

        public void ConfigureDefaultEngine(IScriptingEngine engine)
        {
            engine.GetDefaultModule().AddVariable("test", new Action(() => engine.IO.OutputWriter.WriteLine("Hello world")));
        }
    }
}