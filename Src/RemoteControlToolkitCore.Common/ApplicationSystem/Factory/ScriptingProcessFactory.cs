using System;
using System.Collections.Generic;
using System.Linq;
using RemoteControlToolkitCore.Common.Plugin;
using RemoteControlToolkitCore.Common.Scripting;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.VirtualFileSystem;

namespace RemoteControlToolkitCore.Common.ApplicationSystem.Factory
{
    [Plugin(PluginName = "Scripting")]
    public class ScriptingProcessFactory : PluginModule<ProcessFactorySubsystem>, IProcessFactory
    {
        private ScriptingSubsystem _scriptingSubsystem;
        private IServiceProvider _provider;

        public override void InitializeServices(IServiceProvider provider)
        {
            _provider = provider;
            _scriptingSubsystem = provider.GetService<ScriptingSubsystem>();
        }

        public IProcessBuilder CreateProcessBuilder(CommandRequest request, RctProcess parentProcess, IProcessTable table)
        {
            IScriptingEngine engine = _scriptingSubsystem.CreateEngine();
            var fileSystem = parentProcess.Extensions.Find<IExtensionFileSystem>().GetFileSystem();
            IProcessBuilder builder = table.CreateProcessBuilder()
                .SetProcessName(request.Arguments[0])
                .SetParent(parentProcess)
                .AddProcessExtensions(_provider.GetServices<IExtensionProvider<RctProcess>>())
                .SetAction((current, token) =>
                {
                    engine.ParentProcess = current;
                    engine.Token = token;
                    engine.SetIn(current.In);
                    engine.SetOut(current.Out);
                    engine.SetError(current.Error);
                    string fileName = request.Arguments[0];
                    List<string> argList = new List<string> { fileName };
                    argList.AddRange(request.Arguments.Length >= 1 ? request.Arguments.Skip(1) : request.Arguments);
                    engine.GetDefaultModule().AddVariable("argv", argList.ToArray());
                    return new CommandResponse(engine.ExecuteProgram(fileName, fileSystem));
                });

            return builder;
        }
    }
}