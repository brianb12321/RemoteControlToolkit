using System.Linq;
using System.ServiceModel;
using Microsoft.Scripting.Hosting;
using RemoteControlToolkitCore.Common.Networking;

namespace RemoteControlToolkitCore.Common.Scripting
{
    public class IronPythonScriptExecutionContext : IScriptExecutionContext
    {
        public ScriptScope CurrentScriptScope { get; set; } = null;

        public IronPythonScriptExecutionContext(ScriptEngine engine)
        {
            CurrentScriptScope = engine.CreateScope();
        }
        public IronPythonScriptExecutionContext(ScriptScope scope)
        {
            CurrentScriptScope = scope;
        }
        public void AddVariable<T>(string name, T variable)
        {
            CurrentScriptScope.SetVariable(name, variable);
        }

        public bool ContainsVariable(string name)
        {
            return CurrentScriptScope.ContainsVariable(name);
        }

        public string[] GetAllVariableNames()
        {
            return CurrentScriptScope.GetVariableNames().ToArray();
        }

        public T[] GetAllVariablesByType<T>()
        {
            return CurrentScriptScope.GetVariableNames()
                .Where(s => CurrentScriptScope.GetVariable(s) is T)
                .Select(s => GetVariable<T>(s))
                .ToArray();
        }

        public T GetVariable<T>(string name)
        {
            return CurrentScriptScope.GetVariable<T>(name);
        }

        void IExtension<IInstanceSession>.Attach(IInstanceSession owner)
        {
            
        }

        void IExtension<IInstanceSession>.Detach(IInstanceSession owner)
        {
            
        }
    }
}