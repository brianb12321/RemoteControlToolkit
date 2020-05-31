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

        public IProcessBuilder CreateProcessBuilder(CommandRequest request, RctProcess parentProcess, IProcessTable table)
        {
            IApplication application = _applicationSubsystem.GetApplication(request.Arguments[0]);
            IProcessBuilder builder = table.CreateProcessBuilder()
                .SetAction((current, token) => application.Execute(request, current, token))
                .SetProcessName(application.ProcessName)
                .SetParent(parentProcess)
                .AddProcessExtensions(_provider.GetServices<IExtensionProvider<RctProcess>>());

            return builder;
        }
    }
}