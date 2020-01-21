using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting
{
    public class ScriptingSubsystem : BasePluginSubsystem<IScriptingSubsystem, IScriptExtensionModule>, IScriptingSubsystem
    {
        private List<string> _paths = new List<string>();

        public ScriptingSubsystem(IPluginLibraryLoader loader, IServiceProvider services) : base(loader, services)
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
            foreach (IScriptExtensionModule module in PluginLoader.ActivateAll<IScriptExtensionModule>())
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