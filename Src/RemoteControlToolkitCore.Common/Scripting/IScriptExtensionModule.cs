using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting
{
    public interface IScriptExtensionModule : IPluginModule
    {
        void ConfigureDefaultEngine(IScriptingEngine engine);
    }
}