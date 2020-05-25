using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Hosting;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting.ScriptItems
{
    [Plugin]
    public class IOFunctions : PluginModule<ScriptingSubsystem>, IScriptExtensionModule
    {
        private string handleInput(dynamic prompt, IScriptingEngine engine, bool secure)
        {
            if (secure)
            {
                engine.ParentProcess.ClientContext.GetExtension<ITerminalHandler>().TerminalModes.ECHO = false;
            }
            engine.IO.OutputWriter.Write(prompt);
            string result = engine.IO.InputReader.ReadLine();
            if(secure) engine.ParentProcess.ClientContext.GetExtension<ITerminalHandler>().TerminalModes.ECHO = true;
            return result;
        }
        public void ConfigureDefaultEngine(IScriptingEngine engine)
        {
            engine.GetDefaultModule()
                .AddVariable("input", new Func<dynamic, string>((prompt) => handleInput(prompt, engine, false)));
                engine.GetDefaultModule().AddVariable("raw_input", new Func<dynamic, string>((prompt) => handleInput(prompt, engine, false)));
                engine.GetDefaultModule().AddVariable("secure_input", new Func<dynamic, string>(prompt => handleInput(prompt, engine, true)));
        }
    }
}