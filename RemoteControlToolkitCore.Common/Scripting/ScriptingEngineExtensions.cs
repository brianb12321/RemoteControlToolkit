namespace RemoteControlToolkitCore.Common.Scripting
{
    public static class ScriptingEngineExtensions
    {
        public const string SESSION_NAME = "sessionContext";
        public static IScriptExecutionContext GetSessionContext(this IScriptingEngine engine)
        {
            return engine.GetContext(SESSION_NAME);
        }
        public static void ResetSessionContext(this IScriptingEngine engine)
        {
            engine.AddContext(SESSION_NAME);
        }
    }
}