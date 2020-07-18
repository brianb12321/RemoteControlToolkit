using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Common.ApplicationSystem.Factory
{
    [Plugin(PluginName = "Application")]
    public class ApplicationProcessFactory : PluginModule<ProcessFactorySubsystem>, IProcessFactory
    {
        private IServiceProvider _provider;
        private ApplicationSubsystem _applicationSubsystem;
        public override void InitializeServices(IServiceProvider provider)
        {
            _provider = provider;
            _applicationSubsystem = _provider.GetService<ApplicationSubsystem>();
        }

        public IProcessBuilder CreateProcessBuilder(RctProcess parentProcess, IProcessTable table)
        { ;
            IProcessBuilder builder = table.CreateProcessBuilder()
                .SetAction((args, current, token) =>
                {
                    IApplication application = _applicationSubsystem.GetApplication(args.Arguments[0]);
                    return application.Execute(args, current, token);
                })
                .SetProcessName(args =>
                {
                    IApplication application = _applicationSubsystem.GetApplicationWithoutInit(args);
                    return application.ProcessName;
                })
                .SetParent(parentProcess)
                .AddProcessExtensions(_provider.GetServices<IExtensionProvider<RctProcess>>());

            return builder;
        }
    }
}