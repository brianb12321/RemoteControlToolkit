using System.ServiceModel;
using RemoteControlToolkitCore.Common.Networking;

namespace RemoteControlToolkitCore.Common.Scripting
{
    public interface IScriptExecutionContext : IExtension<IInstanceSession>
    {
        bool ContainsVariable(string name);
        T GetVariable<T>(string name);
        void AddVariable<T>(string name, T variable);
        string[] GetAllVariableNames();
        T[] GetAllVariablesByType<T>();
    }
}