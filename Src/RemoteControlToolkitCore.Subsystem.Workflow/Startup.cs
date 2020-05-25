using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common;
using RemoteControlToolkitCore.Common.Plugin;

namespace RemoteControlToolkitCore.Subsystem.Workflow
{
    [Plugin]
    public class Startup : IApplicationStartup
    {
        public void ConfigureServices(IServiceCollection services, IAppBuilder builder)
        {
            services.AddSingleton(provider =>
                new WorkflowSubsystem(provider.GetRequiredService<IHostApplication>().PluginManager, provider));
        }

        public void PostConfigureServices(IServiceProvider provider, IHostApplication application)
        {
            
        }
    }
}