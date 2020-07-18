using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RemoteControlToolkitCore.Common
{
    public interface IApplicationStartup
    {
        IConfiguration Configuration { get; set; }
        void ConfigureServices(IServiceCollection services);
        void PostConfigureServices(IServiceProvider provider, IHostApplication application);
    }
}