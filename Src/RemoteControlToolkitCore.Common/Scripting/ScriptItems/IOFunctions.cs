using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Hosting;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting.ScriptItems
{
    [PluginModule]
    public class IOFunctions : IScriptExtensionModule
    {
        public void InitializeServices(IServiceProvider kernel)
        {
            
        }
        private string handleInput(dynamic prompt, ScriptIO io)
        {
            io.OutputWriter.Write(prompt);
            return io.InputReader.ReadLine();
        }
        public void ConfigureDefaultEngine(IScriptingEngine engine)
        {
            engine.GetDefaultModule()
                .AddVariable("input", new Func<dynamic, string>((prompt) => handleInput(prompt, engine.IO)));
                engine.GetDefaultModule().AddVariable("raw_input", new Func<dynamic, string>((prompt) => handleInput(prompt, engine.IO)));
        }
    }
}