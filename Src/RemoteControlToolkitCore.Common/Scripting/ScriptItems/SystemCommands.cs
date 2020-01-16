﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteControlToolkitCore.Common.ApplicationSystem;
using RemoteControlToolkitCore.Common.Plugin;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Commandline;
using RemoteControlToolkitCore.Common.Commandline.Parsing.CommandElements;
using RemoteControlToolkitCore.Common.VirtualFileSystem;
using Zio;

namespace RemoteControlToolkitCore.Common.Scripting.ScriptItems
{
    [PluginModule]
    public class SystemCommands : IScriptExtensionModule
    {
        private IApplicationSubsystem _subSystem;
        public void InitializeServices(IServiceProvider kernel)
        {
            _subSystem = kernel.GetService<IApplicationSubsystem>();
        }

        private void addExFunction(IScriptingEngine engine, IScriptExecutionContext context)
        {
            context.AddVariable("ex", new Func<string, CommandResponse>((command) =>
            {
                var application = _subSystem.Factory.CreateOnApplication(engine.ParentProcess.ClientContext,
                    _subSystem.GetApplication("shell"), engine.ParentProcess, new CommandRequest(new[]
                    {
                        new StringCommandElement("shell"),
                        new StringCommandElement("-c"),
                        new StringCommandElement(command)
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
            IScriptExecutionContext context = engine.CreateModule("rSys");
            addExFunction(engine, context);
            addEnvironmentFunctions(engine, context);
        }
    }
}