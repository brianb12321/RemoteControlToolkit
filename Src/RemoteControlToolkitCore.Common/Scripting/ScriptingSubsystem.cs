using System;
using System.Collections.Generic;
using System.Linq;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting
{
    public class ScriptingSubsystem : PluginSubsystem
    {
        private readonly List<string> _paths;
        private readonly IServiceProvider _provider;

        public ScriptingSubsystem(IPluginManager manager, IServiceProvider provider) : base(manager)
        {
            _provider = provider;
            _paths = new List<string>();
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
                module?.InitializeServices(_provider);
                module?.ConfigureDefaultEngine(engine);
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