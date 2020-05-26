using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.Scripting.Hosting;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Networking;
using RemoteControlToolkitCore.Common.VirtualFileSystem.Zio;

namespace RemoteControlToolkitCore.Common.Scripting
{
    public interface IScriptingEngine : IDisposable
    {
        #region Engine Methods
        RctProcess ParentProcess { get; set; }
        CancellationToken Token { get; set; }
        void SetIn(TextReader reader);
        void SetOut(TextWriter writer);
        void SetError(TextWriter writer);
        ScriptIO IO { get; }
        IScriptExecutionContext ExecuteFile(string path);
        int ExecuteProgram(string file, IFileSystem fileSystem);
        IScriptExecutionContext ExecuteFile(string path, IScriptExecutionContext context);
        IScriptExecutionContext CreateModule(string name);
        IScriptExecutionContext GetDefaultModule();
        IScriptExecutionContext CreateContext();
        T ExecuteString<T>(string content);
        T ExecuteString<T>(string content, IScriptExecutionContext context);
        void AddAssembly(Assembly assembly);
        void AddPath(string path);
        void RemovePath(string path);
        #endregion
        #region Context Methods
        IScriptExecutionContext GetContext(string name);
        IScriptExecutionContext AddContext(string name);
        void RemoveContext(string name);
        #endregion
    }
}