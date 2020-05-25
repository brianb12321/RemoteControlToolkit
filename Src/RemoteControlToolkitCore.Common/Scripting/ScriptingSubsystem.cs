using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting
{
    public class ScriptingSubsystem : PluginSubsystem
    {
        private List<string> _paths = new List<string>();

        public ScriptingSubsystem(IPluginManager manager) : base(manager)
        {

        }

        private void addPaths(IScriptingEngine engine)
        {
            foreach (var path in _paths)
            {
                engine.AddPath(path);
            }
        }
        private void populateGlobalScope(IScriptingEngine engine)
        {
            foreach (IScriptExtensionModule module in PluginManager.ActivateAllPluginModules<ScriptingSubsystem>().Select(m => m as IScriptExtensionModule))
            {
                module.ConfigureDefaultEngine(engine);
            }
        }

        public IScriptingEngine CreateEngine()
        {
            IronPythonScriptingEngine engine = new IronPythonScriptingEngine();
            addPaths(engine);
            populateGlobalScope(engine);
            return engine;
        }
    }
}