using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.Scripting
{
    public interface IScriptExtensionModule : IPluginModule<ScriptingSubsystem>
    {
        void ConfigureDefaultEngine(IScriptingEngine engine);
    }
}