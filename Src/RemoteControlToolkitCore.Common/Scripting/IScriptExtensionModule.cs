using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting
{
    [ModuleInstance(TransientMode = true)]
    public interface IScriptExtensionModule : IPluginModule
    {
        void ConfigureDefaultEngine(IScriptingEngine engine);
    }
}