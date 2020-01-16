﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace RemoteControlToolkitCore.Common
{
    public interface IApplicationStartup
    {
        void ConfigureServices(IServiceCollection services, IAppBuilder builder);
        void PostConfigureServices(IServiceProvider provider, IHostApplication application);
    }
}