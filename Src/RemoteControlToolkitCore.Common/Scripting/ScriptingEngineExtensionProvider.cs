using System;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting
{
    public class ScriptingEngineExtensionProvider : IInstanceExtensionProvider
    {
        private IServiceProvider _services;

        public ScriptingEngineExtensionProvider(IServiceProvider services)
        {
            _services = services;
        }
        public void GetExtension(IInstanceSession context)
        {
            var engine = _services.GetService<IScriptingEngine>();
            context.Extensions.Add(engine.CreateContext());
        }

        public void RemoveExtension(IInstanceSession context)
        {
            
        }
    }
}