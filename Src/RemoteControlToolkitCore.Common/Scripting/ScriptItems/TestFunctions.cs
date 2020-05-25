using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting.ScriptItems
{
    [Plugin]
    public class TestFunctions : PluginModule<ScriptingSubsystem>, IScriptExtensionModule
    {
        public void ConfigureDefaultEngine(IScriptingEngine engine)
        {
            engine.GetDefaultModule().AddVariable("test", new Action(() => engine.IO.OutputWriter.WriteLine("Hello world")));
        }
    }
}