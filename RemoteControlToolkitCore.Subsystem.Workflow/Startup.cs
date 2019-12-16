using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RemoteControlToolkitCore.Common;

namespace RemoteControlToolkitCore.Subsystem.Workflow
{
    public class Startup : IApplicationStartup
    {
        public void ConfigureServices(IServiceCollection services, IAppBuilder builder)
        {
            services.AddSingleton<IWorkflowSubsystem, WorkflowSubsystem>();
        }

        public void PostConfigureServices(IServiceProvider provider, IHostApplication application)
        {
            
        }
    }
}