using System;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Plugin;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Commandline;

namespace RemoteControlToolkitCore.Common.Scripting.ScriptItems
{
    [Plugin]
    public class SystemCommands : PluginModule<ScriptingSubsystem>, IScriptExtensionModule
    {
        private ApplicationSubsystem _subSystem;
        public override void InitializeServices(IServiceProvider kernel)
        {
            _subSystem = kernel.GetService<ApplicationSubsystem>();
        }

        private void addExFunction(IScriptingEngine engine, IScriptExecutionContext context)
        {
            context.AddVariable("ex_application", new Func<string, string[], RctProcess>((name, args) =>
            {
                var application = _subSystem.Factory.CreateOnApplication(engine.ParentProcess.ClientContext,
                    _subSystem.GetApplication(name), engine.ParentProcess,
                    new CommandRequest(args), engine.ParentProcess.Identity);
                application.SetOut(engine.IO.OutputWriter);
                application.SetIn(engine.IO.InputReader);
                application.SetError(engine.IO.ErrorWriter);
                return application;
            }));
            context.AddVariable("ex", new Func<string, CommandResponse>((command) =>
            {
                var application = _subSystem.Factory.CreateOnApplication(engine.ParentProcess.ClientContext,
                    _subSystem.GetApplication("shell"), engine.ParentProcess, new CommandRequest(new[]
                    {
                        "shell", "-c", command
                    }), engine.ParentProcess.Identity);
                application.SetOut(engine.IO.OutputWriter);
                application.SetIn(engine.IO.InputReader);
                application.SetError(engine.IO.ErrorWriter);
                application.Start();
                application.WaitForExit();
                //Reset IO
                engine.SetIn(engine.ParentProcess.In);
                engine.SetOut(engine.ParentProcess.Out);
                engine.SetError(engine.ParentProcess.Error);
                return application.ExitCode;
            }));
        }

        private void addTerminalFunctions(IScriptingEngine engine, IScriptExecutionContext context)
        {
            context.AddVariable("get_tty", new Func<ITerminalHandler>(() => engine.ParentProcess.ClientContext.GetExtension<ITerminalHandler>()));
        }
        private void addEnvironmentFunctions(IScriptingEngine engine, IScriptExecutionContext context)
        {
            context.AddVariable("get_envar", new Func<string, string>(variable => engine.ParentProcess.EnvironmentVariables[variable]));
            context.AddVariable("set_envar", new Action<string, string>((variable, value) =>
            {
                if(engine.ParentProcess.EnvironmentVariables.ContainsKey(variable))
                    engine.ParentProcess.EnvironmentVariables[variable] = value;
                else
                {
                    engine.ParentProcess.EnvironmentVariables.Add(variable, value);
                }
            }));
            context.AddVariable("pwd", new Func<string>(() => engine.ParentProcess.WorkingDirectory.ToString()));
        }
        public void ConfigureDefaultEngine(IScriptingEngine engine)
        {
            engine.GetDefaultModule().AddVariable("token", engine.Token);
            IScriptExecutionContext context = engine.CreateModule("remote_sys");
            addExFunction(engine, context);
            addEnvironmentFunctions(engine, context);
            addTerminalFunctions(engine, context);
        }
    }
}